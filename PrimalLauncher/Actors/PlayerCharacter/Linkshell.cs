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
using System.IO;
using System.Text;

namespace PrimalLauncher
{
    [Serializable]
    public class NpcLinkshell
    {
        public bool[] Extra { get; set; }
        public bool[] IsCalling { get; set; }

        public NpcLinkshell()
        {
            Extra = new bool[34];
            IsCalling = new bool[34];
        }

        public void AddLinkpearl(int id)
        {
            Extra[id - 1] = true;
            LinkpearlUpdateWork(id, true, false);
            World.SendTextSheet(0x621E, new object[] { id });
        }

        public void NpcNewMessage(int id)
        {
            LinkpearlUpdateWork(id, true, true);
            World.SendTextSheet(0x621F, new object[] { id });
        }

        public void HasMessage(int id) => LinkpearlUpdateWork(id, false, true);
        public void NoMessage(int id) => LinkpearlUpdateWork(id, true, false);

        private void LinkpearlUpdateWork(int id, bool owned, bool calling)
        {
            WorkProperties work = new WorkProperties(User.Instance.Character.Id, @"playerWork/npcLinkshellChat");
            work.Add(string.Format("playerWork.npcLinkshellChatExtra[{0}]", (id - 1)), owned);
            work.Add(string.Format("playerWork.npcLinkshellChatCalling[{0}]", (id - 1)), calling);
            work.FinishWritingAndSend();
        }

        public void AddToWork(ref WorkProperties work)
        {
            for (int i = 0; i < Extra.Length; i++)
            {
                work.Add(string.Format("playerWork.npcLinkshellChatCalling[{0}]", i), Extra[i]);
                work.Add(string.Format("playerWork.npcLinkshellChatExtra[{0}]", i), IsCalling[i]);
            }
        }

        public static void StartNPCLinkshell()
        {
            byte[] data = new byte[0x88];
            bool isKicked = false;
            string kickedName = "kickedName";
            string lsName = "ls name";

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (BinaryWriter binWriter = new BinaryWriter(mem))
                {
                    binWriter.Write((ushort)(isKicked ? 1 : 0));

                    if (kickedName != null && isKicked)
                        binWriter.Write(Encoding.ASCII.GetBytes(kickedName), 0, Encoding.ASCII.GetByteCount(kickedName) >= 0x20 ? 0x20 : Encoding.ASCII.GetByteCount(kickedName));

                    binWriter.Seek(0x22, SeekOrigin.Begin);
                    binWriter.Write(Encoding.ASCII.GetBytes(lsName), 0, Encoding.ASCII.GetByteCount(lsName) >= 0x20 ? 0x20 : Encoding.ASCII.GetByteCount(lsName));
                }
            }

            Packet.Send(ServerOpcode.StartNPCLinkshell, data);
        }
    }
}
