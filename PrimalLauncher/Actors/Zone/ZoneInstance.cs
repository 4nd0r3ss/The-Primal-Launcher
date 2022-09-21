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

using System.Xml;

namespace PrimalLauncher
{
    public class ZoneInstance : Zone
    {
        public Position StartPoint { get; set; }
        public Position ExitTo { get; set; }
        public uint ZoneId { get; set; }

        public ZoneInstance(XmlNode node)
        {
            StartPoint = new Position(node.SelectSingleNode("startPoint"));
            ExitTo = new Position(node.SelectSingleNode("exitTo"));
            Id = node.GetAttributeAsUint("id");

            Zone zone = World.Instance.GetZone(StartPoint.ZoneId);
            ZoneId = StartPoint.ZoneId;
            StartPoint.ZoneId = Id;

            ClassName = zone.ClassName;
            RegionId = zone.RegionId;           
            LocationName = zone.LocationName;
            MusicSet = zone.MusicSet;
            MapName = zone.MapName;
            Type = ZoneType.Nothing;
            PrivLevel = 1;
            MountAllowed = false;
            ContentFunction = node.GetAttributeAsString("contentFunction");

            LoadActors(node.SelectSingleNode("actors"));
        }

        private void LoadActors(XmlNode node)
        {
            Actors = new System.Collections.Generic.List<Actor>();

            foreach (XmlNode item in node.ChildNodes)
            {
                Actor actor = ActorRepository.LoadActor(item, Id);                

                if (actor != null)
                {
                    actor.Id = 4 << 28 | Id << 19 | (uint)ActorIndex;

                    if (actor is MapObj obj)
                        MapObjects.Add(obj);
                    else
                        Actors.Add(actor);

                    ActorIndex++;
                }
            }
        }

        public void Exit()
        {
            ToggleStoppers(false);
            World.Instance.ChangeZone(ExitTo, 0x0F);
        }
    }
}
