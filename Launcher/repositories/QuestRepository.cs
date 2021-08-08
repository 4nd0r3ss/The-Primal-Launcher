using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace PrimalLauncher
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
            XmlDocument zonesXml = new XmlDocument();
            string file = GetXmlResource(fileName);

            if (!string.IsNullOrEmpty(file))
            {
                zonesXml.LoadXml(file);
                XmlElement root = zonesXml.DocumentElement;

                foreach (XmlNode node in root.ChildNodes)
                {
                    uint id = Convert.ToUInt32(node.Attributes["id"].Value);

                    if(id == questId)
                    {
                        return new Quest()
                        {
                            Id = id,
                            Name = node.Attributes["name"].Value ?? "",
                            StartRegion = node.Attributes["startRegion"] != null ? Convert.ToUInt32(node.Attributes["startRegion"].Value) : 0,
                            Phases = GetQuestPhases(node)
                        };
                    }
                }
            }

            return null; //if reached here, no quest was found.
        }

        public static OrderedDictionary GetQuestPhases(XmlNode node)
        {
            OrderedDictionary phaseList = new OrderedDictionary();
            int i = 0;

            foreach(XmlNode childNode in node.ChildNodes)
            {
                QuestPhase phase = new QuestPhase();
                phase.Steps = GetQuestPhaseSteps(childNode);
                phaseList.Add(i, phase);
                i++;
            }

            return phaseList;
        }

        public static List<QuestPhaseStep> GetQuestPhaseSteps(XmlNode node)
        {
            List<QuestPhaseStep> stepList = new List<QuestPhaseStep>();

            foreach(XmlNode childNode in node.ChildNodes)
            {
                QuestPhaseStep step = new QuestPhaseStep();

                step.ActorClassId = childNode.Attributes["actorClassId"] != null ? Convert.ToUInt32(childNode.Attributes["actorClassId"].Value) : 0;
                step.ActorId = childNode.Attributes["actorId"] != null ? Convert.ToUInt32(childNode.Attributes["actorId"].Value) : 0;
                step.Event = childNode.Attributes["event"] != null ? childNode.Attributes["event"].Value : "";
                step.Value = childNode.Attributes["value"] != null ? childNode.Attributes["value"].Value : "";
                step.OnExecute = childNode.Attributes["onExecute"] != null ? childNode.Attributes["onExecute"].Value : "";
                step.Repeatable = childNode.Attributes["repeatable"] != null ? Convert.ToBoolean(childNode.Attributes["repeatable"].Value) : false;
                step.Parameters = childNode.Attributes["parameters"] != null ? ParseStringParameters(childNode.Attributes["parameters"].Value) : null;               
                step.OnFinish = childNode.Attributes["onFinish"] != null ? childNode.Attributes["onFinish"].Value : "";
                step.OnDelegate = childNode.Attributes["onDelegate"] != null ? childNode.Attributes["onDelegate"].Value : "";
                step.EndPhase = childNode.Attributes["endPhase"] != null ? Convert.ToBoolean(childNode.Attributes["endPhase"].Value) : false;
                step.PhaseIgnore = childNode.Attributes["phaseIgnore"] != null ? Convert.ToBoolean(childNode.Attributes["phaseIgnore"].Value) : false;
                stepList.Add(step);
            }

            return stepList;
        }

        public static object[] ParseStringParameters(string stringParameters)
        {
            if(stringParameters == "none")
            {
                return new object[] { };
            }
            else
            {
                List<object> result = new List<object>();
                string[] parameters = stringParameters.Split(new char[] { ',' });

                foreach (string parameter in parameters)
                    result.Add(ParseStringParameter(parameter));

                return result.ToArray();
            }
            
        }

        public static object ParseStringParameter(string parameter)
        {
            switch (parameter)
            {
                case "null":
                    return null;
                default:
                    return null;
            }
        }

        public static Quest GetMainScenarioQuest(uint questId) => GetQuest("MainScenarioQuests", questId);

        public static void GetRegionAvailableQuests(uint zoneId)
        {
            //check xml for available quests in current region.
            //set available quest icon to start NPCs
        }

        /// <summary>
        /// Get the contents of a XML file configured as a resource.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetXmlResource(string fileName)
        {
            //From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
            string result = string.Empty;
            Stream stream = typeof(QuestRepository).Assembly.GetManifestResourceStream("Launcher.Resources.xml." + fileName + ".xml");
            if (stream != null)
                using (stream)
                using (StreamReader sr = new StreamReader(stream))
                    result = sr.ReadToEnd();

            return result;
        }
    }
}
