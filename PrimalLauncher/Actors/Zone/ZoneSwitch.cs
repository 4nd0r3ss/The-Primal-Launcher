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

using System;

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

        public override void pushDefault()
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

                World.Instance.ChangeZone(position: position, spawnType: 0x0F);
            }
            else
            {
                ChangeMap(action);
            }    
        }

        public static void ChangeMap(string action)
        {
            uint toZoneId = Convert.ToUInt32(action);
            uint fromZoneId = User.Instance.Character.Position.ZoneId;

            if (toZoneId != fromZoneId)
            {
                Zone toZone = World.Instance.GetZone(toZoneId);
                Zone fromZone = User.Instance.Character.GetCurrentZone();

                User.Instance.Character.Position.ZoneId = toZoneId;
                fromZone.DespawnAllActors();
                fromZone.Despawn();
                toZone.LoadActors();
                toZone.Spawn();

                World.Instance.SetMusic(toZone.GetCurrentBGM(), MusicMode.FadeStart);
                User.Instance.Character.ToggleZoneActors();
                User.Instance.Character.Journal.InitializeQuests();
                Log.Instance.Success("You arrived at " + toZone.LocationName + " (" + toZone.Id + ").");
            }
        }

        public static void ChangeRegion(string action)
        {
            

        }
    }
}
