using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class OpeningDirector : Director
    {
        public override void Prepare()
        {
            Zone zone = World.Instance.Zones.Find(x => x.Id == User.Instance.Character.Position.ZoneId);
            string zoneName = MinifyMapName(zone.MapName);

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
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeRequest", Enabled = 0x01, Silent = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "reqForChild", Enabled = 0x01, Silent = 1 });
        } 
    }
}
