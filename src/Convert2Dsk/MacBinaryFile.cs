// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
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

            return new MacBinaryFile()
            {
            };
        }
    }
}
