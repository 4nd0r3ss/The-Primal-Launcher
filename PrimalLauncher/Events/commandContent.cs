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

using System.Collections.Generic;
using System.Linq;
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
            Data = data;
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = (0xA0F00000 | (ushort)CommandId);
            InitLuaParameters();
        }

        public override void Execute()
        {
            Log.Instance.Warning("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            switch (CommandId)
            {
                case Command.Teleport:
                    if (RequestPacket[0x41] == 0x05)
                        Return();
                    else
                        Teleport();
                    break;
                case Command.Logout:
                    Logout();
                    break;
            }
        }

        public override void ProcessEventResult(byte[] data)
        {
            Data = data;

            if (IsQuestion)
            {
                
                GetQuestionSelection();
                RequestPacket = data;
                InvokeMethod(Callback, new object[] {});
            }
            else
            {
                Finish();
            }
        }

        public void Return()
        {
            if (IsQuestion)
            {
                Finish();

                if (Selection[0] == 1)
                {
                    byte subSelection = RequestPacket[0x26];
                    World.SendTextSheet(0x8539);

                    if (subSelection == 0x05)
                    {
                        var destinationAetheryte = from a in World.Instance.Aetherytes
                                                   where a.ClassId == User.Instance.Character.HomePoint
                                                   orderby a.TeleportMenuId ascending
                                                   select a;

                        World.Instance.TeleportPlayerToAetheryte(destinationAetheryte.ToList()[0]);
                    }
                    else if (subSelection == 0x03)
                    {
                        World.Instance.TeleportPlayer(EntryPoints.GetInnEntry(User.Instance.Character.InitialTown));
                    }
                }
            }
            else
            {
                IsQuestion = true;
                Callback = "Return";

                List<object> list = new List<object>();
                list.Add("eventConfirm");
                list.Add(true);
                list.Add(User.Instance.Character.IsEngaged); //if true, player is in active battle
                list.Add(User.Instance.Character.HasInn() ? (int)User.Instance.Character.InitialTown : 0); //we check if player completed the inn quest to unlock inn.
                list.Add((int)User.Instance.Character.HomePoint);
                list.Add(false);

                //new object[] { "eventConfirm", false, false, (int)User.Instance.Character.InitialTown, (int)User.Instance.Character.HomePoint, false }
                DelegateCommand(list.ToArray());
                User.Instance.Character.PlayAnimationEffect(AnimationEffect.TeleportWait);
            }
        }

        public void Logout()
        {
            if (IsQuestion)
            {
                if (Selection[0] == 1)
                    PlayerCharacter.ExitGame();
                else if (Selection[0] == 2)
                    PlayerCharacter.Logout();
                else
                    Finish();
            }
            else
            {
                IsQuestion = true;
                Callback = "Logout";
                DelegateCommand(new object[] { "eventConfirm", false, false, false });
            }
        }

        public void Teleport()
        {            
            GetQuestionSelection();

            if (!Selection[0].HasValue)
            {
                if (MenuPage == 2) //from aetheryte selection back to region selection
                    MenuPage = 0;

                if (MenuPage == 1) //finish event when closing region selection
                {
                    Finish();
                    return;
                }
            }

            InitLuaParameters();

            switch (MenuPage)
            {
                case 0:
                    IsQuestion = true;
                    Callback = "Teleport";
                    DelegateCommand(new object[] { "eventRegion", (int)User.Instance.Character.Anima });
                    break;
                case 1:
                    PageId = (uint)Selection[0];
                    List<object> parameters = new List<object>();
                    parameters.Add("eventAetheryte");
                    parameters.Add((int)Selection[0]);

                    var regionAetherytes = from a in World.Instance.Aetherytes
                                           where a.TeleportMenuPageId == Selection[0]
                                           orderby a.TeleportMenuId ascending
                                           select a;

                    foreach (Aetheryte a in regionAetherytes)
                        parameters.Add((int)a.AnimaCost);

                    DelegateCommand(parameters.ToArray());
                    break;
                case 2:
                    PageItem = (uint)Selection[0];
                    User.Instance.Character.PlayAnimationEffect(AnimationEffect.TeleportWait);
                    DelegateCommand(new object[] { "eventConfirm", false, false, 0x13883f }); //TODO: 0x13883f is the favored aetheryte classid
                    break;
                case 3:
                    if (Selection[0] == 1)
                    {
                        User.Instance.Character.PlayAnimationEffect(AnimationEffect.Teleport);
                        Packet.Send(ServerOpcode.SetUIControl, new byte[] { 0x14, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 });
                        World.SendTextSheet(0x8539);
                        Finish();

                        var destinationAetheryteList = from a in World.Instance.Aetherytes
                                                       where a.TeleportMenuPageId == PageId && a.TeleportMenuId == PageItem
                                                       orderby a.TeleportMenuId ascending
                                                       select a;

                        Aetheryte destinationAetheryte = destinationAetheryteList.ToList()[0];
                        User.Instance.Character.Anima -= destinationAetheryte.AnimaCost;
                        World.Instance.TeleportPlayerToAetheryte(destinationAetheryte);
                    }
                    else
                    {
                        Finish();
                    }
                    break;
            }

            MenuPage++;
        }

        private void DelegateCommand(object[] parameters = null)
        {
            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateCommand"));
            RequestParameters.Add(0xA0F00000 | (uint)CommandId);

            if (parameters == null)
                parameters = new object[] { null, null, null };

            foreach (object obj in parameters)
                RequestParameters.Add(obj);

            Response();
        }
    }
}
