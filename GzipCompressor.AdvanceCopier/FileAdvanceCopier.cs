using System.IO;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class FileAdvanceCopier : StreamAdvanceCopier
    {
        public FileAdvanceCopier(IStreamReader reader, IProcessor processor,
            IQueueToStreamWriter<IndexedBuffer> writer, Logger logger) : base(reader, processor, writer, logger)
        {
        }
        
        public void Copy(string sourceFilePath, string targetFilePath)
        {
            using (var source = File.Open(sourceFilePath, FileMode.OpenOrCreate))
            using (var target = File.Open(targetFilePath, FileMode.OpenOrCreate))
            {
                base.Copy(source, target);
            }
        }
    }
}