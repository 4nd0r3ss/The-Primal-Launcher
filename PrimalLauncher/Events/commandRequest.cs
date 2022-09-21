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

namespace PrimalLauncher
{
    public class commandRequest : EventRequest
    {
        public Command CommandId { get; set; }
        public uint PushCommand { get; set; }
        public commandRequest(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = (uint)(data[0x4E] << 24 | data[0x4F] << 16 | data[0x50] << 8 | data[0x51]);
            PushCommand = (uint)(data[0x42] << 24 | data[0x43] << 16 | data[0x44] << 8 | data[0x45]);
        }

        public override void Execute()
        {
            Log.Instance.Warning("commandRequest.Execute()");

            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            Actor eventOwner = User.Instance.Character.GetCurrentZone().GetActorById(OwnerId);
            string stepValue = null;

            switch (CommandId)
            {
                case Command.QuestData:
                    User.Instance.Character.Journal.GetQuestData(RequestPacket, ref _requestParameters);
                    break;
                case Command.GuildleveData:
                    User.Instance.Character.Journal.GetGuildleveData(ref _requestParameters);
                    break;
                case Command.Umount:
                    User.Instance.Character.ToggleMount(Command.Umount, false);
                    break;
                case Command.DoEmote:
                    stepValue = RequestPacket[0x45].ToString();
                    User.Instance.Character.DoEmote(RequestPacket[0x45]);
                    break;
                case Command.ChangeEquipment:
                    User.Instance.Character.ChangeEquipment(RequestPacket);
                    break;
                case Command.EquipSoulStone:
                    User.Instance.Character.EquipSoulStone(RequestPacket);
                    break;
                case Command.NpcLinkshellChat:
                    QuestDirector director = ((QuestDirector)User.Instance.Character.GetCurrentZone().GetDirector("Quest"));

                    //if(director != null)
                    //    director.
                    break;
                case Command.PlaceDriven: //for iteraction menu   
                    if (eventOwner != null)
                        eventOwner.GetType().GetMethod("PlaceDriven").Invoke(eventOwner, new object[] {});
                    break;
            }

            foreach (Quest quest in User.Instance.Character.Journal.GetAllQuests())
            {
                QuestStep = quest.GetActorStep(GetCommandName(), eventOwner, value: stepValue);

                if (QuestStep != null)
                    break;
            }            
        }

        private string GetCommandName()
        {
            return Enum.GetName(typeof(Command), CommandId);
        }

        //public override void Response()
        //{
        //    //should be empty
        //}
    }
}
