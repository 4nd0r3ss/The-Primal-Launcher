using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class HttpServer
    {
        static bool _httpActive = false;        
        private static TcpListener HttpListener { get; } = new TcpListener(IPAddress.Parse("17.0.0.1"), 80);
        private static Log _log { get; set; } = Log.Instance;

        public static void Initialize()
        {
            //TODO: VERIFY IF PORT 80 IS IN USE
            if (!_httpActive)
            {
                _log.Message("Http server started @ port 80.");
                HttpListener.Start();
                _httpActive = true;
                HttpDeamon();
            }
        }

        public static void Shutdown()
        {
            if (_httpActive)
            {
                _log.Message("Shutting down http server.");
                HttpListener.Stop();
                _httpActive = false;
            }
        }

        public static void HttpDeamon()
        {
            while (_httpActive)
            {
                try
                {
                    using (TcpClient client = HttpListener.AcceptTcpClient())
                    {
                        NetworkStream ns = client.GetStream();
                        string request = Packet.ToText(ns);

                        //LogMsg.LogMessage(LogMsg.CLI,request);

                        if (request.IndexOf("/login") > 0)
                            Packet.Send(ns, "Sending login page.", Packet.PackPage(LoadHtmlPage("index.html")));
                        else
                            Packet.Send(ns, "Sending authorization page.", Packet.PackPage(LoadHtmlPage("auth.html")));
                    }
                }
                catch (Exception)
                {
                    //LogMsg.LogMessage(LogMsg.WNG, "Something went wrong with the http server. Please try again.");
                }
            }            
        }

        private static string LoadHtmlPage(string file)
        {
            string page = "";
            try { page = System.IO.File.ReadAllText(@"login/" + file); }
            catch (Exception) { page = Packet.ToText(Packet.ErrorPage); }
            finally { }
            return page;
        }
    }
}
