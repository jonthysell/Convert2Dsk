// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

namespace Convert2Dsk
{
    public class BinHexFile : MacFile
    {
        public ushort HeaderCRC { get; protected set; }

        public ushort DataCRC { get; protected set; }

        public ushort ResourceCRC { get; protected set; }

        protected BinHexFile() : base() { }

        public static BinHexFile ReadFrom(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            // Based on spec: http://files.stairways.com/other/binhex-40-specs-info.txt

            using StreamReader streamReader = new StreamReader(inputStream);

            string inputText = streamReader.ReadToEnd();

            // Look for the appropriate header

            int headerIndex = inputText.IndexOf(BinHexHeader);
            if (headerIndex < 0)
            {
                throw new Exception("The input does not have a BinHex 4.0 header.");
            }

            // Look for start of the encoeded character data

            int startColonIndex = inputText.IndexOf(':', headerIndex + BinHexHeader.Length);

            if (startColonIndex < 0)
            {
                throw new Exception("The input does not have a starting colon.");
            }

            // Look for end of the encoeded character data

            int endColonIndex = inputText.IndexOf(':', startColonIndex + 1);

            if (endColonIndex < 0)
            {
                throw new Exception("The input does not have an ending colon.");
            }

            // Extract the encoeded character data

            string encodedText = inputText.Substring(startColonIndex + 1, endColonIndex - startColonIndex - 1);

            // Convert the character data into RLE compressed binary data
            // In    In    In    In    In    In    In    In
            // 012345012345012345012345012345012345012345012345
            // Out     Out     Out     Out     Out     Out

            List<byte> compressedBytes = new List<byte>();

            int charCount = 0;
            ulong inputChunk = 0x0;

            for (int i = 0; i < encodedText.Length; i++)
            {
                if (char.IsWhiteSpace(encodedText[i]))
                {
                    continue;
                }

                int charValue = BinHexCharMap.IndexOf(encodedText[i]);
                if (charValue < 0)
                {
                    throw new Exception($"The input contains invalid character {encodedText[i]} at index {i}");
                }

                if (charCount == 0)
                {
                    // Reset input chunk
                    inputChunk = 0x0;
                }

                inputChunk |= ((ulong)charValue) << (42 - (6 * charCount));

                if (charCount == 7)
                {
                    // Process bytes
                    for (int j = 0; j < 6; j++)
                    {
                        compressedBytes.Add((byte)(0xFF & (inputChunk >> (40 - (8 * j)))));
                    }
                }

                charCount = (charCount + 1) % 8;
            }

            if (charCount > 0)
            {
                // Process remaining bytes
                for (int j = 0; j < 6; j++)
                {
                    compressedBytes.Add((byte)(0xFF & (inputChunk >> (40 - (8 * j)))));
                }
            }

            // Uncompress the RLE data

            List<byte> uncompressedBytes = new List<byte>();

            for (int i = 0; i < compressedBytes.Count; i++)
            {
                if (compressedBytes[i] == RLEFlag)
                {
                    byte repeatCount = compressedBytes[++i];
                    if (repeatCount == 0)
                    {
                        // Not a run, literal RLEFlag
                        uncompressedBytes.Add(RLEFlag);
                        continue;
                    }
                    else
                    {
                        // Repeat
                        byte lastByte = uncompressedBytes[^1];
                        for (int j = 0; j < repeatCount - 1; j++)
                        {
                            uncompressedBytes.Add(lastByte);
                        }
                    }
                }
                else
                {
                    // Regular character
                    uncompressedBytes.Add(compressedBytes[i]);
                }
            }

            // Process header

            int index = 0;

            byte filenameLength = uncompressedBytes[index];

            index++;

            string fileName = "";
            for (int i = 0; i < filenameLength; i++)
            {
                fileName += (char)uncompressedBytes[index + i];
            }
            index += filenameLength;

            if (uncompressedBytes[index] != 0x00)
            {
                throw new Exception("The input does not appear to have a BinHex 4.0 header.");
            }
            index++;

            string fileType = "";
            for (int i = 0; i < 4; i++)
            {
                fileType += (char)uncompressedBytes[index + i];
            }
            index += 4;

            string creator = "";
            for (int i = 0; i < 4; i++)
            {
                creator += (char)uncompressedBytes[index + i];
            }
            index += 4;

            ushort flag = (ushort)((uncompressedBytes[index] << 8) | uncompressedBytes[index + 1]);
            index += 2;

            uint dataForkLength = (uint)((uncompressedBytes[index] << 24) | (uncompressedBytes[index + 1] << 16) | (uncompressedBytes[index + 2] << 8) | uncompressedBytes[index+3]);
            index += 4;

            uint resourceForkLength = (uint)((uncompressedBytes[index] << 24) | (uncompressedBytes[index + 1] << 16) | (uncompressedBytes[index + 2] << 8) | uncompressedBytes[index + 3]);
            index += 4;

            ushort headerCRC = (ushort)((uncompressedBytes[index] << 8) | uncompressedBytes[index + 1]);
            index += 2;

            byte[] dataFork = new byte[dataForkLength];
            Array.Copy(uncompressedBytes.ToArray(), index, dataFork, 0, dataForkLength);
            index += (int)dataForkLength;

            ushort dataCRC = (ushort)((uncompressedBytes[index] << 8) | uncompressedBytes[index + 1]);
            index += 2;

            byte[] resourceFork = new byte[resourceForkLength];
            Array.Copy(uncompressedBytes.ToArray(), index, resourceFork, 0, resourceForkLength);
            index += (int)resourceForkLength;

            ushort resourceCRC = (ushort)((uncompressedBytes[index] << 8) | uncompressedBytes[index + 1]);
            index += 2;

            return new BinHexFile()
            {
                FileName = fileName,
                FileType = fileType,
                Creator = creator,
                Flag = flag,
                HeaderCRC = headerCRC,
                DataFork = dataFork,
                DataCRC = dataCRC,
                ResourceFork = resourceFork,
                ResourceCRC = resourceCRC,
            };
        }

        public static readonly string BinHexHeader = "(This file must be converted with BinHex 4.0)";
        public static readonly string BinHexCharMap = @"!""#$%&'()*+,-012345689@ABCDEFGHIJKLMNPQRSTUVXYZ[`abcdefhijklmpqr";
        public const byte RLEFlag = 0x90;
    }
}
