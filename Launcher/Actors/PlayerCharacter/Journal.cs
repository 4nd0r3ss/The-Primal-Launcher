using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    [Serializable]
    public class Journal
    {
        public List<Quest> Quests { get; set; }
        public List<Quest> QuestsFinished { get; set; }
        public uint[] GuildLevesLocal { get; set; }
        public uint[] GuildLevesRegional { get; set; }
        public uint[] GuildLevesDone { get; set; }
        public uint[] GuildLevesChecked { get; set; }

        public Journal(uint initialTown)
        {
            Quests = new List<Quest>();
            QuestsFinished = new List<Quest>();
            GuildLevesLocal = new uint[0x10];
            GuildLevesRegional = new uint[0x10];
            GuildLevesDone = new uint[0x10];
            GuildLevesChecked = new uint[0x10];

            //get initial quest
            Quests.Add(QuestRepository.GetInitialQuest(initialTown));
        }

        public void AddToWork(ref WorkProperties work)
        {
            for (int i = 0; i < Quests.Count; i++)
                work.Add(string.Format("playerWork.questScenario[{0}]", i), 0xA0F00000 | Quests[i].Id);

            //GuildLeve - local
            //for (int i = 0; i < 40; i++)
            //property.Add(string.Format("playerWork.questGuildleve[{0}]", (uint)49), true);

            //GuildLeve - regional
            //for(int i = 0; i < 16; i++)
            //{
            //    if (guildLeveId[i] != 0)
            //property.Add(string.Format("work.guildleveId[{0}]", 0), 1103);

            //if(guildLeveDone[i]!=0)
            //    property.Add(string.Format("work.guildleveDone[{0}]", i), guildLeveDone[i]);

            //if(guildLeveChecked[i]!=0)
            //    property.Add(string.Format("work.guildleveChecked[{0}]", i), guildLeveChecked[i]);
            //}
        }

        public Quest GetQuestById(uint id)
        {
            return Quests.Find(x => x.Id == id);
        }

        #region Quests
        public void AddQuest(Socket sender, uint id)
        {
            Quest quest = QuestRepository.GetMainScenarioQuest(id);
            Quests.Add(quest);

            WorkProperties work = new WorkProperties(sender, User.Instance.Character.Id, "playerWork/journal");
            work.Add((uint)0x6974EDA5, 0xA0F00000 | id);
            work.SendUpdate();

            World.Instance.SendTextQuestUpdated(sender, id);
        }

        public void InitializeQuests(Socket sender)
        {
            foreach (Quest quest in Quests)
            {
                quest.StartPhase(sender);
            }
        }
        public void FinishQuest(Socket sender, uint id)
        {
            Quest finished = Quests.Find(x => x.Id == id);
            Quests.Remove(finished);
            QuestsFinished.Add(finished);
            World.Instance.SendTextSheetMessage(sender, 2122238, id);
        }

        public void GetQuestData(Socket sender, int questId, ref LuaParameters parameters)
        {
            byte[] data = new byte[0xc0];
            //int questId = ;
            parameters.Add("requestedData");
            parameters.Add("qtdata");
            parameters.Add(questId);
            parameters.Add(5);
            LuaParameters.WriteParameters(ref data, parameters, 0);
            Packet.Send(sender, ServerOpcode.GeneralData, data);
        }
        #endregion

        public void GetGuildleveData(Socket sender, ref LuaParameters parameters)
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
            Packet.Send(sender, ServerOpcode.GeneralData, data);
        }
    }
}
