using System;
using System.Net.Sockets;

namespace PrimalLauncher
{
    /// <summary>
    /// 
    /// </summary>
    class ZoneSwitch : Populace
    {
        public ZoneSwitch()
        {
            ClassName = "PopulaceStandard";
        }

        public override void pushDefault(Socket sender)
        {
            string action = Events.Find(x => x.Name == "pushDefault").Action;

            //if it has commas, we are changing regions. 
            if(action.IndexOf(",") > 0)
            {
                string[] split = action.Split(new char[] { ',' });

                Position position = new Position
                {
                    ZoneId = Convert.ToUInt32(split[0]),
                    X = Convert.ToSingle(split[1]),
                    Y = Convert.ToSingle(split[2]),
                    Z = Convert.ToSingle(split[3]),
                    R = Convert.ToSingle(split[4])
                };

                World.Instance.ChangeZone(sender, position: position, spawnType: 0x0F);
            }
            else
            {
                ChangeMap(sender, action);
            }    
        }

        public static void ChangeMap(Socket sender, string action)
        {
            uint toZoneId = Convert.ToUInt32(action);
            uint fromZoneId = User.Instance.Character.Position.ZoneId;

            if (toZoneId != fromZoneId)
            {
                Zone toZone = World.Instance.Zones.Find(x => x.Id == toZoneId);
                Zone fromZone = User.Instance.Character.GetCurrentZone();

                User.Instance.Character.Position.ZoneId = toZoneId;
                fromZone.DespawnAllActors(sender);
                fromZone.Despawn(sender);
                toZone.LoadActors();
                toZone.Spawn(sender);

                World.Instance.SetMusic(sender, toZone.GetCurrentBGM(), MusicMode.FadeStart);
                User.Instance.Character.ToggleZoneActors(sender);
                Log.Instance.Success("You arrived at " + toZone.LocationName + " (" + toZone.Id + ").");
            }
        }

        public static void ChangeRegion(Socket sender, string action)
        {
            

        }
    }
}
