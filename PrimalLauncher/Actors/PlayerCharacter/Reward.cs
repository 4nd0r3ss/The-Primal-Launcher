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
    [Serializable]
    public class Reward
    {
        public uint ItemId { get; set; }

        public int Quantity
        {
            get
            {
                return GetRewardQuantity();
            }
        }
        private int QuantityMax { get; set; }
        private int QuantityMin { get; set; }

        public Reward(XmlNode node)
        {

        }

        private int GetRewardQuantity()
        {
            if(ItemId == 1000001)
            {
                int avg = (QuantityMax + QuantityMin) / 2;
                int range = QuantityMax - QuantityMin;
                int result = avg + (int)(new Random().NextDouble() * range - range / 2);

                return result;
            }
            else
            {
                return QuantityMax;
            }            
        }
    }
}
