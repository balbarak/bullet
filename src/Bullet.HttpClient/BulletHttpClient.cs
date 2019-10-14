using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        private int _numberOfRequests = 0;
        private Memory<byte> _buffer = new Memory<byte>();

        public BulletHttpClient(string url)
        {
            _uri = new Uri(url);
            _requestHeader = Encoding.UTF8.GetBytes($"GET {_uri.PathAndQuery} HTTP/1.1\r\nAccept-Encoding: gzip, deflate, sdch\r\nHost: {_uri.Host}\r\nContent-Length: 0\r\n\r\n");

            SetupTcpClient();
        }

        public async Task GetAsync(string url)
        {
            Connect();

            var buffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            var header = _requestHeader.AsMemory();

            Write(header.Span);


            var readBytes = Read(buffer);

            //if (readBytes == 0)
            //    Reset();

            //var text = Encoding.UTF8.GetString(buffer.Span);
        }

        private void SetupTcpClient()
        {
            _tcpClient = new TcpClient
            {
                NoDelay = true,
                SendTimeout = 10000,
                ReceiveTimeout = 10000
            };

            //var optValue = BitConverter.GetBytes(1);

            //_tcpClient.Client.IOControl(IO_CONTROL_CODE, optValue, null);
        }

        private void Write(ReadOnlySpan<byte> buffer)
        {
            _stream.Write(buffer);
        }

        private void Connect()
        {
            if (_tcpClient.Connected)
                return;

            _tcpClient.Connect(_uri.Host, _uri.Port);

            _stream = _tcpClient.GetStream();
        }

        private int Read(Memory<byte> buffer)
        {
            return _stream.Read(buffer.Span);
        }

        private void Reset()
        {
            _tcpClient?.Close();
        }
    }
}
