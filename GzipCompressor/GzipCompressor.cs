using System.IO;
using System.IO.Compression;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor
{
    public class GzipCompressor
    {
        private readonly Logger logger;

        public GzipCompressor(Logger logger)
        {
            this.logger = logger;
        }

        public void Compress(string sourceFilePath, string targetFilePath)
        {
            using (var sourceStream = File.Open(sourceFilePath, FileMode.OpenOrCreate))
            {
                using (var targetStream = File.Open(targetFilePath, FileMode.OpenOrCreate))
                {
                    logger.Info($"Start compressing file {sourceFilePath}");
                    logger.Debug($"File size: {sourceStream.Length}");
                    const long gzipMaxCapacity = 4 * (long) 1024 * 1024 * 1024 * 1024;
                    long processedBytesCount = 0;
                    while (processedBytesCount <= sourceStream.Length)
                        using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress, true))
                        {
                            sourceStream.CopyAsync(compressionStream, gzipMaxCapacity);
                            processedBytesCount += gzipMaxCapacity;
                            logger.Debug($"processedBytesCount: {processedBytesCount}");
                            logger.Debug($"targetStream length: {targetStream.Length}");
                        }

                    targetStream.Close();
                    logger.Info($"Compressing finished, result file size {targetStream.Length}");
                }
            }
        }

        public void Decompress(string sourceFilePath, string targetFilePath)
        {
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.OpenOrCreate))
            {
                using (var targetStream = File.Create(targetFilePath))
                {
                    using (var decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        logger.Info($"Start decompressing file {sourceFilePath}");
                        logger.Debug($"File size: {sourceStream.Length}");
                        decompressionStream.CopyAsync(targetStream);
                        logger.Info($"Decompressing finished, result file size {targetStream.Length}");
                    }
                }
            }
        }
    }
}