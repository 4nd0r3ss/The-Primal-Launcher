using Ionic.Zlib;
using System.IO;

namespace Launcher
{
    public class Zip
    {
        #region Compression/Decompression
        private static byte[] Zlib(byte[] bytes, CompressionMode mode)
        {
            using (var compressedStream = new MemoryStream(bytes))
            using (var zipStream = new ZlibStream(compressedStream, mode))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
        public static byte[] Compress(byte[] data) => Zlib(data, CompressionMode.Compress);
        public static byte[] Uncompress(byte[] data) => Zlib(data, CompressionMode.Decompress);
        #endregion
    }
}
