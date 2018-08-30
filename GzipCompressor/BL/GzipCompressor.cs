using System.IO;
using GzipCompressor.AdvanceCopier;

namespace GzipCompressor.BL
{
    public class GzipCompressor
    {
        private readonly StreamAdvanceCopier copier;

        public GzipCompressor(StreamAdvanceCopier copier)
        {
            this.copier = copier;
        }

        public void Execute(string sourceFilePath, string targetFilePath)
        {
            using (var source = File.Open(sourceFilePath, FileMode.OpenOrCreate))
            using (var target = File.Open(targetFilePath, FileMode.OpenOrCreate))
            {
                copier.Copy(source, target);
            }
        }
    }
}