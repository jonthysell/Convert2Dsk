// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Convert2Dsk.Test
{
    public abstract class FileTestCase
    {
        public readonly string InputFilePath;
        public readonly string ExpectedOutputFilePath;

        public FileTestCase(string inputFilePath, string expectedOutputFilePath)
        {
            InputFilePath = inputFilePath;
            ExpectedOutputFilePath = expectedOutputFilePath;
        }

        public abstract void ExecuteTest();

        protected void CompareData(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
