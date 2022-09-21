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
using System.Threading;
using System.Xml;

namespace PrimalLauncher
{
    [Serializable]
    public class QuestPhaseStep
    {
        public uint ActorClassId { get; set; } //the event owner's class id.
        public uint ActorId { get; set; } //we need this for when the actor has no classid but has a fixed id# (i.e. directors)
        public uint QuestId { get; set; } //stores the quest id this step belongs to.
        public uint QuestIcon { get; set; }

        public string Event { get; set; } //the event type that will trigger the quest step.
        public string Value { get; set; } //the function/action that must be executed for this step.
        public string ExecutionType { get; set; } //not the best name, this is the best I could think.
        public string Director { get; set; } //if the step is to be executed by a director, keep the director type here.

        public Command Command { get; set; } //if quest step is executed from a command, store the command id.        

        public object[] Parameters { get; set; } //any parameters to be sent by onDeletege function.
        public List<KeyValuePair<string, string>> OnExecute { get; set; } //action(s) to be executed during step.
        public List<KeyValuePair<string, string>> OnFinish { get; set; } //action(s) to be executed after the step.
        public List<KeyValuePair<string, string>> OnDelegate { get; set; } //action(s) to be executed when a delegate function is called.
        public List<QuestionOption> QuestionOptions { get; set; }
        public List<Actor> Actors { get; set; } //actors to be spawned by this step

        public bool Done { get; set; } //flag to store if a step was already executed.
        public bool Repeatable { get; set; } //is true, this step will never be set as 'done' and will be triggered every time the related avent is triggered.
        public bool IsQuestion { get; set; }
        public bool EndPhase { get; set; } //if true, the whole phase will be ended after this step is executed.       
        public bool PhaseIgnore { get; set; } //if true, the phase can be finished if this step was not executed.
        public bool FinishEvent { get; set; } //not being used.
        public bool SwitchEvent { get; set; } //not being used.
                                             
                                            


        public bool ExecuteSelectedOption()
        {
            uint? selection = EventManager.Instance.CurrentEvent.Selection[0];
            uint?    subSelection = EventManager.Instance.CurrentEvent.Selection.Length > 1 ? EventManager.Instance.CurrentEvent.Selection[1] : 0;
            bool finishEvent = false;
            QuestionOption option = QuestionOptions.FirstOrDefault(x => x.Selection == selection && x.SubSelection == subSelection);

            if (option != null)
            {
                //execute tasks for the selected option
                ExecuteTaskList(option.OnExecute);

                //if we want the event to be finished after executing the task list
                finishEvent = option.OnExecute.Any(x => x.Key == "FinishEvent");

                if (finishEvent && option.OnFinish != null)
                    OnFinish = option.OnFinish;
            }
            else
            {
                Log.Instance.Warning("No option found for selection. Selection: " + (selection.HasValue? selection.ToString() : "null"));
            }

            return finishEvent;
        }

        public void Execute()
        {
            ExecuteTaskList(OnExecute);
            if (!Repeatable) Done = true;
        }

        public void ExecuteOnDelegate() => ExecuteTaskList(OnDelegate);

        public void ExecuteOnFinish() => ExecuteTaskList(OnFinish);

        public void ExecuteTaskList(List<KeyValuePair<string, string>> taskList)
        {
            if (taskList.Count > 0)
            {
                ExecuteInstanceTasks(taskList);
                ExecuteStaticTasks(taskList, QuestId);
            }
        }

        private void ExecuteInstanceTasks(List<KeyValuePair<string, string>> taskList)
        {
            Actor actorRef = ActorClassId > 0 ? User.Instance.Character.GetCurrentZone().GetActorByClassId(ActorClassId) : null;

            foreach (var task in taskList)
            {
                if (!string.IsNullOrEmpty(task.Key))
                    Log.Instance.Warning("Executing task: " + task.Key + (!string.IsNullOrEmpty(task.Value) ? ", " + task.Value : ""));

                switch (task.Key)
                {
                    case "Enabled":
                        typeof(Event).GetProperty(task.Key).SetValue(actorRef.Events.Find(x => x.Name == Event), Convert.ToByte(task.Value));
                        actorRef.SetEventStatus();
                        break;
                    case "QuestIcon":
                        actorRef.QuestIcon = Convert.ToInt32(task.Value);
                        actorRef.SetQuestIcon();
                        break;
                    case "Sleep":
                        Thread.Sleep((int)(1000 * Convert.ToDecimal(task.Value)));
                        break;
                    case "StepFunction": //execute function defined in the step 'value' attribute.
                        EventManager.Instance.CurrentEvent.InitLuaParameters();
                        EventManager.Instance.CurrentEvent.DelegateEvent(QuestId, Value, null);
                        break;
                    case "TalkDefault":
                        EventManager.Instance.CurrentEvent.InitLuaParameters();
                        EventManager.Instance.CurrentEvent.DelegateEvent(Actor.GetTalkCode(), actorRef.TalkFunctions[0], null);
                        break;
                    case "SpawnActor":
                        //User.Instance.Character.Journal.GetQuestById(QuestId).CurrentPhase.SpawnActor(Convert.ToUInt32(task.Value), true);
                        break;
                    case "DelegateEvent":
                        EventManager.Instance.CurrentEvent.InitLuaParameters();
                        EventManager.Instance.CurrentEvent.DelegateEvent(QuestId, task.Value, new object[] { null, null, null, null });
                        break;
                    case "StartTalkEvent":
                        EventManager.Instance.CurrentEvent.InitLuaParameters();
                        actorRef.StartEvent("talkDefault");
                        break;
                    case "PhaseCounterIncrement":
                        ExecuteTaskList(User.Instance.Character.Journal.GetQuestById(QuestId).CurrentPhase.CheckCounter());
                        break;
                }
            }
        }

