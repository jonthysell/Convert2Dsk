// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Convert2Dsk.Test
{
    [TestClass]
    [DeploymentItem("TestAssets")]
    public class DiskCopyImageTests
    {
        [TestMethod]
        public void DiskCopyImage_Blank1440KTest()
        {
            var testCase = new DiskCopyImageTestCase("Blank1440K.img", "Blank1440K.dsk");
            testCase.ExecuteTest();
        }

        [TestMethod]
        public void DiskCopyImage_Blank800KTest()
        {
            var testCase = new DiskCopyImageTestCase("Blank800K.img", "Blank800K.dsk");
            testCase.ExecuteTest();
        }

        [TestMethod]
        public void DiskCopyImage_Blank400KTest()
        {
            var testCase = new DiskCopyImageTestCase("Blank400K.img", "Blank400K.dsk");
            testCase.ExecuteTest();
        }

        public class DiskCopyImageTestCase : FileTestCase
        {
            public DiskCopyImageTestCase(string inputFilePath, string expectedOutputFilePath) : base(inputFilePath, expectedOutputFilePath) { }

            public override void ExecuteTest()
            {
                using FileStream inputFileStream = new FileStream(InputFilePath, FileMode.Open, FileAccess.Read);

                byte[] actualOutputBytes = DiskCopyImage.ReadFrom(inputFileStream).Data;

                byte[] expectedOutputBytes = File.ReadAllBytes(ExpectedOutputFilePath);

                CompareData(expectedOutputBytes, actualOutputBytes);
            }
        }
    }
}
