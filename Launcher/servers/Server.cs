using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Launcher
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 0xFFFF;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();        
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BasePacketHeader
    {
        public byte isAuthenticated;
        public byte isCompressed;
        public ushort connectionType;
        public ushort packetSize;
        public ushort numSubpackets;
        public ulong timestamp; //Miliseconds
    }

    abstract class Server
    {
        private Log _log = Log.Instance;
        public Blowfish _blowfish;
        public ManualResetEvent _allDone = new ManualResetEvent(false);
        public const string SERVER_ADDRESS = "127.0.0.1";
        private int filenum = 0;

        public void Start(string server, int port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse(SERVER_ADDRESS), port));
                _log.Message(server + " server started @ port " + port);
            }
            catch (Exception)
            {
                _log.Message("It looks like port " + port + " is in use by another process. Aborting.");
            }

            try
            {
                socket.Listen(100);
                while (true)
                {
                    _allDone.Reset();
                    _log.Message("Waiting for connection...");
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                    _allDone.WaitOne();
                }
            }
            catch (Exception)
            {
                _log.Message("Could not start the " + server + " server. Please try again.");
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _allDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject { workSocket = handler };
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            int bytesRead = state.workSocket.EndReceive(ar);            

            if (bytesRead > 0)
            {
                //<-- packet debug output
                //byte[] tmp = new byte[(ushort)(state.buffer[0x05] << 8 | state.buffer[0x04])];
                //Buffer.BlockCopy(state.buffer, 0, tmp, 0, (ushort)(state.buffer[0x05] << 8 | state.buffer[0x04]));

                //if (_blowfish != null)
                //{
                //    _blowfish.Decipher(tmp, 0, tmp.Length);
                //}

                //File.WriteAllBytes(@"packets\" + filenum + "_in.txt", tmp);
                //filenum++;
                //-->    
                
                ProcessPacket(state.workSocket, new Packet(state.buffer));
                state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        public abstract void ProcessPacket(Socket handler, Packet packet);        

        public static byte[] GenerateKey(string ticketPhrase, uint clientNumber)
        {
            byte[] key;
            using (MemoryStream memStream = new MemoryStream(0x2C))
            {
                using (BinaryWriter binWriter = new BinaryWriter(memStream))
                {
                    binWriter.Write((Byte)0x78);
                    binWriter.Write((Byte)0x56);
                    binWriter.Write((Byte)0x34);
                    binWriter.Write((Byte)0x12);
                    binWriter.Write((UInt32)clientNumber);
                    binWriter.Write((Byte)0xE8);
                    binWriter.Write((Byte)0x03);
                    binWriter.Write((Byte)0x00);
                    binWriter.Write((Byte)0x00);
                    binWriter.Write(Encoding.ASCII.GetBytes(ticketPhrase), 0, Encoding.ASCII.GetByteCount(ticketPhrase) >= 0x20 ? 0x20 : Encoding.ASCII.GetByteCount(ticketPhrase));
                }
                byte[] nonMD5edKey = memStream.GetBuffer();

                using (MD5 md5Hash = MD5.Create())
                {
                    key = md5Hash.ComputeHash(nonMD5edKey);
                }
            }
            return key;
        }

        public static int GetTimeStamp() => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        public static byte[] GetTimeStampHex() => BitConverter.GetBytes(GetTimeStamp());
    }
}
