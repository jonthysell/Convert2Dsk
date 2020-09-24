// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Convert2Dsk
{
    public static class Logger
    {
        public static bool VerboseEnabled { get; set; } = false;

        public static event Action<string> LogWrite;

        public static void Write(string msg)
        {
            LogWrite?.Invoke(msg);
        }

        public static void WriteLine(string msg = "")
        {
            Write(msg + Environment.NewLine);
        }

        public static void VerboseWrite(string msg)
        {
            if (VerboseEnabled)
            {
                Write(msg);
            }
        }

        public static void VerboseWriteLine(string msg)
        {
            if (VerboseEnabled)
            {
                WriteLine(msg);
            }
        }
    }
}
