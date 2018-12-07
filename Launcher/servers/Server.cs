using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Launcher
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 0xFFFF;
        public byte[] buffer = new byte[BufferSize];           
    }

    abstract class Server
    {
        protected static Log _log = Log.Instance;
        protected Blowfish _blowfish;
        private ManualResetEvent _allDone = new ManualResetEvent(false);
        private bool _listening = true;
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(string serverName, int port)
        {
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse(Preferences.Instance.Options.ServerAddress), port));
                _log.Message(serverName + " server started @ port " + port);
            }
            catch (Exception){_log.Message("It looks like port " + port + " is in use by another process. Aborting.");}

            try
            {
                socket.Listen(100);
                while (_listening)
                {
                    _allDone.Reset();
                    _log.Message("Waiting for connection...");
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                    _allDone.WaitOne();
                }
                _log.Warning(serverName + " server has been shut down.");
            }
            catch (Exception){_log.Message("Could not start the " + serverName + " server. Please try again.");}
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _allDone.Set();
            if (!_listening) return;
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject { workSocket = handler };
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                int bytesRead = state.workSocket.EndReceive(ar);                

                if (bytesRead > 0)
                {
                    ProcessIncoming(state);
                    state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);                    
                }
            }
            catch (SocketException){ /*Shutdown(); //this doesn't work proprly here...*/ }           
        }

        public void ServerShutDown()
        {
            _listening = false;            
            socket.Close();
            ServerTransition();
        }

        public abstract void ProcessIncoming(StateObject state);

        public abstract void ServerTransition();

        public static int GetTimeStamp() => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        public static byte[] GetTimeStampHex() => BitConverter.GetBytes(GetTimeStamp());
    }
}
