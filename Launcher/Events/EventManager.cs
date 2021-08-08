using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PrimalLauncher
{
    public class EventManager
    {
        private static EventManager _instance = null;
        private static readonly object _padlock = new object();
        public EventRequest CurrentEvent { get; set; } = null;

        public static EventManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new EventManager();

                    return _instance;
                }
            }
        }

        private EventManager() { }
             
        public void ProcessIncoming(Socket sender, byte[] data)
        {
            try
            {
                string eventName = GetEventType(data);
                Type type = Type.GetType(eventName);
                CurrentEvent = (EventRequest)Activator.CreateInstance(type, data);
                CurrentEvent.Execute(sender);
            }catch(Exception e)
            {
                Log.Instance.Error(e.Message);
                File.WriteAllBytes("test.txt", data);
                throw e;
            }                             
        }    
        
        private string GetEventType(byte[] data)
        {
            byte nameSize;

            for (nameSize = 1; nameSize < 0x20; nameSize++)
                if (data[0x20 + nameSize] == 0) break;

            byte[] nameBytes = new byte[nameSize - 1];
            Array.Copy(data, 0x21, nameBytes, 0, nameSize - 1);
           return "PrimalLauncher." + Encoding.ASCII.GetString(nameBytes);
        }
    }

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
        public byte[] Data { get; set; }
        public string FunctionName { get; set; }
        public string Name { get; set; }
        public bool IsQuestion { get; set; }
        public uint Selection { get; set; }
        public QuestPhaseStep QuestStep { get; set; }
        public bool ReturnToOwner { get; set; }

        public EventRequest(byte[] data)
        {
            RequestParameters = new LuaParameters();

            CallerId = (uint)(data[0x13] << 24 | data[0x12] << 16 | data[0x11] << 8 | data[0x10]);
            OwnerId = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x14]);
            Unknown1 = (uint)(data[0x1b] << 24 | data[0x1a] << 16 | data[0x19] << 8 | data[0x18]);
            Unknown2 = (uint)(data[0x1f] << 24 | data[0x1e] << 16 | data[0x1d] << 8 | data[0x1c]);
            Data = data;
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
        protected void Response(Socket sender)
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
                }catch(Exception e)
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

            foreach(object obj in parameters)
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

            if(sleepSeconds > 0)
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

                if(openingStoper != null)
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

            World.Instance.SendTextEnteredDuty(sender); 
        }

        /// <summary>
        /// Logout function
        /// </summary>
        public void AskLogout(Socket sender) 
        {
            if (IsQuestion)
            {
                switch (Selection)
                {                    
                    case 2:
                        PlayerCharacter.ExitGame(sender);
                        break;
                    case 3:
                        PlayerCharacter.Logout(sender);
                        break;
                    case 4:
                        Log.Instance.Success("Check bed selected.");
                        break;
                }
            }
            else
            {
                IsQuestion = true;
                FunctionName = "AskLogout";
                RequestParameters.Add(Encoding.ASCII.GetBytes("askLogout"));
                RequestParameters.Add(User.Instance.Character.Id);
                Response(sender);
            }
        }

        /// <summary>
        /// Get player selection from a question dialog.
        /// </summary>
        public void GetQuestionSelection(byte[] data)
        {
            if (data[0x21] == 0x05)
                Selection = 0xFF;
            else
                Selection = (uint)(data[0x22] << 24 | data[0x23] << 16 | data[0x24] << 8 | data[0x25]); //get player selection from event result packet
        }
    }
      
    public class commandRequest : EventRequest
    {
        public Command CommandId { get; set; }
        public commandRequest(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = 0;
        }

        public override void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            switch (CommandId)
            {
                case Command.QuestData:
                    User.Instance.Character.Journal.GetQuestData(sender, (Data[0x42] << 24 | Data[0x43] << 16 | Data[0x44] << 8 | Data[0x45]), ref _requestParameters);
                    break;
                case Command.GuildleveData:
                    User.Instance.Character.Journal.GetGuildleveData(sender, ref _requestParameters);
                    break;
                case Command.Umount:
                    User.Instance.Character.ToggleMount(sender, Command.Umount, false);                   
                    break;
                case Command.DoEmote:
                    Log.Instance.Warning("emote id:" + Data[0x45].ToString("X2"));
                    User.Instance.Character.DoEmote(sender);
                    break;
                case Command.ChangeEquipment:
                    User.Instance.Character.ChangeGear(sender, Data);
                    break;
                case Command.EquipSoulStone:
                    User.Instance.Character.EquipSoulStone(sender, Data);
                    break;
            }
        }     
    }

    public class commandForced : EventRequest
    {
        public Command CommandId { get; set; }

        public commandForced(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = 0;
        }

        public override void Execute(Socket sender) 
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            foreach (Quest quest in User.Instance.Character.Journal.Quests)            
                QuestStep = quest.ActorStepComplete(sender, GetType().Name);

            switch (CommandId)
            {
                case Command.BattleStance:   
                case Command.NormalStance:
                    User.Instance.Character.ToggleStance(sender, CommandId);
                    
                    break;
                case Command.Mount:
                    User.Instance.Character.ToggleMount(sender, Command.Mount, (Data[0x41] == 0x05 ? true : false));                    
                    break;
            }
            
            Finish(sender);            
        }

        public override void Finish(Socket sender)
        {
            base.Finish(sender);

            if (QuestStep != null)
            {
                Thread.Sleep(1000);
                ((QuestDirector)World.Instance.GetDirector("Quest")).StartEvent(sender, QuestStep.Value);
            }
        }
    }

    public class commandContent : EventRequest
    {
        public Command CommandId { get; set; }
        private int MenuPage { get; set; }
        private uint PageId { get; set; } 
        private uint PageItem { get; set; } 

        public commandContent(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = (0xA0F00000 | (ushort)CommandId);
            InitLuaParameters();
        }

        public override void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            switch (CommandId)
            {
                case Command.Teleport:
                    Teleport(sender);
                    break;
            }
        }

        public override void ProcessEventResult(Socket sender, byte[] data)
        {            
             if (IsQuestion)
             {
                 GetQuestionSelection(data);
                 InvokeMethod(FunctionName, new object[] { sender });
            }
            else
            {
                Finish(sender);
            }                 
        }

        public void Teleport(Socket sender)
        {
            
            if (Selection == 0xFF)
            {
                if (MenuPage == 2) //from aetheryte selection back to region selection
                    MenuPage = 0;

                if (MenuPage == 1) //finish event when closing region selection
                {
                    Finish(sender);
                    return;
                }                                  
            }

            InitLuaParameters();

            switch (MenuPage)
            {
                case 0:
                
                    IsQuestion = true;
                    FunctionName = "Teleport";
                    DelegateCommand(sender, new object[] { "eventRegion", (int)User.Instance.Character.Anima });
                    break;
                case 1:
                    PageId = Selection;
                    List<object> parameters = new List<object>();
                    parameters.Add("eventAetheryte");
                    parameters.Add((int)Selection);

                    var regionAetherytes = from a in ActorRepository.Instance.Aetherytes
                                           where a.TeleportMenuPageId == Selection
                                           orderby a.TeleportMenuId ascending
                                           select a;

                    foreach (Aetheryte a in regionAetherytes)
                        parameters.Add((int)a.AnimaCost);

                    DelegateCommand(sender, parameters.ToArray());
                    break;
                case 2:
                    PageItem = Selection;
                    User.Instance.Character.PlayAnimationEffect(sender, AnimationEffect.TeleportWait);
                    DelegateCommand(sender, new object[] { "eventConfirm", false, false, 0x02, 0x13883f, false });                    
                    break;
                case 3:
                    if(Selection == 1)
                    {
                        User.Instance.Character.PlayAnimationEffect(sender, AnimationEffect.Teleport);                        
                        Packet.Send(sender, ServerOpcode.SetUIControl, new byte[] { 0x14, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 });
                        World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0x01, 0x00, 0xF8, 0x5F, 0x39, 0x85, 0x20, 0x00 });
                        Finish(sender);

                        var destinationAetheryte = from a in ActorRepository.Instance.Aetherytes
                                               where a.TeleportMenuPageId == PageId && a.TeleportMenuId == PageItem
                                               orderby a.TeleportMenuId ascending
                                               select a;

                        if (destinationAetheryte != null)
                        {
                            Position aethPosition = destinationAetheryte.ToList()[0].Position;
                            Position newPosition = new Position
                            {
                                ZoneId = aethPosition.ZoneId,
                                X = aethPosition.X + (float)(7 * Math.Sin(aethPosition.R)),
                                Z = aethPosition.Z + (float)(7 * Math.Cos(aethPosition.R)),
                                Y = aethPosition.Y,
                                R = aethPosition.R + 0.8f
                            };
                        
                            World.Instance.TeleportPlayer(sender, newPosition);
                        }                           
                        else
                            Log.Instance.Error("Something went wrong, aetheryte not found.");
                    }
                    else
                    {
                        Finish(sender);
                    }
                    break;
            }

            MenuPage++;
        }

        private void DelegateCommand(Socket sender, object[] parameters = null)
        {
            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateCommand"));            
            RequestParameters.Add(0xA0F00000 | (uint)CommandId);

            if (parameters == null)
                parameters = new object[] { null, null, null };

            foreach (object obj in parameters)
                RequestParameters.Add(obj);

            Response(sender);
        }
    }

    public class talkDefault : EventRequest
    {
        public talkDefault(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        public override void ProcessEventResult(Socket sender, byte[] data)
        {
            if (!string.IsNullOrEmpty(FunctionName) && GetType().GetMethod(FunctionName) != null && !IsQuestion)
            {
               InvokeMethod(FunctionName, new object[] { sender, data });
            }
            else
            {
                if (IsQuestion)
                {
                    GetQuestionSelection(data);

                    if (ReturnToOwner)
                    {
                        GetActor().InvokeMethod(FunctionName, new object[] { sender });
                        return;
                    }else
                    {
                        InvokeMethod(FunctionName, new object[] { sender });
                    }
                }

                Finish(sender);
            }
        }

        public void processEvent020_9(Socket sender, byte[] eventResult)
        {
            byte resultType = eventResult[0x21];
            uint selection = (uint)(eventResult[0x22] << 24 | eventResult[0x23] << 16 | eventResult[0x24] << 8 | eventResult[0x25]);

            if (resultType == 0x05) //null
            {
                Finish(sender);
                FinishQuest(sender, "110001");
                AddQuest(sender, "110002");               
                World.Instance.ChangeZone(sender, EntryPoints.Get(4), 0x0F);
            }
            else
            {
                switch (selection)
                {
                    case 1:
                        InitLuaParameters(); //clear
                        DelegateEvent(sender, 0x01ADB2, "processEvent010", new object[] { null, null, null }); //put this in the quest script
                        break;
                    case 2:
                        Finish(sender);
                        break;
                }
            }
        }

        #region Possible methods
        private void EventTalkCard() { }
        private void EventTalkDetail() { }
        private void EventTalkAfterOffer() { }
        private void EventTalkPack() { }
        private void EventTalkType() { }
        private void EventTalkOfferWelcome() { }
        private void AskOfferPack() { }
        private void AskOfferRank() { }
        private void AskOfferQuest() { }
        private void TalkOfferDecide() { }
        private void FinishTalkTurn() { }
        private void SwitchEvent() { }
        private void WelcomeTalk() { }
        private void SelectModeOfMultiWeaponVendor() { }
        private void OpenShopBuy() { }
        private void SelectShopBuy() { }
        private void CloseShopBuy() { }
       
        #endregion
    }

    public class pushDefault : EventRequest
    {      
        public pushDefault(byte[] data) : base(data)
        {
            InitLuaParameters();
        }              

        public override void ProcessEventResult(Socket sender, byte[] data)
        {
           if(!string.IsNullOrEmpty(FunctionName) && GetType().GetMethod(FunctionName) != null)
            {
                InvokeMethod(FunctionName, new object[] { sender, data });
            }
            else
            {
                Finish(sender);
            }
        }

        public void processEventNewRectAsk(Socket sender, byte[] eventResult)
        {
            byte resultType = eventResult[0x21];
            uint selection = (uint)(eventResult[0x22] << 24 | eventResult[0x23] << 16 | eventResult[0x24] << 8 | eventResult[0x25]);

            if(resultType == 0x05) //null
            {
                Finish(sender);

                Actor eventOwner = GetActor();
                //finish exit door repeatable step.
                User.Instance.Character.Journal.Quests.Find(x => x.Id == QuestId).ActorStepComplete(sender, GetType().Name, eventOwner.ClassId, eventOwner.Id, finishRepeatable: true);
                World.Instance.SendTextQuestUpdated(sender, QuestId);
                BattleTutorialStart(sender);
            }
            else
            {
                switch (selection)
                {
                    case 1:
                        InitLuaParameters();
                        DelegateEvent(sender, QuestId, "processEvent000_2", new object[] { null, null, null, null });//put this in the quest script
                        break;
                    case 2:
                        Finish(sender);
                        break;
                }
            }
        }
    }

    public class noticeEvent : EventRequest
    {
        public noticeEvent(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)eventType);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        public override void Finish(Socket sender)
        {
            base.Finish(sender);

            if(!string.IsNullOrEmpty(FunctionName))
                InvokeMethod(FunctionName, new object[]{ sender });           
        }

        /// <summary>
        /// Battle tutorial method. This method is common for all 3 openings.
        /// </summary>
        /// <param name="sender"></param>
        public void processTtrBtl002(Socket sender)
        {
            SendData(sender, new object[] { 0x05 }, 2);
            SendData(sender, new object[] { 0x02, null, null, 0x235f }, 2); //attack success
            SendData(sender, new object[] { 0x04, null, null, 0x01, 0x0C }, 2);//TP tutorial (4th parameter is keyboard_controller)
            SendData(sender, new object[] { 0x05 }, 2);
            SendData(sender, new object[] { 0x04, null, null, 0x01, 0x0D }, 2);//weaponskill tutorial (4th parameter is keyboard_controller)
            SendData(sender, new object[] { 0x05 }, 2);
            SendData(sender, new object[] { 0x02, null, null, 0x2369 }, 2);
            SendData(sender, new object[] { "attention", World.Instance.Id, "", 0xC781, (int)User.Instance.Character.InitialTown });
           
            World.Instance.SetMusic(sender, 0x07, MusicMode.Crossfade);
            User.Instance.Character.ToggleStance(sender, Command.NormalStance);
            ((QuestDirector)World.Instance.GetDirector("Quest")).StartEvent(sender, "noticeEvent");
        }

        /// <summary>
        /// TODO: put this in the quest script later.
        /// </summary>
        /// <param name="sender"></param>
        public void processEvent000_3(Socket sender) => GoToQuestPrivateZone(sender, "Man0l001", 2);

        public void processEvent020_1(Socket sender) => GoToQuestPrivateZone(sender, "Man0g001", 5);
        
        public void processEvent020(Socket sender) => GoToQuestPrivateZone(sender, "Man0u001", 1);       
    }

    public class exit : EventRequest
    {
        public exit(byte[] data) : base(data) { }

        public override void Execute(Socket sender)
        {
            base.Execute(sender);            
            Finish(sender);
        }
    }

    public class caution : EventRequest
    {
        public caution(byte[] data) : base(data) { }

        public override void Execute(Socket sender)
        {
            base.Execute(sender);
            Finish(sender);
        }
    }
}
