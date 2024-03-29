﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Core.Helpers
{
    internal class HttpStrings
    {
        public static readonly ReadOnlyMemory<byte> HeaderEnd = Encoding.ASCII.GetBytes("\r\n\r\n");
        public static readonly ReadOnlyMemory<byte> HeaderReturn = Encoding.ASCII.GetBytes("\r\n");
        public static readonly ReadOnlyMemory<byte> HeaderContentLength = Encoding.ASCII.GetBytes("\r\nContent-Length: ");
        public static readonly ReadOnlyMemory<byte> HeaderTransferEncoding = Encoding.ASCII.GetBytes("\r\nTransfer-Encoding: ");
        public static readonly ReadOnlyMemory<byte> EndOfChunkedResponse = Encoding.ASCII.GetBytes("0\r\n\r\n");
    }
}
