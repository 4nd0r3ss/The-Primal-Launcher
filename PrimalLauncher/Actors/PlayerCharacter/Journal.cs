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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PrimalLauncher
{
    [Serializable]
    public class Journal
    {
        public Dictionary<sbyte, object> QuestScenario { get; set; }
        public Dictionary<sbyte, object> QuestGuildleve { get; set; }
        public Dictionary<sbyte, object> Guildleves { get; set; }

        public bool[] QuestScenarioComplete { get; set; }
        public bool[] QuestGuildleveComplete { get; set; }
        public List<int> GuildlevesComplete { get; set; }

        //non-native members
        public List<Quest> QuestsAvailable { get; set; }
        public List<uint> QuestsFinished { get; set; }   

        public Journal(uint initialTown)
        {
            QuestsAddEmptySlots();
            LocalLevesAddEmptySlots();
            GuildlevesAddEmptySlots();        
           
            QuestGuildleveComplete = new bool[2048];
            QuestScenarioComplete = new bool[2048];

            QuestsAvailable = new List<Quest>();
            QuestsFinished = new List<uint>();

            //get initial quest
            QuestScenario[0] = QuestXmlLoader.GetFirstQuest(initialTown);   
        }

        public void AddToWork(ref WorkProperties work)
        {
            for (sbyte i = 0; i < 0x10; i++)   
                if(QuestScenario[i] != null)
                    work.Add(string.Format("playerWork.questScenario[{0}]", i), 0xA0F00000 | ((Quest)QuestScenario[i]).Id);

            //Local Leves 
            for (sbyte i = 0; i < 0x08; i++)
                if (QuestGuildleve[i] != null)
                    work.Add(string.Format("playerWork.questGuildleve[{0}]", i), 0xA0F00000 | ((PassiveGL)QuestGuildleve[i]).Id);

            //add guildleves to the first 8 lots
            AddGuildLeveData(ref work, Guildleves, 0);

            //add local leves to remaining 8 slots.
            //AddGuildLeveData(ref work, QuestGuildleve, 8);            
        }

        private void AddGuildLeveData(ref WorkProperties work, Dictionary<sbyte, object> guildLeves, sbyte index)
        {
            //TODO: need to get Id as short, as this wont work if an int or uint is sent as id.
            //ex: for local leve id 120203, we do 120203 - 120000 = 203, then we store 203 only.
            //for guildleves that are not local, need to test as they have different id prefixes.

            for (sbyte i = 0; i < guildLeves.Count; i++)
            {
                if (guildLeves[i] != null)
                {
                    GuildLeve gl = (GuildLeve)guildLeves[i];
                    work.Add(string.Format("work.guildleveId[{0}]", i), gl.Id);
                    work.Add(string.Format("work.guildleveDone[{0}]", i), gl.Done);
                    work.Add(string.Format("work.guildleveChecked[{0}]", i), gl.Checked);
                }
            }
        }

        private void QuestsAddEmptySlots()
        {
            if (QuestScenario == null)
                QuestScenario = new Dictionary<sbyte, object>();

            for (int i = 0; i < 0x10; i++)
                QuestScenario.Add((sbyte)i, null);
        }

        /// <summary>
        /// Returns the first empty slot in an inventory this is necessary to keep continuity
        /// </summary>
        /// <param name="inventory"></param>
        /// <returns></returns>
        private sbyte GetQuestsFirstEmptySlot()
        {
            foreach (var slot in QuestScenario)
                if (slot.Value == null) return slot.Key;

            return -1; //no empty slots
        }

        #region Quests
        public Quest GetQuestById(uint id)
        {
            Quest quest = null; 

            foreach (var slot in QuestScenario)        
                if(slot.Value != null && ((Quest)slot.Value).Id == id)               
                    quest = (Quest)slot.Value;

            if(quest ==null)
                quest = QuestsAvailable.FirstOrDefault(x => x.Id == id);

            return quest;
        }

        public sbyte GetQuestSlot(uint id)
        {
            foreach (var slot in QuestScenario)
            {
                if (slot.Value != null)
                {
                    Quest quest = (Quest)slot.Value;

                    if (quest.Id == id)
                        return slot.Key;
                }                
            }

            return -1; //quest not found
        }

        public List<Quest> GetAllQuests()
        {
            List<Quest> quests = new List<Quest>();

            foreach (var slot in QuestScenario)
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

        public bool HasAvailableQuest(uint questId)
        {
            if (QuestsAvailable.FirstOrDefault(x => x.Id == questId) != null)
                return true;
            else
                return false;
        }

        public bool HasFinishedQuest(uint questId)
        {
            if (QuestsFinished.FirstOrDefault(x => x == questId) > 0) //found in finished quests
                return true;
            else
                return false;
        }

        public void AcceptQuest(uint id)
        {
            Quest quest = QuestsAvailable.FirstOrDefault(x => x.Id == id);
            quest.Accepted = true;

            QuestScenario[GetQuestsFirstEmptySlot()] = quest;
            QuestsAvailable.Remove(quest);

            AddQuestUpdate(id);
        }
        
        public void AddQuest(uint id)
        {
            Quest quest = QuestXmlLoader.GetMainScenarioQuest(id);
            QuestScenario[GetQuestsFirstEmptySlot()] = quest;

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

            object quest = GetQuestObject(questId);

            toSend.Add("requestedData");

            if(parameters[1] == null)
            {
                toSend.Add("qtdata");
                toSend.Add(questId);

                if(quest is Quest)
                    toSend.Add(((Quest)quest).HistoryIndex);
            }
            else
            {
                int option = Convert.ToInt32(parameters[1]);

                //put this swtich here as maybe there are other options...
                switch (option)
                {
                    case 2:                        
                        toSend.Add("qtmap");
                        toSend.Add(questId);
                        toSend.Add(GetQuestMapMarker(quest));
                        break;
                }
                
            }

            LuaParameters.WriteParameters(ref data, toSend, 0);
            Packet.Send(ServerOpcode.GeneralData, data);
        }

        private int GetQuestMapMarker(object quest)
        {
            if (quest is Quest)
            {
                Quest q = (Quest)quest;
                return q.NoMapMarker ? 1 : Convert.ToInt32(q.Id.ToString() + (q.PhaseIndex + 1).ToString("D2"));
            }
            else if (quest is PassiveGL)
            {
                return Convert.ToInt32(((PassiveGL)quest).Id * 100);
            }
            
            return 0;
        }

        private object GetQuestObject(uint questId)
        {
            if (questId > 120000 && questId < 180000)
            {
                return GetLocalLeveById(questId);
            }
            else
            {
                return GetQuestById(questId);
            }
        }

        public void FinishQuest(uint id)
        {
            Quest finished = GetQuestById(id);            

            if(finished != null)
            {
                sbyte slot = GetQuestSlot(id);
                QuestsFinished.Add(finished.Id);
                AddQuestUpdate(id, true);
                QuestScenario[slot] = null;
                World.SendTextSheet(0x61FE, new object[] { (int)id });                
            }
            else
            {
                Log.Instance.Error("An error occurred when trying to finish quest " + id + ".");
            }            
        }

        public void InitializeQuests()
        {        
            QuestsAvailable.AddRange(QuestXmlLoader.GetAvailableQuests("MainScenarioQuests.xml"));
            QuestsAvailable.AddRange(QuestXmlLoader.GetAvailableQuests("SideQuests.xml"));

            foreach (var item in QuestScenario)            
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

        #region Local Leves
        private void LocalLevesAddEmptySlots()
        {
            if (QuestGuildleve == null)
                QuestGuildleve = new Dictionary<sbyte, object>();

            for (int i = 0; i < 0x08; i++)
                QuestGuildleve.Add((sbyte)i, null);
        }

        public PassiveGL GetLocalLeveById(uint id)
        {
            PassiveGL leve = null;

            foreach (var slot in QuestGuildleve)
                if (slot.Value != null && ((PassiveGL)slot.Value).Id == id)
                    leve = (PassiveGL)slot.Value;

            return leve;
        }

        public sbyte GetLocalLeveSlot(uint id)
        {
            foreach (var slot in QuestGuildleve)
            {
                if (slot.Value != null)
                {
                    PassiveGL leve = (PassiveGL)slot.Value;

                    if (leve.Id == id)
                        return slot.Key;
                }
            }

            return 0;
        }

        private sbyte GetLocalLevesFirstEmptySlot()
        {
            foreach (var slot in QuestGuildleve)
                if (slot.Value == null) return slot.Key;

            return -1; //no empty slots
        }

        public void AddLocalleve(PassiveGL leve)
        {            
            QuestGuildleve[GetLocalLevesFirstEmptySlot()] = leve;
            World.SendTextSheet(0xC3E8, new object[] { Convert.ToInt32(leve.Id) });
            AddLocalLeveUpdate(leve.Id);
        }

        public void SendLeveAllowancesRamaining()
        {
            World.SendTextSheet(0xC3DD, new object[] { User.Instance.Character.LeveAllowances });
        }
               
        public void LocalLeveChecked(uint id)
        {

        }

        public void GetGuildleveData(ref LuaParameters response, byte[] request)
        {
            byte[] data = new byte[0xc0];
            List<object> requestParams = LuaParameters.ReadParameters(request, 0x31);
            sbyte slot = Convert.ToSByte(requestParams[1]);
            GuildLeve gl = (GuildLeve)Guildleves[slot];
            
            response.Add("requestedData");
            response.Add("activegl");
            response.Add(1);// gl.OfferLimit); 
            response.Add(null);
            response.Add(2);//gl.Reward1Type);
            response.Add(3);//gl.Reward1Value);
            response.Add(4);//gl.Reward2Type);
            response.Add(5);//gl.Reward2Value);
            response.Add(6);//gl.Evaluation);
            response.Add(7);//gl.StageVisible);
            LuaParameters.WriteParameters(ref data, response, 0);
            Packet.Send(ServerOpcode.GeneralData, data);
        }

        public void AddLocalLeveUpdate(uint id, sbyte slot = 0)
        {
            WorkProperties work = new WorkProperties(User.Instance.Character.Id, "work/guildleve");
            slot = slot > 0 ? slot : GetLocalLeveSlot(id);
            uint questActor = 0;

            if (QuestGuildleve[slot] != null)
                questActor = 0xA0F00000 | ((PassiveGL)QuestGuildleve[slot]).Id;

            work.Add(string.Format("playerWork.questGuildleve[{0}]", slot), questActor);             
            work.SendUpdate(0x90);
        }
        #endregion

        #region Guildleves
        public bool HasCompletedGuildLeve(int id)
        {
            return GuildlevesComplete.Any(x => x == id);
        }

        public void AddGuildLeve(GuildLeve leve)
        {
            sbyte slot = GetGuildlevesFirstEmptySlot();
            Guildleves[slot] = leve;
            AddGuildLeveUpdate(slot);

        }
        private void GuildlevesAddEmptySlots()
        {
            if (Guildleves == null)
                Guildleves = new Dictionary<sbyte, object>();

            for (int i = 0; i < 0x08; i++)
                Guildleves.Add((sbyte)i, null);
        }

        private sbyte GetGuildlevesFirstEmptySlot()
        {
            foreach (var slot in Guildleves)
                if (slot.Value == null) return slot.Key;

            return -1; //no empty slots
        }

        public void AddGuildLeveUpdate(sbyte slot)
        {
            GuildLeve gl = (GuildLeve)Guildleves[slot];
            WorkProperties work = new WorkProperties(User.Instance.Character.Id, "work/guildleve");            
            work.Add(string.Format("work.guildleveId[{0}]", slot), gl.Id); //TODO: need to fix the id, see line 76 in this file.
            work.Add(string.Format("work.guildleveDone[{0}]", slot), gl.Done);            
            work.Add(string.Format("work.guildleveChecked[{0}]", slot), gl.Checked);
            work.SendUpdate();
        }

        public void CompleteGuildLeve(uint id)
        {

        }
        #endregion

        public void AbandonQuestLeve(byte[] data)
        {
            var parameters = LuaParameters.ReadParameters(data, 0x41);
            uint questId = Convert.ToUInt32(parameters[0]);
            int option = Convert.ToInt32(parameters[1]); //might be useful moving forward.

            if (questId > 120000 && questId < 180000)
            {
                sbyte slot = GetLocalLeveSlot(questId);
                QuestGuildleve[slot] = null;
                AddLocalLeveUpdate(questId, slot);
            }
            else
            {
                sbyte slot = GetQuestSlot(questId);
                QuestScenario[slot] = null;
            }

            World.SendTextSheet(50147, new object[] { Convert.ToInt32(questId) });            
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
                Quest quest = QuestXmlLoader.GetQuest("MainScenarioQuests.xml", questId);

                if (quest == null)
                    quest = QuestXmlLoader.GetQuest("SideQuests.xml", questId);

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
                Quest quest = QuestXmlLoader.GetQuest("MainScenarioQuests.xml", questId);
                sbyte slot = GetQuestSlot(questId);

                if (quest == null)
                    quest = QuestXmlLoader.GetQuest("SideQuests.xml", questId);

                User.Instance.Character.Journal.QuestScenario[slot] = quest;               
                InitializeQuests();
            }
        }
        #endregion
    }
}
