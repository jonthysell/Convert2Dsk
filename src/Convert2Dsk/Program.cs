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

            Logger.LogWrite += Console.Write;

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

                Logger.VerboseEnabled = ProgramArgs.Verbose;

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

                Logger.VerboseWriteLine("Searching for image files...");

                var filePaths = FindAllUniqueFilePaths(ProgramArgs.Paths);

                Logger.VerboseWriteLine($"Found {filePaths.Count} image file(s).");

                if (filePaths.Count > 0)
                {
                    Logger.VerboseWriteLine("Converting file(s)...");
                    foreach (var filePath in filePaths)
                    {
                        ConvertFile(filePath);
                    }
                    Logger.VerboseWriteLine("Conversion(s) complete.");
                }
            }
            catch (ParseArgumentsException ex)
            {
                PrintException(ex);
                ShowHelp();
            }
            catch (Exception ex)
            {
                PrintException(ex);
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
                    foreach (string extension in SupportedExtensions)
                    {
                        foreach (var filePath in Directory.EnumerateFiles(originalPath, $"*.{extension}", SearchOption.AllDirectories))
                        {
                            resultPaths.Add(filePath);
                        }
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
                Logger.Write($"Converting \"{filePath}\"...");

                string ext = Path.GetExtension(filePath).ToLower();

                byte[] rawDskData = null;

                if (ext == ".hqx")
                {
                    // Decode BinHex
                    using FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    rawDskData = DiskCopyImage.ReadFrom(BinHexFile.ReadFrom(inputFileStream).DataFork).Data;
                }
                else if (ext == ".img" || ext == ".image")
                {
                    // DC42 image
                    using FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    rawDskData = DiskCopyImage.ReadFrom(inputFileStream).Data;
                }
                else
                {
                    throw new Exception($"Unable to determine file format from extension \"{ext}\".");
                }

                string outputFilePath = $"{Path.GetFullPath(filePath)}.dsk";

                if (File.Exists(outputFilePath))
                {
                    throw new Exception($"The output file \"{outputFilePath}\" already exists and would be overwritten.");
                }

                using FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using BinaryWriter binaryWriter = new BinaryWriter(outputFileStream);

                binaryWriter.Write(rawDskData);
                binaryWriter.Flush();

                Logger.WriteLine($" success!");
            }
            catch (Exception ex)
            {
                Logger.WriteLine($" failed.");
                PrintException(ex);
            }
        }

        private static readonly string[] SupportedExtensions = { "img", "image", "hqx" };

        #region Version

        private void ShowVersion()
        {
            Logger.WriteLine($"{ AppInfo.Name } v{ AppInfo.Version }");
        }

        #endregion

        #region Help

        private void ShowHelp()
        {
            Logger.WriteLine("Usage: convert2dsk [--version] [--help]");
            Logger.WriteLine("                   [options...] <paths...>");
            Logger.WriteLine();

            Logger.WriteLine("Paths can be files or directories of files.");
            Logger.WriteLine();

            Logger.WriteLine("Supports:");
            Logger.WriteLine("DiskCopy 4.2 images: .img, .image");
            Logger.WriteLine("DiskCopy 4.2 images (encoded with BinHex 4.0): .hqx");
            Logger.WriteLine();

            Logger.WriteLine("Options:");
            Logger.WriteLine("-v, --verbose  Show verbose output");
        }

        #endregion
    }
}
