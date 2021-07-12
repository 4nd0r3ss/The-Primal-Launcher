using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public static class ExtensionMethods
    {
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            string str = "";
            char ch;

            while ((int)(ch = reader.ReadChar()) != 0)
                str += ch;

            return str;
        }

        public static void WriteNullTerminatedString(this BinaryWriter writer, string str)
        {
            byte[] strBytes = Encoding.ASCII.GetBytes(str);

            foreach (byte b in strBytes)
                writer.Write(b);

            writer.Write((byte)0);         
        }
    }
}
