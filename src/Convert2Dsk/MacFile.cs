﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

namespace Convert2Dsk
{
    public abstract class MacFile
    {
        public string FileName { get; protected set; }

        public string FileType { get; protected set; }

        public string FileCreator { get; protected set; }

        public ushort FinderFlags { get; protected set; }

        public byte[] DataFork { get; protected set; }

        public byte[] ResourceFork { get; protected set; }

        protected MacFile() { }

        public const int FileTypeLength = 4;
        public const int FileCreatorLength = 4;
    }
}
