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
using System.Xml;

namespace PrimalLauncher
{
    public static class QuestRepository
    {
        public static Quest GetFirstQuest(uint initialTown)
        {
            uint initialQuestId = 0;

            switch (initialTown)
            {
                case 1: //Limsa
                    initialQuestId = 0x0001ADB1;
                        break;
                case 2: //Gridania
                    initialQuestId = 0x0001ADB5;
                        break;
                case 3: //Uldah
                    initialQuestId = 0x0001ADB9;
                        break;
            }

            return GetMainScenarioQuest(initialQuestId);
        }

        public static Quest GetQuest(string fileName, uint questId)
        {
            List<Zone> zones = new List<Zone>();
            XmlDocument questXml = new XmlDocument();
            questXml.LoadFromResource(fileName);            

            if (questXml.HasChildNodes)
            {               
                XmlElement root = questXml.DocumentElement;

                foreach (XmlNode node in root.ChildNodes)
                {
                    uint id = Convert.ToUInt32(node.Attributes["id"].Value);

                    if(id == questId)
                    {
                        return new Quest(node);
                    }
                }
            }

            return null; //if reached here, no quest with given id was found.
        }        

        public static Quest GetMainScenarioQuest(uint questId) => GetQuest("MainScenarioQuests.xml", questId);

        public static void GetRegionAvailableQuests(uint zoneId)
        {
            //check xml for available quests in current region.
            //set available quest icon to start NPCs
        }  
        
        //TODO: temporary
        public static List<Quest> GetAvailableQuests(string fileName)
        {
            int currentLevel = User.Instance.Character.GetCurrentLevel();

            List<Quest> quests = new List<Quest>();
            XmlDocument questXml = new XmlDocument();
            questXml.LoadFromResource(fileName);

            if (questXml.HasChildNodes)
            {
                XmlElement root = questXml.DocumentElement;

                foreach (XmlNode node in root.ChildNodes)
                {
                    uint questLevel = node.GetAttributeAsUint("level");
                    uint id = node.GetAttributeAsUint("id");
                    string questName = node.GetAttributeAsString("name");
                    bool autoAccept = node.GetAttributeAsBool("autoAccept");

                    if (
                        !autoAccept &&
                        questLevel > 0 && 
                        questLevel <= currentLevel && 
                        !User.Instance.Character.Journal.HasQuest(id) &&
                        User.Instance.Character.CanAcceptQuest(questName)
                        )                    
                        quests.Add(new Quest(node));                    
                }
            }

            return quests;
        }
    }
}
