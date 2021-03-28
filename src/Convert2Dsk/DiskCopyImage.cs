// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;

namespace Convert2Dsk
{
    public class DiskCopyImage
    {
        public byte[] Header { get; protected set; }

        public byte[] Data { get; protected set; }

        protected DiskCopyImage() { }

        public static DiskCopyImage ReadFrom(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            using BinaryReader binaryReader = new BinaryReader(inputStream);

            byte[] rawData = binaryReader.ReadBytes((int)inputStream.Length);

            return ReadFrom(rawData);
        }

        public static DiskCopyImage ReadFrom(byte[] dataFork)
        {
            if (null == dataFork)
            {
                throw new ArgumentNullException(nameof(dataFork));
            }

            // Based on: https://www.bigmessowires.com/2013/12/16/macintosh-diskcopy-4-2-floppy-image-converter/

            // Process header

            byte[] header = new byte[HeaderLength];
            Array.Copy(dataFork, header, HeaderLength);

            if (header[0x52] != 0x01)
            {
                throw new Exception("The input does not appear to have a DiskCopy 4.2 header. Missing one byte at offset 0x52.");
            }

            if (header[0x53] != 0x00)
            {
                throw new Exception("The input does not appear to have a DiskCopy 4.2 header. Missing zero byte at offset 0x53.");
            }

            int dataLength = ByteListExtensions.ReadInt32(header, DataLengthOffset);

            // Extract embedded data

            byte[] data = new byte[dataLength];
            Array.Copy(dataFork, HeaderLength, data, 0, dataLength);

            return new DiskCopyImage()
            {
                Header = header,
                Data = data,
            };
        }

        public const int HeaderLength = 0x54;

        public const int DataLengthOffset = 0x40;
    }
}
