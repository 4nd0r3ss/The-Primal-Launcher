using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class StateObject
    {
        public Socket socket = null;
        public const int BufferSize = 0xFFFF;
        public byte[] buffer = new byte[BufferSize];   
        public Queue<byte[]> bufferQueue = new Queue<byte[]>();

        public void Send(byte[] buffer)
        {
            if (socket.Connected)
                socket.Send(buffer);                    
        }
    }

    abstract class Server
    {        
        protected Blowfish _blowfish;
        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);                
        private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public bool _listening = true;
        public StateObject _connection = new StateObject();        

        public void Start(string serverName, int port)
        {     
            try
            {
                _socket.Bind(new IPEndPoint(IPAddress.Parse(Preferences.Instance.Options.ServerAddress), port));
                Log.Instance.Info(serverName + " server started @ port " + port);
            }
            catch (Exception) { Log.Instance.Info("It looks like port " + port + " is in use by another process. Aborting."); }

            try
            {
                _socket.Listen(100);
                while (_listening)
                {
                    _allDone.Reset();
                    Log.Instance.Info("Waiting for connection...");
                    _socket.BeginAccept(new AsyncCallback(AcceptCallback), _socket);
                    _allDone.WaitOne();                   
                }
                Log.Instance.Warning(serverName + " server has been shut down.");
            }
            catch (Exception) { Log.Instance.Info("Could not start the " + serverName + " server. Please try again."); }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _allDone.Set();
            if (!_listening) return;
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject { socket = handler };
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public virtual void ReadCallback(IAsyncResult ar)
        {
            _connection = (StateObject)ar.AsyncState;

            try
            {
                //This EndReceive() overload fixes the "connection forcibliy closed by the remote host" exception.
                int bytesRead = _connection.socket.EndReceive(ar, out SocketError errorCode);

                if (errorCode != SocketError.Success) 
                    bytesRead = 0;

                if (bytesRead > 0)
                {
                    _connection.bufferQueue.Enqueue(_connection.buffer);                   
                    _connection.socket.BeginReceive(_connection.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), _connection); //mine
                }
            }
            catch (SocketException e) { throw e; }                    
        }

        public void ServerShutDown()
        {
            _listening = false;
            _socket.Close();
            ServerTransition();
        }       

        public abstract void ProcessIncoming(ref StateObject connection);       

        public abstract void ServerTransition();

        public static int GetTimeStamp() => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        public static byte[] GetTimeStampHex() => BitConverter.GetBytes(GetTimeStamp());

        public static ulong GetTimeStampHexMiliseconds() => (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

    }
}
