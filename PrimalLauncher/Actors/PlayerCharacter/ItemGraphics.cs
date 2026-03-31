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
using System.Linq;
using System.Xml;

namespace PrimalLauncher
{
    public class  ItemGraphics
    {
        private static ItemGraphics _instance = null;
        public Dictionary<uint, uint> Throwing { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Weapon { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Head { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Body { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Legs { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Hands { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Feet { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Waist { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Wrist { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Ears { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Neck { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, uint> Finger { get; set; } = new Dictionary<uint, uint>();

        private ItemGraphics()
        {
            Load();
        }

        public static ItemGraphics Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemGraphics();
                }
                return _instance;
            }
        }

        public uint[] GetByGraphicsId(uint[] graphicsIds)
        {
            uint[] result = new uint[graphicsIds.Length];

            result[1] = Weapon.FirstOrDefault(x => x.Value == graphicsIds[1]).Key;
            result[8] = Head.FirstOrDefault(x => x.Value == graphicsIds[8]).Key;
            result[9] = Body.FirstOrDefault(x => x.Value == graphicsIds[9]).Key;
            result[10] = Legs.FirstOrDefault(x => x.Value == graphicsIds[10]).Key;
            result[11] = Hands.FirstOrDefault(x => x.Value == graphicsIds[11]).Key;
            result[12] = Feet.FirstOrDefault(x => x.Value == graphicsIds[12]).Key;
            result[13] = Waist.FirstOrDefault(x => x.Value == graphicsIds[13]).Key;

            return result;
        }

        public void Load()
        {
            XmlDocument itemGraphicsFile = new XmlDocument();
            itemGraphicsFile.LoadFromResource("ItemGraphics.xml");

            if (itemGraphicsFile.HasChildNodes)
            {
                try
                {
                    XmlElement root = itemGraphicsFile.DocumentElement;
                    XmlNode list = root;

                    foreach (XmlNode childNode in list.ChildNodes)
                    {
                        switch(childNode.Name)
                        {
                            case "Throwing":
                                Throwing = LoadItems(childNode);
                                break;
                            case "Weapons":
                                Weapon = LoadItems(childNode);
                                break;
                            case "Head":
                                Head = LoadItems(childNode);
                                break;
                            case "Body":
                                Body = LoadItems(childNode);
                                break;
                            case "Legs":
                                Legs = LoadItems(childNode);
                                break;
                            case "Hands":
                                Hands = LoadItems(childNode);
                                break;
                            case "Feet":
                                Feet = LoadItems(childNode);
                                break;
                            case "Waist":
                                Waist = LoadItems(childNode);
                                break;
                            case "Wrist":
                                Wrist = LoadItems(childNode);
                                break;
                            case "Ears":
                                Ears = LoadItems(childNode);
                                break;
                            case "Neck":
                                Neck = LoadItems(childNode);
                                break;
                            case "Finger":
                                Finger = LoadItems(childNode);
                                break;
                        }
                    }
                }
                catch { }
            }                 
        }

        private Dictionary<uint, uint> LoadItems(XmlNode node)
        {
            Dictionary<uint, uint> result = new Dictionary<uint, uint>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                uint itemId = childNode.GetAttributeAsUint("itemId");
                uint graphicsId = childNode.GetAttributeAsUint("graphicsId");
                result.Add(itemId, graphicsId);
            }

            return result;
        }
    }
}
