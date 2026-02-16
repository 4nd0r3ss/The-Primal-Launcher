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
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class MonsterGroup : GroupBase
    {
        public uint ZoneId { get; set; }    
        public long SequenceId { get; set; }
        public List<Monster> Monsters { get; set; }
        public List<BattleGroupMember> BattleGroupMembers { get; set; }
        public bool IsEngaged { get; set; }

        public MonsterGroup(uint zoneId) : base(GroupType.Monster)
        {
            Random rng = new Random();
            SequenceId = ((long)rng.Next() << 32) | (uint)rng.Next(); //get a random long for packet sequence.
            ZoneId = zoneId;   
            Monsters = new List<Monster>();
            BattleGroupMembers = new List<BattleGroupMember>();
        }

        private void Header()
        {
            byte[] data = GetPrepByteArray(0x80);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write((ulong)ZoneId); 
                bw.Write(SequenceId);
                bw.Write((ulong)0);        
                bw.Write((ulong)0); //some other id. player group owning this one, set by occupancy packet?
                bw.Write((ulong)0);        
                bw.Write(Id);
                bw.Write((long)Type);      
                bw.Write((ulong)0); //owner group id?
                bw.Write(-1);
                bw.Write(new byte[0x20]); //0x20 max name size, this is just a placeholder as no names were found yet. //bw.Write(Name.GetBytes());
                bw.Write((int)0); //unknown
                bw.Write((int)0); //unknown
                bw.Write((int)0); //unknown
                bw.Write((int)0); //unknown                
                bw.Write((byte)(Monsters.Count));
            }

            Packet.Send(ServerOpcode.GroupHeader, data);
        }

        private void Begin()
        {
            byte[] data = GetPrepByteArray(0x20);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write((ulong)ZoneId);
                bw.Write(SequenceId);
                bw.Write(Id);                             
                bw.Write((byte)(Monsters.Count));
            }

            Packet.Send(ServerOpcode.GroupBegin, data);
        }

        private void Body()
        {
            byte[] data = GetPrepByteArray(0x80);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write((ulong)ZoneId);
                bw.Write(SequenceId);

                foreach (Monster monster in Monsters)
                {
                    bw.Write(monster.Id);
                    bw.Write(monster.NameId);
                    bw.Write(1);
                }

                bw.Write(0x45606e24);
                bw.Write(0x0193);
                bw.Write(1);

                bw.Seek(0x70, SeekOrigin.Begin);
                bw.Write((byte)(Monsters.Count));
            }

            Packet.Send(ServerOpcode.GroupMembers, data);
        }

        private void End()
        {
            byte[] data = GetPrepByteArray(0x18);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write((ulong)ZoneId);
                bw.Write(SequenceId);               
                bw.Write(Id);                
            }

            Packet.Send(ServerOpcode.GroupEnd, data);
        }

        public void SetOccupancy(long occupierGroupId)
        {
            byte[] data = GetPrepByteArray(0x40);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write(Id);
                bw.Write((long)Type);
                bw.Write(occupierGroupId);
                bw.Write(-1);
            }

            Packet.Send(ServerOpcode.GroupOccupancy, data);
        }

        public void LoadBattleMembers()
        {
            foreach(Monster member in MemberList)
            {
                BattleGroupMembers.Add(new BattleGroupMember { Actor = member });
            }
        }

        public override void BattleBeat()
        {
            if (BattleGroupMembers.Count == 0)
                LoadBattleMembers();

            //Log.Instance.Info("GroupDuty.BattleBeat");
            var mylist = BattleGroupMembers.Where(x => x.Actor.State.Main != MainState.Dead2); //get all enemies who are not dead
            
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

        public void Send()
        {
            Header();
            Begin();
            Body();
            End();
        }
    }
}
