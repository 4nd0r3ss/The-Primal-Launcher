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
using System.Collections.Generic;
using System.Xml;

namespace PrimalLauncher
{
    public static class ZoneRepository
    {
        public static List<Zone> GetZones()
        {
            List<Zone> zones = new List<Zone>();
            XmlDocument zonesXml = new XmlDocument();
            zonesXml.LoadFromResource("ZoneList.xml");

            if (zonesXml.HasChildNodes)
            {                
                XmlElement root = zonesXml.DocumentElement;
                               
                foreach (XmlNode node in root.ChildNodes)
                {
                    Type type = Type.GetType("PrimalLauncher." + node.Name);
                    Zone zone = (Zone)Activator.CreateInstance(type);
                  
                    zone.ClassName = "ZoneMaster" + node.Attributes["className"].Value;
                    zone.RegionId = Convert.ToUInt32(node.Attributes["regionId"].Value);
                    zone.Id = Convert.ToUInt32(node.Attributes["id"].Value);
                    zone.LocationName = node.Attributes["locationName"].Value;
                    zone.MusicSet = MusicSet.Get(Convert.ToInt32(node.Attributes["musicSetId"].Value));                    
                    zone.MapName = node.Attributes["mapName"].Value;
                    zone.Type = node.Attributes["type"] != null ? (ZoneType)Enum.Parse(typeof(ZoneType), node.Attributes["type"].Value) : ZoneType.Default;
                    zones.Add(zone);
                }
            }

            Log.Instance.Success("Loaded " + zones.Count + " zones.");

            return zones;
        }  
        
        public static ZoneInstance GetInstance(uint id)
        {
            XmlDocument zonesXml = new XmlDocument();
            zonesXml.LoadFromResource("InstanceList.xml");

            if (zonesXml.HasChildNodes)
            {
                XmlElement root = zonesXml.DocumentElement;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.GetAttributeAsUint("id") == id)
                        return new ZoneInstance(node);
                }
            }

            return null;
        }
    }
}
