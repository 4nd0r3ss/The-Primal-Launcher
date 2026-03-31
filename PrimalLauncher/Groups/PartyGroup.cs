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

namespace PrimalLauncher
{
    [Serializable]
    public class PartyGroup : GroupBase
    {
        public List<BattleGroupMember> BattleGroupMembers { get; set; }
        public PartyGroup() : base(GroupType.Party)
        {
            MemberList = new List<Actor>
            {
                User.Instance.Character
            };

            BattleGroupMembers = new List<BattleGroupMember>();
        }

        public override void InitWork()
        {
            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Id);
                    bw.Write((uint)0x039d0814);
                    bw.Write((uint)0x6f929dce);
                    bw.Write((ushort)0x00b3);
                    bw.Write((uint)User.Instance.Character.Id);
                    bw.Write((byte)0x88);
                    bw.Write(Encoding.ASCII.GetBytes("/_init"));
                }
            }

            Packet.Send(ServerOpcode.GroupInitWork, data);
        }

        public void LoadBattleMembers()
        {
            foreach (ActorBattle member in MemberList)
            {
                BattleGroupMembers.Add(new BattleGroupMember { Actor = member });
            }
        }

        public override void BattleBeat()
        {
            if(BattleGroupMembers == null)
                BattleGroupMembers = new List<BattleGroupMember>();

            if (BattleGroupMembers.Count == 0)
                LoadBattleMembers();

            //Log.Instance.Info("GroupDuty.BattleBeat");
            var mylist = BattleGroupMembers.Where(x => x.Actor.State.Main != MainState.Dead2); //get all members who are not dead

            //if all members are dead, disengage immediately
            if (!mylist.Any())
            {
                BattleManager.Instance.Disengage();
            }
            else
            {
                foreach (BattleGroupMember member in BattleGroupMembers)
                {
                    if (!member.Actor.IsDead())
                        member.UpdateActionTimer();
                }
            }
        }



    }
}
