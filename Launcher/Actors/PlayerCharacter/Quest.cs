using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Sockets;

namespace PrimalLauncher
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
                return (QuestPhase)Phases[PhaseIndex];
            }
        }

        public void StartPhase(Socket sender) => CurrentPhase.Start(sender);
       

        public QuestPhaseStep SearchActorStep(string type, uint actorClassId, uint actorId)
        {
            QuestPhaseStep stepForActor = null;

            foreach (QuestPhaseStep step in CurrentPhase.Steps)
            {
                if (step.Event == type && step.Done == false)
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

        public void EndPhase()
        {
            foreach(QuestPhaseStep step in CurrentPhase.Steps)
            {
                step.Done = true;
            }
        }

        public void CheckPhase(Socket sender)
        {
            if(CurrentPhase.Steps.TrueForAll(x => x.Done == true) && PhaseIndex < Phases.Count-1)
            {
                //if all steps are done, we set the phase as done.
                CurrentPhase.Done = true;

                //if there is a next phase, start it.
                if(PhaseIndex + 1 < Phases.Count)
                {
                    PhaseIndex++;
                    CurrentPhase.Start(sender);
                }                    
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

                if (step.EndPhase)
                    EndPhase();

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
        public bool Done { get; set; }
        public List<QuestPhaseStep> Steps { get; set; } = new List<QuestPhaseStep>();       

        public void Start(Socket sender)
        {
            Zone zone = User.Instance.Character.GetCurrentZone();

            foreach (var step in Steps)
            {                
                Populace actorRef = (Populace)zone.Actors.Find(x => x.ClassId == step.ActorClassId);
                
                if(actorRef != null)
                {
                    switch (step.Event)
                    {
                        case "questIcon":
                            actorRef.QuestIcon = Convert.ToInt32(step.Value);
                            if (actorRef.Spawned) actorRef.SetQuestIcon(sender);
                            step.Done = true;
                            break;
                        case "pushDefault":
                        case "talkDefault":
                            actorRef.Events.Find(x => x.Name == step.Event).Enabled = 1;
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
        public string Event { get; set; }


        public string Value { get; set; }
        public string OnExecute { get; set; }
        public bool Done { get; set; }
        public bool Repeatable { get; set; }
        public object[] Parameters { get; set; }
        public string OnFinish { get; set; }
        public string OnDelegate { get; set; }   
        public bool EndPhase { get; set; }
        public uint QuestId { get; set; }

        public void Execute(Socket sender)
        {
            if (!string.IsNullOrEmpty(OnExecute))
            {
                Populace actorRef = ActorClassId > 0 ? (Populace)User.Instance.Character.GetCurrentZone().GetActorByClassId(ActorClassId) : null;
                List<string> tasks = new List<string>();

                if (OnExecute.IndexOf(";") > 0)
                    tasks.AddRange(OnExecute.Split(new char[] { ';' }));
                else
                    tasks.Add(OnExecute);
                
                foreach(string task in tasks)
                {
                    if (task.IndexOf(":") > 0)
                    {
                        string[] command = task.Split(new[] { ':' });

                        switch (command[0])
                        {
                            case "Enabled":
                                typeof(Event).GetProperty(command[0]).SetValue(actorRef.Events.Find(x => x.Name == Event), Convert.ToByte(command[1]));
                                actorRef.SetEventStatus(sender);
                                break;
                            case "QuestIcon":
                                actorRef.QuestIcon = Convert.ToInt32(command[1]);
                                actorRef.SetQuestIcon(sender);
                                break;
                            case "AddKeyItem":
                                User.Instance.Character.Inventory.AddKeyItem(command[1]);
                                break;
                            case "FinishQuest":
                                User.Instance.Character.Journal.FinishQuest(sender, Convert.ToUInt32(command[1]));
                                break;
                            case "AddQuest":
                                QuestId = Convert.ToUInt32(command[1]);
                                User.Instance.Character.Journal.AddQuest(sender, QuestId);
                                break;
                            case "GoToZone":
                                World.Instance.ChangeZone(sender, EntryPoints.Get(Convert.ToUInt32(command[1])), 0x0F);
                                break;
                        }
                    }
                }
                
            }

            if(!Repeatable) Done = true;
        }
    }
}
