// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Convert2Dsk
{
    public class ProgramArgs
    {
        public bool Verbose { get; private set; } = false;

        public bool ShowVersion { get; private set; } = false;

        public bool ShowHelp { get; private set; } = false;

        public List<string> Paths { get; private set; } = new List<string>();

        private ProgramArgs() { }

        public static ProgramArgs ParseArgs(string[] args)
        {
            var result = new ProgramArgs();

            if (null != args && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "-v":
                        case "/v":
                        case "--verbose":
                        case "/verbose":
                            result.Verbose = true;
                            break;
                        case "--version":
                        case "/version":
                            result.ShowVersion = true;
                            break;
                        case "-?":
                        case "/?":
                        case "-h":
                        case "/h":
                        case "--help":
                        case "/help":
                            result.ShowHelp = true;
                            break;
                        default:
                            result.Paths.Add(args[i]);
                            break;
                    }
                }
            }

            return result;
        }
    }

    #region Exceptions

    public class ParseArgumentsException : Exception
    {
        public ParseArgumentsException(string message) : base(message) { }

        public ParseArgumentsException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion
}
