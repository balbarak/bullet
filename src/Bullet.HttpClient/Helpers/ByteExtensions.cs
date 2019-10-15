using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Client.Helpers
{
    public class ByteExtensions
    {
        public static int ConvertToInt(ReadOnlySpan<byte> buffer)
        {
            var result = 0;

            for (var i = 0; i < buffer.Length; i++)
            {
                result = result * 10 + (buffer[i] - '0');
            }

            return result;
        }
    }
}
