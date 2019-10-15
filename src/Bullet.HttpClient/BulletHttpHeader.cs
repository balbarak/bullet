using Bullet.Client.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Client
{
    public enum ResponseType
    {
        Unknown,
        Chunked,
        ContentLength
    }

    public class BulletHttpHeader
    {
        public static readonly ReadOnlyMemory<byte> HeaderEnd = Encoding.ASCII.GetBytes("\r\n\r\n");
        public static readonly ReadOnlyMemory<byte> HeaderReturn = Encoding.ASCII.GetBytes("\r\n");
        public static readonly ReadOnlyMemory<byte> HeaderContentLength = Encoding.ASCII.GetBytes("\r\nContent-Length: ");
        public static readonly ReadOnlyMemory<byte> HeaderTransferEncoding = Encoding.ASCII.GetBytes("\r\nTransfer-Encoding: ");
        public static readonly ReadOnlyMemory<byte> EndOfChunkedResponse = Encoding.ASCII.GetBytes("0\r\n\r\n");

        public int StatusCode { get; private set; }

        public BulletHttpHeader()
        {

        }

        public BulletHttpHeader(int statusCode) 
        {
            StatusCode = statusCode;
        }

        public static int GetStatusCode(ReadOnlySpan<byte> buffer)
        {
            return ByteExtensions.ConvertToInt(buffer.Slice(9, 3));
        }

        public static ResponseType GetResponseType(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IndexOf(HeaderContentLength.Span) != -1)
            {
                return ResponseType.ContentLength;
            }

            if (buffer.IndexOf(HeaderTransferEncoding.Span) != -1)
            {
                return ResponseType.Chunked;
            }

            return ResponseType.Unknown;
        }
    }
}
