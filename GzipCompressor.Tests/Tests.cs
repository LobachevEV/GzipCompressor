using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace GzipCompressor.Tests
{
    [TestFixture]
    public class GzipCompressorTests
    {
        public GzipCompressorTests()
        {
            LogSettings.LogLevel = LogLevel.Debug;
        }

        private static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        [Test]
        [TestCase(@"SampleCSVFile_53000kb.csv")]
        [TestCase(@"SampleXLSFile_6800kb.xls")]
        public void CountHash_Compress_Decompress_CheckHash(string fileName)
        {
            const string dirPath = @"D:\Repos\GzipCompressor\GzipCompressor.Tests\Assets\";
            var sourceFilePath = Path.Combine(dirPath, fileName);
            if (!File.Exists(sourceFilePath)) Assert.Fail("File does not exests");

            var compressedFilePath = Path.Combine(dirPath, "compressed.gz");
            if (File.Exists(compressedFilePath)) File.Delete(compressedFilePath);

            var expected = CalculateMD5(sourceFilePath);
            var compressor = new GzipCompressor(new WorkerScheduler(16, null), null);
            var defaultReader = new DefaultStreamReader();
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            new FileAdvanceCopier(defaultReader, compressor, new OrderingWriter(), logger).Copy(sourceFilePath,
                compressedFilePath);


            var decompressedFilePath = Path.Combine(dirPath, $"decompressed{fileName}");
            var decompressor = new GzipDecompressor(new WorkerScheduler(16, null), null);
            var gzipReader = new DefaultStreamReader();
            new FileAdvanceCopier(gzipReader, decompressor, new OrderingWriter(), logger).Copy(compressedFilePath,
                decompressedFilePath);
            var actual = CalculateMD5(decompressedFilePath);
            Assert.AreEqual(expected, actual);
            File.Delete(compressedFilePath);
        }

        [Test]
        public void FindingSubArray()
        {
            byte[] array = {0x1A, 0x65, 0x3E, 0x00, 0x01, 0x00, 0x4C, 0xAA, 0x00, 0x01};
            byte[] subArray = {0x00, 0x01};

            var actual = StartingIndex(array, subArray).ToArray();
            var expected = new[] {3, 8};
            CollectionAssert.AreEqual(expected, actual);
        }


        public static IEnumerable<int> StartingIndex(byte[] array, byte[] subArray)
        {
            var result = new List<int>();
            var position = 0;
            while (true)
            {
                var index = StartingIndex(array, subArray, position);
                if (index == -1)
                {
                    return result;
                }

                if (index != -2)
                {
                    result.Add(index);
                    position = index + subArray.Length;
                }
                else
                {
                    position += subArray.Length;
                }

            }
        }

        private static int StartingIndex(byte[] array, byte[] subArray, int position)
        {
            var index = Array.FindIndex(array, position, b => b == subArray[0]);
            if (index == -1)
            {
                return -1;
            }

            if (array.Length - index < subArray.Length)
            {
                return -1;
            }

            bool Check()
            {
                for (var i = 1; i < subArray.Length; i++)
                {
                    if (array[index + i] != subArray[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return Check() ? index : -2;
        }
    }
}