// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

namespace Convert2Dsk
{
    public abstract class MacFile
    {
        public string FileName { get; protected set; }

        public string FileType { get; protected set; }

        public string Creator { get; protected set; }

        public ushort Flag { get; protected set; }

        public byte[] DataFork { get; protected set; }

        public byte[] ResourceFork { get; protected set; }

        protected MacFile() { }
    }
}
