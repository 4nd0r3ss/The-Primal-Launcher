using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public static class QuestRepository
    {
        public static Quest GetInitialQuest(uint initialTown)
        {
            uint initialQuestId = 0;

            switch (initialTown)
            {
                case 1: //Limsa
                    initialQuestId = 0x0001ADB1;
                        break;
                case 2: //Gridania
                    initialQuestId = 0;
                        break;
                case 3: //Uldah
                    initialQuestId = 0;
                        break;
            }

            return GetQuest(initialQuestId);
        }

        public static Quest GetQuest(uint questId)
        {
            //get quest from XML by id

            return new Quest();
        }

        public static void GetRegionAvailableQuests(uint zoneId)
        {
            //check xml for available quests in current region.
            //set available quest icon to start NPCs
        }
    }
}
