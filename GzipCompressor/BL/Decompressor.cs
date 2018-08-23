using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.BL
{
    public class Decompressor : IProcessor
    {
        private readonly byte[] gzipHeader = {31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
        private readonly Logger logger;
        private readonly WorkerScheduler scheduler;

        public Decompressor(WorkerScheduler scheduler, Logger logger)
        {
            this.scheduler = scheduler;
            this.logger = logger;
        }

        public void Process(BoundedBlockingQueue<byte[]> source, BoundedBlockingQueue<IndexedBuffer> target)
        {
            var i = 0;
            foreach (var buffer in source.Consume())
            {
                var indexedBuffer = new IndexedBuffer(i);
                i++;
                scheduler.StartNew(() =>
                {
                    indexedBuffer.Data = Decompress(buffer);
                    target.Add(indexedBuffer);
                    logger.Debug($"Processed buffer {indexedBuffer.Index}");
                });
            }

            scheduler.WaitAll();
        }

        private byte[] Decompress(byte[] compressedBuffer)
        {
            logger.Debug($"Get to process bytes {compressedBuffer.Length}");
            var result = new List<byte>();
            foreach (var chunk in Parse(compressedBuffer))
                using (var memoryStream = new MemoryStream(chunk))
                {
                    using (var compressedStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        while (true)
                        {
                            var decompressedBuffer = new byte[chunk.Length];
                            var readedBytes = compressedStream.Read(decompressedBuffer, 0, decompressedBuffer.Length);

                            if (readedBytes == 0) break;

                            if (readedBytes < decompressedBuffer.Length)
                                Array.Resize(ref decompressedBuffer, readedBytes);
                            result.AddRange(decompressedBuffer);
                        }
                    }
                }

            logger.Debug($"Processed bytes {result.Count}");
            return result.ToArray();
        }

        private byte[][] Parse(byte[] buffer)
        {
            var indexes = buffer.FindStartingIndexes(gzipHeader).ToList();
            logger.Debug($"Decopmressor - Indexes found {indexes.Count}");
            return indexes.Select((currentIndex, i) =>
            {
                var nextIndex = indexes.Count == i + 1 ? buffer.Length : indexes[i + 1];
                var toCopyCount = nextIndex - currentIndex;
                var result = new byte[toCopyCount];
                Array.Copy(buffer, currentIndex, result, 0, toCopyCount);

                return result;
            }).ToArray();
        }
    }
}