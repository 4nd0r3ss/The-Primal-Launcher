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
using System.Linq;

namespace PrimalLauncher
{
    [Serializable]
    public class Journal
    {
        public Dictionary<sbyte, object> Quests { get; set; }
        public List<Quest> QuestsAvailable { get; set; }
        public List<uint> QuestsFinished { get; set; }
        public List<uint> GuildLevesLocal { get; set; }
        public List<uint> GuildLevesRegional { get; set; }
        public List<uint> GuildLevesDone { get; set; }
        public List<uint> GuildLevesChecked { get; set; }

        public Journal(uint initialTown)
        {
            QuestsAddEmptySlots();
            QuestsAvailable = new List<Quest>();
            QuestsFinished = new List<uint>();
            GuildLevesLocal = new List<uint>();
            GuildLevesRegional = new List<uint>();
            GuildLevesDone = new List<uint>();
            GuildLevesChecked = new List<uint>();

            //get initial quest
            Quests[0] = QuestRepository.GetFirstQuest(initialTown);
        }

        public void AddToWork(ref WorkProperties work)
        {
            for (sbyte i = 0; i < 0x10; i++)   
                if(Quests[i] != null)
                    work.Add(string.Format("playerWork.questScenario[{0}]", i), 0xA0F00000 | ((Quest)Quests[i]).Id);
            
                

            //GuildLevesLocal[0] = 120242;
            //GuildLevesLocal[1] = 12483;
            //GuildLevesLocal[2] = 12484;

            //GuildLevesRegional.Add(10922);
            //GuildLevesDone.Add(10923);
            //GuildLevesChecked.Add(10924);


            //GuildLeve - local
            for (int i = 0; i < GuildLevesLocal.Count; i++)
                work.Add(string.Format("playerWork.questGuildleve[{0}]", i), GuildLevesLocal[i]);

            //GuildLeve - regional
            for (int i = 0; i < GuildLevesRegional.Count; i++)
                work.Add(string.Format("work.guildleveId[{0}]", i), GuildLevesRegional[i]);

            for (int i = 0; i < GuildLevesDone.Count; i++)
                work.Add(string.Format("work.guildleveDone[{0}]", i), GuildLevesDone[i]);

            for (int i = 0; i < GuildLevesChecked.Count; i++)
                work.Add(string.Format("work.guildleveChecked[{0}]", i), GuildLevesChecked[i]);
            
        }

        private void QuestsAddEmptySlots()
        {
            if (Quests == null)
                Quests = new Dictionary<sbyte, object>();

            for (int i = 0; i < 0x10; i++)
                Quests.Add((sbyte)i, null);
        }

        /// <summary>
        /// Returns the first empty slot in an inventory this is necessary to keep the continuity
        /// </summary>
        /// <param name="inventory"></param>
        /// <returns></returns>
        private sbyte GetFirstEmptySlot()
        {
            foreach (var slot in Quests)
                if (slot.Value == null) return slot.Key;

            return 0; //no empty slots
        }

        #region Quests
        public Quest GetQuestById(uint id)
        {
            Quest quest = null; 

            foreach (var slot in Quests)        
                if(slot.Value != null && ((Quest)slot.Value).Id == id)               
                    quest = (Quest)slot.Value;

            if(quest ==null)
                quest = QuestsAvailable.FirstOrDefault(x => x.Id == id);

            return quest;
        }

        public sbyte GetQuestSlot(uint id)
        {
            foreach (var slot in Quests)
            {
                if (slot.Value != null)
                {
                    Quest quest = (Quest)slot.Value;

                    if (quest.Id == id)
                        return slot.Key;
                }                
            }

            return 0;
        }

        public List<Quest> GetAllQuests()
        {
            List<Quest> quests = new List<Quest>();

            foreach (var slot in Quests)
                if (slot.Value != null)                
                    quests.Add((Quest)slot.Value);                

            quests.AddRange(QuestsAvailable);

            return quests;
        }

        public bool HasQuest(uint questId)
        {
            if(GetQuestById(questId) != null) //found in active quests
                return true;

            if (QuestsAvailable.FirstOrDefault(x => x.Id == questId) != null)
                return true;

            if (QuestsFinished.FirstOrDefault(x => x == questId) > 0) //found in finished quests
                return true;                

            return false;
        }

        public void AcceptQuest(uint id)
        {
            Quest quest = QuestsAvailable.FirstOrDefault(x => x.Id == id);
            quest.Accepted = true;

            Quests[GetFirstEmptySlot()] = quest;
            QuestsAvailable.Remove(quest);

            AddQuestUpdate(id);
        }
        
        public void AddQuest(uint id)
        {
            Quest quest = QuestRepository.GetMainScenarioQuest(id);
            Quests[GetFirstEmptySlot()] = quest;

            AddQuestUpdate(id);
        }

        private void AddQuestUpdate(uint id, bool isFinished = false)
        {
            WorkProperties work = new WorkProperties(User.Instance.Character.Id, "playerWork/journal");
            sbyte slot = GetQuestSlot(id);
            uint questActor = 0;

            if (!isFinished)
            {
                questActor = 0xA0F00000 | id;
                World.SendTextSheet(0x6288, new object[] { (int)id });
                World.Instance.ShowAttentionDialog(new object[] { 0x6288, (int)id });
            }

            work.Add("playerWork.questScenario[" + slot + "]", questActor);
            work.SendUpdate();
        }

