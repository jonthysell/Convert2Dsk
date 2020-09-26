// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
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

        public static DiskCopyImage ReadFrom(byte[] rawData)
        {
            if (null == rawData)
            {
                throw new ArgumentNullException(nameof(rawData));
            }

            // Adapted from https://www.bigmessowires.com/2013/12/16/macintosh-diskcopy-4-2-floppy-image-converter/

            // Ensure the file is at least 400K, and has the DiskCopy 4.2 magic bytes
            if (rawData.Length < 400 * 1024)
            {
                throw new Exception("The input is too small to be a DiskCopy 4.2 image.");
            }

            // Process header

            byte[] header = new byte[HeaderSizeInBytes];
            Array.Copy(rawData, header, HeaderSizeInBytes);

            if (header[0x52] != 0x01 || header[0x53] != 0x00)
            {
                throw new Exception("The input does not appear to have a DiskCopy 4.2 header.");
            }

            // Ensure the embedded data size is 400, 800, or 1440 K
            uint dataSize = (uint)(header[0x40] * 16777216 + header[0x41] * 65536 + header[0x42] * 256 + header[0x43]);
            if (dataSize != 400 * 1024 && dataSize != 800 * 1024 && dataSize != 1440 * 1024)
            {
                throw new Exception($"The input has an unsupported data size.");
            }

            // Extract embedded data
            byte[] data = new byte[dataSize];
            Array.Copy(rawData, HeaderSizeInBytes, data, 0, dataSize);

            return new DiskCopyImage()
            {
                Header = header,
                Data = data,
            };
        }

        public const int HeaderSizeInBytes = 0x54;
    }
}
