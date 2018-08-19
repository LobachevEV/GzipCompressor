using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        private readonly byte[] gzipHeader = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
        private readonly List<byte> temp = new List<byte>();
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
            
        private static bool CompareArrays(byte[] array, int startIndex, byte[] arrayToCompare)
        {
            if (startIndex < 0 || startIndex > array.Length - arrayToCompare.Length)
            {
                return false;
            }

            return !arrayToCompare.Where((t, i) => array[startIndex + i] != t).Any();
        }
    }
}