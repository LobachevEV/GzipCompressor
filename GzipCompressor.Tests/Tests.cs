using System;
using System.IO;
using System.Security.Cryptography;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure.Logging;
using NUnit.Framework;

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
            var compressor = new GzipCompressStrategy();
            var logger = LogFactory.GetInstance().GetLogger<ConsoleLogger>();
            new FileAdvanceCopier(compressor, logger).Copy(sourceFilePath, compressedFilePath);
            

            var decompressedFilePath = Path.Combine(dirPath, $"decompressed{fileName}");
            var decompressor = new GzipDecompressionStrategy();
            new FileAdvanceCopier(decompressor, logger).Copy(compressedFilePath, decompressedFilePath);
            var actual = CalculateMD5(decompressedFilePath);
            Assert.AreEqual(expected, actual);
            File.Delete(compressedFilePath);
        }
    }
}