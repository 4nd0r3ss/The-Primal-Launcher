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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrimalLauncher
{
    public static class GuildLeveXmlLoader
    {
        public static GuildLevePackSet GetGuildLevePackSet(int id)
        {
            XmlDocument guildLevesFile = new XmlDocument();
            guildLevesFile.LoadFromResource("GuildLeves.xml");

            List<Aetheryte> result = new List<Aetheryte>();

            if (guildLevesFile.HasChildNodes)
            {
                try
                {
                    XmlElement root = guildLevesFile.DocumentElement;
                    XmlNode list = root;

                    foreach (XmlNode node in list.ChildNodes)
                    {
                        if (node.GetAttributeAsInt("id") == id)
                            return new GuildLevePackSet(node);
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }

            return null;
        }

        public static List<PassiveGL> GetPassiveGLs(uint region)
        {
            XmlDocument passiveGls = new XmlDocument();
            passiveGls.LoadFromResource("PassiveGLs.xml");
            List<PassiveGL> result = new List<PassiveGL>();

            if (passiveGls.HasChildNodes)
            {
                try
                {
                    XmlElement root = passiveGls.DocumentElement;
                    XmlNode list = root;
                    foreach (XmlNode node in list.ChildNodes)
                    {
                        uint nodeRegion = (uint)node.GetAttributeAsInt("region");

                        if (nodeRegion == region)
                        {
                            result.Add(new PassiveGL
                            {
                                Id = (uint)node.GetAttributeAsInt("id"),
                                Class = node.GetAttributeAsInt("class"),
                                Level = node.GetAttributeAsInt("level"),
                                Region = nodeRegion
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }
            return result;
        }
    }
}
