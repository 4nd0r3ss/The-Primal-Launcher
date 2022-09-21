/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Ionic.Zlib;
using System.IO;

namespace PrimalLauncher
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
