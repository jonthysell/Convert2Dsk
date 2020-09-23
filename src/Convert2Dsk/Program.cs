// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Convert2Dsk
{
    public class Program
    {
        #region Main Statics

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var p = new Program(args);
            p.Run();
        }

        private static void PrintException(Exception ex)
        {
            var oldColor = StartConsoleError();

            if (!(ex is null))
            {
                Console.Error.WriteLine($"Error: { ex.Message }");

#if DEBUG
                Console.Error.WriteLine(ex.StackTrace);
#endif

                EndConsoleError(oldColor);

                if (null != ex.InnerException)
                {
                    PrintException(ex.InnerException);
                }
            }
        }

        private static ConsoleColor StartConsoleError()
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            return oldColor;
        }

        private static void EndConsoleError(ConsoleColor oldColor)
        {
            Console.ForegroundColor = oldColor;
        }

        #endregion

        #region Properties

        public readonly string[] Arguments;

        public ProgramArgs ProgramArgs { get; private set; }

        #endregion

        public Program(string[] args)
        {
            Arguments = args;
        }

        public void Run()
        {
            if (Arguments is null || Arguments.Length == 0)
            {
                ShowHelp();
                return;
            }

            try
            {
                ProgramArgs = ProgramArgs.ParseArgs(Arguments);

                if (ProgramArgs.ShowVersion)
                {
                    ShowVersion();
                    return;
                }
                else if (ProgramArgs.ShowHelp || ProgramArgs.Paths.Count == 0)
                {
                    ShowHelp();
                    return;
                }

                VerboseWriteLine($"Searching for potential files...");

                var filePaths = FindAllUniqueFilePaths(ProgramArgs.Paths);

                VerboseWriteLine($"Found {filePaths.Count} potential file(s).");

                if (filePaths.Count > 0)
                {
                    VerboseWriteLine($"Converting file(s)...");
                    foreach (var filePath in filePaths)
                    {
                        ConvertFile(filePath);
                    }
                    VerboseWriteLine($"Conversion(s) complete.");
                }
            }
            catch (ParseArgumentsException ex)
            {
                PrintException(ex);
                Console.WriteLine();
                ShowHelp();
            }
            catch (Exception ex)
            {
                PrintException(ex);
                Console.WriteLine();
            }
        }

        private static HashSet<string> FindAllUniqueFilePaths(IEnumerable<string> originalPaths)
        {
            var resultPaths = new HashSet<string>();

            foreach (var originalPath in originalPaths)
            {
                if (File.Exists(originalPath))
                {
                    resultPaths.Add(Path.GetFullPath(originalPath));
                }
                else if (Directory.Exists(originalPath))
                {
                    foreach (var filePath in Directory.EnumerateFiles(originalPath, "*", SearchOption.AllDirectories))
                    {
                        resultPaths.Add(filePath);
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Unable to find \"{Path.GetFullPath(originalPath)}\".");
                }
            }

            return resultPaths;
        }

        private void ConvertFile(string filePath)
        {
            try
            {
                // Adapted from https://www.bigmessowires.com/2013/12/16/macintosh-diskcopy-4-2-floppy-image-converter/

                Console.Write($"Converting \"{filePath}\"...");

                using FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using BinaryReader binaryReader = new BinaryReader(inputFileStream);

                // ensure the file is at least 400K, and has the DiskCopy 4.2 magic bytes
                if (inputFileStream.Length < 400 * 1024)
                {
                    throw new Exception($"The file is too small to be a DiskCopy 4.2 image.");
                }

                byte[] bytes = binaryReader.ReadBytes(0x54);
                if (bytes[0x52] != 0x01 || bytes[0x53] != 0x00)
                {
                    throw new Exception($"The file does not appear to be a DiskCopy 4.2 image.");
                }

                // ensure the embedded data size is 400, 800, or 1440 K
                uint dataSize = (uint)(bytes[0x40] * 16777216 + bytes[0x41] * 65536 + bytes[0x42] * 256 + bytes[0x43]);
                if (dataSize != 400 * 1024 && dataSize != 800 * 1024 && dataSize != 1440 * 1024)
                {
                    throw new Exception($"The file is an unsupported image size.");
                }

                // get the embedded image data, and write it out as a new file with a .dsk extension
                byte[] imageData = binaryReader.ReadBytes((int)dataSize);
                string outputFilePath = $"{Path.GetFullPath(filePath)}.dsk";

                if (File.Exists(outputFilePath))
                {
                    throw new Exception($"The output file \"{outputFilePath}\" already exists and would be overwritten.");
                }

                using FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using BinaryWriter binaryWriter = new BinaryWriter(outputFileStream);

                binaryWriter.Write(imageData);
                binaryWriter.Flush();

                Console.WriteLine($" success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" failed.");
                PrintException(ex);
            }
        }

        private void VerboseWriteLine(string message)
        {
            if (ProgramArgs.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        #region Version

        private void ShowVersion()
        {
            Console.WriteLine($"{ AppInfo.Name } v{ AppInfo.Version }");
            Console.WriteLine();
        }

        #endregion

        #region Help

        private void ShowHelp()
        {
            Console.WriteLine("Usage: convert2dsk [--version] [--help]");
            Console.WriteLine("                   [options...] <paths...>");
            Console.WriteLine();

            Console.WriteLine("Paths can be files or directories of files.");
            Console.WriteLine();

            Console.WriteLine("Options:");
            Console.WriteLine("-v, --verbose  Show verbose output");
            Console.WriteLine();
        }

        #endregion
    }
}
