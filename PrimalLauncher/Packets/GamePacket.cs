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

namespace PrimalLauncher
{
    public class GamePacket
    {
        byte[] _data;
        public ushort Opcode { get; set; }
        public byte[] TimeStamp { get; set; }
        public byte[] Data
        {
            get => _data;
            set
            {
                _data = value;
                Size = (ushort)(0x10 + _data.Length); //0x10 = header size
            }
        }
        public ushort Size { get; set; }

        #region Constructors
        public GamePacket(byte[] packet) => GamePacketSetup(packet);

        public GamePacket() { }

        public GamePacket(ushort opcode, byte[] data)
        {
            Opcode = opcode;
            Data = data;
        }
        #endregion

        private void GamePacketSetup(byte[] packet)
        {
            Opcode = (ushort)(packet[2] << 8 | packet[3]);
            TimeStamp = new byte[] { packet[0x08], packet[0x09], packet[0x0A], packet[0x0B] };
            Data = packet.GetSubset(packet.Length + 0x10, packet.Length - 0x10);
        }
                       
        public byte[] ToBytes(Blowfish blowfish)
        {
            byte[] result = new byte[Data.Length + 0x10];

            result.Write(new Dictionary<int, object>
            {
                {0, 0x14},
                {0x02, Opcode},
                {0x08, Server.GetTimeStampHex()},
                {0x10, Data}               
            });                        

            if(blowfish != null)
                blowfish.Encipher(result, 0, result.Length);

            return result;
        }  
        
        public string Stringify(bool isFromClient, uint sourceActor, uint targetActor)
        {
            string result = "";
            int bytecount = 0;
            string byteString = "";
            string charString = " ";            

            if (
                //data[0] == 0x14 &&
                !(Opcode == 0x0ca && isFromClient) &&
                Opcode != 0x01 &&
                Opcode != 0xcf
            //opcode != 0x018d &&
            //opcode != 0x14b
            )
            {
                result += "\r\nSource: " + (isFromClient ? "Client" : "Server") + "\r\n";
                result += "0x" + sourceActor.ToString("X4") + " -> " + "0x" + targetActor.ToString("X4") + "\r\n";
                result += "Timestamp: 0x" + Convert.ToInt32(TimeStamp).ToString("X4") + "\r\n";
                result += GetOpcodeName(isFromClient);

                int index = 0;

                for (int i = index; i < Data.Length; i++)
                {
                    byteString += Data[i].ToString("X2") + " ";
                    char a = Convert.ToChar(Data[i]);
                    a = (char.IsControl(a) || char.IsSeparator(a)) ? '.' : a;
                    charString += a;
                    if (bytecount == 0x0f)
                    {
                        result += byteString + charString + "\r\n";
                        byteString = "";
                        charString = " ";
                        bytecount = -1;
                    }

                    bytecount++;
                }

                if (byteString != "")
                {
                    while (byteString.Length < 0x10 * 3) byteString += "  ";
                    result += byteString + charString + "\r\n";
                }
            }

            return result;
        }

        public string GetOpcodeName(bool isFromClient)
        {
            string result = "UNKNOWN";
            var enumType = isFromClient ? typeof(ClientOpcode) : typeof(ServerOpcode);
            bool opcodeExists = Enum.IsDefined(enumType, (int)Opcode);

            if (opcodeExists)
            {
                result = "0x" + Opcode.ToString("X3") + " - " + Enum.GetName(enumType, (int)Opcode) + "\r\n";
            }
            else
            {
                result = "0x" + Opcode.ToString("X3") + " - " + result + "\r\n";
            }

            return result;
        }
    }
}
