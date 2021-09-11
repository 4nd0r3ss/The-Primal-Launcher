using System.Net.Sockets;

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

        public override void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            switch (CommandId)
            {
                case Command.QuestData:
                    User.Instance.Character.Journal.GetQuestData(sender, (RequestPacket[0x42] << 24 | RequestPacket[0x43] << 16 | RequestPacket[0x44] << 8 | RequestPacket[0x45]), ref _requestParameters);
                    break;
                case Command.GuildleveData:
                    User.Instance.Character.Journal.GetGuildleveData(sender, ref _requestParameters);
                    break;
                case Command.Umount:
                    User.Instance.Character.ToggleMount(sender, Command.Umount, false);
                    break;
                case Command.DoEmote:
                    Log.Instance.Warning("emote id:" + RequestPacket[0x45].ToString("X2"));
                    User.Instance.Character.DoEmote(sender);
                    break;
                case Command.ChangeEquipment:
                    User.Instance.Character.ChangeGear(sender, RequestPacket);
                    break;
                case Command.EquipSoulStone:
                    User.Instance.Character.EquipSoulStone(sender, RequestPacket);
                    break;
                case Command.PlaceDriven: //from iteraction menu triggered by pushCommandIn
                    Actor eventOwner = User.Instance.Character.GetCurrentZone().GetActorById(OwnerId);

                    if (eventOwner != null)
                        eventOwner.GetType().GetMethod("PlaceDriven").Invoke(eventOwner, new object[] { sender });

                    break;
            }
        }
    }
}
