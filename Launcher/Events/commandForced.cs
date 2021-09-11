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

        public override void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            foreach (Quest quest in User.Instance.Character.Journal.Quests)
                QuestStep = quest.ActorStepComplete(sender, GetType().Name);

            switch (CommandId)
            {
                case Command.BattleStance:
                case Command.NormalStance:
                    User.Instance.Character.ToggleStance(sender, CommandId);

                    break;
                case Command.Mount:
                    User.Instance.Character.ToggleMount(sender, Command.Mount, (RequestPacket[0x41] == 0x05 ? true : false));
                    break;
            }

            Finish(sender);
        }

        public override void Finish(Socket sender)
        {
            base.Finish(sender);

            if (QuestStep != null)
            {
                Thread.Sleep(1000);
                ((QuestDirector)World.Instance.GetDirector("Quest")).StartEvent(sender, QuestStep.Value);
            }
        }
    }
}
