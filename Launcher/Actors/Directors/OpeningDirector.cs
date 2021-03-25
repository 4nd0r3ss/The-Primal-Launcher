using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class OpeningDirector : Actor
    {



        public OpeningDirector(uint id)
        {
            Id = id;
            ClassName = "OpeningDirector";
            ClassCode = 0x30400000;      
            Appearance.BaseModel = 0;
            Appearance.Size = 0;
            SkinColor = 0;
            HairColor = 0;
            EyeColor = 0;
            NameId = -1;
        }

        public override void Prepare(ushort actorIndex = 0)
        {
            //TODO:remove this
            string debugNameNumber = "01";

            if (Id == 0x66080000)
                debugNameNumber = "00";

            LuaParameters = new LuaParameters
            {
                ActorName = "openingDire_ocn0Btl02_"+ debugNameNumber + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = ClassCode
            };

            LuaParameters.Add("/Director/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);

            EventConditions = new List<EventCondition>();
            EventConditions.Add(new EventCondition { Opcode = ServerOpcode.NoticeEvent, EventName = "noticeEvent", Priority = 0x0e });
            EventConditions.Add(new EventCondition { Opcode = ServerOpcode.NoticeEvent, EventName = "noticeRequest", IsDisabled = 0x01 });
            EventConditions.Add(new EventCondition { Opcode = ServerOpcode.NoticeEvent, EventName = "reqForChild", IsDisabled = 0x01 });
        }


        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare(actorIndex);
            CreateActor(sender);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender, spawnType, isZoning);
            SetName(sender);
            SetMainState(sender);
            SetIsZoning(sender);
            LoadActorScript(sender);
            Getwork(sender);            
        }

        public void Getwork(Socket sender)
        {            
            byte[] data = new byte[0x88];
            string init = "/_init";
            Buffer.BlockCopy(BitConverter.GetBytes(0x8807), 0, data, 0, sizeof(ushort));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(init), 0, data, 0x02, init.Length);
            SendPacket(sender, ServerOpcode.ActorInit, data);
        }       

        public void StartClientOrderEvent(Socket sender)
        {          
            byte[] data = new byte[0x70];
            uint characterId = User.Instance.Character.Id;
            Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(0x75dc1705), 0, data, 0x08, 4); //the last byte is the event type (05). the other bytes are unknown.
            Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);

            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("noticeEvent"));            
            parameters.Add(true);

            LuaParameters.WriteParameters(ref data, parameters, 0x10);
            SendPacket(sender, ServerOpcode.StartEvent, data, sourceId: characterId);            
        }      

        public void ProcessEventRequest(Socket sender, ClientEventRequest eventRequest)
        {
            switch (eventRequest.Code)
            {
                case 0x05:
                    NoticeEvent(sender, eventRequest);
                    break;
            }
        }

        private void NoticeEvent(Socket sender, ClientEventRequest eventRequest)
        {
            byte[] data = new byte[0x298]; 
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
            data[0x08] = eventRequest.Code;

            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("noticeEvent"));
            parameters.Add(Encoding.ASCII.GetBytes("delegateEvent"));
            parameters.Add((Command)eventRequest.CallerId);
            parameters.Add(unchecked((Command)0xa0f1adb1));
            parameters.Add("processTtrNomal001withHQ");
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);           

            LuaParameters.WriteParameters(ref data, parameters, 0x09);   
            SendPacket(sender, ServerOpcode.StartEventRequest, data);
        }
    }
}
