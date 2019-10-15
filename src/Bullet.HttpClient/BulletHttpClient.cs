using Bullet.Client.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bullet.Client
{
    public class BulletHttpClient
    {
        private TcpClient _tcpClient;
        private Uri _uri;
        private const int IO_CONTROL_CODE = -1744830448;
        private const int BUFFER_SIZE = 8192;
        private Stream _stream;
        private byte[] _requestHeader;
        private Pipe _pipe;
        private bool _isReadingFromStream = false;
        private int _totalBytesRead = 0;
        private int _totalRequests = 0;
        private int _totalReads = 0;
        private List<Task> _readTasks;

        private SemaphoreSlim _lock;

        public List<BulletHttpHeader> RequestHeaders { get; private set; } = new List<BulletHttpHeader>();

        public BulletHttpClient(string url)
        {
            _uri = new Uri(url);
            _requestHeader = Encoding.UTF8.GetBytes($"GET {_uri.PathAndQuery} HTTP/1.1\r\nAccept-Encoding: gzip, deflate, sdch\r\nHost: {_uri.Host}\r\nContent-Length: 0\r\n\r\n");
            _pipe = new Pipe();
            _lock = new SemaphoreSlim(1,1);
            _readTasks = new List<Task>();

            SetupTcpClient();
        }

        public async ValueTask GetAsync(string url)
        {
            Connect();

            var socket = _tcpClient.Client;

            var header = _requestHeader.AsMemory();
            
            await SendAsnc(socket, header)
                .AsTask();
            
            _totalRequests++;

            var readTask = ReadAsyncTwo(socket);
        }

        private void SetupTcpClient()
        {
            _tcpClient = new TcpClient
            {
                NoDelay = true,
                SendTimeout = 10000,
                ReceiveTimeout = 1000
            };

            //var optValue = BitConverter.GetBytes(1);

            //_tcpClient.Client.IOControl(IO_CONTROL_CODE, optValue, null);
        }

        private void Write(ReadOnlySpan<byte> buffer)
        {
            _stream.Write(buffer);
        }

        private ValueTask<int> SendAsnc(Socket socket, ReadOnlyMemory<byte> buffer)
        {
            return socket.SendAsync(buffer, SocketFlags.None);
        }

        private async ValueTask<int> ReadAsync(Socket socket)
        {
            var writer = _pipe.Writer;
            int totalBytesRead = 0;

            while (socket.Available > 0)
            {
                Memory<byte> memory = writer.GetMemory(BUFFER_SIZE);

                try
                {
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    totalBytesRead += bytesRead;

                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    break;
                }

                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
            
            //writer.Complete();

            return totalBytesRead;
        }

        private async Task ReadAsyncTwo(Socket socket)
        {
            _totalReads++;

            try
            {
                var writer = _pipe.Writer;

                int totalBytesRead = 0;

                var buffer = writer.GetMemory(BUFFER_SIZE);

                int readBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);

                if (readBytes == 0)
                {
                    _lock.Release();
                    throw new SocketException();
                }

                writer.Advance(readBytes);

                totalBytesRead += readBytes;

                var responseType = BulletHttpHeader.GetResponseType(buffer.Span);
                var statusCode = BulletHttpHeader.GetStatusCode(buffer.Span);

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(buffer.Span.Slice(0, readBytes)))
                    {
                        buffer = writer.GetMemory(BUFFER_SIZE);

                        readBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);

                        totalBytesRead += readBytes;

                        writer.Advance(readBytes);
                    }
                }

                FlushResult result = await writer.FlushAsync();
            }
            finally
            {

            }
        }

        public async Task ParseRequest()
        {
            var reader = _pipe.Reader;

            var readResult = await reader.ReadAsync();

            reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);


        }
        private void Connect()
        {
            if (_tcpClient.Connected)
                return;

            _tcpClient.Connect(_uri.Host, _uri.Port);

            _stream = _tcpClient.GetStream();
        }

        private async Task WriteToPipe(PipeWriter writer)
        {
            var socket = _tcpClient.Client;

            var buffer = writer.GetMemory(BUFFER_SIZE);

            //var readBytes = await _stream.ReadAsync(buffer);
            var readBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);

            if (readBytes == 0)
            {
                _isReadingFromStream = false;

                return;
            }

            writer.Advance(readBytes);

            await writer.FlushAsync();

            writer.Complete();

            _totalBytesRead += readBytes;
        }

        private async Task WriteToPipeFast()
        {
            var socket = _tcpClient.Client;

            var writer = _pipe.Writer;

            var buffer = writer.GetMemory(BUFFER_SIZE);

            //var readBytes = await _stream.ReadAsync(buffer);
            var readBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);

            if (readBytes == 0)
            {
                _isReadingFromStream = false;

                return;
            }

            writer.Advance(readBytes);


            _totalBytesRead += readBytes;
        }
        private async Task ReadPipe(PipeReader reader)
        {
            var readerResult = await reader.ReadAsync();

            var buffer = readerResult.Buffer;

            var ee = readerResult.Buffer.ToArray();

            reader.AdvanceTo(buffer.Start, buffer.End);

            await reader.CompleteAsync();
        }

        async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {

            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                Memory<byte> memory = writer.GetMemory(BUFFER_SIZE);
                try
                {
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        break;
                    }
                    // Tell the PipeWriter how much was read from the Socket
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    break;
                }

                // Make the data available to the PipeReader
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Tell the PipeReader that there's no more data coming
            writer.Complete();
        }
        async Task ReadPipeAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // Look for a EOL in the buffer
                    position = buffer.PositionOf((byte)'\n');

                    if (position != null)
                    {
                        // Process the line
                        //ProcessLine(buffer.Slice(0, position.Value));

                        // Skip the line + the \n character (basically position)
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);

                // Tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete
            reader.Complete();
        }
        private void Reset()
        {
            _tcpClient?.Close();
        }

    }
}
