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
    public class GuildLevePackSet
    {
        public int Id { get; }
        private List<GuildLevePack> GuildLevePacks { get; }

        public GuildLevePackSet(XmlNode node)
        {
            Id = node.GetAttributeAsInt("id");
            GuildLevePacks = new List<GuildLevePack>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                GuildLevePacks.Add(new GuildLevePack(childNode));
            }
        }

        private int GetPackLevel()
        {
            short playerLevel = User.Instance.Character.CharaWork.CurrentClass.Level;
            int packLevel = 1;

            if (playerLevel >= 10 && playerLevel < 20)
                packLevel = 10;
            else if (playerLevel >= 20 && playerLevel < 30)
                packLevel = 20;
            else if (playerLevel >= 30 && playerLevel < 40)
                packLevel = 30;
            else
                packLevel = 40;

            return packLevel;
        }

        public List<object> GetStartEndPacks()
        {           
            List<object> result = new List<object>();
            
            //start pack
            result.Add(GuildLevePacks.OrderBy(x => x.Id).First().Id);

            //end pack   
            int endPack = GuildLevePacks.Where(x => x.Level == GetPackLevel()).OrderByDescending(x => x.Id).FirstOrDefault().Id;
            result.Add(endPack);

            return result;
        }

        public List<object> GetGuildLevesFromPack(int packId)
        {
            List<object> result = new List<object>();
            GuildLevePack selectedPack = GuildLevePacks.FirstOrDefault(x => x.Id == packId);

            if (selectedPack != null)
            {
                foreach(var leve in selectedPack.GuildLeves)
                {
                    result.Add(leve.Id);
                }
            }

            return result;
        }
    }
}
