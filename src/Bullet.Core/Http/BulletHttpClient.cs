using Bullet.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
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
        
        public BulletHttpClient(string url)
        {
            _uri = new Uri(url);

            var header = Encoding.UTF8.GetBytes($"GET {_uri.PathAndQuery} HTTP/1.1\r\nAccept-Encoding: gzip, deflate, sdch\r\nHost: {_uri.Host}\r\nContent-Length: 0\r\n\r\n");

            _request = new Memory<byte>(header);
            _buffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            SetupTcpClient();
        }

        public async ValueTask<BulletHttpResponse> GetAsync()
        {
            Connect();

            var socket = _tcpClient.Client;

            var header = _request;

            await SendAsnc(socket, header)
                .ConfigureAwait(false);

            var result = await ReadAsync(socket)
                .ConfigureAwait(false);

            return result;
        }

        private ValueTask<int> SendAsnc(Socket socket, ReadOnlyMemory<byte> buffer)
        {
            return socket.SendAsync(buffer, SocketFlags.None);
        }

        private async ValueTask<BulletHttpResponse> ReadAsync(Socket socket)
        {
            BulletHttpResponse result = null;

            try
            {
                var sw = Stopwatch.StartNew();

                int length = 0;

                int readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None);

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
                        readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None);
                        length += readBytes;
                    }
                }

                if (responseType == ResponseType.Chunked)
                {
                    while (!HttpStreamHelper.IsEndOfChunkedStream(_buffer.Span.Slice(0, readBytes)))
                    {
                        readBytes = await socket.ReceiveAsync(_buffer, SocketFlags.None);
                        length += readBytes;
                    }
                }

                result = new BulletHttpResponse(statusCode, length, sw.Elapsed.TotalMilliseconds);
            }
            catch
            {

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
            };
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
            
            SetupTcpClient();
        }

    }
}
