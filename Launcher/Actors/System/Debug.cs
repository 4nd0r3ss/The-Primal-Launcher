using System.Net.Sockets;
using System.Text;

namespace Launcher
{
    public class Debug : Actor
    {
        public Debug()
        {
            Name = Encoding.ASCII.GetBytes("debug");
            Id = 0x5ff80002; //id from hardcoded packet (just bc it works)                  
        }

        public override void Spawn(Socket handler, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(handler);
            SetSpeeds(handler);
            SetPosition(handler, 1, isZoning);
            SetName(handler);
            SetMainState(handler);
            SetIsZoning(handler);
            SetLuaScript(handler);            
        }

        public override void Prepare()
        {
            LuaParameters = new LuaParameters
            {
                ActorName = "debug",
                ClassName = "Debug",
                ClassCode = 0x30400000
            };

            LuaParameters.Add("/System/Debug.prog");
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add((int)0xc51f); //???
            LuaParameters.Add(true);
            LuaParameters.Add(true);            
        }
    }
}
