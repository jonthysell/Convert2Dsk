﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
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
                throw new Exception("The input text does not have a BinHex 4.0 header.");
            }

            // Look for start of the encoeded character data

            int startColonIndex = inputText.IndexOf(':', headerIndex + BinHexHeader.Length);

            if (startColonIndex < 0)
            {
                throw new Exception("The input text does not appear to be BinHex 4.0. Missing the starting colon.");
            }

            // Look for end of the encoeded character data

            int endColonIndex = inputText.IndexOf(':', startColonIndex + 1);

            if (endColonIndex < 0)
            {
                throw new Exception("The input text does not appear to be BinHex 4.0. Missing the ending colon.");
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
                    throw new Exception($"The input text does not appear to be BinHex 4.0. Contains invalid character {encodedText[i]} at offset 0x{i:X}.");
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

            byte fileNameLength = uncompressedBytes[index];
            index++;

            string fileName = uncompressedBytes.ReadString(index, fileNameLength);
            index += fileNameLength;

            if (uncompressedBytes[index] != 0x00)
            {
                throw new Exception($"The uncompressed input data does not appear to have a BinHex 4.0 header. Missing zero byte at offset 0x{index:X}.");
            }
            index++;

            string fileType = uncompressedBytes.ReadString(index, FileTypeLength);
            index += FileTypeLength;

            string fileCreator = uncompressedBytes.ReadString(index, FileCreatorLength);
            index += FileCreatorLength;

            ushort finderFlags = uncompressedBytes.ReadUInt16(index);
            index += 2;

            int dataForkLength = uncompressedBytes.ReadInt32(index);
            index += 4;

            int resourceForkLength = uncompressedBytes.ReadInt32(index);
            index += 4;

            ushort headerCRC = uncompressedBytes.ReadUInt16(index);
            index += 2;

            byte[] dataFork = new byte[dataForkLength];
            Array.Copy(uncompressedBytes.ToArray(), index, dataFork, 0, dataForkLength);
            index += dataForkLength;

            ushort dataCRC = uncompressedBytes.ReadUInt16(index);
            index += 2;

            byte[] resourceFork = new byte[resourceForkLength];
            Array.Copy(uncompressedBytes.ToArray(), index, resourceFork, 0, resourceForkLength);
            index += resourceForkLength;

            ushort resourceCRC = uncompressedBytes.ReadUInt16(index);
            index += 2;

            return new BinHexFile()
            {
                FileName = fileName,
                FileType = fileType,
                FileCreator = fileCreator,
                FinderFlags = finderFlags,
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
