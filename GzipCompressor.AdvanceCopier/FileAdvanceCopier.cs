using System.IO;
using GzipComressor.Infrastructure.Logging;

namespace GzipCompressor.AdvanceCopier
{
    public class FileAdvanceCopier : StreamAdvanceCopier
    {
        public FileAdvanceCopier(IAdvanceCopierStrategy strategy, Logger logger) : base(strategy, logger)
        {
        }

        public void Copy(string sourceFilePath, string targetFilePath)
        {
            using (var source = File.Open(sourceFilePath, FileMode.OpenOrCreate))
            using (var target = File.Open(targetFilePath, FileMode.OpenOrCreate))
                base.Copy(source, target);
        }
    }
}