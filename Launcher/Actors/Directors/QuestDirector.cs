using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class QuestDirector : Director
    {
        public void Prepare(string questFunction)
        {
            Zone zone = World.Instance.Zones.Find(x => x.Id == User.Instance.Character.Position.ZoneId);
            string zoneName = MinifyMapName(zone.MapName);   

            LuaParameters = new LuaParameters
            {
                ActorName = "questDirect" + "_"+ zoneName + "_" + (Id & 0xff).ToString("X2") + "@0" + LuaParameters.SwapEndian(User.Instance.Character.Position.ZoneId).ToString("X").Substring(0, 4),
                ClassName = ClassName + questFunction,
                ClassCode = ClassCode
            };

            LuaParameters.Add("/Director/Quest/" + ClassName + questFunction);
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

        public void Spawn(Socket sender, string questFunction = "")
        {
            Prepare(questFunction);
            CreateActor(sender);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender);
            SetName(sender);
            SetMainState(sender);
            SetIsZoning(sender);
            SetLuaScript(sender);
            Getwork(sender);           
        }      
    }
}