        public void UpdateQuest(uint questId, int index = 0)
        {
            Quest quest = GetQuestById(questId);

            if (index == 0)
                quest.HistoryIndex++;
            else
                quest.HistoryIndex = index;

            Log.Instance.Warning("Quest history index: " + quest.HistoryIndex);
            World.SendTextSheet(0x621C, new object[] { (int)questId });
        }

        public void GetQuestData(byte[] packet, ref LuaParameters toSend)
        {
            List<object> parameters = LuaParameters.ReadParameters(packet, 0x41);
            uint questId = Convert.ToUInt32(parameters[0]);
            byte[] data = new byte[0xc0];
            Quest quest = GetQuestById(questId);
            toSend.Add("requestedData");

            if(parameters[1] == null)
            {
                toSend.Add("qtdata");
                toSend.Add(questId);
                toSend.Add(quest.HistoryIndex);
            }
            else
            {
                int option = Convert.ToInt32(parameters[1]);

                //put this swtich here as maybe there are other options...
                switch (option)
                {
                    case 2:
                        int mapMarker = quest.NoMapMarker ? 1 : Convert.ToInt32(questId.ToString() + (quest.PhaseIndex + 1).ToString("D2"));
                        toSend.Add("qtmap");
                        toSend.Add(questId);
                        toSend.Add(mapMarker);
                        break;
                }
                
            }

            LuaParameters.WriteParameters(ref data, toSend, 0);
            Packet.Send(ServerOpcode.GeneralData, data);
        }

        public void FinishQuest(uint id)
        {
            Quest finished = GetQuestById(id);            

            if(finished != null)
            {
                sbyte slot = GetQuestSlot(id);
                QuestsFinished.Add(finished.Id);
                AddQuestUpdate(id, true);
                Quests[slot] = null;
                World.SendTextSheet(0x61FE, new object[] { (int)id });                
            }
            else
            {
                Log.Instance.Error("An error occurred when trying to finish quest " + id + ".");
            }            
        }

        public void InitializeQuests()
        {        
            QuestsAvailable.AddRange(QuestRepository.GetAvailableQuests("MainScenarioQuests.xml"));
            QuestsAvailable.AddRange(QuestRepository.GetAvailableQuests("SideQuests.xml"));

            foreach (var item in Quests)            
                InitializeQuest((Quest)item.Value);

            //we do not offer new quests on instances or private areas.
            if(User.Instance.Character.GetCurrentZone().PrivLevel == 0)
            {
                foreach (Quest quest in QuestsAvailable)
                    InitializeQuest(quest);
            }            
        }

        private void InitializeQuest(Quest quest)
        {
            if (quest != null)
            {
                quest.StartPhase();

                if (!string.IsNullOrEmpty(quest.Director) && !User.Instance.Character.GetCurrentZone().Directors.Any(x => x is QuestDirector q && q.QuestName == quest.Director))
                {
                    var q = new QuestDirector(quest.Director);
                    User.Instance.Character.GetCurrentZone().Directors.Add(new QuestDirector(quest.Director));
                }                    
            }
        }
        #endregion

        public void LocalGuidleveAdd(uint id)
        {
            GuildLevesLocal.Add(id);
        }

        public void LocalGuildleveDone(uint id)
        {
            GuildLevesLocal.Remove(id);
            GuildLevesDone.Add(id);
        }

        public void LocalGuildleveChecked(uint id)
        {

        }

        public void GetGuildleveData(ref LuaParameters parameters)
        {
            byte[] data = new byte[0xc0];
            parameters.Add("requestedData");
            parameters.Add("activegl");
            parameters.Add(0x07); //???
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);
            LuaParameters.WriteParameters(ref data, parameters, 0);
            Packet.Send(ServerOpcode.GeneralData, data);
        }

        #region Debug functions
        public void ResetQuestHistory(uint questId)
        {
            Quest quest = GetQuestById(questId);
            quest.HistoryIndex = 1;
            Log.Instance.Warning("Quest history index reset.");
        }

        public void ReloadQuestPhase(uint questId, int previous = 0)
        {
            Quest playerQuest = GetQuestById(questId);

            if (playerQuest != null)
            {
                Quest quest = QuestRepository.GetQuest("MainScenarioQuests.xml", questId);

                if (quest == null)
                    quest = QuestRepository.GetQuest("SideQuests.xml", questId);

                QuestPhase phase = (QuestPhase)quest.Phases[playerQuest.PhaseIndex - previous];                

                playerQuest.Phases[playerQuest.PhaseIndex - previous] = phase;
                playerQuest.PhaseIndex = (byte)(playerQuest.PhaseIndex - previous);
                InitializeQuests();
            }
        }

        public void ReloadQuest(uint questId)
        {
            Quest playerQuest = GetQuestById(questId);

            if (playerQuest != null)
            {
                Quest quest = QuestRepository.GetQuest("MainScenarioQuests.xml", questId);
                sbyte slot = GetQuestSlot(questId);

                if (quest == null)
                    quest = QuestRepository.GetQuest("SideQuests.xml", questId);

                User.Instance.Character.Journal.Quests[slot] = quest;               
                InitializeQuests();
            }
        }
        #endregion
    }
}
