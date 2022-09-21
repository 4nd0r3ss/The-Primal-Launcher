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

namespace PrimalLauncher
{
    public class BattleManager
    {
        private static BattleManager _instance = null;
        private static readonly object _padlock = new object();
        public GroupDuty DutyGroup { get; set; }
        public GroupParty PartyGroup { get; set; }
        public GroupMob MobGroup { get;set; }

        private Queue<Tuple<uint, string, int>> ActionQueue { get; set; }

        public bool IsEngaged { get; set; }

        public static BattleManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new BattleManager();

                    return _instance;
                }
            }
        }

        public void Process()
        {
            if (IsEngaged)
            {
                if (!User.Instance.Character.IsTutorialComplete)
                {

                }
                else { }


                    if (DutyGroup != null)
                {
                    DutyGroup.BattleBeat();
                }
                else
                {
                    //what to do when it's party group vs mob group

                    
                }
                    
            }
        } 

        public void Engage(ActorBattle attacker)
        {
            if (!IsEngaged)
            {
                IsEngaged = true;

                //if we are in tutorial, do not change BGM.
                if (!User.Instance.Character.IsTutorialComplete)
                {
                    Zone zone = World.Instance.GetZone(attacker.Position.ZoneId);
                    zone.ToggleBattleMusic();
                }                

                if (DutyGroup != null)
                {
                    DutyGroup.Engage(attacker);
                }
                else
                {
                    //if it's not a duty group
                    Instance.AddDutyGroup(new List<uint>(), false);
                }
            }            
        }

        public void Disengage()
        {
            IsEngaged = false;
            Log.Instance.Warning("Battle manager disengaged battle.");
        }

        public void AddDutyGroup(List<uint> membersClassId, bool addQuestDirector = false)
        {
            DutyGroup = new GroupDuty();

            if (addQuestDirector)
                DutyGroup.MemberList.Add(((QuestDirector)User.Instance.Character.GetCurrentZone().GetDirector("Quest")).Id);

            DutyGroup.AddMembers(User.Instance.Character.GetCurrentZone().GetActorsByClassId(membersClassId));
            DutyGroup.InitializeGroup();
            DutyGroup.SendPackets();           
            World.Instance.SendData(new object[] { 0x09 });
        }

        public void GetGroupInitWork(byte[] data)
        {
            ulong groupId = 0;

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(stream))
            {
                br.ReadUInt64();
                br.ReadUInt64();
                groupId = br.ReadUInt64();
            }

            if (DutyGroup != null && DutyGroup.Id != groupId)
                DutyGroup.InitWork();
            else if (User.Instance.Character.PartyGroup.Id == groupId)
                User.Instance.Character.PartyGroup.InitWork();
            else if (User.Instance.Character.RetainerGroup.Id == groupId)
                User.Instance.Character.RetainerGroup.InitWork();


        }
    }
}
