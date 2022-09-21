/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class Debug : Actor
    {
        public Debug()
        {
            Name = Encoding.ASCII.GetBytes("debug");
            Id = 0x5ff80002; //id from hardcoded packet (just bc it works)                  
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor();
            SetSpeeds();
            SetPosition(1, isZoning);
            SetName();
            SetMainState();
            SetIsZoning();
            SetLuaScript();            
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
