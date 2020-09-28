// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Convert2Dsk.Test
{
    [TestClass]
    [DeploymentItem("TestAssets")]
    public class MacBinaryFileTests
    {
        [TestMethod]
        public void MacBinaryFile_Blank1440KTest()
        {
            var testCase = new BinHexFileTestCase("Blank1440K.img.bin", "Blank1440K.dsk");
            testCase.ExecuteTest();
        }

        [TestMethod]
        public void MacBinaryFile_Blank800KTest()
        {
            var testCase = new BinHexFileTestCase("Blank800K.img.bin", "Blank800K.dsk");
            testCase.ExecuteTest();
        }

        [TestMethod]
        public void MacBinaryFile_Blank400KTest()
        {
            var testCase = new BinHexFileTestCase("Blank400K.img.bin", "Blank400K.dsk");
            testCase.ExecuteTest();
        }

        public class BinHexFileTestCase : FileTestCase
        {
            public BinHexFileTestCase(string inputFilePath, string expectedOutputFilePath) : base(inputFilePath, expectedOutputFilePath) { }

            public override void ExecuteTest()
            {
                using FileStream inputFileStream = new FileStream(InputFilePath, FileMode.Open, FileAccess.Read);

                byte[] binhexBytes = MacBinaryFile.ReadFrom(inputFileStream).DataFork;

                byte[] actualOutputBytes = DiskCopyImage.ReadFrom(binhexBytes).Data;

                byte[] expectedOutputBytes = File.ReadAllBytes(ExpectedOutputFilePath);

                CompareData(expectedOutputBytes, actualOutputBytes);
            }
        }
    }
}
