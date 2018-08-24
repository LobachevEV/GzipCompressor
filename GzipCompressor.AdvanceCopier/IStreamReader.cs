using System.IO;
using GzipCompressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public interface IStreamReader
    {
        void Read(Stream source, BoundedBlockingQueue<byte[]> target);
    }
}