using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class PrivateArea : Zone
    {
        public uint ParentZoneId { get; set; }

        public PrivateArea()
        {
                               
        }

        public override void Prepare()
        {
            ClassName = "PrivateAreaMasterPast";
            LuaParameters = new LuaParameters
            {
                ActorName = "_areaMaster" + "@0" + LuaParameters.SwapEndian(Id).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = 0x30400000
            };

            LuaParameters.Add("/Area/PrivateArea/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(MapName);
            LuaParameters.Add(ClassName);
            LuaParameters.Add(1);
            LuaParameters.Add((byte)0);

            for (int i = 7; i > -1; i--)
                LuaParameters.Add(((byte)Type & (1 << i)) != 0);
        }
    }
}
