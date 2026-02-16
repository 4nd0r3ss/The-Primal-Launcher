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
    public class GuildLeve
    {
        public int Id { get; set; }
        public int Reward1 { get; set; }
        public int Reward1Qty { get; set; }
        public int Reward2 { get; set; }
        public int Reward2Qty { get; set; }

        public GuildLeve(XmlNode node)
        {
            Id = node.GetAttributeAsInt("id");
            Reward1 = node.GetAttributeAsInt("reward1");
            Reward1Qty = node.GetAttributeAsInt("reward1Qty");
            Reward2 = node.GetAttributeAsInt("reward2");
            Reward2Qty = node.GetAttributeAsInt("reward2Qty");
        }
    }
}
