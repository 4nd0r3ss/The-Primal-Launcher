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
        public Event CurrentEvent { get; set; } = null;

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
            CurrentEvent = (Event)Activator.CreateInstance(type, data);
            CurrentEvent.Execute(sender);
            Log.Instance.Warning("Event: " + CurrentEvent.GetType().Name + ", Actor: 0x" + CurrentEvent.OwnerId.ToString("X"));        
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
    public class Event
    {
        public LuaParameters RequestParameters { get; set; }

        public uint CallerId { get; set; }
        public uint OwnerId { get; set; }

        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

        public byte Code { get; set; }
        public string Name { get; set; }

        public byte[] Data { get; set; }

        public Event(byte[] data)
        {
            RequestParameters = new LuaParameters();

            CallerId = (uint)(data[0x13] << 24 | data[0x12] << 16 | data[0x11] << 8 | data[0x10]);
            OwnerId = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x14]);
            Unknown1 = (uint)(data[0x1b] << 24 | data[0x1a] << 16 | data[0x19] << 8 | data[0x18]);
            Unknown2 = (uint)(data[0x1f] << 24 | data[0x1e] << 16 | data[0x1d] << 8 | data[0x1c]);
        }       

        public void EndClientOrder(Socket sender, string eventType)
        {
            byte[] data = new byte[0x30];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            data[0x08] = 1;
            string name = GetType().Name;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(name), 0, data, 0x09, name.Length);
            SendPacket(sender, ServerOpcode.EndClientOrderEvent, data);
            EventManager.Instance.CurrentEvent = null;
        }

        private void RequestResponse(Socket sender)
        {
            byte[] data = new byte[0xb0];
            Buffer.BlockCopy(BitConverter.GetBytes(CallerId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(OwnerId), 0, data, 0x04, 4);            
            LuaParameters.WriteParameters(ref data, RequestParameters, 0x08);
            SendPacket(sender, ServerOpcode.StartEventRequest, data);
        }

        public void SendPacket(Socket sender, ServerOpcode opcode, byte[] data, uint sourceId = 0, uint targetId = 0)
        {
            sender.Send(new Packet(new GamePacket{ Opcode = (ushort)opcode, Data = data }).ToBytes());
        }

        public virtual void Execute(Socket sender)
        {
            Actor eventOwner = GetActor();

            if (eventOwner != null)
                eventOwner.InvokeMethod(GetType().Name, new object[] { sender });
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");
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

        public static void Trigger()
        {

        }

        public virtual void DelegateEvent(Socket sender, uint quest, string functionName)
        {
            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateEvent"));
            RequestParameters.Add(CallerId);
            RequestParameters.Add(quest);
            RequestParameters.Add(functionName);
            RequestParameters.Add(null);
            RequestParameters.Add(null);
            RequestParameters.Add(null);

            RequestResponse(sender);
        }
    }
      
    public class commandRequest : Event
    {
        public Command CommandId { get; set; }
        public commandRequest(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x14]);
        }

        public override void Execute(Socket sender)
        {
            
        }

        //private 

    }

    public class commandForced : Event
    {
        public commandForced(byte[] data) : base(data)
        {

        }
    }

    public class commandContent : Event
    {
        public commandContent(byte[] data) : base(data)
        {

        }
    }

    public class talkDefault : Event
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

    public class pushDefault : Event
    {
        public pushDefault(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)eventType);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }
    }

    public class noticeEvent : Event
    {
        public noticeEvent(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)eventType);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

       
    }

}