        /// <summary>
        /// Execute tasks that do NOT depend on the current event object instance.
        /// </summary>
        /// <param name="taskList"></param>
        /// <param name="questId"></param>
        public static void ExecuteStaticTasks(List<KeyValuePair<string, string>> taskList, uint questId = 0)
        {
            foreach (var task in taskList)
            {
                if (!string.IsNullOrEmpty(task.Key))
                    Log.Instance.Warning("Executing task: " + task.Key + (!string.IsNullOrEmpty(task.Value) ? ", " +  task.Value : ""));

                switch (task.Key)
                {
                    case "AddKeyItem":
                        User.Instance.Character.Inventory.AddKeyItem(task.Value);
                        break;
                    case "FinishQuest":
                        User.Instance.Character.Journal.FinishQuest(Convert.ToUInt32(task.Value));
                        break;
                    case "AddQuest":
                        User.Instance.Character.Journal.AddQuest(Convert.ToUInt32(task.Value));
                        break;
                    case "GoToZone":
                        World.Instance.ChangeZone(EntryPoints.Get(Convert.ToUInt32(task.Value)), 0x0F);
                        break;
                    case "TurnBack":
                        User.Instance.Character.TurnBack(Convert.ToSingle(task.Value));
                        break;
                    case "NpcLSAdd":
                        User.Instance.Character.Linkshell.NpcAddLinkpearl(Convert.ToInt32(task.Value));
                        break;
                    case "NpcLSMessage":
                        User.Instance.Character.Linkshell.NpcNewMessage(Convert.ToInt32(task.Value));
                        break;
                    case "SetPlayerPosition":
                        User.Instance.Character.Position.Set(task.Value);
                        break;
                    case "UpdateQuest":
                        User.Instance.Character.Journal.UpdateQuest(questId, Convert.ToInt32(task.Value));
                        break;
                    case "AcceptQuest":
                        User.Instance.Character.Journal.AcceptQuest(questId);
                        break;
                    case "CloseTutorialWidget":
                        World.Instance.CloseTutorialWidget();
                        break;
                    case "BattleTutorialStart":
                        BattleTutorial.Instance.Start();
                        break;
                    case "EndPhase":
                        User.Instance.Character.Journal.GetQuestById(questId).EndPhase();
                        break;
                    case "AddExp":
                        User.Instance.Character.AddExp(Convert.ToInt32(task.Value));
                        break;
                    case "AddGil":
                        User.Instance.Character.Inventory.AddGil(Convert.ToInt32(task.Value));
                        break;
                    case "RemoveActor":
                        User.Instance.Character.Journal.GetQuestById(questId).CurrentPhase.RemoveActor(Convert.ToUInt32(task.Value));
                        break;
                    case "ToInstance":
                        World.Instance.ToInstance(Convert.ToUInt32(task.Value));
                        break;
                    case "ExitInstance":
                        World.Instance.ZoneInstance.Exit();
                        break;
                    case "DisableStep":
                        User.Instance.Character.Journal.GetQuestById(questId).CurrentPhase.Steps[Convert.ToInt32(task.Value)].Done = true;
                        break;
                    case "EnableStep":
                        User.Instance.Character.Journal.GetQuestById(questId).CurrentPhase.Steps[Convert.ToInt32(task.Value)].Done = false;
                        break;
                    case "NoMapMarker":
                        User.Instance.Character.Journal.GetQuestById(questId).NoMapMarker = Convert.ToBoolean(task.Value);
                        break;
                    case "TextSheet":
                        World.SendTextSheet(task.Value);
                        break;
                    case "StartQuestDirectorNotice":
                        ((QuestDirector)User.Instance.Character.GetCurrentZone().GetDirector("Quest")).StartEvent("noticeEvent", task.Value);
                        break;
                }
            }
        }
    }

    [Serializable]
    public class QuestionOption
    {
        public uint? Selection { get; set; }
        public uint? SubSelection { get; set; }
        public uint QuestId { get; set; }
        public List<KeyValuePair<string, string>> OnExecute { get; set; } //tasks to execute  
        public List<KeyValuePair<string, string>> OnFinish { get; set; } //tasks to execute  

        public QuestionOption(XmlNode node)
        {
            Selection = GetSelectionValue(node, "selection");
            SubSelection = GetSelectionValue(node, "subSelection");
            QuestId = (uint)GetSelectionValue(node, "questId");
            OnExecute = Quest.GetStepTasks(node, "onExecute");
            OnFinish = Quest.GetStepTasks(node, "onFinish");
        }

        private uint? GetSelectionValue(XmlNode node, string attributeName)
        {
            string value = node.GetAttributeAsString(attributeName);

            if (value == "null")
                return null;

            if (string.IsNullOrEmpty(value))
                value = "0";

            int selectionValue = Convert.ToInt32(value);

            return selectionValue.IntToUint32();
        }
    }
}
