// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;

namespace Convert2Dsk
{
    public class MacBinaryFile : MacFile
    {
        protected MacBinaryFile() : base() { }

        public static MacBinaryFile ReadFrom(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            // Based on spec: https://github.com/mietek/theunarchiver/wiki/MacBinarySpecs

            using BinaryReader binaryReader = new BinaryReader(inputStream);

            byte[] header = binaryReader.ReadBytes(HeaderLength);

            if (header[0x00] != 0)
            {
                throw new Exception("The input does not appear to have a MacBinary header. Missing zero byte at offset 0x00.");
            }

            int filenameLength = header[FileNameLengthOffset];

            if (filenameLength < MinFileNameLength || filenameLength > MaxFileNameLength)
            {
                throw new Exception("The input does not appear to have a MacBinary header. Filename length is out of range.");
            }

            string fileName = ByteListExtensions.ReadString(header, FileNameOffset, filenameLength);

            string fileType = ByteListExtensions.ReadString(header, FileTypeOffest, FileTypeLength);
            string fileCreator = ByteListExtensions.ReadString(header, FileCreatorOffest, FileCreatorLength);

            ushort finderFlags = (ushort)((header[FinderFlagsHighOffset] << 8) + header[FinderFlagsLowOffset]);

            if (header[0x4A] != 0)
            {
                throw new Exception("The input does not appear to have a MacBinary header. Missing zero byte at offset 0x4A.");
            }

            if (header[0x52] != 0)
            {
                throw new Exception("The input does not appear to have a MacBinary header. Missing zero byte at offset 0x52.");
            }

            int dataForkLength = ByteListExtensions.ReadInt32(header, DataForkLengthOffset);

            if (dataForkLength < MinForkLength || dataForkLength > MaxForkLength)
            {
                throw new Exception("The input does not appear to have a MacBinary header. Data fork length is out of range.");
            }

            int resourceForkLength = ByteListExtensions.ReadInt32(header, ResourceForkLengthOffset);

            if (resourceForkLength < MinForkLength || resourceForkLength > MaxForkLength)
            {
                throw new Exception("The input does not appear to have a MacBinary header. Resource fork length is out of range.");
            }

            byte[] dataFork = dataForkLength > 0 ? binaryReader.ReadBytes(dataForkLength) : new byte[0];

            binaryReader.ReadBytes(dataForkLength % 128);

            byte[] resourceFork = resourceForkLength > 0 ? binaryReader.ReadBytes(resourceForkLength) : new byte[0];

            return new MacBinaryFile()
            {
                FileName = fileName,
                FileType = fileType,
                FileCreator = fileCreator,
                FinderFlags = finderFlags,
                DataFork = dataFork,
                ResourceFork = resourceFork,
            };
        }

        public const int HeaderLength = 128;

        public const int FileNameLengthOffset = 1;
        public const int FileNameOffset = 2;

        public const int MinFileNameLength = 1;
        public const int MaxFileNameLength = 63;

        public const int FileTypeOffest = 65;

        public const int FileCreatorOffest = 69;

        public const int FinderFlagsHighOffset = 74;
        public const int FinderFlagsLowOffset = 101;

        public const int DataForkLengthOffset = 83;
        public const int ResourceForkLengthOffset = 87;

        public const int MinForkLength = 0;
        public const int MaxForkLength = 0x7FFFFF;
    }
}
