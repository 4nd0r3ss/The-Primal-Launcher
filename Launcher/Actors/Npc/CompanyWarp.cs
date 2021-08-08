using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{ 
    class CompanyWarp : Populace
    {
        public uint Region { get; set; }
        public uint Zone { get; set; }
        public int WarpId { get; set; }

        public CompanyWarp()
        {
            ClassName = "PopulaceCompanyWarp";
            Events.Add(new Event { Opcode = ServerOpcode.TalkEvent, Name = "talkDefault", Priority = 4, Enabled = 1 });
            Events.Add(new Event { Opcode = ServerOpcode.NoticeEvent, Name = "noticeEvent", Priority = 0, Enabled = 0, Silent = 0 });
        }

        #region Event methods       
        public override void talkDefault(Socket sender)
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                LuaParameters parameters = new LuaParameters()
                {
                    Parameters = new object[]
                    {
                            (sbyte)0x05,
                            Encoding.ASCII.GetBytes("talkDefault"),
                            Encoding.ASCII.GetBytes("eventAskMainMenu"),
                            User.Instance.Character.Id,
                            WarpId
                    }
                };

                byte[] data = new byte[0x298];
                Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);
                LuaParameters.WriteParameters(ref data, parameters, 0x08);
                Packet.Send(sender, ServerOpcode.EventRequestResponse, data);
                EventManager.Instance.CurrentEvent.FunctionName = "talkDefault";
                EventManager.Instance.CurrentEvent.IsQuestion = true;
            }
            else
            {
                uint selection = EventManager.Instance.CurrentEvent.Selection;

                //if a warp location was selected
                if (selection > 0)
                {
                    //we warp to somewhere near the selected location warp npc.
                    Position destination = ActorRepository.Instance.GetCompanyWarpPositionById(Region, selection);

                    if (destination != null)
                    {
                        destination.X += (float)(3 * Math.Sin(destination.R));
                        destination.Z += (float)(3 * Math.Cos(destination.R));
                        destination.R += 0.8f;
                    }
                    else
                    {
                        destination = EntryPoints.GetTownWarpExit(Region, selection);
                    }

                    EventManager.Instance.CurrentEvent.Finish(sender);
                    World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0x01, 0x00, 0xF8, 0x5F, 0x39, 0x85, 0x20, 0x00 });
                    World.Instance.TeleportPlayer(sender, destination);
                }
                else //selected quit
                {
                    EventManager.Instance.CurrentEvent.Finish(sender);
                }
            }
        }
        #endregion 
    }
}
