using System.IO;
using GzipCompressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public interface IQueueToStreamWriter<T>
    {
        void Write(BoundedBlockingQueue<T> source, Stream target);
    }
}