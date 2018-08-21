using System.IO;
using GzipComressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public interface IQueueToStreamWriter<T>
    {
        void Write(BoundedBlockingQueue<T> source, Stream target);
    }
}