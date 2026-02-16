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
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace PrimalLauncher
{
    public class LinkshellGroup : GroupBase
    {        
        public int Crest { get; set; }
        public int Master { get; set; }
        public int Rank { get; set; }

        public LinkshellGroup(byte[] data) : base(GroupType.Linkshell)
        {
            //user selections on linkshell creation dialog.
            var parameters = LuaParameters.ReadParameters(data, 0x21);
            Master = (int)parameters[0];
            Name = (string)parameters[1];
            Crest = (int)parameters[2];

            Rank = 0x0a;
        }

        public override void InitWork()
        {
            //byte[] data = new byte[0xB0];

            //using (MemoryStream ms = new MemoryStream(data))
            //{
            //    using (BinaryWriter bw = new BinaryWriter(ms))
            //    {
            //        bw.Write(Id);
            //        bw.Write((uint)0x039d0814);
            //        bw.Write((uint)0x6f929dce);
            //        bw.Write((ushort)0x00b3);
            //        bw.Write((uint)User.Instance.Character.Id);
            //        bw.Write((byte)0x88);
            //        bw.Write(Encoding.ASCII.GetBytes("/_init"));
            //    }
            //}




            WorkProperties properties = new WorkProperties(User.Instance.Character.Id, @"/_init");

            properties.Add("work._globalSave.master", (ulong)((0xB36F92 << 8) | User.Instance.Character.Id));
            properties.Add("work._globalSave.crestIcon[0]", (ushort)Crest);
            properties.Add("work._globalSave.rank", 1);

            for(int i=0; i< MemberList.Count; i++)
            {
                properties.Add("work._memberSave[{0}].rank", i);
            }

            properties.FinishWritingAndSend(opcode: ServerOpcode.GroupLinkshellWork);
            //SynchGroupWorkValuesPacket groupWork = new SynchGroupWorkValuesPacket(groupIndex);
            //groupWork.addProperty(this, "work._globalSave.master");
            //groupWork.addProperty(this, "work._globalSave.crestIcon[0]");
            //groupWork.addProperty(this, "work._globalSave.rank");

            //for (int i = 0; i < members.Count; i++)
            //{
            //    work._memberSave[i].rank = members[i].rank;
            //    groupWork.addProperty(this, String.Format("work._memberSave[{0}].rank", i));
            //}

            //groupWork.setTarget("/_init");
            //SubPacket test = groupWork.buildPacket(session.sessionId);
            //test.DebugPrintSubPacket();
            //session.clientConnection.QueuePacket(test);
        }

        public void SetActive()
        {
            byte[] data = new byte[0x68];
            data.Write(0, Id);
            data.Write(0x40, 0x4e22.GetBytes());
            data.Write(0x60, 1.GetBytes());

            Packet.Send(ServerOpcode.ActiveLinkshell, data);
        }
    }
}
