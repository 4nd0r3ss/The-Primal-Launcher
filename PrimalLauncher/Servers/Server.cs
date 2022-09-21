/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        
        public bool ClientIsConnected { get; set; }

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
                    Log.Instance.Info("Waiting for game client connection...");
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
            Log.Instance.Success("Game client connection received.");
            ClientIsConnected = true;
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

        public abstract void ProcessIncoming();       

        public abstract void ServerTransition();

        public static int GetTimeStamp(int addSeconds = 0) => (int)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds + addSeconds);

        public static byte[] GetTimeStampHex(int addSeconds = 0) => BitConverter.GetBytes(GetTimeStamp(addSeconds));

        public static ulong GetTimeStampHexMiliseconds() => (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

    }
}
