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

        public HttpServer() => Start("Http", PORT);

        public override void ProcessIncoming(ref StateObject connection)
        {
            string request = Encoding.ASCII.GetString(connection.buffer);
            byte[] response = HttpPacket.ErrorPage("Something went wrong. Here is the game client request:<br><br>" + request.Replace("\r\n", "<br>").Trim(new[] { '\0' }));

            //if (Preferences.Instance.Options.ShowLoginPage && request.IndexOf("GET /login") >= 0)
            //{
                response = PackPage(@"login/index.html");
                Log.Instance.Info("Login page sent.");
            //}
            //else
            //{
                //< --dirty way to get uname and pwd from http headers...
                //string[] tmp = request.Split(' ');
                //string queryString = tmp[1].Substring(11);
                //tmp = queryString.Split('&');
                string uname = "FFXIVUser";// tmp[0].Substring(6);
                string pwd = "FFXIVUser";// tmp[1].Substring(4);  
                //-->

                User.Load(uname, pwd); //get current user stored data

                if (User.Instance != null)
                {
                    response = HttpPacket.AuthPage(User.Instance.Id);
                    Log.Instance.Info("Authorization page sent.");
                    //ServerShutDown();
                }
                else
                {
                    response = HttpPacket.ErrorPage("User not found!<br><a href='/login'>Try again.</a>");
                }
            //}

            connection.Send(response);
            connection.socket.Disconnect(true);
        }            
       
        private static byte[] PackPage(string file) => new HttpPacket(file).ToBytes();  

        public override void ReadCallback(IAsyncResult ar)
        {
            _connection = (StateObject)ar.AsyncState;

            int bytesRead = _connection.socket.EndReceive(ar, out SocketError errorCode);

            if (errorCode != SocketError.Success)
               bytesRead = 0;

           if (bytesRead > 0)
           {
               ProcessIncoming(ref _connection);
               try
               {
                   if(_connection.socket.Connected)
                        _connection.socket.BeginReceive(_connection.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), _connection);
               }catch(SocketException e) { throw e; }
              
           }
           
        }

        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
