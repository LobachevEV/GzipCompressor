using System.IO;
using GzipComressor.Infrastructure;

namespace GzipCompressor.AdvanceCopier
{
    public interface IStreamReader
    {
        void Read(Stream source, BoundedBlockingQueue<byte[]> target);
    }
}