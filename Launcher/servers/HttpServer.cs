using Launcher.packets;
using Launcher.Characters;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class HttpServer : Server
    {
        private const int PORT = 80;
        public static readonly string HTTP_SERVER_VERSION = "Primal-Http-Server/0.1"; //for html headers
        private static UserRepository _userRepo = UserRepository.Instance;
        private User _user;  

        public HttpServer() => Start("Http", PORT);

        public override void ProcessIncoming(StateObject state)
        {
            string request = Encoding.ASCII.GetString(state.buffer);
            byte[] response = HtmlPacket.ErrorPage("Something went wrong. Here is the game client request:<br><br>" + request.Replace("\r\n", "<br>").Trim(new[] { '\0' }));
                                 
            //if (Preferences.Instance.Options.ShowLoginPage && request.IndexOf("GET /login") >= 0)
            //{
            //    response = PackPage(@"login/index.html");
            //    _log.Message("Login page sent.");
            //}
            //else
            //{
                //<-- dirty way to get uname and pwd from http headers...
                //string[] tmp = request.Split(' ');
                //string queryString = tmp[1].Substring(11);
                //tmp = queryString.Split('&');
                string uname = "FFXIVPlayer";// tmp[0].Substring(6);
                string pwd = "FFXIVPlayer";// tmp[1].Substring(4);  
                //-->

                _user = _userRepo.GetUser(uname, pwd); //get current user stored data

                if (_user != null)
                {                    
                    response = HtmlPacket.AuthPage(_user.Id);
                    _log.Message("Authorization page sent.");
                    ServerShutDown();
                }
                else
                {
                    response = HtmlPacket.ErrorPage("User not found!<br><a href='/login'>Try again.</a>");
                }
            //}

        state.workSocket.Send(response);
            state.workSocket.Disconnect(true);
        }            
       
        private static byte[] PackPage(string file) => new HtmlPacket(file).ToBytes();       

        public override void ServerTransition() => Task.Run(() => { LobbyServer lobby = new LobbyServer(_user); });
    }
}
