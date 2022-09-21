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
using System.Collections.Specialized;

namespace PrimalLauncher
{
    [Serializable]
    public class CharaWork
    {
        public readonly uint DepictionJudge = 0xA0F50911;
        public readonly byte CommandBorder = 0x20;

        public bool[] Property { get; set; } = new bool[0x20];       
        public uint[] StatusShownTime { get; set; }

        public ushort[] Command { get; set; } = new ushort[0x20]; //original size is 0x40. here only the size of commandborder, the rest is hotbar.
        public byte[] CommandCategory { get; set; } = new byte[0x40];
        public bool[] CommandAquired { get; set; } = new bool[0x1000];
        public bool[] AdditionalCommandAquired { get; set; } = new bool[0x24];        

        public OrderedDictionary GeneralParameters { get; set; }

        public Dictionary<byte, Job> Jobs { get; set; }
        public byte CurrentClassId { get; set; }

        public Job CurrentJob
        {
            get
            {
                return Jobs[CurrentClassId];
            }
        }

        public CharaWork()
        {
            //descriptions from game data file. 
            Command[0x00] = 21001; //active mode
            Command[0x01] = 21001; //active mode
            Command[0x02] = 21002; //passive mode
            Command[0x03] = 12004; //Begin the designated Battle Regimen.
            Command[0x04] = 21005; //Cast magic quickly at reduced potency.
            Command[0x05] = 21006; //Stop casting a spell.
            Command[0x06] = 21007; //use item
            Command[0x07] = 12009; //equip items
            Command[0x08] = 12010; //set abilities
            Command[0x09] = 12005; //attribute points
            Command[0x0A] = 12007; //skill change
            Command[0x0B] = 12011; //Place marks on enemies to coordinate your party's actions.
            Command[0x0C] = 22012; //Bazaar
            Command[0x0D] = 22013; //Repair
            Command[0x0E] = 29497; //Engage in competitive discourse to win what you seek.
            Command[0x0F] = 22015; //[no description] 
        }

        public void AddNpcJob()
        {
            Jobs = new Dictionary<byte, Job>();
            Job npcJob = Job.NpcJob();
            CurrentClassId = npcJob.Id;
            Jobs.Add(npcJob.Id, npcJob);
        }

        public void AddProperties(byte[] indexes)
        {
            for(int i = 0; i < indexes.Length; i++)
            {
                Property[indexes[i]] = true;
            }
        }

        public void AddStatusShownTime(ref WorkProperties property)
        {
            //status buff/ailment timer? database.cs ln 892
            //property.Add(string.Format("charaWork.statusShownTime[{0}]", i), );
        }

        public void AddGeneralParameters(ref WorkProperties property)
        {
            //Write character's parameters
            for (int i = 0; i < GeneralParameters.Count; i++)
            {
                if ((ushort)GeneralParameters[i] > 0)
                    property.Add(string.Format("charaWork.battleTemp.generalParameter[{0}]", i), GeneralParameters[i]);
            }
        }

        public void AddWorkCommands(ref WorkProperties property)
        {
            for (int i = 0; i < CommandBorder; i++)
                if (Command[i] != 0)
                    property.Add(string.Format("charaWork.command[{0}]", i), 0xA0F00000 | Command[i]);

            AddHotbar(ref property);            
        }

        private void AddHotbar(ref WorkProperties property)
        {
            CurrentJob.Hotbar[1] = 27181;
            CurrentJob.Hotbar[2] = 27193;
            CurrentJob.Hotbar[3] = 27182;
            CurrentJob.Hotbar[4] = 27191;

            //hotbar
            // for(int i = CharaWork.CommandBorder; i < (CharaWork.CommandBorder + CurrentJob.Hotbar.Length); i++)
            for (int i = 0; i < CurrentJob.Hotbar.Length; i++)
            {
                if (CurrentJob.Hotbar[i] != 0)
                    property.Add(string.Format("charaWork.command[{0}]", CommandBorder + i), 0xA0F00000 | CurrentJob.Hotbar[i]);
            }
            //add hotbar here
            //if (i >= commandBorder)
            //{
            //    property.Add(string.Format("charaWork.parameterTemp.maxCommandRecastTime[{0}]", i - commandBorder), (ushort)5);
            //    property.Add(string.Format("charaWork.parameterSave.commandSlot_recastTime[{0}]", i - commandBorder), (uint)(Server.GetTimeStamp() + 5));
            //}
        }

        public void AddWorkClassParameters(ref WorkProperties property)
        {
            property.Add("charaWork.parameterSave.hp[0]", CurrentJob.Hp);
            property.Add("charaWork.parameterSave.hpMax[0]", CurrentJob.MaxHp);
            property.Add("charaWork.parameterSave.mp", CurrentJob.Mp);
            property.Add("charaWork.parameterSave.mpMax", CurrentJob.MaxMp);
            property.Add("charaWork.parameterTemp.tp", CurrentJob.Tp);
            property.Add("charaWork.parameterSave.state_mainSkill[0]", CurrentJob.Id);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CurrentJob.Level);
            property.Add("charaWork.battleSave.skillPoint[" + (CurrentClassId - 1) + "]", (int)CurrentJob.TotalExp);
        }

        public void AddWorkSystem(ref WorkProperties property)
        {
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[0]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[1]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[0]", (short)-1);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[1]", (short)-1);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[0]", (byte)4);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[1]", (byte)5);
            property.Add("charaWork.parameterTemp.giftCount[1]", (byte)5);
            property.Add("charaWork.depictionJudge", DepictionJudge);
        }

       
    }   
}
