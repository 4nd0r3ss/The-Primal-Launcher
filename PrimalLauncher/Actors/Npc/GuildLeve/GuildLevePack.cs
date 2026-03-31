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


using System.Collections.Generic;
using System.Xml;

namespace PrimalLauncher
{
    public class GuildLevePack
    {
        public int Id { get; set; }
        public int Level { get; set; }
       
        public List<GuildLeve> GuildLeves { get; set; }

        public GuildLevePack(XmlNode node) 
        { 
            Id = node.GetAttributeAsInt("id");
            Level = node.GetAttributeAsInt("level");
            GuildLeves = new List<GuildLeve>();

            foreach(XmlNode node2 in node.ChildNodes)
            {
                GuildLeves.Add(new GuildLeve(node2));
            }
        }      
    }
}
