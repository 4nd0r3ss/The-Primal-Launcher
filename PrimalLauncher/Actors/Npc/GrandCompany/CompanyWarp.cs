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
using System.Text;
using System.Xml;

namespace PrimalLauncher
{
    class CompanyWarp : Populace
    {
        public uint Region { get; set; }
        public uint Zone { get; set; }
        public int WarpId { get; set; }

        public CompanyWarp()
        {
            ClassName = "PopulaceCompanyWarp";
            Events.Add(new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 4, Enabled = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Priority = 0, Enabled = 0, Silent = 0 });
        }

        #region Event methods
        public override void talkDefault()
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                LuaParameters parameters = new LuaParameters()
                {
                    Parameters = new object[]
                    {
                            (sbyte)0x05,
                            Encoding.ASCII.GetBytes("talkDefault"),
                            Encoding.ASCII.GetBytes("eventAskMainMenu"),
                            User.Instance.Character.Id,
                            WarpId
                    }
                };

                byte[] data = new byte[0x298];
                Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
                LuaParameters.WriteParameters(ref data, parameters, 0x08);
                Packet.Send(ServerOpcode.EventRequestResponse, data);
                EventManager.Instance.CurrentEvent.Callback = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }
            else
            {
                uint selection = (uint)EventManager.Instance.CurrentEvent.Selection[0];

                //if a warp location was selected
                if (selection > 0)
                {
                    //we warp to somewhere near the selected location warp npc.
                    Position destination = GetPositionById(Region, selection);

                    if (destination != null)
                    {
                        destination.X += (float)(3 * Math.Sin(destination.R));
                        destination.Z += (float)(3 * Math.Cos(destination.R));
                        destination.R += 0.8f;
                    }
                    else
                    {
                        destination = EntryPoints.GetTownWarpExit(Region, selection);
                    }

                    EventManager.Instance.CurrentEvent.Finish();
                    World.SendTextSheet(0x8539);
                    World.Instance.TeleportPlayer(destination);
                }
                else //selected quit
                {
                    EventManager.Instance.CurrentEvent.Finish();
                }
            }
        }
        private static Position GetPositionById(uint region, uint id)
        {
            XmlDocument npcFile = new XmlDocument();
            npcFile.LoadFromResource("CompanyWarp.xml");

            try
            {
                XmlElement root = npcFile.DocumentElement;
                XmlNode chara = root.FirstChild;

                //each npc node in xml 
                foreach (XmlNode node in chara.ChildNodes)
                {
                    if (Convert.ToUInt32(node.Attributes["region"].Value) == region && Convert.ToUInt32(node.Attributes["id"].Value) == id)
                        return new Position(node.SelectSingleNode("position"), Convert.ToUInt32(node.Attributes["zone"].Value));
                }
            }
            catch (Exception e)
            {
                Log.Instance.Warning(e.Message);
            }

            return null;
        }
        #endregion 
    }
}
