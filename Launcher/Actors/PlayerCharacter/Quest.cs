using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Sockets;

namespace Launcher
{
    [Serializable]
    public class Quest
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint StartRegion { get; set; }
        public OrderedDictionary Phases { get; set; } = new OrderedDictionary();
        public byte PhaseIndex { get; set; } = 0;

        public QuestPhase CurrentPhase
        {
            get
            {
                return ((QuestPhase)Phases[PhaseIndex]);
            }
        }

        public void StartPhase(Socket sender) => CurrentPhase.Start(sender);
       

        public QuestPhaseStep SearchActorStep(string type, uint actorClassId, uint actorId)
        {
            QuestPhaseStep stepForActor = null;

            foreach (QuestPhaseStep step in CurrentPhase.Steps)
            {
                if (step.Type == type && step.Done == false)
                {
                    if(step.ActorClassId == actorClassId)
                    {
                        stepForActor = step;
                        break;
                    }                        

                    if(step.ActorClassId == 0 && step.ActorId == actorId)
                    {
                        stepForActor = step;
                        break;
                    }
                }
            }

            return stepForActor;
        }

        public void CheckPhase(Socket sender)
        {
            if(CurrentPhase.Steps.TrueForAll(x => x.Done == true) && PhaseIndex < Phases.Count-1)
            {
                PhaseIndex++;
                CurrentPhase.Start(sender);
            }
        }

        public QuestPhaseStep ActorStepComplete(Socket sender, string eventName, uint classId = 0, uint actorid = 0, bool finishRepeatable = false)
        {            
             QuestPhaseStep step = SearchActorStep(eventName, classId, actorid);           

             if (step != null)
             {
                step.Execute(sender);

                if (finishRepeatable)
                    step.Done = true;

                CheckPhase(sender);                
                return step;
            }
            else
            {
                return null;
            }     
        }
    }

    [Serializable]
    public class QuestPhase
    {
        public List<QuestPhaseStep> Steps { get; set; } = new List<QuestPhaseStep>();       

        public void Start(Socket sender)
        {
            Zone zone = User.Instance.Character.GetCurrentZone();

            foreach (var step in Steps)
            {                
                Populace actorRef = (Populace)zone.Actors.Find(x => x.ClassId == step.ActorClassId);
                
                if(actorRef != null)
                {
                    switch (step.Type)
                    {
                        case "questIcon":
                            actorRef.QuestIcon = Convert.ToInt32(step.Value);
                            if (actorRef.Spawned) actorRef.SetQuestIcon(sender);
                            step.Done = true;
                            break;
                        case "pushDefault":
                        case "talkDefault":
                            actorRef.Events.Find(x => x.Name == step.Type).Enabled = 1;
                            if (actorRef.Spawned) actorRef.SetEventStatus(sender);
                            break;
                    }
                }                                
            }            
        }
    }

    [Serializable]
    public class QuestPhaseStep
    {
        public uint ActorClassId { get; set; }
        public uint ActorId { get; set; } //we need this for when the actor has no classid but has a fixed id# (i.e. directors)
        public string Type { get; set; }
        public string Value { get; set; }
        public string OnExecute { get; set; }
        public bool Done { get; set; }
        public bool Repeatable { get; set; }
        public object[] Parameters { get; set; }
        public string OnFinish { get; set; }

        public void Execute(Socket sender)
        {
            if (!string.IsNullOrEmpty(OnExecute))
            {
                Populace actorRef = ActorClassId > 0 ? (Populace)User.Instance.Character.GetCurrentZone().GetActorByClassId(ActorClassId) : null;

                if (OnExecute.IndexOf(":") > 0)
                {
                    string[] command = OnExecute.Split(new[] { ':' });                    

                    switch (command[0])
                    {
                        case "Enabled":
                            typeof(Event).GetProperty(command[0]).SetValue(actorRef.Events.Find(x => x.Name == Type), Convert.ToByte(command[1]));
                            actorRef.SetEventStatus(sender);
                            break;
                        case "QuestIcon":
                            actorRef.QuestIcon = Convert.ToInt32(command[1]);
                            actorRef.SetQuestIcon(sender);
                            break;
                        case "AddKeyItem":
                            User.Instance.Character.Inventory.AddKeyItem(command[1]);
                            break;
                    }                    
                }
            }

            if(!Repeatable) Done = true;
        }
    }
}
