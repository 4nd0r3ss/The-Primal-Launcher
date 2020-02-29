using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class Debug : Actor
    {
        public Debug()
        {
            //Name = "debug",
            Id = 0x5ff80002; //id from hardcoded packet (just bc it works)     
            TargetId = UserFactory.Instance.User.Character.Id;
           
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

            //Spawn(sender, 0x01);
        }

    }
}
