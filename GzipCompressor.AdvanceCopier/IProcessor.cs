using GzipCompressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public interface IProcessor
    {
        void Process(BoundedBlockingQueue<byte[]> source, BoundedBlockingQueue<IndexedBuffer> target);
    }
}