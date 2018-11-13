using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class Updater
    {
        private static Log LogMsg = Log.Instance;
        public static byte[] Boot { get; set; } = Packet.UpToDate();
        public static byte[] Game { get; set; } = Packet.GameUpdatePacket;

        public static byte[] CheckBootVer()
        {                     
            bool upToDate = true;
            LogMsg.LogMessage(LogMsg.MSG, "Checking ffxivboot.exe version...");

            if (upToDate)
            {
                LogMsg.LogMessage(LogMsg.OK, "ffxivboot.exe is up-to-date!");
                return Boot;
            }
            else
            {
                LogMsg.LogMessage(LogMsg.WNG, "ffxivboot.exe is outdated! Starting update...");
                return Boot;
            }            
        }

        public static byte[] CheckGameVer()
        {            
            bool upToDate = true;
            LogMsg.LogMessage(LogMsg.MSG, "Checking game version...");

            if (upToDate)
            {
                LogMsg.LogMessage(LogMsg.OK, "You have the latest verion (1.23b).");
                return Boot;
            }
            else
            {
                LogMsg.LogMessage(LogMsg.WNG, "The game is outdated! Starting update...");
                return Boot;
            }
        }
    }
}
