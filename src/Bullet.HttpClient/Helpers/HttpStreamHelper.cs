using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Client.Helpers
{
    public class HttpStreamHelper
    {
        public static bool IsEndOfChunkedStream(ReadOnlySpan<byte> buffer)
        {
            return buffer.Length >= 5 && buffer.Slice(buffer.Length - 5, 5).SequenceEqual(BulletHttpHeader.EndOfChunkedResponse.Span);
        }
    }
}
