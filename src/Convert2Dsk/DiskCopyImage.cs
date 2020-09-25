// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;

namespace Convert2Dsk
{
    public class DiskCopyImage
    {
        public byte[] Header { get; protected set; }

        public byte[] ImageData { get; protected set; }

        protected DiskCopyImage() { }

        public static DiskCopyImage ReadFrom(Stream inputStream)
        {
            using BinaryReader binaryReader = new BinaryReader(inputStream);

            // ensure the file is at least 400K, and has the DiskCopy 4.2 magic bytes
            if (inputStream.Length < 400 * 1024)
            {
                throw new Exception("The file is too small to be a DiskCopy 4.2 image.");
            }

            byte[] header = binaryReader.ReadBytes(HeaderSizeInBytes);
            if (header[0x52] != 0x01 || header[0x53] != 0x00)
            {
                throw new Exception("The file does not appear to be a DiskCopy 4.2 image.");
            }

            // ensure the embedded data size is 400, 800, or 1440 K
            uint dataSize = (uint)(header[0x40] * 16777216 + header[0x41] * 65536 + header[0x42] * 256 + header[0x43]);
            if (dataSize != 400 * 1024 && dataSize != 800 * 1024 && dataSize != 1440 * 1024)
            {
                throw new Exception($"The file is an unsupported image size.");
            }

            byte[] imageData = binaryReader.ReadBytes((int)dataSize);

            return new DiskCopyImage()
            {
                Header = header,
                ImageData = imageData,
            };
        }

        public const int HeaderSizeInBytes = 0x54;
    }
}
