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
using System.Threading;

namespace PrimalLauncher
{
    public class BattleManager
    {
        private static BattleManager _instance = null;
        private static readonly object _padlock = new object();
        private long LastTickStamp = Environment.TickCount;       
        private float DeltaTime = 0.005f;
        public int TickIntervalSeconds { get; private set; } = 1000;
        private List<GroupBase> Groups { get; set; } = new List<GroupBase>();

        public bool BattleEngaged { get; set; }

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

        public void Tick()
        {
            var currentTickStamp = Environment.TickCount;

            //we only tick once every second.
            if (currentTickStamp - LastTickStamp >= TickIntervalSeconds)
            {
                LastTickStamp = currentTickStamp;
                Process();
            }

            DeltaTime = currentTickStamp - LastTickStamp;
        }

        private float GetDeltaTime()
        {
            return (DeltaTime / 1000.0f);
        }

        private void Process()
        {
            if (BattleEngaged)
            {
                if (Groups.Count > 0)
                    foreach (var group in Groups)
                        group.BattleBeat();
            }
            else
            {
                Groups.Clear();
            }                        
        } 

        public void StartBattle(ActorBattle attacker)
        {            
            var attackerGroup = attacker.BattleGroup;
            var targetGroup = ((ActorBattle)attacker.GetTargetActor()).BattleGroup;

            EngageGroupMembers(attackerGroup);
            EngageGroupMembers(targetGroup); 

            User.Instance.Character.GetCurrentZone().ToggleBattleMusic();

            Groups.Add(attackerGroup);
            Groups.Add(targetGroup);

            BattleEngaged = true;
        }

        public void StartDuty(List<Actor> members)
        {
            DutyGroup dutyGroup = new DutyGroup();

            dutyGroup.AddMembers(members);
            dutyGroup.InitializeGroup();
            dutyGroup.SendPackets();

            Groups.Add(dutyGroup);
            EngageGroupMembers(dutyGroup);  
            BattleEngaged = true;
        }

        private void EngageGroupMembers(GroupBase group)
        {
            foreach (Actor member in group.MemberList)
                if(member is ActorBattle actorBattle)
                    actorBattle.Engage(0);
        }

        public void Disengage()
        {
            BattleEngaged = false;       
            Log.Instance.Warning("Battle manager disengaged battle.");
        }

        public void AddDutyGroup(List<uint> membersClassId, bool addQuestDirector = false)
        {
            
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

            var group = User.Instance.Character.Groups.FirstOrDefault(x => x.Id == groupId);

            if (group != null)
            {
                group.InitWork();
            }
            else
            {
                //if (DutyGroup != null && DutyGroup.Id != groupId)
                //    DutyGroup.InitWork();
            }           
        }
    }
}
