using Bullet.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bullet.Core.Http
{
    public class BulletHttpClient
    {
        private TcpClient _tcpClient;
        private Uri _uri;
        private Memory<byte> _buffer;
        private const int BUFFER_SIZE = 8192;
        private Memory<byte> _request;
        private CancellationToken _ctk;
        private Stopwatch _localWatch;
        private Stream _stram;

        public BulletHttpClient(string url)
        {
            _uri = new Uri(url);

            var header = Encoding.UTF8.GetBytes($"GET {_uri.PathAndQuery} HTTP/1.1\r\nAccept-Encoding: gzip, deflate, sdch\r\nHost: {_uri.Host}\r\nContent-Length: 0\r\n\r\n");
            _localWatch = new Stopwatch();
            _request = new Memory<byte>(header);
            _buffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            SetupTcpClient();
        }

        public async Task<BulletHttpResponse> GetAsync(CancellationToken ctk = default)
        {
            _ctk = ctk;

            _localWatch.Restart();

            var socket = _tcpClient.Client;
            var header = _request;

            await ConnectAsync()
                .ConfigureAwait(false);

            await SendAsnc(socket, header, _ctk);

            var result = await ReadAsync(socket, _ctk);

            return result;
        }

        public BulletHttpResponse Get()
        {

            _localWatch.Restart();

            Connect();

            var socket = _tcpClient.Client;

            var header = _request;

            SendStream(header);

            return Read(socket);
        }

        private ValueTask<int> SendAsnc(Socket socket, ReadOnlyMemory<byte> buffer, CancellationToken ctk = default)
        {
            return socket.SendAsync(buffer, SocketFlags.None, ctk);
        }

        private int Send(Socket socket, ReadOnlyMemory<byte> buffer)
        {
            return socket.Send(buffer.Span, SocketFlags.None);
        }

        private void SendStream(ReadOnlyMemory<byte> buffer)
        {
            _stram.Write(buffer.Span);
        }

        private async ValueTask<BulletHttpResponse> ReadAsync(Socket socket, CancellationToken ctk = default)
        {
            BulletHttpResponse result = null;

            try
            {
                var sw = Stopwatch.StartNew();

                int length = 0;

                int readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None, ctk)
                    .ConfigureAwait(false);

                if (readBytes == 0)
                    return result;

                sw.Stop();

                length += readBytes;
                var responseSpan = _buffer.Slice(0, readBytes);
                var statusCode = HttpStreamHelper.GetStatusCode(responseSpan.Span);
                var responseType = HttpStreamHelper.GetResponseType(responseSpan.Span);

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(_buffer.Span.Slice(0, readBytes)))
                    {
                        readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None, ctk).ConfigureAwait(false);
                        length += readBytes;
                    }
                }

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(_buffer.Span.Slice(0, readBytes)))
                    {
                        readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None, ctk);
                        length += readBytes;
                    }
                }

                //result = new BulletHttpResponse(statusCode, length, sw.Elapsed.TotalMilliseconds, _localWatch.Elapsed.TotalMilliseconds);
            }
            catch
            {

            }
            finally
            {

            }

            return result;
        }

        private BulletHttpResponse Read(Socket socket)
        {
            BulletHttpResponse result = null;

            try
            {
                var sw = Stopwatch.StartNew();

                int length = 0;

                //int readBytes = socket.Receive(_buffer.Span, SocketFlags.None);
                int readBytes = _stram.Read(_buffer.Span);

                if (readBytes == 0)
                    return result;

                sw.Stop();

                length += readBytes;
                var responseSpan = _buffer.Slice(0, readBytes);
                var statusCode = HttpStreamHelper.GetStatusCode(responseSpan.Span);
                var responseType = HttpStreamHelper.GetResponseType(responseSpan.Span);

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(_buffer.Span.Slice(0, readBytes)))
                    {
                        //readBytes = socket.Receive(_buffer.Span, SocketFlags.None);
                        readBytes = _stram.Read(_buffer.Span);
                        length += readBytes;
                    }
                }

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(_buffer.Span.Slice(0, readBytes)))
                    {
                        //readBytes = socket.Receive(_buffer.Span, SocketFlags.None);
                        readBytes = _stram.Read(_buffer.Span);

                        length += readBytes;
                    }
                }

                result = new BulletHttpResponse(statusCode, length, sw.Elapsed.TotalMilliseconds, _localWatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex}");
            }
            finally
            {

            }

            return result;
        }

        private void SetupTcpClient()
        {
            _tcpClient = new TcpClient
            {
                NoDelay = true,
                SendTimeout = 10000,
                ReceiveTimeout = 1000,
                ReceiveBufferSize = Int32.MaxValue
            };

            //_stream = _tcpClient.GetStream();
        }

        private Task ConnectAsync()
        {
            if (_tcpClient.Connected)
                return Task.CompletedTask;

            return _tcpClient.ConnectAsync(_uri.Host, _uri.Port);
        }

        private void Connect()
        {
            if (_tcpClient.Connected)
                return;

            _tcpClient.Connect(_uri.Host, _uri.Port);
            _stram = GetStream();
        }

        private Stream GetStream()
        {
            if (_uri.Scheme == Uri.UriSchemeHttp)
                return _tcpClient.GetStream();

            var stream = new SslStream(_tcpClient.GetStream());
            var xc = new X509Certificate2Collection();
            stream.AuthenticateAsClient(_uri.Host, xc, SslProtocols.Tls12, false);

            return stream;
        }

        private void Reset()
        {
            _tcpClient?.Close();

            SetupTcpClient();
        }

    }
}
