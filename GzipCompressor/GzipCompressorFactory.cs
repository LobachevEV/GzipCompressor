using System;
using GzipCompressor.AdvanceCopier;
using GzipCompressor.BL;
using GzipCompressor.Infrastructure;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor
{
    public class GzipCompressorFactory
    {
        private readonly Logger logger;
        private readonly WorkerScheduler workerScheduler;

        public GzipCompressorFactory(Logger logger, WorkerScheduler workerScheduler)
        {
            this.logger = logger;
            this.workerScheduler = workerScheduler;
        }

        public BL.GzipCompressor Get(string mode)
        {
            var processor = GetProcessor(mode);
            var reader = GetReader(mode);
            var copier = new StreamAdvanceCopier(reader, processor, new OrderingWriter(logger), logger);
            return new BL.GzipCompressor(copier);
        }

        private IProcessor GetProcessor(string mode)
        {
            switch (mode.ToLowerInvariant())
            {
                case GzipConstants.Compress:
                    return new Compressor(workerScheduler, logger);
                case GzipConstants.Decompress:
                    return new Decompressor(workerScheduler, logger);
                default:
                    throw new ArgumentException(
                        "The mode is incorrect. Please choose one of the following options: compress, decompress.");
            }
        }

        private IStreamReader GetReader(string mode)
        {
            switch (mode.ToLowerInvariant())
            {
                case GzipConstants.Compress:
                    return new DefaultStreamReader();
                case GzipConstants.Decompress:
                    return new GzipStreamReader(logger);
                default:
                    throw new ArgumentException(
                        "The mode is incorrect. Please choose one of the following options: compress, decompress.");
            }
        }
    }
}