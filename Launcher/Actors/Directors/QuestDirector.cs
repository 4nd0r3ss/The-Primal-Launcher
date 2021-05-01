using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class QuestDirector : Actor
    {
        public string QuestFunction { get; set; }

        public QuestDirector(uint id)
        {
            Id = id;
            ClassName = "QuestDirector";
            ClassCode = 0x30400000;
            Appearance.BaseModel = 0;
            Appearance.Size = 0;
            SkinColor = 0;
            HairColor = 0;
            EyeColor = 0;
            NameId = -1;
        }

        public void Prepare()
        {
            Zone zone = World.Instance.Zones.Find(x => x.Id == User.Instance.Character.Position.ZoneId);
            string zoneName = MinifyMapName(zone.MapName, zone.PrivLevel);   

            LuaParameters = new LuaParameters
            {
                ActorName = "questDirect" + "_"+ zoneName + "_" + (Id & 0xff).ToString("X2") + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
                ClassName = ClassName + QuestFunction,
                ClassCode = ClassCode
            };

            LuaParameters.Add("/Director/Quest/" + ClassName + QuestFunction);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);

            Events = new List<Event>();
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Priority = 0x0e });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeRequest", Silent = 0x01 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "reqForChild", Silent = 0x01 });
        }

        public void Spawn(Socket sender)
        {           
            Prepare();
            CreateActor(sender);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender);
            SetName(sender);
            SetMainState(sender);
            SetIsZoning(sender);
            LoadScript(sender);
            Getwork(sender);
            QuestFunction = "";
        }

        public void Getwork(Socket sender)
        {
            byte[] data = new byte[0x88];
            string init = "/_init";
            Buffer.BlockCopy(BitConverter.GetBytes(0x8807), 0, data, 0, sizeof(ushort)); //TODO: 88 wrapper byte, 07 total bytes. change this to work property class.
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(init), 0, data, 0x02, init.Length);
            SendPacket(sender, ServerOpcode.ActorInit, data);
        }
    }
}
