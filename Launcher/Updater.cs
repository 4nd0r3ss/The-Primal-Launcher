using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class Updater
    {
        private static Log _log = Log.Instance;
        public static byte[] Boot { get; set; } = Packet.UpToDate();
        public static byte[] Game { get; set; } = Packet.GameUpdatePacket;

        public static byte[] CheckBootVer()
        {                     
            bool upToDate = true;
            _log.Message("Checking ffxivboot.exe version...");

            if (upToDate)
            {
                _log.Success("ffxivboot.exe is up-to-date!");
                return Boot;
            }
            else
            {
                _log.Warning("ffxivboot.exe is outdated! Starting update...");
                return Boot;
            }            
        }

        public static byte[] CheckGameVer()
        {            
            bool upToDate = true;
            _log.Message("Checking game version...");

            if (upToDate)
            {
                _log.Success("You have the latest verion (1.23b).");
                return Boot;
            }
            else
            {
                _log.Warning("The game is outdated! Starting update...");
                return Boot;
            }
        }
    }
}
