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

        public Queue<byte[]> PacketQueue { get; set; } //TODO: try to get rid of this.

        public bool[] Property { get; set; } = new bool[0x20];       
        public uint[] StatusShownTime { get; set; }

        public ushort[] Command { get; set; } = new ushort[0x20]; //original size is 0x40. here only the size of commandborder, the rest is hotbar.
        public byte[] CommandCategory { get; set; } = new byte[0x40];
        public bool[] CommandAquired { get; set; } = new bool[0x1000];
        public bool[] AdditionalCommandAquired { get; set; } = new bool[0x24];        

        public OrderedDictionary GeneralParameters { get; set; }

        public Dictionary<byte, Job> Jobs { get; set; }
        public byte CurrentClassId { get; set; }

        public Job CurrentClass
        {
            get
            {
                return Jobs[CurrentClassId];
            }
        }

        public Job CurrentJob
        {
            get
            {
                Job charClass = Jobs[CurrentClassId];

                if (charClass.IsSoulStoneEquippped)
                {

                    return Jobs[GetClassJobId()];
                }
                else
                {
                    return charClass;
                }
            }
        }

        public CharaWork(Actor actor)
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

            if (actor is PlayerCharacter)
            {
                Jobs = Job.LoadAll();
            }
            else
            {
                Jobs = Job.LoadNpcJob();
                CurrentClassId = 0x03;
            }
        }      

        public void AddProperties(byte[] indexes)
        {
            for(int i = 0; i < indexes.Length; i++)
            {
                Property[indexes[i]] = true;
            }
        }

        private void SetStatusShownTime(ref WorkProperties property)
        {
            //status buff/ailment timer? 
            //property.Add(string.Format("charaWork.statusShownTime[{0}]", i), );
        }

        private void SetCommands(ref WorkProperties property)
        {
            property.Add("charaWork.commandBorder", CommandBorder);

            for (int i = 0; i < 64; i++)
                property.Add(string.Format("charaWork.commandCategory[{0}]", i), (byte)1);

            for (int i = 0; i < 4096; i++)
            property.Add(string.Format("charaWork.commandAcquired[{0}]", i), false);

            //job abilities
            for (int i = 0; i < 36; i++)
                property.Add(string.Format("charaWork.additionalCommandAcquired[{0}]", i), true);

            //default internal commands
            for (int i = 0; i < CommandBorder; i++)
                if (Command[i] != 0)
                    property.Add(string.Format("charaWork.command[{0}]", i), 0xA0F00000 | Command[i]);
                       
            SetHotbar(ref property);
        }

        private void AddDohActions(ref WorkProperties property)
        {
            uint[] actions = new uint[] { 0, 0, 0, 0, 0 };

            if (User.Instance.Character.CharaWork.CurrentClass.GetCategory() == JobClassCategory.DoH)
            {
                actions[0] = 0xA0F00000 | 22001;
                actions[1] = 0xA0F00000 | 22501;
                actions[2] = 0xA0F00000 | 22502;
                actions[3] = 0xA0F00000 | 22014;
                actions[4] = 0xA0F00000 | 22016;
            }

            property.Add("charaWork.command[15]", actions[0]);
            property.Add("charaWork.command[17]", actions[1]);
            property.Add("charaWork.command[18]", actions[2]);
            property.Add("charaWork.command[19]", actions[3]);
            property.Add("charaWork.command[20]", actions[4]);
        }

        public void SetHotbar(ref WorkProperties property)
        {                        
            for (int i = 0; i < CurrentClass.Hotbar.Length; i++)
            {
                uint slotValue = CurrentClass.Hotbar[i] != 0 ? 0xA0F00000 | CurrentClass.Hotbar[i] : 0;               
                property.Add(string.Format("charaWork.command[{0}]", CommandBorder + i), slotValue);                            
            }

            AddDohActions(ref property); //couldn't think of a batter place to put this...
        }

        public void UpdateHotbar()
        {
            WorkProperties property = new WorkProperties(User.Instance.Character.Id, @"charaWork/command");
            
            SetHotbar(ref property);

            for (int i = 0; i < 64; i++)
                property.Add(string.Format("charaWork.commandCategory[{0}]", i), (byte)1);

            property.FinishWritingAndSend();
        }

        public void RemoveFromHotbar(int slot)
        {
            WorkProperties property = new WorkProperties(User.Instance.Character.Id, @"charaWork/command");
            property.Add(string.Format("charaWork.command[{0}]", CommandBorder + slot), 0);
            property.Add(string.Format("charaWork.commandCategory[{0}]", CommandBorder + slot), (byte)1);
            property.FinishWritingAndSend();
        }

        private void SetParameterSave(ref WorkProperties property)
        {
            for (int i = 0; i < 40; i++)
                property.Add(string.Format("charaWork.parameterSave.commandSlot_compatibility[{0}]", i), true);

            property.Add("charaWork.parameterSave.hp[0]", CurrentClass.Hp);
            property.Add("charaWork.parameterSave.hpMax[0]", CurrentClass.MaxHp);
            property.Add("charaWork.parameterSave.mp", CurrentClass.Mp);
            property.Add("charaWork.parameterSave.mpMax", CurrentClass.MaxMp);
            
            property.Add("charaWork.parameterSave.state_mainSkill[0]", CurrentClass.Id);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CurrentClass.Level);
            property.Add("charaWork.battleSave.skillPoint[" + (CurrentClassId - 1) + "]", (int)CurrentClass.TotalExp);
        }

        private void SetParameterTemp(ref WorkProperties property)
        {
            property.Add("charaWork.parameterTemp.tp", CurrentClass.Tp);
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[0]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_float_forClientSelf[1]", 1.0f);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[0]", (short)-1);
            property.Add("charaWork.parameterTemp.forceControl_int16_forClientSelf[1]", (short)-1);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[0]", (byte)4);
            property.Add("charaWork.parameterTemp.otherClassAbilityCount[1]", (byte)5);
            property.Add("charaWork.parameterTemp.giftCount[1]", (byte)5);
            property.Add("charaWork.depictionJudge", DepictionJudge);
        }

        private void SetBattleTemp(ref WorkProperties property)
        {
            //Write character's parameters
            for (int i = 0; i < GeneralParameters.Count; i++)
            {
                if ((ushort)GeneralParameters[i] > 0)
                    property.Add(string.Format("charaWork.battleTemp.generalParameter[{0}]", i), GeneralParameters[i]);
            }

            property.Add("charaWork.battleTemp.castGauge_speed[0]", 1.0f);
            property.Add("charaWork.battleTemp.castGauge_speed[1]", 0.25f);
        }

        private void SetBattleSave(ref WorkProperties property)
        {
            property.Add("charaWork.battleSave.potencial", 6.6f);
            property.Add("charaWork.battleSave.negotiationFlag[0]", true);
        }

        private void SetProperties(ref WorkProperties property)
        {
            for (int i = 0; i < 32; i++)
                if (i < 5 && i != 3) property.Add(string.Format("charaWork.property[{0}]", i), (byte)1);
        }

        private void SetEventSave(ref WorkProperties property)
        {
            property.Add("charaWork.eventSave.bazaarTax", (byte)5);
            property.Add("charaWork.eventSave.bazaar", true);
        }

        public void AddToWork(ref WorkProperties property)
        {
            SetStatusShownTime(ref property);
            SetCommands(ref property);
            SetHotbar(ref property);
            SetParameterSave(ref  property);
            SetParameterTemp(ref  property);
            SetBattleTemp(ref  property);
            SetBattleSave(ref  property);
            SetProperties(ref property);
            SetEventSave(ref property);
        }

        public void AddExp(int exp)
        {
            //we want to add exp only if level is below cap.
            if (CurrentClass.Level < CurrentClass.LevelCap)
            {
                //add exp bonus multiplier TODO:put multiplier definition somewhere else (add as an option in UI?)
                float expBonus = 1.2f;
                CurrentClass.TotalExp += Convert.ToInt64(exp * expBonus);

                //send add exp command result
                User.Instance.Character.SendCommandResult(0, new List<CommandResult> {
                    new CommandResult
                    {
                        TargetId = User.Instance.Character.Id,
                        TotalPoints = (short)(exp * expBonus),
                        TextSheetId = 33934,
                        HitPosition = (byte)(expBonus > 1 ? ((expBonus -1) * 100) : 0)
                    }
                });

                //calculate leveling
                long totalExp = CurrentClass.TotalExp;
                short currentLevel = CurrentClass.Level;
                short levelsToUp = 0;

                while (totalExp >= Job.ExpTable[currentLevel])
                {
                    totalExp -= Job.ExpTable[currentLevel];
                    levelsToUp++;
                }

                for(int i = 0; i < levelsToUp; i++)
                {
                    LevelUp();
                }

                //we show 0 exp when player reaches level cap.
                CurrentClass.TotalExp = (currentLevel + levelsToUp) >= CurrentClass.LevelCap ? 0 : totalExp;

                //refresh exp values in game client UI.
                UpdateExp();
            }
        }

        public void UpdateExp()
        {
            WorkProperties prop = new WorkProperties(User.Instance.Character.Id, "charaWork/battleStateForSelf");
            prop.Add("charaWork.battleSave.skillPoint[" + (CurrentClassId - 1) + "]", (int)CurrentClass.TotalExp);
            prop.FinishWritingAndSend();
        }

        private void LevelUp()
        {
            CurrentClass.Level++;

            User.Instance.Character.SendCommandResult(0, new List<CommandResult> {
                new CommandResult
                {
                    TargetId = User.Instance.Character.Id,
                    TotalPoints = CurrentClass.Level,
                    TextSheetId = 33909
                }
            });

            UpdateLevel();
            World.Instance.SetMusic(0x52, MusicMode.Layer);
            User.Instance.Character.Journal.InitializeQuests();
            CurrentClass.AddLevelActionsToHotbar();
            UpdateHotbar();
        }

        public void LevelDown(short toLevel)
        {
            if (toLevel > 0)
            {
                CurrentClass.Level = toLevel;
                CurrentClass.TotalExp = 0;
                UpdateLevel();
                UpdateExp();
            }
        }

        public void UpdateLevel()
        {
            WorkProperties property = new WorkProperties(User.Instance.Character.Id, @"charaWork/stateForAll");
            property.Add("charaWork.battleSave.skillLevel[" + (CurrentClassId - 1) + "]", CurrentClass.Level);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CurrentClass.Level);
            property.FinishWritingAndSend();
        }        

        public int GetCurrentLevel()
        {
            return CurrentClass.Level;
        }

        public void UpdateClass()
        {
            WorkProperties property = new WorkProperties(User.Instance.Character.Id, @"charaWork/stateForAll");
            property.Add("charaWork.parameterSave.state_mainSkill[0]", CurrentClassId);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", CurrentClass);
            property.FinishWritingAndSend();
        }

        /// <summary>
        /// Sent on data request
        /// </summary>
        /// <returns></returns>
        public byte[] Exp()
        {
            if (PacketQueue == null || PacketQueue.Count == 0)
            {
                User.Instance.Character.Inventory.Update();

                Queue<short> jobLevel = new Queue<short>();
                Queue<short> jobLevelCap = new Queue<short>();
                int count = 0;

                foreach (var item in Jobs)
                {
                    count++;
                    if (count > 52)
                        break;
                    Job job = item.Value;
                    jobLevel.Enqueue(job.Level);
                    jobLevelCap.Enqueue(job.LevelCap);
                }

                WorkProperties property = new WorkProperties(User.Instance.Character.Id, @"charaWork/exp");
                property.Add("charaWork.battleSave.skillLevel", jobLevel);
                property.Add("charaWork.battleSave.skillLevelCap", jobLevelCap, true);
                PacketQueue = property.PacketQueue;
            }

            return PacketQueue.Dequeue();
        }

        public static void CommandSequence()
        {
            List<KeyValuePair<uint, string>> commands = new List<KeyValuePair<uint, string>>
            {
                new KeyValuePair<uint, string>(0x0b, "commandForced"),
                new KeyValuePair<uint, string>(0x0a, "commandDefault"),
                new KeyValuePair<uint, string>(0x06, "commandWeak"),
                new KeyValuePair<uint, string>(0x04, "commandContent"),
                new KeyValuePair<uint, string>(0x06, "commandJudgeMode"),
                new KeyValuePair<uint, string>(0x100, "commandRequest"),
                new KeyValuePair<uint, string>(0x100, "widgetCreate"),
                new KeyValuePair<uint, string>(0x100, "macroRequest"),
            };

            foreach (var command in commands)
            {
                byte[] data = new byte[0x28];
                data.Write(0, command.Key);
                data.Write(0x02, command.Value);
                Packet.Send(ServerOpcode.PlayerCommand, data);
            }
        }

        public byte GetClassJobId()
        {
            byte jobIndex = 13;

            if (CurrentClassId == 7 || CurrentClassId == 8)
                jobIndex = 11;
            else if (CurrentClassId == 22 || CurrentClassId == 23)
                jobIndex = 4;

            return (byte)(CurrentClassId + jobIndex);
        }

        public int GetHotbarSlot(ushort value)
        {          
            for (int i = 0; i < CurrentClass.Hotbar.Length; i++)
            {
                if (CurrentClass.Hotbar[i] == value)
                    return i;
            }

            return -1;
        }

        public void SetCommandRecast(uint actorId, float recastTime, int slot)
        {
            WorkProperties prop = new WorkProperties(actorId, "charaWork/commandDetailForSelf");
            prop.Add("charaWork.parameterTemp.maxCommandRecastTime[" + slot + "]", (short)recastTime);
            prop.Add("charaWork.parameterSave.commandSlot_recastTime[" + slot + "]", Server.GetTimeStamp(recastTime));
            prop.FinishWritingAndSend();
        }

        public void SetComboAction(uint actorId, uint nextCommandId)
        {
            WorkProperties prop = new WorkProperties(actorId, "playerWork/combo");
            prop.Add("playerWork.comboNextCommandId[0]", (short)nextCommandId);
            prop.Add("playerWork.comboCostBonusRate", 0x3F800000); //float?
            prop.FinishWritingAndSend();
        }
    }   
}
