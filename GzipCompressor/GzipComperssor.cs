using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GzipCompressor
{
    public class GzipComperssor
    {
        public void Compress(string sourceFile, string compressedFile)
        {
            using (var sourceStream = File.Open(sourceFile, FileMode.OpenOrCreate))
            {
                using (var targetStream = File.Open(compressedFile, FileMode.OpenOrCreate))
                {
                    const long gzipMaxCapacity = 4 * (long) 1024 * 1024 * 1024 * 1024;
                    if (targetStream.Length >= gzipMaxCapacity)
                    {
                        CompressInternal(sourceStream, targetStream, gzipMaxCapacity);
                    }
                    else
                    {
                        CompressInternal(sourceStream, targetStream);
                    }
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
            {
                using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress, true))
                {
                    sourceStream.CopyAsync(compressionStream, gzipMaxCapacity);
                    processedBytesCount += gzipMaxCapacity;
                }
            }

            targetStream.Close();
        }

        public void Decompress(string compressedFile, string targetFile)
        {
            using (var sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                using (var targetStream = File.Create(targetFile))
                {
                    using (var decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyAsync(targetStream);
                    }
                }
            }
        }
    }

    public static class SteamExtentions
    {
        public static void CopyAsync(this Stream input, Stream output, long batchSize = 0)
        {
            var queue = new BoundedBlockingQueue<byte[]>(100);

            var readThread = new Thread(() =>
            {
                const int bufferSize = 16 * 1024;
                var buffer = new byte[bufferSize];
                var readBytesCount = bufferSize;
                while (batchSize == 0 ? readBytesCount != 0 : readBytesCount < batchSize)
                {
                    readBytesCount += input.Read(buffer, 0, buffer.Length);
                    queue.Add(buffer);
                }

                queue.CompleteAdding();
            });
            readThread.Start();

            var writeThread = new Thread(() =>
            {
                foreach (var bytese in queue.Consume())
                {
                    output.Write(bytese, 0, bytese.Length);
                }
            });
            writeThread.Start();
            writeThread.Join();
        }
    }
}