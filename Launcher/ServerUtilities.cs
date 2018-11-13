using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class ServerUtilities
    {
        private static long _epoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        public static IPAddress IPLocalhost { get; } = IPAddress.Parse("127.0.0.1");
        public static string FFXIVBootFilePath { get; } = Preferences.Instance.Options.GameInstallPath + @"\ffxivboot.exe";
        private static Log LogMsg { get; set; } = Log.Instance;       

        public static void LaunchGame()
        {
            LogMsg.LogMessage(LogMsg.MSG, "Launching ffxivboot.exe.");
            Process.Start(new ProcessStartInfo { FileName = ServerUtilities.FFXIVBootFilePath });
        }

        public static string GetEpochString()
        {
            return _epoch.ToString("X");
        }

        public static string GetEpoch()
        {
            return _epoch.ToString("X");
        }
    }
}
