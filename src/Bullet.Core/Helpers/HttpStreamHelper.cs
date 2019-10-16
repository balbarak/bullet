using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Core.Helpers
{
    internal class HttpStreamHelper
    {
        public static bool IsEndOfChunkedStream(ReadOnlySpan<byte> buffer)
        {
            return buffer.Length >= 5 && buffer.Slice(buffer.Length - 5, 5).SequenceEqual(HttpStrings.EndOfChunkedResponse.Span);
        }

        public static bool IsEndOfHeader(ReadOnlySpan<byte> buffer)
        {

            return buffer.SequenceEqual(HttpStrings.HeaderEnd.Span);
        }

        public static int GetHeaderContentLength(ReadOnlySpan<byte> buffer)
        {
            SeekHeader(buffer, HttpStrings.HeaderContentLength.Span, out var index, out var length);

            return ConvertToInt(buffer.Slice(index, length));
        }

        public static int GetResponseLength(ReadOnlySpan<byte> buffer)
        {
            if (!SeekHeader(buffer, HttpStrings.HeaderContentLength.Span, out var index, out var length))
            {
                return -1;
            }

            var headerEndIndex = buffer.Slice(index + length).IndexOf(HttpStrings.HeaderEnd.Span);

            if (headerEndIndex < 0)
            {
                return -1;
            }

            var contentLength = ConvertToInt(buffer.Slice(index, length));
            return index + length + headerEndIndex + 4 + contentLength;
        }

        public static bool SeekHeader(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> header, out int index, out int length)
        {
            index = buffer.IndexOf(header);

            if (index < 0)
            {
                length = 0;
                return false;
            }

            index += header.Length;
            length = buffer.Slice(index).IndexOf(HttpStrings.HeaderReturn.Span);
            return true;
        }

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
