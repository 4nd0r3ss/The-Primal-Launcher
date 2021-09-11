using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class commandContent : EventRequest
    {
        public Command CommandId { get; set; }
        private int MenuPage { get; set; }
        private uint PageId { get; set; }
        private uint PageItem { get; set; }

        public commandContent(byte[] data) : base(data)
        {
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = (0xA0F00000 | (ushort)CommandId);
            InitLuaParameters();
        }

        public override void Execute(Socket sender)
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            switch (CommandId)
            {
                case Command.Teleport:
                    if (RequestPacket[0x41] == 0x05)
                        Return(sender);
                    else
                        Teleport(sender);
                    break;
                case Command.Logout:
                    Logout(sender);
                    break;
            }
        }

        public override void ProcessEventResult(Socket sender, byte[] data)
        {
            if (IsQuestion)
            {
                GetQuestionSelection(data);
                RequestPacket = data;
                InvokeMethod(FunctionName, new object[] { sender });
            }
            else
            {
                Finish(sender);
            }
        }

        public void Return(Socket sender)
        {
            if (IsQuestion)
            {
                Finish(sender);

                if (Selection == 1)
                {
                    byte subSelection = RequestPacket[0x26];
                    World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0x01, 0x00, 0xF8, 0x5F, 0x39, 0x85, 0x20, 0x00 });

                    if (subSelection == 0x05)
                    {
                        var destinationAetheryte = from a in ActorRepository.Instance.Aetherytes
                                                   where a.ClassId == User.Instance.Character.HomePoint
                                                   orderby a.TeleportMenuId ascending
                                                   select a;

                        World.Instance.TeleportPlayerToAetheryte(sender, destinationAetheryte.ToList()[0]);
                    }
                    else if (subSelection == 0x03)
                    {
                        World.Instance.TeleportPlayer(sender, EntryPoints.GetInnEntry(User.Instance.Character.InitialTown));
                    }
                }
            }
            else
            {
                IsQuestion = true;
                FunctionName = "Return";
                DelegateCommand(sender, new object[] { "eventConfirm", true, false, (int)User.Instance.Character.InitialTown, User.Instance.Character.HomePoint, false });
                User.Instance.Character.PlayAnimationEffect(sender, AnimationEffect.TeleportWait);
            }
        }

        public void Logout(Socket sender)
        {
            if (IsQuestion)
            {
                if (Selection == 1)
                    PlayerCharacter.ExitGame(sender);
                else if (Selection == 2)
                    PlayerCharacter.Logout(sender);
                else
                    Finish(sender);
            }
            else
            {
                IsQuestion = true;
                FunctionName = "Logout";
                DelegateCommand(sender, new object[] { "eventConfirm", false, false, false });
            }
        }

        public void Teleport(Socket sender)
        {
            if (Selection == 0xFF)
            {
                if (MenuPage == 2) //from aetheryte selection back to region selection
                    MenuPage = 0;

                if (MenuPage == 1) //finish event when closing region selection
                {
                    Finish(sender);
                    return;
                }
            }

            InitLuaParameters();

            switch (MenuPage)
            {
                case 0:
                    IsQuestion = true;
                    FunctionName = "Teleport";
                    DelegateCommand(sender, new object[] { "eventRegion", (int)User.Instance.Character.Anima });
                    break;
                case 1:
                    PageId = Selection;
                    List<object> parameters = new List<object>();
                    parameters.Add("eventAetheryte");
                    parameters.Add((int)Selection);

                    var regionAetherytes = from a in ActorRepository.Instance.Aetherytes
                                           where a.TeleportMenuPageId == Selection
                                           orderby a.TeleportMenuId ascending
                                           select a;

                    foreach (Aetheryte a in regionAetherytes)
                        parameters.Add((int)a.AnimaCost);

                    DelegateCommand(sender, parameters.ToArray());
                    break;
                case 2:
                    PageItem = Selection;
                    User.Instance.Character.PlayAnimationEffect(sender, AnimationEffect.TeleportWait);
                    DelegateCommand(sender, new object[] { "eventConfirm", false, false, 0x02, 0x13883f, false }); //TODO: 0x13883f is the favored aetheryte classid
                    break;
                case 3:
                    if (Selection == 1)
                    {
                        User.Instance.Character.PlayAnimationEffect(sender, AnimationEffect.Teleport);
                        Packet.Send(sender, ServerOpcode.SetUIControl, new byte[] { 0x14, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 });
                        World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0x01, 0x00, 0xF8, 0x5F, 0x39, 0x85, 0x20, 0x00 });
                        Finish(sender);

                        var destinationAetheryteList = from a in ActorRepository.Instance.Aetherytes
                                                       where a.TeleportMenuPageId == PageId && a.TeleportMenuId == PageItem
                                                       orderby a.TeleportMenuId ascending
                                                       select a;

                        Aetheryte destinationAetheryte = destinationAetheryteList.ToList()[0];
                        User.Instance.Character.Anima -= destinationAetheryte.AnimaCost;
                        World.Instance.TeleportPlayerToAetheryte(sender, destinationAetheryte);
                    }
                    else
                    {
                        Finish(sender);
                    }
                    break;
            }

            MenuPage++;
        }

        private void DelegateCommand(Socket sender, object[] parameters = null)
        {
            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateCommand"));
            RequestParameters.Add(0xA0F00000 | (uint)CommandId);

            if (parameters == null)
                parameters = new object[] { null, null, null };

            foreach (object obj in parameters)
                RequestParameters.Add(obj);

            Response(sender);
        }
    }
}
