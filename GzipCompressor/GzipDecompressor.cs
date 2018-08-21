using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor
{
    public class GzipDecompressor : IProcessor
    {
        private readonly Logger logger;
        private readonly WorkerScheduler scheduler;

        public GzipDecompressor(WorkerScheduler scheduler, Logger logger)
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
                scheduler.WaitAll();
            }
        }

        private byte[] Decompress(byte[] buffer)
        {
            using (var bufferStream = new MemoryStream(buffer))
            {
                using (var compressedStream = new GZipStream(bufferStream, CompressionMode.Decompress))
                {
                    var result = new List<byte>();
                    while (true)
                    {
                        var temp = new byte[buffer.Length];
                        var readedBytes = compressedStream.Read(temp, 0, temp.Length);
                        
                        if (readedBytes == 0) break;

                        if (readedBytes < temp.Length) Array.Resize(ref temp, readedBytes);
                        result.AddRange(temp);
                    }
                    logger.Debug($"Read bytes {result.Count}");
                    return result.ToArray();
                }
            }
        }
    }
}