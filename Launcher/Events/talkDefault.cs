using System;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class talkDefault : EventRequest
    {
        public talkDefault(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        public override void ProcessEventResult(Socket sender, byte[] data)
        {
            if (!string.IsNullOrEmpty(FunctionName) && GetType().GetMethod(FunctionName) != null && !IsQuestion)
            {
                InvokeMethod(FunctionName, new object[] { sender, data });
            }
            else
            {
                if (IsQuestion)
                {
                    GetQuestionSelection(data);

                    if (ReturnToOwner)
                    {
                        GetActor().InvokeMethod(FunctionName, new object[] { sender });
                        return;
                    }
                    else
                    {
                        InvokeMethod(FunctionName, new object[] { sender });
                    }
                }

                Finish(sender);
            }
        }

        public void processEvent020_9(Socket sender, byte[] eventResult)
        {
            byte resultType = eventResult[0x21];
            uint selection = (uint)(eventResult[0x22] << 24 | eventResult[0x23] << 16 | eventResult[0x24] << 8 | eventResult[0x25]);

            if (resultType == 0x05) //null
            {
                Finish(sender);
                FinishQuest(sender, "110001");
                AddQuest(sender, "110002");
                World.Instance.ChangeZone(sender, EntryPoints.Get(4), 0x0F);
            }
            else
            {
                switch (selection)
                {
                    case 1:
                        InitLuaParameters(); //clear
                        DelegateEvent(sender, 0x01ADB2, "processEvent010", new object[] { null, null, null }); //put this in the quest script
                        break;
                    case 2:
                        Finish(sender);
                        break;
                }
            }
        }      
    }
}
