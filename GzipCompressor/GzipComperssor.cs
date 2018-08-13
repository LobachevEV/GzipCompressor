using System;
using System.IO;
using System.IO.Compression;

namespace GzipCompressor
{
    public class GzipComperssor
    {
        public void Compress(string sourceFilePath, string targetFilePath)
        {
            using (var sourceStream = File.Open(sourceFilePath, FileMode.OpenOrCreate))
            {
                using (var targetStream = File.Open(targetFilePath, FileMode.OpenOrCreate))
                {
                    const long gzipMaxCapacity = 4 * (long) 1024 * 1024 * 1024 * 1024;
                    if (targetStream.Length >= gzipMaxCapacity)
                        CompressInternal(sourceStream, targetStream, gzipMaxCapacity);
                    else
                        CompressInternal(sourceStream, targetStream);
                }
            }
        }

        private void CompressInternal(Stream sourceStream, Stream targetStream)
        {
            using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
            {
                sourceStream.CopyAsync(compressionStream);
            }
        }

        private void CompressInternal(Stream sourceStream, Stream targetStream, long gzipMaxCapacity)
        {
            long processedBytesCount = 0;
            while (processedBytesCount < targetStream.Length)
                using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress, true))
                {
                    sourceStream.CopyAsync(compressionStream, gzipMaxCapacity);
                    processedBytesCount += gzipMaxCapacity;
                }

            targetStream.Close();
        }

        public void Decompress(string sourceFilePath, string targetFilePath)
        {
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.OpenOrCreate))
            {
                using (var targetStream = File.Create(targetFilePath))
                {
                    using (var decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyAsync(targetStream);
                    }
                }
            }
        }
    }
}