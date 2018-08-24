using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GzipCompressor.AdvanceCopier;
using GzipCompressor.BL;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;
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

        // ReSharper disable once InconsistentNaming
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

        private const string DirPath = @"D:\Repos\GzipCompressor\GzipCompressor.Tests\Assets\";
        private const string Sample1 = "SampleCSVFile_53000kb.csv";
        private const string Sample2 = "SampleXLSFile_6800kb.xls";
        
        [TestCase(Sample1)]
        [TestCase(Sample2)]
        public void CountHash_Compress_Decompress_CheckHash(string fileName)
        {
            var sourceFilePath = Path.Combine(DirPath, fileName);
            if (!File.Exists(sourceFilePath)) Assert.Fail("File does not exists");

            var compressedFilePath = Path.Combine(DirPath, "compressed.gz");
            if (File.Exists(compressedFilePath)) File.Delete(compressedFilePath);

            var expected = CalculateMD5(sourceFilePath);
             
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            var workerScheduler = new WorkerScheduler(16, logger);
            var gzipCompressorFactory = new GzipCompressorFactory(logger, workerScheduler);
            gzipCompressorFactory.Get(GzipConstants.Compress).Execute(sourceFilePath, compressedFilePath);


            var decompressedFilePath = Path.Combine(DirPath, $"decompressed{fileName}");
            gzipCompressorFactory.Get(GzipConstants.Decompress).Execute(compressedFilePath,
                decompressedFilePath);
            var actual = CalculateMD5(decompressedFilePath);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FindingSubArray()
        {
            byte[] array = {0x1A, 0x65, 0x3E, 0x00, 0x01, 0x00, 0x4C, 0xAA, 0x00, 0x01};
            byte[] subArray = {0x00, 0x01};

            var actual = array.FindStartingIndexes(subArray);
            var expected = new[] {3, 8};
            CollectionAssert.AreEqual(expected, actual);
        }

        [TearDown]
        public void Clear()
        {
            var decompressedSample1FilePath = Path.Combine(DirPath, $"decompressed{Sample1}");
            DeleteIfExists(decompressedSample1FilePath);
            var decompressedSample2FilePath = Path.Combine(DirPath, $"decompressed{Sample2}");
            DeleteIfExists(decompressedSample2FilePath);
            var compressedFilePath = Path.Combine(DirPath, "compressed.gz");
            DeleteIfExists(compressedFilePath);
        }

        private static void DeleteIfExists(string decompressedSample1FilePath)
        {
            if (File.Exists(decompressedSample1FilePath))
            {
                File.Delete(decompressedSample1FilePath);
            }
        }
    }
}