using System.IO;
using System.IO.Compression;
using GzipCompressor.AdvanceCopier;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.BL
{
    public class Compressor : ParallelProcessor
    {
        public Compressor(WorkerScheduler scheduler, Logger logger) : base(scheduler, logger)
        {
        }

        protected override byte[] ProcessInternal(byte[] buffer)
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