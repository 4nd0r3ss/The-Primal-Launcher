using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PrimalLauncher
{
    public class EventRequest
    {
        protected LuaParameters _requestParameters;

        public LuaParameters RequestParameters
        {
            get { return _requestParameters; }
            set { _requestParameters = value; }
        }

        public uint CallerId { get; set; }
        public uint OwnerId { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint QuestId { get; set; }
        public byte Code { get; set; }
        public byte[] RequestPacket { get; set; }
        public string FunctionName { get; set; }
        public string Name { get; set; }
        public bool IsQuestion { get; set; }
        public uint Selection { get; set; }
        public QuestPhaseStep QuestStep { get; set; }
        public bool ReturnToOwner { get; set; }
        public byte[] Data { get; set; }

        public EventRequest(byte[] requestPacket)
        {
            RequestParameters = new LuaParameters();

            CallerId = (uint)(requestPacket[0x13] << 24 | requestPacket[0x12] << 16 | requestPacket[0x11] << 8 | requestPacket[0x10]);
            OwnerId = (uint)(requestPacket[0x17] << 24 | requestPacket[0x16] << 16 | requestPacket[0x15] << 8 | requestPacket[0x14]);
            Unknown1 = (uint)(requestPacket[0x1b] << 24 | requestPacket[0x1a] << 16 | requestPacket[0x19] << 8 | requestPacket[0x18]);
            Unknown2 = (uint)(requestPacket[0x1f] << 24 | requestPacket[0x1e] << 16 | requestPacket[0x1d] << 8 | requestPacket[0x1c]);
            RequestPacket = requestPacket;
        }

        public virtual void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Actor: 0x" + OwnerId.ToString("X"));

            Actor eventOwner = GetActor();

            //if actor exists in current zone
            if (eventOwner != null)
            {
                //check if any of the active quests phases has a step for the actor
                //TODO: what if more than one quest phase has a step for this actor?
                foreach (Quest quest in User.Instance.Character.Journal.Quests)
                {
                    //if a step is found for this actor, execute and return the executed step
                    QuestStep = quest.ActorStepComplete(sender, GetType().Name, eventOwner.ClassId, eventOwner.Id);

                    //if there is an executed step for the actor
                    if (QuestStep != null)
                    {
                        //execute the step function.
                        FunctionName = QuestStep.Value ?? "";
                        QuestId = QuestStep.QuestId > 0 ? QuestStep.QuestId : quest.Id;

                        if (!string.IsNullOrEmpty(QuestStep.Value))
                            DelegateEvent(sender, QuestId, QuestStep.Value, QuestStep.Parameters);
                        else
                            Finish(sender);

                        return;
                    }
                }

                //if reached here, there wasn't a step for the actor in any active quest, so we give back control to the actor.
                var type = GetType();
                eventOwner.InvokeMethod(type.Name, new object[] { sender });
                ReturnToOwner = true;
            }
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");
        }

        /// <summary>
        /// Prepare the lua parameters packet for the current event.
        /// </summary>
        protected void InitLuaParameters()
        {
            RequestParameters = new LuaParameters();
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        /// <summary>
        /// Send a packet to the client acknowledging the start of an event processing.
        /// </summary>
        /// <param name="sender"></param>
        public void Response(Socket sender)
        {
            byte[] data = new byte[0x298];
            Buffer.BlockCopy(BitConverter.GetBytes(CallerId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(OwnerId), 0, data, 0x04, 4);
            LuaParameters.WriteParameters(ref data, RequestParameters, 0x08);
            Packet.Send(sender, ServerOpcode.EventRequestResponse, data);
        }

        public virtual void Finish(Socket sender)
        {
            byte[] data = new byte[0x30];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            data[0x08] = 1;
            string name = GetType().Name;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(name), 0, data, 0x09, name.Length);
            Packet.Send(sender, ServerOpcode.EventRequestFinish, data);

            if (QuestStep != null && !string.IsNullOrEmpty(QuestStep.OnFinish))
            {
                try
                {
                    string tasks = QuestStep.OnFinish;
                    List<string> taskList = new List<string>();

                    if (tasks.IndexOf(";") > 0)
                        taskList.AddRange(tasks.Split(new char[] { ';' })); //if there is more than one task, we split them.
                    else
                        taskList.Add(tasks); //if just one task, just add it.

                    foreach (string task in taskList)
                    {
                        if (task.IndexOf(":") > 0)
                        {
                            string[] split = task.Split(new char[] { ':' });
                            InvokeMethod(split[0], new object[] { sender, split[1] });
                        }
                        else
                        {
                            InvokeMethod(task, new object[] { sender });
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }

        /// <summary>
        /// Send a packet to the client acknowledging the end of an event processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public virtual void ProcessEventResult(Socket sender, byte[] data)
        {
            Finish(sender);
        }

        /// <summary>
        /// Position player character according to the coordinates passed as parameters. 
        /// Used from quest scripts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameters"></param>
        #region helper methods  
        public void SetPlayerPosition(Socket sender, string parameters)
        {
            string[] split = parameters.Split(new char[] { ',' });
            Position pos = new Position
            {
                ZoneId = Convert.ToUInt32(split[0]),
                X = (float)Convert.ToDouble(split[1]),
                Y = (float)Convert.ToDouble(split[2]),
                Z = (float)Convert.ToDouble(split[3]),
                R = (float)Convert.ToDouble(split[4]),
                SpawnType = Convert.ToUInt16(split[0])
            };
            World.Instance.RepositionPlayer(sender, pos);
        }

        public void BackInsideStopper(Socket sender) => User.Instance.Character.TurnBack(sender, 5);

        /// <summary>
        /// This function is used by reflection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="questName"></param>
        /// <param name="privateAreaId"></param>
        public void GoToQuestPrivateZone(Socket sender, string questName, ushort privateAreaId)
        {
            User.Instance.Character.BindQuestDirector(sender, questName);
            World.Instance.ChangeZone(sender, EntryPoints.Get(privateAreaId), 0x0F);
        }

        /// <summary>
        /// This function is used by reflection only, so no reference will be shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="privateAreaId"></param>
        public void GoToPrivateZone(Socket sender, string privateAreaId)
        {
            World.Instance.ChangeZone(sender, EntryPoints.Get(Convert.ToUInt32(privateAreaId)), 0x0F);
        }

        /// <summary>
        /// This function is used by reflection only, so no reference will be shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="zoneId"></param>
        public void GoToZone(Socket sender, string zoneId)
        {
            World.Instance.ChangeZone(sender, EntryPoints.Get(Convert.ToUInt32(zoneId)), 0x0F);
        }

        /// <summary>
        /// This function is used by reflection, so only 1 reference will be shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="questId"></param>
        public void FinishQuest(Socket sender, string questId)
        {
            uint id = Convert.ToUInt32(questId);
            User.Instance.Character.Journal.FinishQuest(sender, id);
        }

        /// <summary>
        /// This function is used by reflection, so only 1 reference will be shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="questId"></param>
        public void AddQuest(Socket sender, string questId)
        {
            uint id = Convert.ToUInt32(questId);
            User.Instance.Character.Journal.AddQuest(sender, id);
        }

        /// <summary>
        /// This will close the current tutorial dialog being shown.
        /// </summary>
        /// <param name="sender"></param>
        public void CloseTutorialDialog(Socket sender) => SendData(sender, new object[] { 0x05 }, 2);

        /// <summary>
        /// Get the Actor obj by its id#.
        /// </summary>
        /// <returns></returns>
        public Actor GetActor()
        {
            Actor eventOwner = World.Instance.Directors.FirstOrDefault(x => x.Id == OwnerId);

            if (eventOwner == null)
                eventOwner = World.Instance.Zones
                    .FirstOrDefault(x => x.Id == User.Instance.Character.GetCurrentZone().Id)
                    .Actors.FirstOrDefault(x => x.Id == OwnerId);

            return eventOwner;
        }

        /// <summary>
        /// Use reflection to call methods by string name.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodParams"></param>
        public void InvokeMethod(string methodName, object[] methodParams = null)
        {
            Type type = GetType();
            var method = type.GetMethod(methodName);

            if (method != null)
                method.Invoke(this, methodParams);
            //else //should be enabled/disabled by debug option?
            //Log.Instance.Warning("EventManager.InvokeMethod: Type " + GetType().Name + " has no method " + methodName + ".");
        }
        #endregion

        /// <summary>
        /// Delegates a specified function to the caller event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="questId"></param>
        /// <param name="functionName"></param>
        /// <param name="parameters"></param>
        public virtual void DelegateEvent(Socket sender, uint questId, string functionName, object[] parameters = null)
        {
            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateEvent"));
            RequestParameters.Add(CallerId);
            RequestParameters.Add(0xA0F00000 | questId);
            RequestParameters.Add(functionName);

            if (parameters == null)
                parameters = new object[] { null, null, null };

            foreach (object obj in parameters)
                RequestParameters.Add(obj);

            if (QuestStep != null && !string.IsNullOrEmpty(QuestStep.OnDelegate))
                Thread.Sleep(5000);

            Response(sender);
        }

        /// <summary>
        /// This is used to send data to the client during an event, i.e.: battle tutorial dialogs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="toSend"></param>
        /// <param name="sleepSeconds"></param>
        public static void SendData(Socket sender, object[] toSend, int sleepSeconds = 0)
        {
            byte[] data = new byte[0xC0];
            LuaParameters parameters = new LuaParameters();

            foreach (object obj in toSend)
                parameters.Add(obj);

            LuaParameters.WriteParameters(ref data, parameters, 0);
            Packet.Send(sender, ServerOpcode.GeneralData, data);

            if (sleepSeconds > 0)
                Thread.Sleep(sleepSeconds * 1000);
        }

        /// <summary>
        /// This function will start the battle tutorial in the beginning of the game.
        /// </summary>
        /// <param name="sender"></param>
        public void BattleTutorialStart(Socket sender)
        {
            string simpleContentNumber = "";
            string questName = "";
            uint openingStoperClassId = 0;
            Actor openingStoper = null;
            List<uint> dutyActorsClassId = null;

            switch (User.Instance.Character.InitialTown)
            {
                case 1:
                    simpleContentNumber = "30002";
                    questName = "Man0l001";
                    dutyActorsClassId = new List<uint> { 2290001, 2290002, 2205403 };
                    break;
                case 2:
                    simpleContentNumber = "30010";
                    questName = "Man0g001";
                    openingStoperClassId = 1090384;
                    dutyActorsClassId = new List<uint> { 2290005, 2290006, 2201407 };
                    break;
                case 3:
                    simpleContentNumber = "30079";
                    questName = "Man0u001";
                    openingStoperClassId = 1090373;
                    dutyActorsClassId = new List<uint> { 2290003, 2290004, 2203301 };
                    break;
            }

            //if there is an opening stoper, we want to disable its events before antering battle.
            //this is to fix the player being positioned in the wrong place in the battle tutorial.
            if (openingStoperClassId > 0)
            {
                openingStoper = User.Instance.Character.GetCurrentZone().GetActorByClassId(openingStoperClassId);

                if (openingStoper != null)
                {
                    openingStoper.ToggleEvents(sender, false);
                    Thread.Sleep(1500);
                }
            }

            User.Instance.Character.GetCurrentZone().SpawnAsInstance("SimpleContent" + simpleContentNumber, "opening.monster.xml");
            World.Instance.ChangeZone(sender, EntryPoints.Get(User.Instance.Character.GetCurrentZone().Id, 0x10));

            User.Instance.Character.BindQuestDirector(sender, questName, true);
            User.Instance.Character.AddDutyGroup(sender, dutyActorsClassId, true);
            SendData(sender, new object[] { 0x09 });

            List<Actor> fighters = User.Instance.Character.GetCurrentZone().GetActorsByFamily("fighter");

            foreach (Actor fighter in fighters)
            {
                fighter.State.Main = MainState.Active;
                fighter.SetMainState(sender);
            }
        }

        /// <summary>
        /// Get player selection from a question dialog.
        /// </summary>
        public void GetQuestionSelection(byte[] data)
        {
            Data = data;

            if (data[0x21] == 0x05)
                Selection = 0xFF;
            else
                Selection = (uint)(data[0x22] << 24 | data[0x23] << 16 | data[0x24] << 8 | data[0x25]); //get player selection from event result packet
        }
    }
}
