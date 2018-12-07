using Launcher.packets;

namespace Launcher
{
    class Updater
    {
        private static Log _log = Log.Instance;

        private static HtmlPacket BootUpToDate { get; } = new HtmlPacket()
        {
            HttpResponse = "204 No Content",
            ContentLocation = "ffxiv/2d2a390f/vercheck.dat",
            XRepository = "ffxiv/win32/release/boot",
            XPatchModule = "ZiPatch",
            XProtocol = "torrent",
            XInfoUrl = "http://example.com",
            XLatestVersion = "2010.09.18.0000"
        };

        private static HtmlPacket BootOutdated { get; } = new HtmlPacket
        {
            HttpResponse = "200 OK",
            ContentLocation = "ffxiv/48eca647/vercheck.dat",
            ContentType = "multipart/mixed; boundary=477D80B1_38BC_41d4_8B48_5273ADB89CAC",
            XRepository = "ffxiv/win32/release/boot",
            XPatchModule = "ZiPatch",
            XProtocol = "torrent",
            XInfoUrl = "http://example.com",
            XLatestVersion = "2012.05.20.0000.0001",
            Connection = "keep-alive"
        };

        private static HtmlPacket GameUpToDate { get; } = new HtmlPacket
        {
            HttpResponse = "204 No Content",   
            ContentLocation = "ffxiv/48eca647/vercheck.dat",
            XRepository = "ffxiv/win32/release/game",
            XPatchModule = "ZiPatch",
            XProtocol = "torrent",
            XInfoUrl = "http://www.example.com/",
            XLatestVersion = "2010.07.10.0000",
            ContentLength = "0",
            KeepAlive = "timeout=5, max=99",
            Connection = "Keep-Alive",
            ContentType = "text/plain"
        };

        private static HtmlPacket GameOutdated { get; } = new HtmlPacket();

        public static byte[] CheckBootVer()
        {                     
            bool upToDate = true;
            _log.Message("Checking ffxivboot.exe version...");

            /* TODO: ffxivboot.exe version check */

            if (upToDate)
            {
                _log.Success("ffxivboot.exe is up-to-date!");
                return BootUpToDate.ToBytes();
            }
            else
            {
                _log.Warning("ffxivboot.exe is outdated! Starting update...");
                return BootOutdated.ToBytes();
            }            
        }

        public static byte[] CheckGameVer()
        {            
            bool upToDate = true;
            _log.Message("Checking game version...");

            /* TODO: ffxivgame.exe version check */

            if (upToDate)
            {
                _log.Success("You have the latest verion (1.23b).");
                return GameUpToDate.ToBytes();
            }
            else
            {
                _log.Warning("The game is outdated! Starting update...");
                return GameOutdated.ToBytes();
            }
        }
    }
}
