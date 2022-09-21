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
using System.Text;

namespace PrimalLauncher
{
    /// <summary>
    /// This class is used to keep data about event conditions for actors. Actors have a list of event conditions populated by specialized class' constructor. 
    /// </summary>
    [Serializable]
    public class Event
    {
        public ServerOpcode Opcode { get; set; }
        public string Name { get; set; }
        public ushort EmoteId { get; set; }
        public float Radius { get; set; }       //circle size
        public byte Priority { get; set; }      //unknown
        public byte Enabled { get; set; }    //0 won't fire event.
        public byte Silent { get; set; }      //0x1 do NOT lock UI and player.
        public byte Direction { get; set; }     //possible values: 0x11 leave circle, 0x1 enter circle.
        public uint ServerCodes { get; set; }

        //For BG objects
        public uint BgObjectId { get; set; }
        public uint LayoutId { get; set; }
        public uint ActorId { get; set; } = 0x4;
        public string ReactionName { get; set; }

        public byte Option1 { get; set; }
        public byte Option2 { get; set; }

        public string Action { get;set; }

        public Event() { }


        public void SetEventCondition(uint id)
        {
            byte[] data = new byte[0x28];
            byte[] conditionName = Encoding.ASCII.GetBytes(Name);
            int conditionNameLength = Name.Length;

            switch (Opcode)
            {
                case ServerOpcode.EmoteEvent:
                    Buffer.BlockCopy(BitConverter.GetBytes(Priority), 0, data, 0, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(Enabled), 0, data, 0x1, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(EmoteId), 0, data, 0x2, sizeof(ushort));
                    Buffer.BlockCopy(conditionName, 0, data, 0x4, conditionNameLength);
                    break;
                case ServerOpcode.PushEventCircle:
                    data = new byte[0x38];
                    Buffer.BlockCopy(BitConverter.GetBytes(Radius), 0, data, 0, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(id), 0, data, 0x04, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(0x41200000), 0, data, 0x08, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(0), 0, data, 0x0c, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(Direction), 0, data, 0x10, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(0), 0, data, 0x11, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(Silent), 0, data, 0x12, sizeof(byte));
                    Buffer.BlockCopy(conditionName, 0, data, 0x13, conditionNameLength);
                    break;
                case ServerOpcode.PushEvenFan:
                    data = new byte[0x40];
                    Buffer.BlockCopy(BitConverter.GetBytes(Radius), 0, data, 0, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(id), 0, data, 0x04, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(Radius), 0, data, 0x08, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(Direction), 0, data, 0x10, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(Priority), 0, data, 0x11, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(Silent), 0, data, 0x12, sizeof(byte));
                    Buffer.BlockCopy(conditionName, 0, data, 0x13, conditionNameLength);
                    break;
                case ServerOpcode.PushEventTriggerBox:
                    data = new byte[0x40];
                    Buffer.BlockCopy(BitConverter.GetBytes(BgObjectId), 0, data, 0, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(LayoutId), 0, data, 0x4, sizeof(uint));
                    Buffer.BlockCopy(BitConverter.GetBytes(ActorId), 0, data, 0x8, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(Direction), 0, data, 0x14, sizeof(byte));
                    Buffer.BlockCopy(conditionName, 0, data, 0x17, conditionNameLength);
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(ReactionName), 0, data, 0x38, ReactionName.Length);
                    break;
                case ServerOpcode.NoticeEvent:
                case ServerOpcode.TalkEvent:
                default:
                    Buffer.BlockCopy(BitConverter.GetBytes(Priority), 0, data, 0, sizeof(byte));
                    Buffer.BlockCopy(BitConverter.GetBytes(Silent), 0, data, 0x1, sizeof(byte));
                    Buffer.BlockCopy(conditionName, 0, data, 0x2, conditionNameLength);
                    break;
            }

            Packet.Send(Opcode, data, id);
        }
    }
}
