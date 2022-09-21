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

namespace PrimalLauncher
{
    [Serializable]
    public class QuestPhase
    {
        public bool Done { get; set; }
        public List<QuestPhaseStep> Steps { get; set; } = new List<QuestPhaseStep>();
        public List<Actor> Actors { get; set; }
        public int Count { get; set; }
        public int Targetcount { get; set; }
        public List<KeyValuePair<string, string>> OnTargetCount { get; set; }

        public void Start()
        {
            Zone zone = User.Instance.Character.GetCurrentZone();

            if(Actors != null && Actors.Count > 0)
            {
                foreach(Actor a in Actors)
                {
                    SpawnActor(a);
                }
            }

            if(zone.Actors != null)
            {
                foreach (var step in Steps)
                {
                    if (!step.Done)
                    {
                        Actor actorRef = zone.Actors.Find(x => x.ClassId == step.ActorClassId);

                        if (actorRef != null)
                        {
                            switch (step.Event)
                            {
                                //case "questIcon":
                                //    actorRef.QuestIcon = Convert.ToInt32(step.Value);
                                //    if (actorRef.Spawned) actorRef.SetQuestIcon();
                                //    step.Done = true;
                                //    break;
                                case "pushDefault":
                                case "talkDefault":
                                    actorRef.Events.Find(x => x.Name == step.Event).Enabled = 1;
                                    if (actorRef.Spawned) actorRef.SetEventStatus();
                                    break;
                            }

                            //changed how quest icons are shown
                            if(step.QuestIcon > 0)
                            {
                                actorRef.QuestIcon = Convert.ToInt32(step.QuestIcon);
                                if (actorRef.Spawned) actorRef.SetQuestIcon();
                            }
                        }
                    }                    
                }
            }            
        }

        public void SpawnActor(Actor actor)
        {
            Zone zone = User.Instance.Character.GetCurrentZone();            

            if(actor != null && !zone.Actors.Contains(actor))            
                zone.LoadActor(actor);            
        }

        public void RemoveActor(uint classId)
        {
            Actor actor = Actors.Find(x => x.ClassId == classId);

            if(actor != null)
            {               
                World.Instance.GetZone(actor.Position.ZoneId).RemoveActor(classId);
            }            
        }

        public void RemoveActors()
        {           
            if (Actors != null && Actors.Count > 0)
            {
                foreach (Actor a in Actors)
                {
                    Zone zone = World.Instance.GetZone(a.Position.ZoneId);

                    if (zone.Actors.Contains(a))
                    {
                        a.Despawn();
                        zone.Actors.Remove(a);
                    }
                }
            }
        }
 
        public List<KeyValuePair<string, string>> CheckCounter()
        {
            Count++;
            return Count == Targetcount ? OnTargetCount : new List<KeyValuePair<string, string>>();
        }
    }
}
