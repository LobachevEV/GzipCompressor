namespace GzipCompressor
{
    public class GzipConstants
    {
        public const string Compress = "compress";
        public const string Decompress = "decompress";
        public static readonly byte[] Header = {31, 139, 8, 0, 0, 0, 0, 0, 4, 0};
    }
}