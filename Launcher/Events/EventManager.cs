using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
            Type type = Type.GetType(GetEventType(data));
            CurrentEvent = (EventRequest)Activator.CreateInstance(type, data);
            CurrentEvent.Execute(sender);                 
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

        public byte Code { get; set; }
        public string Name { get; set; }

        public byte[] Data { get; set; }

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
                eventOwner.InvokeMethod(GetType().Name, new object[] { sender });
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");
        }

        private void Response(Socket sender)
        {
            byte[] data = new byte[0xb0];
            Buffer.BlockCopy(BitConverter.GetBytes(CallerId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(OwnerId), 0, data, 0x04, 4);
            LuaParameters.WriteParameters(ref data, RequestParameters, 0x08);
            SendPacket(sender, ServerOpcode.EventRequestResponse, data);
        }

        public void Finish(Socket sender)
        {
            byte[] data = new byte[0x30];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            data[0x08] = 1;
            string name = GetType().Name;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(name), 0, data, 0x09, name.Length);
            SendPacket(sender, ServerOpcode.EventRequestFinish, data);
            EventManager.Instance.CurrentEvent = null;
        }

        #region helper methods
        public void SendPacket(Socket sender, ServerOpcode opcode, byte[] data, uint sourceId = 0, uint targetId = 0)
        {
            sender.Send(new Packet(new GamePacket{ Opcode = (ushort)opcode, Data = data }).ToBytes());
        }        

        public Actor GetActor()
        {
            Actor eventOwner = World.Instance.Actors.FirstOrDefault(x => x.Id == OwnerId);

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

        public virtual void DelegateEvent(Socket sender, uint questId, string functionName)
        {
            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateEvent"));
            RequestParameters.Add(CallerId);
            RequestParameters.Add(questId);
            RequestParameters.Add(functionName);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            Response(sender);
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
                case Command.UmountChocobo:
                    User.Instance.Character.ToggleMount(sender, Command.UmountChocobo);                   
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
            int questId = data[0x32] << 24 | data[0x33] << 16 | data[0x34] << 8 | data[0x35];
            RequestParameters.Add("requestedData");            
            RequestParameters.Add("qtdata");
            RequestParameters.Add(questId);
            RequestParameters.Add(0);
            LuaParameters.WriteParameters(ref data, RequestParameters, 0);
            SendPacket(sender, ServerOpcode.GeneralData, data);
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
            SendPacket(sender, ServerOpcode.GeneralData, data);
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
                case Command.MountChocobo:
                    User.Instance.Character.ToggleMount(sender, Command.MountChocobo);                    
                    break;

            }
            
            Finish(sender);
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

        public override void Execute(Socket sender)
        {
            Actor eventOwner = GetActor();

            if (eventOwner != null)
            {
                foreach (Quest quest in User.Instance.Character.Quests)
                {
                    string stepResult = quest.ActorStepComplete(sender, eventOwner.ClassId, GetType().Name);

                    if (stepResult != null)
                    {
                        DelegateEvent(sender, 0xA0F00000 | quest.Id, stepResult);                       
                        return;
                    }
                }

                eventOwner.InvokeMethod(GetType().Name, new object[] { sender });
            }
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");
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
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        private void AddEventInfo(byte code, string eventName)
        {
            RequestParameters.Add((sbyte)code);
            RequestParameters.Add(Encoding.ASCII.GetBytes(eventName));
        }

        public override void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Actor: 0x" + OwnerId.ToString("X"));

            Actor eventOwner = GetActor();            

            if (eventOwner != null)
            {
                foreach (Quest quest in User.Instance.Character.Quests)
                {
                    string stepResult = quest.ActorStepComplete(sender, eventOwner.ClassId, GetType().Name);

                    if (stepResult != null)
                    {
                        DelegateEvent(sender, 0xA0F00000 | quest.Id, stepResult);
                        return;
                    }
                }

                eventOwner.InvokeMethod(GetType().Name, new object[] { sender });               
            }               
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");
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

       
    }

}
