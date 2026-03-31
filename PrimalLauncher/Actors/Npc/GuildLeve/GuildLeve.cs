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
using System.Xml.Linq;

namespace PrimalLauncher
{
    [Serializable]
    public class GuildLeve
    {
        public int Id { get; set; }

        public bool Done { get; set; }
        public bool Checked { get; set; }

        public int OfferLimit { get; set; }
        public int RewardItem { get; set; }
        public int RewardNumber { get; set; }
        public int RewardSubItem { get; set; }
        public int RewardSubNumber { get; set; }
        public int Evaluation { get; set; }
        public bool StageVisible { get; set; }

        public GuildLeve(XmlNode node)
        {
            Id = node.GetAttributeAsInt("id");
            Done = true;
            Checked = true;
            RewardItem = node.GetAttributeAsInt("reward1");
            RewardNumber = node.GetAttributeAsInt("reward1Qty");
            RewardSubItem = node.GetAttributeAsInt("reward2");
            RewardSubNumber = node.GetAttributeAsInt("reward2Qty");
        }

        public GuildLeve(int id)
        {
            Id = id;
            Done = true;
            Checked = true;
        }

        public GuildLeve() { }
    }
}
