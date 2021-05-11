using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Launcher
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
                Type type = Type.GetType(GetEventType(data));
                CurrentEvent = (EventRequest)Activator.CreateInstance(type, data);
                CurrentEvent.Execute(sender);
            }catch(Exception e)
            {
                Log.Instance.Error(e.Message);
            }                             
        }    
        
        private string GetEventType(byte[] data)
        {
            byte nameSize;

            for (nameSize = 1; nameSize < 0x20; nameSize++)
                if (data[0x20 + nameSize] == 0) break;

            byte[] nameBytes = new byte[nameSize - 1];
            Array.Copy(data, 0x21, nameBytes, 0, nameSize - 1);
           return "Launcher." + Encoding.ASCII.GetString(nameBytes);
        }
    }
    public class EventRequest
    {
        public LuaParameters RequestParameters { get; set; }
        public uint CallerId { get; set; }
        public uint OwnerId { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint QuestId { get; set; }
        public byte Code { get; set; }        
        public byte[] Data { get; set; }
        public string FunctionName { get; set; }
        public string Name { get; set; }

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

            if (eventOwner != null)
            {
                foreach (Quest quest in User.Instance.Character.Quests)
                {
                    KeyValuePair<string, object[]> function = quest.ActorStepComplete(sender, GetType().Name, eventOwner.ClassId, eventOwner.Id);

                    if (!string.IsNullOrEmpty(function.Key))
                    {
                        FunctionName = function.Key;
                        QuestId = quest.Id;
                        DelegateEvent(sender, quest.Id, function.Key, function.Value);
                        return;
                    }
                }

                eventOwner.InvokeMethod(GetType().Name, new object[] { sender });
            }
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");
        }

        private void Response(Socket sender)
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
        }

        public virtual void ProcessEventResult(Socket sender, byte[] data)
        {
            Finish(sender);
        }

        #region helper methods   
        public Actor GetActor()
        {
            Actor eventOwner = World.Instance.Directors.FirstOrDefault(x => x.Id == OwnerId);

            if (eventOwner == null)
                eventOwner = World.Instance.Zones
                    .FirstOrDefault(x => x.Id == User.Instance.Character.Position.ZoneId)
                    .Actors.FirstOrDefault(x => x.Id == OwnerId);

            return eventOwner;
        } 
        
        public void InvokeMethod(string methodName, object[] methodParams)
        {
            var method = GetType().GetMethod(methodName);

            if (method != null)
                method.Invoke(this, methodParams);
            else
                Log.Instance.Error("EventManager.InvokeMethod: Type " + GetType().Name + " has no method " + methodName + ".");
        }
        #endregion

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

            if (functionName == "processTtrBtl001")
                SendData(sender, new object[] { 0x09 });
            
            Response(sender);
        }

        public void SendData(Socket sender, object[] toSend, int sleepSeconds = 0)
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
                    GetQuestData(sender);
                    break;
                case Command.GuildleveData:
                    GetGuildleveData(sender);
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

        private void GetQuestData(Socket sender)
        {
            byte[] data = new byte[0xc0];
            int questId = Data[0x42] << 24 | Data[0x43] << 16 | Data[0x44] << 8 | Data[0x45];
            RequestParameters.Add("requestedData");            
            RequestParameters.Add("qtdata");
            RequestParameters.Add(questId);
            RequestParameters.Add(5);
            LuaParameters.WriteParameters(ref data, RequestParameters, 0);
            Packet.Send(sender, ServerOpcode.GeneralData, data);
        }

        private void GetGuildleveData(Socket sender)
        {
            byte[] data = new byte[0xc0];            
            RequestParameters.Add("requestedData");
            RequestParameters.Add("activegl");
            RequestParameters.Add(0x07); //???
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            LuaParameters.WriteParameters(ref data, RequestParameters, 0);
            Packet.Send(sender, ServerOpcode.GeneralData, data);
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
            Thread.Sleep(1000);
            ((QuestDirector)World.Instance.GetDirector("Quest")).StartEvent(sender, "noticeEvent");
        }

    }

    public class commandContent : EventRequest
    {
        public commandContent(byte[] data) : base(data)
        {

        }
    }

    public class talkDefault : EventRequest
    {
        public talkDefault(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)eventType);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

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
        private void AskLogout() { }
    }

    public class pushDefault : EventRequest
    {      
        public pushDefault(byte[] data) : base(data)
        {
            InitLuaParameters();
        }

        private void InitLuaParameters()
        {
            RequestParameters = new LuaParameters();
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
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
                User.Instance.Character.Quests.Find(x => x.Id == QuestId).ActorStepComplete(sender, GetType().Name, eventOwner.ClassId, eventOwner.Id, finishRepeatable: true);
                World.Instance.SendTextQuestUpdated(sender, QuestId);
                Zone zone = World.Instance.Zones.Find(x => x.Id == User.Instance.Character.Position.ZoneId);                
                DutyGroup dutyGroup = new DutyGroup();
                QuestDirector questDirector = ((QuestDirector)World.Instance.GetDirector("Quest"));

                zone.LoadActors("Monster");
                dutyGroup.MemberList.Add(questDirector.Id);
                dutyGroup.AddMembers(zone.Actors);
                dutyGroup.InitializeGroup(sender);
                dutyGroup.SendPackets(sender);
                User.Instance.Character.Groups.RemoveAll(x => x.GetType().Name == "DutyGroup"); //remove later
                User.Instance.Character.Groups.Add(dutyGroup);
                World.Instance.SendTextEnteredDuty(sender);
                questDirector.StartEvent(sender);
                World.Instance.ChangeZone(sender, new Position(193, -5f, 16.35f, 6f, 0.5f, 16));
            }
            else
            {
                switch (selection)
                {
                    case 1:
                        InitLuaParameters();
                        DelegateEvent(sender, QuestId, "processEvent000_2", new object[] { null, null, null, null });
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

            if (FunctionName == "processTtrBtl002")
            {
                int sleepSeconds = 3;
                SendData(sender, new object[] { 0x05 }, sleepSeconds);                
                SendData(sender, new object[] { 0x02, null, null, 0x235f }, sleepSeconds);               
                SendData(sender, new object[] { 0x04, null, null, 0x01, 0x0C }, sleepSeconds);               
                SendData(sender, new object[] { 0x05 }, sleepSeconds);                
                SendData(sender, new object[] { 0x04, null, null, 0x01, 0x0D }, sleepSeconds);                
                SendData(sender, new object[] { 0x05 }, sleepSeconds);                
                SendData(sender, new object[] { 0x02, null, null, 0x2369 }, sleepSeconds);                
                SendData(sender, new object[] { "attention", (uint)0x5FF80001, "", 0xC781, 0x01 }); //second parameter is world id
               
                User.Instance.Character.ToggleStance(sender, Command.NormalStance);
                ((QuestDirector)World.Instance.GetDirector("Quest")).StartEvent(sender, "noticeEvent");
            }
        }
    }

}
