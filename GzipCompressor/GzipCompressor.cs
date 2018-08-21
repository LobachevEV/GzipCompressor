using System.IO;
using System.IO.Compression;
using GzipCompressor.AdvanceCopier;
using GzipComressor.Infrastructure;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor
{
    public class GzipCompressor : IProcessor
    {
        private readonly Logger logger;
        private readonly WorkerScheduler scheduler;

        public GzipCompressor(WorkerScheduler scheduler, Logger logger)
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
                    indexedBuffer.Data = Compress(buffer);
                    target.Add(indexedBuffer);
                });
            }
            scheduler.WaitAll();
        }

        private byte[] Compress(byte[] buffer)
        {
            using (var bufferStream = new MemoryStream())
            {
                using (var compressedStream = new GZipStream(bufferStream, CompressionMode.Compress, true))
                {
                    compressedStream.Write(buffer, 0, buffer.Length);
                }

                bufferStream.Position = 0;

                var buf = new byte[bufferStream.Length];
                bufferStream.Read(buf, 0, buf.Length);
                return buf;
            }
        }
    }
}