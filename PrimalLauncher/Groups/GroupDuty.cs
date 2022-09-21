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
    public class GroupDuty : GroupBase
    {        
        public List<Director> DirectorList { get; set; }
        public List<DutyMember> AllyList { get; set; }       
        public List<DutyMember> EnemyList { get; set; }       

        public GroupDuty() : base(0x01, GroupType.Duty)
        {
            _idMask = 0x3000000000000000;
            AllyList = new List<DutyMember>();
            EnemyList = new List<DutyMember>();
        }

        protected override void Members()
        {
            byte[] data = GetPrepByteArray(0x198); //only groups of 8 allowed here. Many more are allowed in linkshell  groups for example, but I chose to limit to 8.

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Seek(0x10, SeekOrigin.Begin);

                foreach (uint member in MemberList)
                {
                    bw.Write(member);
                    bw.Write(0x03E9);  //unknown
                    bw.Write(1);       //unknown
                }

                for (int i = 0; i < (8 - MemberList.Count); i++)
                {
                    //empty member slot
                    bw.Write(0);
                    bw.Write(0);
                    bw.Write(0);
                }

                bw.Write((byte)(MemberList.Count));
            }

            Packet.Send(ServerOpcode.GroupDutyMembers, data);
        }

        public void Engage(ActorBattle attacker)
        {
            //attacker first attack
            attacker.AutoAttack();

            //engage allies
            foreach(DutyMember member in AllyList)
            {
                if(member.Actor != attacker)
                {
                    //TODO: allies should engage based on their role:
                    //healers will target player for healing
                    //attackers will target player target. 
                    //for now I'm hardcoding so that all allies attack player target.
                    member.Actor.Engage(attacker.CurrentTargetId);
                }
            }

            //engage enemies
            foreach (DutyMember member in EnemyList)            
                member.Actor.Engage(attacker.Id); 

            Log.Instance.Warning("Duty group engaged in battle.");
        }

        public void BattleBeat()
        {
            //Log.Instance.Info("GroupDuty.BattleBeat");
            var mylist = EnemyList.Where(x => x.Actor.State.Main != MainState.Dead2);
            bool enemiesDead = !mylist.Any();

            //if all objectives are dead, disengage immediately
            if (enemiesDead)
            {
                BattleManager.Instance.Disengage();

                foreach (DutyMember member in AllyList)
                    member.Actor.Disengage();

                if (!User.Instance.Character.IsTutorialComplete) BattleTutorial.Instance.Finish();
            }                
            else
            {
                foreach(DutyMember member in AllyList)
                {
                    if(!member.Actor.IsDead())
                        member.UpdateActionTimer();
                }

                foreach (DutyMember member in EnemyList)
                {
                    if (!member.Actor.IsDead())
                        member.UpdateActionTimer();   
                }
            }
        }

        /// <summary>
        /// So far this was only used during the intro when changing to instanced area. It will possible be used whenever an instance starts.
        /// </summary>
        /// <param name="sender"></param>
        public void InitializeGroup()
        {
            WorkProperties prop = new WorkProperties(User.Instance.Character.Id, "charaWork/currentContentGroup");
            prop.Add("charaWork.currentContentGroup", Type);           
            prop.FinishWritingAndSend();          
        }

        public override void InitWork()
        {
            byte[] init = new byte[0x28];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, init, 0, sizeof(long));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("/_init"), 0, init, 0x08, 6);
            Packet.Send(ServerOpcode.GeneralData, init);

            byte[] data = new byte[0x90];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Id);
                    bw.Write((byte)0x1A); //# of bytes from here on

                    // unknow property -->
                    bw.Write((byte)0x08); //data type (0x08 == array of ints? or long?)
                    bw.Write(0x33D2BD7B); //hashed string - property name
                    bw.Write(0);          //first value
                    bw.Write(User.Instance.Character.GetCurrentZone().GetDirector("Quest").Id); //director id
                    bw.Write((byte)0x01); //data type - bool
                    bw.Write(0x9627ABD8); //hashed string - property name
                    bw.Write((byte)0x01); // true
                    //<--
                    bw.Write((byte)0x88); //wrapper
                    bw.Write(Encoding.ASCII.GetBytes("/_init"));
                }
            }

            Packet.Send(ServerOpcode.GroupInitWork, data);
        }

        public override void AddMembers(List<Actor> membersToAdd)
        {
            //add player
            MemberList.Add(User.Instance.Character.Id);
            AllyList.Add(new DutyMember{ Actor = User.Instance.Character });

            foreach (Actor a in membersToAdd)
            {
                MemberList.Add(a.Id);

                if (a is Monster monster)
                {
                    if(monster.Family != "fighter")
                    {
                        EnemyList.Add(new DutyMember
                        {
                            Actor = monster,
                            IsObjective = true
                        });
                    }
                    else
                    {
                        //NPC allies are also monster actors(?)
                        AllyList.Add(new DutyMember { Actor = monster });
                    }
                }
                else if(a is Director director) //not sure if this will be useful...
                {
                    DirectorList.Add(director);
                }
            }
        }        
    }

    public class DutyMember
    {
        public ActorBattle Actor { get; set; }
        public int ActionTimer { get; set; }
        public bool IsObjective { get; set; }        

        public void UpdateActionTimer()
        {
            if (!Actor.IsDead())
            {
                ActionTimer += 100;
                Actor.MoveToTarget();

                if (ActionTimer >= Actor.AutoAttackDelay)
                {
                    Log.Instance.Info("Actor: 0x" + Actor.Id.ToString("X2") + ", classid: " + Actor.ClassId + " attacks!");

                    Actor.AutoAttack();
                    ActionTimer = new Random().Next(0, 300); //TODO: need to calculate this based on [weapon delay] and [char attr speed] (?)
                }
            }                    
        }
    }
}
