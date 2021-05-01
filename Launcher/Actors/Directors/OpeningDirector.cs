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
            Zone zone = World.Instance.Zones.Find(x => x.Id == User.Instance.Character.Position.ZoneId);
            string zoneName = MinifyMapName(zone.MapName, zone.PrivLevel);

            LuaParameters = new LuaParameters
            {
                ActorName = MinifyClassName() + "_" + zoneName + "_" + (Id & 0xff).ToString("X2") + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = ClassCode
            };

            LuaParameters.Add("/Director/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);

            Events = new List<Event>();
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Priority = 0x0e });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeRequest", Enabled = 0x01 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "reqForChild", Enabled = 0x01 });
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
            Buffer.BlockCopy(BitConverter.GetBytes(0x8807), 0, data, 0, sizeof(ushort)); //TODO: 88 wrapper byte, 07 total bytes. change this to work property class.
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(init), 0, data, 0x02, init.Length);
            SendPacket(sender, ServerOpcode.ActorInit, data);
        }

        #region Event methods
        public void StartEvent(Socket sender, string functionName = null)
        {          
            byte[] data = new byte[0x70];
            uint characterId = User.Instance.Character.Id;
            Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);

            Buffer.BlockCopy(BitConverter.GetBytes(0x75dc1705), 0, data, 0x08, 4); //the last byte is the event type (05). the other bytes are unknown.
            Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);
            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("noticeEvent")); 
            
            if(functionName == null)            
                parameters.Add(true);            
            else
                parameters.Add(functionName);

            LuaParameters.WriteParameters(ref data, parameters, 0x10);
            SendPacket(sender, ServerOpcode.StartEvent, data, sourceId: characterId);            
        }

        public override void noticeEvent(Socket sender)
        {           
            //EventManager.Instance.CurrentEvent.DelegateEvent(sender, 0xA0F1ADB1, "processTtrBtl001");           
        }
        #endregion
    }
}
