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
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace PrimalLauncher
{
    [Serializable]
    public class Job
    {
        public byte Id { get; set; }
        public string Name { get; set; }       
        public bool IsCurrent { get; set; } //the class the player is using 
        public bool IsSoulStoneEquippped { get; set; }

        public short Level { get; set; } 
        public short LevelCap { get; set; }         
        public long TotalExp { get; set; }      

        public short MaxHp { get; set; }
        public short MaxMp { get; set; }
        public short MaxTp { get; set; }

        public short Hp { get; set; }
        public short Mp { get; set; }
        public short Tp { get; set; }

        public int AutoAttackMaxDistance { get; set; }

        public ushort[] Hotbar { get; set; }

        public List<ActionCommand> Actions { get; set; }
               
        public Job(byte id, string name, short maxLevel)
        {
            Id = id;
            Name = name;
            IsCurrent = false;            

            Level = 0; //if the player doesn't have a class, then level is 0.
            LevelCap = maxLevel; //put these default number somewhere else
           
            MaxHp = 300;
            MaxMp = 200;
            MaxTp = 3000;

            //can use these fields to start the character with less HP, MP or TP.
            Hp = MaxHp;
            Mp = MaxMp;
            Tp = 0;

            AutoAttackMaxDistance = 4;

            Hotbar = new ushort[30];  
            Actions = new List<ActionCommand>();
        }

        private void AddToHotbarEmptySlot(ushort actionId)
        {
            for (int i = 0; i < Hotbar.Length; i++)
            {
                if (Hotbar[i] == 0)
                {
                    Hotbar[i] = actionId; 
                    break;
                }                    
            }
        }

        public static Dictionary<byte, Job> LoadNpcJob()
        {
            Job job = new Job(0x03, "Npc", 99)
            {
                MaxHp = 0x01F4,
                Hp = 0x01F4,
                MaxMp = 0,
                Mp = 0,
                Tp = 0,
                Level = 2
            };

            ActionCommand action = new ActionCommand
            {
                Id = Command.MonsterAutoAttack,
                AnimationId = 0x11001000,
                TextSheet = 0x765D
            };

            job.Actions.Add(action);

            return new Dictionary<byte, Job> {{ 0x03, job }};
        }

        public JobClassCategory GetCategory()
        {
            ushort category = Category.FirstOrDefault(x => x.Value == Id).Key;

            if (category < 500 || Name == "Npc")
                return JobClassCategory.DoW;
            else if (category > 500 && category < 600)
                return JobClassCategory.DoM;
            else if (category > 600 && category < 700)
                return JobClassCategory.DoH;
            else
                return JobClassCategory.DoL;                    
        }

        public JobClassRole GetRole()
        {
            switch (Id)
            {
                case 27:
                    return JobClassRole.Healer;
                case 3:
                case 4:
                    return JobClassRole.Tank;
                default:
                    return JobClassRole.Attacker;
            }
        }
      
        public short EnmityGenerated()
        {
            //temporary while I don't implement real calculations
            switch (GetRole())
            {
                case JobClassRole.Healer:
                    return 5;
                case JobClassRole.Tank:
                    return 8;
                default:
                    return 3;
            }
        }

        public static Dictionary<byte, Job> LoadAll()
        {
            Dictionary<byte, Job> jobs = new Dictionary<byte, Job>();            

            try
            {
                //get job info from game data
                DataTable jobsTable = GameData.Instance.GetGameData("xtx/text_jobName"); 

                //couldn't find a way in the game's files to say which ones of the jobs/classes are disabled, so using this for now
                List<uint> disabledJobs = new List<uint>
                {
                    1, 9, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 25, 26, 27, 28, 37, 
                    38, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58
                };                

                foreach (DataRow row in jobsTable.Rows)
                {
                    uint jobId = (uint)row.ItemArray[0];
                    short maxLevel = (short)(disabledJobs.Any(x => x == jobId) ? 255 : 50);
                    string jobName = (string)row.ItemArray[1];
                    Job job = new Job((byte)jobId, jobName, maxLevel);
                    job.LoadActions();    
                    jobs.Add((byte)jobId, job);
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message);
            }

            return jobs;
        }  
        
        public void LoadActions()
        {
            DataTable jobActionsTable = GameData.Instance.GetGameData("gameCommandBasic");
            XmlDocument jobActionList = new XmlDocument();
            jobActionList.LoadFromResource("JobActionList.xml");
            XmlNodeList jobActions = jobActionList.SelectNodes("//JobAction[@class=" + Id + "]");

            //add auto-attack action
            Actions.Add(new ActionCommand
            {
                Id = Command.PlayerAutoAttack,
                AnimationId = 0x19001000,
                TextSheet = 0x765D
            });

            //add actions from xml
            foreach (XmlNode node in jobActions)
            {
                uint actionId = node.GetAttributeAsUint("id");
                DataRow[] jobActionSearch = jobActionsTable.Select("id = '" + actionId + "'");
                DataRow jobActionData = jobActionSearch != null && jobActionSearch.Length > 0 ? jobActionSearch[0] : null;
                Actions.Add(new ActionCommand(node, jobActionData));

                //if action required level is 1, we add it to the hotbar.
                if (jobActionData.ItemArray[3].ToString() == "1")
                    AddToHotbarEmptySlot(Convert.ToUInt16(jobActionData.ItemArray[0]));
            }
        }

        public void AddLevelActionsToHotbar()
        {
            List<ActionCommand> actions = Actions.Where(x => x.LevelRequired == Level).ToList();

            foreach (ActionCommand action in actions)
            {
                AddToHotbarEmptySlot((ushort)action.Id);
            }            
        }

        public static long[] ExpTable = {
                0, 570, 700, 880, 1100, 1500, 1800, 2300, 3200, 4300, 5000,
                5900, 6800, 7700, 8700, 9700, 11000, 12000, 13000, 15000, 16000,
                20000, 22000, 23000, 25000, 27000, 29000, 31000, 33000, 35000, 38000,
                45000, 47000, 50000, 53000, 56000, 59000, 62000, 65000, 68000, 71000,
                74000, 78000, 81000, 85000, 89000, 92000, 96000, 100000, 100000, 110000
        };

        public static Dictionary<byte, ushort> ExpTextIds = new Dictionary<byte, ushort>
        {
            { 2, 33934 },   //Pugilist
            { 3, 33935 },   //Gladiator
            { 4, 33936 },   //Marauder
            { 7, 33937 },   //Archer
            { 8, 33938 },   //Lancer
            { 10, 33939 },  //Sentinel, this doesn't exist but it's still in the files so may as well put it here just in case
            { 22, 33940 },  //Thaumaturge
            { 23, 33941 },  //Conjurer
            { 29, 33945 },  //Carpenter, for some reason there's a a few different messages between 33941 and 33945
            { 30, 33946 },  //Blacksmith
            { 31, 33947 },  //Armorer
            { 32, 33948 },  //Goldsmith
            { 33, 33949 },  //Leatherworker
            { 34, 33950 },  //Weaver
            { 35, 33951 },  //Alchemist
            { 36, 33952 },  //Culinarian
            { 39, 33953 },  //Miner
            { 40, 33954 },  //Botanist
            { 41, 33955 }   //Fisher
        };

        //TODO: find a better way to do this.
        public static Dictionary<ushort, byte> Category = new Dictionary<ushort, byte>
        {
            {402, 2}, {403, 3},{404, 4},{407, 7},{408, 8},                                      //Disciples of War
            {502, 22},{503, 23},                                                                //Disciples of Magic
            {601, 29},{602, 30}, {603, 31},{604, 32},{605, 33},{606, 34},{607, 35},{608, 36},   //Disciples of the Hand
            {701, 39},{702, 40},{703, 41}                                                       //Disciples of the Land
        };

        public static AnimationEffect AnimationEffectId(byte jobId)
        {
            switch (jobId)
            {
                case 0x0f:
                    return AnimationEffect.ChangeTo_MNK;
                case 0x10:
                    return AnimationEffect.ChangeTo_PAL;
                case 0x11:
                    return AnimationEffect.ChangeTo_WAR;
                case 0x12:
                    return AnimationEffect.ChangeTo_BRD;
                case 0x13:
                    return AnimationEffect.ChangeTo_DRG;
                case 0x1a:
                    return AnimationEffect.ChangeTo_BLM;
                case 0x1b:
                    return AnimationEffect.ChangeTo_WHM;
                default:
                    return 0;
            }
        }

        public void ChangeHotbar(byte[] data)
        {            
            int paramsIndex = data.IndexOfString("commandForced") + 0x20;
            var parameters = LuaParameters.ReadParameters(data, paramsIndex);
            int slot = ((int)parameters[0]) - 1;
            
            if(slot < 0) //additional action item clicked
            {
                 
            }
            else 
            {
                ushort commandId = (ushort)(int)parameters[1];
                Hotbar[slot] = commandId;

                //00-09 hotbar 1
                //10-19 hotbar 2
                //20-29 hotbar 3

                if (commandId == 0) //double-clicked hotbar item to remove it
                {
                    User.Instance.Character.CharaWork.RemoveFromHotbar(slot);
                }                 
                else
                {
                    int oldSlot = Array.IndexOf(Hotbar, commandId);

                    if (oldSlot >= 0)
                        Hotbar[oldSlot] = 0;

                    User.Instance.Character.CharaWork.UpdateHotbar();
                }
            }
        }
    }
}
