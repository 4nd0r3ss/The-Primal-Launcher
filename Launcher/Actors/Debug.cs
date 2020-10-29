using System.Net.Sockets;

namespace Launcher
{
    public class Debug : Actor
    {
        public Debug()
        {
            //Name = "debug",
            Id = 0x5ff80002; //id from hardcoded packet (just bc it works)     
            TargetId = UserRepository.Instance.User.Character.Id;  
            //Spawn(sender, 0x01);       
        }

        public override void Spawn(Socket handler, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare(actorIndex);
            CreateActor(handler, 0x08);
            SetSpeeds(handler, Speeds);
            SetPosition(handler, Position, spawnType, isZoning);
            SetName(handler);
            SetIsZoning(handler);
            LoadActorScript(handler, LuaParameters);
            ActorInit(handler);
        }

        public override void Prepare(ushort actorIndex = 0)
        {
            LuaParameters = new LuaParameters
            {
                ActorName = "debug",
                ClassName = "Debug"
            };

            LuaParameters.Add("/System/Debug.prog");
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add((uint)0xc51f); //???
            LuaParameters.Add(true);
            LuaParameters.Add(true);

            Speeds = new uint[] { 0, 0, 0, 0 };

            Position = new Position();
        }
    }
}
