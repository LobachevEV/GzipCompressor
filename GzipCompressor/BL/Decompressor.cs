using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.BL
{
    public class Decompressor : ParallelProcessor
    {
        public Decompressor(WorkerScheduler scheduler, Logger logger) : base(scheduler, logger)
        {
        }

        protected override byte[] ProcessInternal(byte[] compressedBuffer)
        {
            Logger.Debug($"Get to process bytes {compressedBuffer.Length}");
            var result = new List<byte>();
            foreach (var chunk in Parse(compressedBuffer))
                using (var memoryStream = new MemoryStream(chunk))
                {
                    using (var compressedStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        while (true)
                        {
                            var decompressedBuffer = new byte[chunk.Length];
                            var readBytes = compressedStream.Read(decompressedBuffer, 0, decompressedBuffer.Length);

                            if (readBytes == 0) break;

                            if (readBytes < decompressedBuffer.Length)
                                Array.Resize(ref decompressedBuffer, readBytes);
                            result.AddRange(decompressedBuffer);
                        }
                    }
                }

            Logger.Debug($"Processed bytes {result.Count}");
            return result.ToArray();
        }

        private byte[][] Parse(byte[] buffer)
        {
            var indexes = buffer.FindStartingIndexes(GzipConstants.Header).ToList();
            Logger.Debug($"Decompressor - Indexes found {indexes.Count}");
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