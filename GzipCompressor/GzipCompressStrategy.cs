using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using GzipCompressor.AdvanceCopier;

namespace GzipCompressor
{
    public class GzipCompressStrategy : IAdvanceCopierStrategy
    {
        public byte[] Process(byte[] buffer)
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

    public class GzipDecompressionStrategy : IAdvanceCopierStrategy
    {
        public byte[] Process(byte[] buffer)
        {
            using (var bufferStream = new MemoryStream(buffer))
            {
                using (var compressedStream = new GZipStream(bufferStream, CompressionMode.Decompress, true))
                {
                    var buf = new byte[bufferStream.Length];
                    compressedStream.Read(buf, 0, buf.Length);
                    return buf;
                }
            }
        }
    }
}