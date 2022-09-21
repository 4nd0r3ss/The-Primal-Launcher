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
using System.Linq;
using System.Xml;

namespace PrimalLauncher
{
    [Serializable]
    public class Quest
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Director { get; set; }

        public bool Accepted { get; set; }
        public int HistoryIndex { get; set; } = 1;
        public OrderedDictionary Phases { get; set; } = new OrderedDictionary();
        public byte PhaseIndex { get; set; } = 0;

        //if set to true, will show a blank map with a '???' marker when user clicks 'map' button in quest details widget.
        public bool NoMapMarker { get; set; } 

        public QuestPhase CurrentPhase
        {
            get
            {
                return (QuestPhase)Phases[PhaseIndex];
            }
        }

        public Quest(XmlNode node)
        {
            uint id = Convert.ToUInt32(node.Attributes["id"].Value);

            Id = id;
            Name = node.Attributes["name"].Value ?? "";
            //StartZone = node.GetAttributeAsUint("startRegion");
            Phases = GetQuestPhases(node);
            Accepted = node.GetAttributeAsBool("autoAccept");
            Director = node.Attributes["director"] != null ? Name + node.Attributes["director"].Value : "";
        }

        public void StartPhase() => CurrentPhase.Start();               

        public void EndPhase()
        {
            foreach (QuestPhaseStep step in CurrentPhase.Steps)
            {
                step.Done = true;
            }

            CurrentPhase.RemoveActors();
            CheckPhase();
        }

        public void CheckPhase()
        {
            if(CurrentPhase.Steps.FindAll(x => !x.PhaseIgnore).TrueForAll(x => x.Done == true) && PhaseIndex < Phases.Count-1)
            {
                //if all steps are done, we set the phase as done.
                CurrentPhase.Done = true;

                //remove step actors
                CurrentPhase.RemoveActors();

                //if there is a next phase, start it.
                if (PhaseIndex + 1 < Phases.Count)
                {
                    PhaseIndex++;
                    CurrentPhase.Start();
                }                    
            }

            Log.Instance.Warning("Quest " + Name + ", Current phase: " + PhaseIndex);
            Log.Instance.Warning("==========================================================================");
        }

