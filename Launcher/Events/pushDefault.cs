using System.Net.Sockets;

namespace PrimalLauncher
{
    public class pushDefault : EventRequest
    {
        public pushDefault(byte[] data) : base(data)
        {
            InitLuaParameters();
        }

        public override void ProcessEventResult(Socket sender, byte[] data)
        {
            if (!string.IsNullOrEmpty(FunctionName) && GetType().GetMethod(FunctionName) != null)
            {
                InvokeMethod(FunctionName, new object[] { sender, data });
            }
            else
            {
                Finish(sender);
            }
        }

        public void processEventNewRectAsk(Socket sender, byte[] eventResult)
        {
            byte resultType = eventResult[0x21];
            uint selection = (uint)(eventResult[0x22] << 24 | eventResult[0x23] << 16 | eventResult[0x24] << 8 | eventResult[0x25]);

            if (resultType == 0x05) //null
            {
                Finish(sender);

                Actor eventOwner = GetActor();
                //finish exit door repeatable step.
                User.Instance.Character.Journal.Quests.Find(x => x.Id == QuestId).ActorStepComplete(sender, GetType().Name, eventOwner.ClassId, eventOwner.Id, finishRepeatable: true);
                World.Instance.SendTextQuestUpdated(sender, QuestId);
                BattleTutorialStart(sender);
            }
            else
            {
                switch (selection)
                {
                    case 1:
                        InitLuaParameters();
                        DelegateEvent(sender, QuestId, "processEvent000_2", new object[] { null, null, null, null });//put this in the quest script
                        break;
                    case 2:
                        Finish(sender);
                        break;
                }
            }
        }
    }
}
