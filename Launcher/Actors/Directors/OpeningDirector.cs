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
            LuaParameters = new LuaParameters
            {
                ActorName = MinifyClassName() + "_ocn0Btl02_" + (Id & 0xff).ToString("X2") + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
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
            LoadScript(sender);
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

        #region Event methods
        public void StartEvent(Socket sender)
        {          
            byte[] data = new byte[0x70];
            uint characterId = User.Instance.Character.Id;
            Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);

            Buffer.BlockCopy(BitConverter.GetBytes(0x00000005), 0, data, 0x08, 4); //the last byte is the event type (05). the other bytes are unknown.
            Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);
            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("noticeEvent"));            
            parameters.Add(true);

            LuaParameters.WriteParameters(ref data, parameters, 0x10);
            SendPacket(sender, ServerOpcode.StartEvent, data, sourceId: characterId);            
        }

        public override void noticeEvent(Socket sender)
        {           
            EventManager.Instance.CurrentEvent.DelegateEvent(sender, 0xA0F1ADB1, "processTtrNomal001withHQ");           
        }
        #endregion
    }
}
