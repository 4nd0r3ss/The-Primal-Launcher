using System;
using System.IO;
using System.Text;

namespace Launcher
{
    class GameDataFileDecoder
    {
        /// <summary>
        /// Decode a game data file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A string containing the decoded XML bytes.</returns>
        public static string Decode(string filePath)
        {
            byte[] buffer = File.ReadAllBytes(filePath);

            int bufferLength = buffer.Length;

            if (bufferLength == 0)
                return "";

            if (buffer[bufferLength - 1] != 0xf1)
                return "";

            Array.Resize(ref buffer, bufferLength - 1);
            ScrambleBuffer(ref buffer);

            bufferLength = buffer.Length;
            ushort xA = (ushort)(bufferLength * 7);
            ushort xB = (ushort)((buffer[0x07] << 8 | buffer[0x06]) ^ 0x6c6d);

            DecodeSequence(ref buffer, xA, 0);
            DecodeSequence(ref buffer, xB, 2);

            return Encoding.Default.GetString(buffer);
        }

        private static void ScrambleBuffer(ref byte[] buffer)
        {
            int ptr = 0;
            int end = buffer.Length - 1;

            while (ptr < end)
            {
                byte a = buffer[ptr];
                byte b = buffer[end];

                buffer[ptr] = b;
                buffer[end] = a;

                ptr += 2;
                end -= 2;
            }
        }

        private static void DecodeSequence(ref byte[] buffer, ushort key, int offset)
        {
            while (offset < buffer.Length - 1)
            {
                ushort val = (ushort)(buffer[offset + 1] << 8 | buffer[offset]);
                ushort calc = (ushort)(val ^ key);
                buffer[offset] = (byte)calc;
                buffer[offset + 1] = (byte)(calc >> 8);
                offset += 4;
            }
        }
    }
}
