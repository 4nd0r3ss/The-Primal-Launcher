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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace PrimalLauncher
{
    public static class ExtensionMethods
    {
        #region XmlNode
        public static bool GetAttributeAsBool(this XmlNode node, string attributeName)
        {
            return node != null && node.Attributes[attributeName] != null && Convert.ToBoolean(node.Attributes[attributeName].Value);
        }

        public static string GetAttributeAsString(this XmlNode node, string attributeName, string defaultValue = "")
        {
            return node != null && node.Attributes[attributeName] != null ? node.Attributes[attributeName].Value : defaultValue;
        }

        public static uint GetAttributeAsUint(this XmlNode node, string attributeName, uint defaultValue = 0)
        {
            return node != null && node.Attributes[attributeName] != null ? Convert.ToUInt32(node.Attributes[attributeName].Value) : defaultValue;
        }
        public static int GetAttributeAsInt(this XmlNode node, string attributeName, int defaultValue = 0)
        {
            return node != null && node.Attributes[attributeName] != null ? Convert.ToInt32(node.Attributes[attributeName].Value) : defaultValue;
        }

        public static uint GetNodeAsUint(this XmlNode node, string attributeName, uint defaultValue = 0)
        {
            return node != null && node.SelectSingleNode(attributeName) != null ? Convert.ToUInt32(node.SelectSingleNode(attributeName).InnerText) : defaultValue;
        }

        public static ushort GetNodeAsUshort(this XmlNode node, string attributeName, ushort defaultValue = 0)
        {
            return node != null && node.SelectSingleNode(attributeName) != null ? Convert.ToUInt16(node.SelectSingleNode(attributeName).InnerText) : defaultValue;
        }
        public static int GetNodeAsInt(this XmlNode node, string attributeName, int defaultValue = 0)
        {
            return node != null && node.SelectSingleNode(attributeName) != null ? Convert.ToInt32(node.SelectSingleNode(attributeName).InnerText) : defaultValue;
        }

        public static string[] GetAttributeAsStringArray(this XmlNode node, string attributeName, char separator)
        {
            if(node != null && node.Attributes[attributeName] != null)
            {
                if (node.Attributes[attributeName].Value.IndexOf(separator) >= 0)
                    return node.Attributes[attributeName].Value.Split(separator);
                else
                    return new string[] { node.Attributes[attributeName].Value };
            }
            else
            {
                return null;
            }
        }

        public static int[] GetAttributeAsIntArray(this XmlNode node, string attributeName, char separator)
        {
            if (node != null && node.Attributes[attributeName] != null)
            {
                string attributeValue = node.Attributes[attributeName].Value;

                if (attributeValue.IndexOf(separator) >= 0)
                {
                    string[] tmp = attributeValue.Split(separator);
                    return Array.ConvertAll(tmp, x => int.Parse(x));
                }                
                else
                    return new int[] { int.Parse(node.Attributes[attributeName].Value) };
            }
            else
            {
                return null;
            }
        }

        #endregion

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            string str = "";
            char ch;

            while ((int)(ch = reader.ReadChar()) != 0)
                str += ch;

            return str;
        }

        public static uint ReadUInt32BigEndian(this BinaryReader reader)
        {           
            return BitConverter.ToUInt32(reader.ReadBytes(sizeof(uint)).Reverse().ToArray(), 0);
        }

        public static int ReadInt32BigEndian(this BinaryReader reader)
        {
            return BitConverter.ToInt32(reader.ReadBytes(sizeof(int)).Reverse().ToArray(), 0);
        }

        public static uint IntToUint32(this int n)
        {
            byte[] temp = BitConverter.GetBytes(n);
            return BitConverter.ToUInt32(temp, 0);
        }

        public static void WriteNullTerminatedString(this BinaryWriter writer, string str)
        {
            byte[] strBytes = Encoding.ASCII.GetBytes(str);

            foreach (byte b in strBytes)
                writer.Write(b);

            writer.Write((byte)0);         
        }

        public static string ReadNullTeminatedString(this byte[] data, int startIndex)
        {
            string result = "";

            for(int i = startIndex; i < data.Length; i++)
            {
                if (data[i] == 0)
                    break;
                else
                    result += (char)data[i];
            }

            return result;
        }

        /// <summary>
        /// Writes the dictionary values into the byte array, where the dictionary keys are the indexes from where to start writing a value. 
        /// The value size is inferred by its type when a primitive, or value length when a byte array or string.
        /// </summary>
        /// <param name="dstArr"></param>
        /// <param name="toWrite"></param>
        public static void Write(this byte[] dstArr, Dictionary<int, object> toWrite)
        {
            foreach(var item in toWrite)
            {
                dstArr.Write(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Writes the value into the byte array, where start index the index from where to start writing a value. 
        /// The value size is inferred by its type when a primitive, or value length when a byte array or string.
        /// </summary>
        /// <param name="dstArr"></param>
        /// <param name="startIndex"></param>
        /// <param name="toWrite"></param>
        public static void Write(this byte[] dstArr, int startIndex, object toWrite)
        {
            if (toWrite is byte[] @array)
            {
                Buffer.BlockCopy(@array, 0, dstArr, startIndex, @array.Length);
            }
            else if (toWrite is string @string)
            {
                Buffer.BlockCopy(@string.GetBytes(), 0, dstArr, startIndex, @string.Length);
            }
            else if (toWrite is int @int)
            {
                Buffer.BlockCopy(@int.GetBytes(), 0, dstArr, startIndex, sizeof(int));
            }
            else if (toWrite is uint @uint)
            {
                Buffer.BlockCopy(@uint.GetBytes(), 0, dstArr, startIndex, sizeof(uint));
            }
            else if (toWrite is short @short)
            {
                Buffer.BlockCopy(@short.GetBytes(), 0, dstArr, startIndex, sizeof(short));
            }
            else if (toWrite is ushort @ushort)
            {
                Buffer.BlockCopy(@ushort.GetBytes(), 0, dstArr, startIndex, sizeof(ushort));
            }
            else if (toWrite is long @long)
            {
                Buffer.BlockCopy(@long.GetBytes(), 0, dstArr, startIndex, sizeof(long));
            }
            else if (toWrite is ulong @ulong)
            {
                Buffer.BlockCopy(@ulong.GetBytes(), 0, dstArr, startIndex, sizeof(ulong));
            }
            else if (toWrite is byte @byte)
            {
                dstArr[startIndex] = @byte;
            }
        }

        public static byte[] GetSubset(this byte[] array, int startIndex, int length)
        {
            var subset = new byte[length];
            Array.Copy(array, startIndex, subset, 0, length);
            return subset;
        }

        public static byte[] GetBytes(this string stringVar)
        {
            return Encoding.ASCII.GetBytes(stringVar);
        }

        public static byte[] GetBytes(this int intVar)
        {
            return BitConverter.GetBytes(intVar);
        }

        public static byte[] GetBytes(this uint intVar)
        {
            return BitConverter.GetBytes(intVar);
        }

        public static byte[] GetBytes(this short intVar)
        {
            return BitConverter.GetBytes(intVar);
        }

        public static byte[] GetBytes(this ushort intVar)
        {
            return BitConverter.GetBytes(intVar);
        }

        public static byte[] GetBytes(this long intVar)
        {
            return BitConverter.GetBytes(intVar);
        }

        public static byte[] GetBytes(this ulong intVar)
        {
            return BitConverter.GetBytes(intVar);
        }

        public static string GetString(this byte[] array)
        {
            return Encoding.ASCII.GetString(array);
        }
        public static int GetInt32(this byte[] array, int startIndex)
        {
            return BitConverter.ToInt32(array, startIndex);
        }

        public static uint GetUInt32(this byte[] array, int startIndex)
        {
            return BitConverter.ToUInt32(array, startIndex);
        }

        public static short GetInt16(this byte[] array, int startIndex)
        {
            return BitConverter.ToInt16(array, startIndex);
        }

        public static ushort GetUInt16(this byte[] array, int startIndex)
        {
            return BitConverter.ToUInt16(array, startIndex);
        }

        public static long GetInt64(this byte[] array, int startIndex)
        {
            return BitConverter.ToInt64(array, startIndex);
        }

        public static ulong GetUInt64(this byte[] array, int startIndex)
        {
            return BitConverter.ToUInt64(array, startIndex);
        }

        public static byte[] GetInt32Bytes(this byte[] array, int startIndex)
        {
            return array.GetSubset(startIndex, sizeof(int));
        }

        public static byte[] GetInt16Bytes(this byte[] array, int startIndex)
        {
            return array.GetSubset(startIndex, sizeof(short));
        }

        public static byte[] GetInt64Bytes(this byte[] array, int startIndex)
        {
            return array.GetSubset(startIndex, sizeof(long));
        }





        /// <summary>
        /// Parses a formatted string parameter list into proper types with values.        /// 
        /// </summary>
        /// <param name="strParams">
        /// String parameters must obey formatting as follows:
        /// type1|value1,type2|value2,type3|value3...
        /// *Type names are case sensitive.
        /// </param>
        /// <returns></returns>
        public static List<object> ParseStringParameters(this string strParamList)
        {
            List<object> result = new List<object> ();
            string[] strParams = strParamList.Split(new char[] { ',' });

            foreach(string str in strParams)
            {
                string[] temp = str.Split(new char[] { '|' });               
                result.Add(Convert.ChangeType(temp[1], Type.GetType("System." + temp[0])));
            }

            return result;
        }

        /// <summary>
        /// Get the contents of a XML file configured as a resource.
        /// From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
        /// Slightly modified for this project, all credit goes to original author.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static void LoadFromResource(this XmlDocument document, string fileName)
        {
            string result = GetFileBytes("xml", fileName); ;
          
            if (!string.IsNullOrEmpty(result)){
                document.LoadXml(result);
            }
        }

        public static string GetFileBytes(string folder, string fileName)
        {
            string result = string.Empty;
            Stream stream = typeof(ActorRepository).Assembly.GetManifestResourceStream("PrimalLauncher.Resources." + folder + "." + fileName);

            if (stream != null)
                using (stream)
                using (StreamReader sr = new StreamReader(stream))
                    result = sr.ReadToEnd();           

            return result;
        }
    }
}
