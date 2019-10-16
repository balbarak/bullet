using Bullet.Client.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
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
        private byte[] _requestHeader;
        private Memory<byte> _requestMemory;
        private bool _isReadingFromStream = false;
        private int _totalBytesRead = 0;
        private int _totalRequests = 0;
        private int _totalReads = 0;
        private Memory<byte> _buffer;
        public List<BulletHttpHeader> ResponseHeaders { get; private set; } = new List<BulletHttpHeader>();

        public BulletHttpClient(string url)
        {
            _uri = new Uri(url);
            _requestHeader = Encoding.UTF8.GetBytes($"GET {_uri.PathAndQuery} HTTP/1.1\r\nAccept-Encoding: gzip, deflate, sdch\r\nHost: {_uri.Host}\r\nContent-Length: 0\r\n\r\n");
            _buffer = new Memory<byte>(new byte[BUFFER_SIZE]);
            _requestMemory = new Memory<byte>(_requestHeader);

            SetupTcpClient();
        }

        public async ValueTask GetAsync()
        {
            Connect();

            var socket = _tcpClient.Client;

            var header = _requestMemory;

            await SendAsnc(socket, header)
                .ConfigureAwait(false);

            await ReadAsync(socket)
                .ConfigureAwait(false);

            _totalRequests++;
        }

        private void SetupTcpClient()
        {
            _tcpClient = new TcpClient
            {
                NoDelay = true,
                SendTimeout = 10000,
                ReceiveTimeout = 1000,
                ReceiveBufferSize = Int32.MaxValue,
            };

            //var optValue = BitConverter.GetBytes(1);

            //_tcpClient.Client.IOControl(IO_CONTROL_CODE, optValue, null);
        }

        private ValueTask<int> SendAsnc(Socket socket, ReadOnlyMemory<byte> buffer)
        {
            return socket.SendAsync(buffer, SocketFlags.None);
        }

        private async ValueTask ReadAsync(Socket socket)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                int lenght = 0;

                int readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None);

                if (readBytes == 0)
                    return;

                lenght += readBytes;
                var responseSpan = _buffer.Slice(0, readBytes);
                var statusCode = BulletHttpHeader.GetStatusCode(responseSpan.Span);
                var responseType = BulletHttpHeader.GetResponseType(responseSpan.Span);

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(_buffer.Span.Slice(0, readBytes)))
                    {
                        readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None);
                        lenght += readBytes;
                    }
                }

                sw.Stop();

                _totalReads++;
                
                ResponseHeaders.Add(new BulletHttpHeader(statusCode,lenght,sw.Elapsed.TotalMilliseconds));
            }
            catch
            {

            }
            finally
            {

            }
        }

        private void Connect()
        {
            if (_tcpClient.Connected)
                return;

            _tcpClient.Connect(_uri.Host, _uri.Port);
        }

        private void Reset()
        {
            _tcpClient?.Close();
        }

    }
}
