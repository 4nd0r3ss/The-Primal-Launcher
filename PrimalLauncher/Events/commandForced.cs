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

using System.Net.Sockets;
using System.Threading;

namespace PrimalLauncher
{
    public class commandForced : EventRequest
    {
        public Command CommandId { get; set; }

        public commandForced(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = 0;
        }

        public override void Execute()
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            foreach (Quest quest in User.Instance.Character.Journal.GetAllQuests())
            {
                QuestStep = quest.GetActorStep(GetType().Name);

                if (QuestStep != null)
                    break;
            }                

            switch (CommandId)
            {
                case Command.BattleStance:
                case Command.NormalStance:
                    User.Instance.Character.ToggleStance(CommandId);

                    break;
                case Command.Mount:
                    User.Instance.Character.ToggleMount(Command.Mount, (RequestPacket[0x41] == 0x05 ? true : false));
                    break;
            }

            Finish();
        }
    }
}