        private QuestPhaseStep GetActorStep(string eventType, Actor owner = null, string value = null)
        {
            //Log.Instance.Warning("Quest.GetActorStep2()");

            //get steps from event type the were not executed yet
            var steps = CurrentPhase.Steps.FindAll(x => x.Event == eventType && x.Done == false);

            if (steps.Any())
            {
                uint ownerClassId = owner != null ? owner.ClassId : 0;
                uint ownerId = owner != null ? owner.Id : 0;

                foreach (QuestPhaseStep step in steps)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        //get step by director type
                        if (owner is Director)
                        {
                            //Log.Instance.Warning("Quest.GetActorStep2() -> owner is director");

                            if (owner is QuestDirector q && step.Director == "quest" && q.QuestName == Director)
                                return step;

                            if (owner is OpeningDirector && step.Director == "opening")
                                return step;
                        }
                        else
                        {
                            //get step by actor class id
                            if (step.ActorClassId == ownerClassId)
                                return step;

                            //get step by actor id
                            if (step.ActorClassId == 0 && step.ActorId == ownerId)
                                return step;
                        }
                    }
                    //else we search step by actor and value.
                    else
                    {
                        if (step.ActorClassId == ownerClassId && step.Value == value)
                            return step;
                    }
                }               
            }

            return null;
        }

        public QuestPhaseStep GetActorStep(string eventName, Actor owner = null, bool finishRepeatable = false, string value = null)
        {
            //Log.Instance.Warning("Quest.GetActorStep()");

            QuestPhaseStep step = GetActorStep(eventName, owner, value);           

             if (step != null)
             {
                step.Execute();

                if (finishRepeatable)
                    step.Done = true;

                if (step.EndPhase)
                    EndPhase();
                               
                return step;
            }
            else
            {
                return null;
            }     
        }

        public QuestPhaseStep GetCommandStep(string eventName, Command command)
        {
            return CurrentPhase.Steps.Find(x => x.Event == eventName && x.Command == command);
        }

        private OrderedDictionary GetQuestPhases(XmlNode node)
        {
            OrderedDictionary phaseList = new OrderedDictionary();
            int i = 0;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                QuestPhase phase = new QuestPhase();
                phase.Steps = GetQuestPhaseSteps(childNode);
                phase.Actors = GetQuestPhaseActors(childNode);
                phase.Targetcount = childNode.GetAttributeAsInt("targetCount");
                phase.OnTargetCount = GetStepTasks(childNode, "onTargetCount");
                phaseList.Add(i, phase);
                i++;
            }

            return phaseList;
        }

        private List<QuestPhaseStep> GetQuestPhaseSteps(XmlNode node)
        {
            List<QuestPhaseStep> stepList = new List<QuestPhaseStep>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if(childNode.Name == "step")
                {         
                    QuestPhaseStep step = new QuestPhaseStep
                    {
                        QuestId = childNode.GetAttributeAsUint("questId", Id),
                        ActorClassId = childNode.GetAttributeAsUint("actorClassId"),
                        ActorId = childNode.GetAttributeAsUint("actorId"),
                        Director = childNode.GetAttributeAsString("director"),
                        Event = childNode.GetAttributeAsString("event"),
                        Value = childNode.GetAttributeAsString("value"),

                        Repeatable = childNode.GetAttributeAsBool("repeatable"),
                        Parameters = ParseStringParameters(childNode),

                        OnExecute = GetStepTasks(childNode, "onExecute"),
                        OnFinish = GetStepTasks(childNode, "onFinish"),
                        OnDelegate = GetStepTasks(childNode, "onDelegate"),

                        EndPhase = childNode.GetAttributeAsBool("endPhase"),
                        PhaseIgnore = childNode.GetAttributeAsBool("phaseIgnore"),
                        ExecutionType = childNode.GetAttributeAsString("executionType", "instead"),
                        Command = childNode.Attributes["command"] != null ? (Command)Enum.Parse(typeof(Command), childNode.Attributes["command"].Value) : 0,
                        FinishEvent = childNode.GetAttributeAsBool("finishEvent"),
                        IsQuestion = childNode.GetAttributeAsBool("isQuestion"),
                        QuestionOptions = GetQuestionOptions(childNode),
                        SwitchEvent = childNode.GetAttributeAsBool("switchEvent"),
                        Actors = GetActors(childNode),
                        QuestIcon = childNode.GetAttributeAsUint("questIcon")
                    };
                    stepList.Add(step);
                }                
            }

            return stepList;
        }

        private List<Actor> GetQuestPhaseActors(XmlNode node)
        {
            List<Actor> actors = new List<Actor>();
            XmlNode actorsNode = node.SelectSingleNode("actors");

            if(actorsNode != null)
            {
                foreach(XmlNode actorNode in actorsNode.ChildNodes)
                {
                    actors.Add(ActorRepository.CreateActorObj(actorNode));
                }
            }          

            return actors;
        }

        private List<Actor> GetActors(XmlNode childNode)
        {
            List<Actor> result = new List<Actor>();
            XmlNode actorsNode = childNode.SelectSingleNode("actors");

            if(actorsNode != null)
            {
                foreach (XmlNode node in actorsNode.ChildNodes)
                {
                    result.Add(ActorRepository.CreateActorObj(node));
                }
            }

            return result;
        }

        private List<QuestionOption> GetQuestionOptions(XmlNode node)
        {
            List<QuestionOption> questionOptions = new List<QuestionOption>();

            foreach(XmlNode xmlNode in node.ChildNodes)
            {
                questionOptions.Add(new QuestionOption(xmlNode));
            }

            return questionOptions;
        }

        public static List<KeyValuePair<string, string>> GetStepTasks(XmlNode node, string attributeName)
        {
            List<KeyValuePair<string, string>> tasks = new List<KeyValuePair<string, string>>();
            string taskString = node.Attributes[attributeName] != null ? node.Attributes[attributeName].Value : "";

            if (!string.IsNullOrEmpty(taskString))
            {
                string[] split;

                if (taskString.IndexOf(";") > 0)
                    split = taskString.Split(new char[] { ';' });
                else
                    split = new string[] { taskString };

                foreach (string s in split)
                {
                    string[] split2;

                    if (s.IndexOf(":") > 0)
                        split2 = s.Split(new char[] { ':' });
                    else
                        split2 = new string[] { s, "" };

                    tasks.Add(new KeyValuePair<string,string>(split2[0], split2[1]));
                }
            }

            return tasks;
        }

        private object[] ParseStringParameters(XmlNode node)
        {
            if (node.Attributes["parameters"] != null)
            {
                string stringParameters = node.Attributes["parameters"].Value;

                if (stringParameters == "none")
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
            else
            {
                return null;
            }
        }

        private static object ParseStringParameter(string parameter)
        {
            switch (parameter)
            {
                case "null":
                    return null;
                default:
                    return Convert.ToInt32(parameter);
            }
        }
    }    
}
