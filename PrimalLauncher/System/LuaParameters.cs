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
using System.Text;

namespace PrimalLauncher
{
    [Serializable]
    public class LuaParameters
    {
        //remove this?
        public string ActorName { get; set; }
        public string ClassName { get; set; }
        public uint ClassCode { get; set; }
        public object[] Parameters
        {
            set
            {
                object[] toAdd = value;

                if (toAdd != null && toAdd.Length > 0)
                    AddRange(toAdd);
            }
        }

        public List<KeyValuePair<byte, object>> List { get; set; }

        public LuaParameters()
        {
            List = new List<KeyValuePair<byte, object>>();
        }

        public LuaParameters(object[] parameters)
        {
            List = new List<KeyValuePair<byte, object>>();
            Parameters = parameters;
        }

        /// <summary>
        /// Adds one single Lua parameter to the parameter list of the instanced obj.
        /// </summary>
        /// <param name="param">The parameter to be written.</param>
        public void Add(object param)
        {
            if (param is int)
                List.Add(new KeyValuePair<byte, object>(0, (int)param));
            else if (param is string)
                List.Add(new KeyValuePair<byte, object>(0x02, (string)param));
            else if (param is bool)
            {
                if ((bool)param)
                    List.Add(new KeyValuePair<byte, object>(0x03, null));
                else
                    List.Add(new KeyValuePair<byte, object>(0x04, null));
            }
            else if (param is null)
                List.Add(new KeyValuePair<byte, object>(0x05, null));
            else if (param is uint)
                List.Add(new KeyValuePair<byte, object>(0x06, (uint)param));                 
            else if (param is byte)
                List.Add(new KeyValuePair<byte, object>(0xc, (byte)param));
            else if (param is sbyte)
                List.Add(new KeyValuePair<byte, object>(0x98, (sbyte)param));
            else if (param is byte[])
                List.Add(new KeyValuePair<byte, object>(0x99, (byte[])param));
           
        }

        public void AddRange(object[] toAdd)
        {
            foreach (object obj in toAdd)
                Add(obj);
        }

        /// <summary>
        /// Writes all parameters in the instanced obj parameter list to the packet to be sent.
        /// </summary>
        /// <param name="data">A pointer to the packet buffer. </param>
        /// <param name="luaParameters">The list of parameters to be witten.</param>
        public static void WriteParameters(ref byte[] data, LuaParameters luaParameters, byte startIndex = 0x44)
        {
            //Write Params - using binary writer bc sizes, types, #items can vary.
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Seek(startIndex, SeekOrigin.Begin); //points to the right position

                    foreach (var parameter in luaParameters.List)
                    {                        
                        if(parameter.Key != 0x98 && parameter.Key != 0x99)
                            writer.Write(parameter.Key);

                        switch (parameter.Key)
                        {
                            case 0:
                                writer.Write(SwapEndian((int)parameter.Value));
                                break;
                            case 0x01:
                                writer.Write(SwapEndian((uint)parameter.Value));
                                break;
                            case 0x02:
                                string str = (string)parameter.Value;
                                writer.Write(Encoding.ASCII.GetBytes(str), 0, Encoding.ASCII.GetByteCount(str));
                                writer.Write((byte)0);
                                break;
                            case 0x05: //null
                                break;
                            case 0x06:                                
                                writer.Write(SwapEndian((uint)parameter.Value));
                                break;
                            case 0x07:
                                break;
                            case 0x09:
                                break;
                            case 0x0c:
                                writer.Write((byte)parameter.Value);
                                break;
                            case 0x1b:
                                break;
                            case 0x98:
                                writer.Write((sbyte)parameter.Value);                               
                                break;
                            case 0x99:
                                byte[] strArr = new byte[0x20];
                                byte[] value = (byte[])parameter.Value;
                                Buffer.BlockCopy(value, 0, strArr, 0, value.Length);
                                writer.Write(strArr, 0, strArr.Length);                                
                                break;
                            case 0x0f:
                                continue;
                        }
                    }

                    writer.Write((byte)0x0f);
                }
            }
        }

        public static List<object> ReadParameters(byte[] data, int startIndex)
        {
            List<object> parameters = new List<object>();

            using(MemoryStream ms = new MemoryStream(data))
            {
                ms.Seek(startIndex, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    bool looping = true;

                    while (looping)
                    {
                        byte lead = br.ReadByte();

                        switch (lead)
                        {
                            case 0x0F:
                                looping = false;
                                break;
                            case 0:
                                parameters.Add(br.ReadInt32BigEndian());
                                break;
                            case 0x01:
                            case 0x06:
                                parameters.Add(br.ReadUInt32BigEndian());
                                break;
                            case 0x02:
                                parameters.Add(br.ReadNullTerminatedString());
                                break;
                            case 0x03:
                            case 0x04:
                                parameters.Add(lead == 0x03);
                                break;
                            case 0x05:
                                parameters.Add(null);
                                break;
                            case 0x0C:
                                parameters.Add(br.ReadByte());
                                break;
                        }
                    }
                }
            }
            

            return parameters;
        }

        public int GetTotalByteSize()
        {
            int totalSize = 0;

            foreach(var param in List)
            {
                switch (param.Key)
                {
                    case 0:
                    case 0x01:
                    case 0x06:
                        totalSize += sizeof(int) + 1;
                        break;
                    case 0x02:
                        totalSize += ((string)param.Value).Length + 2;
                        break;
                    case 0x03:
                    case 0x04:
                    case 0x05:
                        totalSize += sizeof(byte);
                        break;
                    case 0x0C:
                        totalSize += sizeof(byte) + 1;
                        break;
                    case 0x98:
                        totalSize += sizeof(byte);
                        break;
                    case 0x99:
                        totalSize += 0x20;
                        break;
                }
            }

            totalSize++; //plus wrapper byte

            return totalSize;
        }

        #region Endian swappers
        public static ulong SwapEndian(ulong input)
        {
            return 0x00000000000000FF & (input >> 56) |
                   0x000000000000FF00 & (input >> 40) |
                   0x0000000000FF0000 & (input >> 24) |
                   0x00000000FF000000 & (input >> 8) |
                   0x000000FF00000000 & (input << 8) |
                   0x0000FF0000000000 & (input << 24) |
                   0x00FF000000000000 & (input << 40) |
                   0xFF00000000000000 & (input << 56);
        }

        public static uint SwapEndian(uint input)
        {
            return ((input >> 24) & 0xff) |
                   ((input << 8) & 0xff0000) |
                   ((input >> 8) & 0xff00) |
                   ((input << 24) & 0xff000000);
        }

        public static int SwapEndian(int input)
        {
            var inputAsUint = (uint)input;

            input = (int)
                (((inputAsUint >> 24) & 0xff) |
                 ((inputAsUint << 8) & 0xff0000) |
                 ((inputAsUint >> 8) & 0xff00) |
                 ((inputAsUint << 24) & 0xff000000));

            return input;
        }
        #endregion  
    }
}
