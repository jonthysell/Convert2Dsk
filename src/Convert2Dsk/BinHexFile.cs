// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;

namespace Convert2Dsk
{
    public class BinHexFile
    {
        public byte[] Data { get; protected set; }

        protected BinHexFile() { }

        public static BinHexFile ReadFrom(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            return new BinHexFile();
        }
    }
}
