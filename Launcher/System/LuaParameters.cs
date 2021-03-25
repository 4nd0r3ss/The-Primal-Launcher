using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    [Serializable]
    public class LuaParameters
    {
        //remove this?
        public string ActorName { get; set; }
        public string ClassName { get; set; }
        public uint ClassCode { get; set; }
        public List<KeyValuePair<byte, object>> List { get; set; } = new List<KeyValuePair<byte, object>>();

        /// <summary>
        /// Adds one single Lua parameter to the parameter list of the instanced obj.
        /// </summary>
        /// <param name="param">The parameter to be written.</param>
        public void Add(object param)
        {
            if (param is int)
                List.Add(new KeyValuePair<byte, object>(0, (int)param));
            else if (param is uint)
                List.Add(new KeyValuePair<byte, object>(0x01, (uint)param));
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
            else if (param is Command)
                List.Add(new KeyValuePair<byte, object>(0x06, (Command)param)); 
            else if (param is DirectorCode)
                List.Add(new KeyValuePair<byte, object>(0x98, (DirectorCode)param));
            else if (param is byte)
                List.Add(new KeyValuePair<byte, object>(0xc, (byte)param));
            else if (param is byte[])
                List.Add(new KeyValuePair<byte, object>(0x99, (byte[])param));
            
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
                        if (parameter.Key == 0x01)
                            writer.Write((byte)0);
                        else if(parameter.Key != 0x98 && parameter.Key != 0x99)
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
                                writer.Write(SwapEndian((uint)(Command)parameter.Value));
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
                                writer.Write((byte)0x06);
                                writer.Write((uint)(DirectorCode)parameter.Value);
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
