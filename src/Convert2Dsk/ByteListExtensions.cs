// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Convert2Dsk
{
    public static class ByteListExtensions
    {
        public static string ReadString(this IReadOnlyList<byte> data, int offset, int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += (char)data[offset + i];
            }
            return result;
        }

        public static int ReadInt32(this IReadOnlyList<byte> data, int offset)
        {
            int result = 0;
            result += data[offset + 0] << 24;
            result += data[offset + 1] << 16;
            result += data[offset + 2] << 8;
            result += data[offset + 3] << 0;
            return result;
        }

        public static ushort ReadUInt16(this IReadOnlyList<byte> data, int offset)
        {
            ushort result = 0;
            result += (ushort)(data[offset + 0] << 8);
            result += (ushort)(data[offset + 1] << 0);
            return result;
        }
    }
}
