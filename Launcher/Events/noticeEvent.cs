using System;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class noticeEvent : EventRequest
    {
        public noticeEvent(byte[] data) : base(data)
        {
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)eventType);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        public override void Finish(Socket sender)
        {
            base.Finish(sender);

            if (!string.IsNullOrEmpty(FunctionName))
                InvokeMethod(FunctionName, new object[] { sender });
        }

        /// <summary>
        /// Battle tutorial method. This method is common for all 3 openings.
        /// </summary>
        /// <param name="sender"></param>
        public void processTtrBtl002(Socket sender)
        {
            SendData(sender, new object[] { 0x05 }, 2);
            SendData(sender, new object[] { 0x02, null, null, 0x235f }, 2); //attack success
            SendData(sender, new object[] { 0x04, null, null, 0x01, 0x0C }, 2);//TP tutorial (4th parameter is keyboard_controller)
            SendData(sender, new object[] { 0x05 }, 2);
            SendData(sender, new object[] { 0x04, null, null, 0x01, 0x0D }, 2);//weaponskill tutorial (4th parameter is keyboard_controller)
            SendData(sender, new object[] { 0x05 }, 2);
            SendData(sender, new object[] { 0x02, null, null, 0x2369 }, 2);
            SendData(sender, new object[] { "attention", World.Instance.Id, "", 0xC781, (int)User.Instance.Character.InitialTown });

            World.Instance.SetMusic(sender, 0x07, MusicMode.Crossfade);
            User.Instance.Character.ToggleStance(sender, Command.NormalStance);
            ((QuestDirector)World.Instance.GetDirector("Quest")).StartEvent(sender, "noticeEvent");
        }

        /// <summary>
        /// TODO: put this in the quest script later.
        /// </summary>
        /// <param name="sender"></param>
        public void processEvent000_3(Socket sender) => GoToQuestPrivateZone(sender, "Man0l001", 2);

        public void processEvent020_1(Socket sender) => GoToQuestPrivateZone(sender, "Man0g001", 5);

        public void processEvent020(Socket sender) => GoToQuestPrivateZone(sender, "Man0u001", 1);
    }
}
