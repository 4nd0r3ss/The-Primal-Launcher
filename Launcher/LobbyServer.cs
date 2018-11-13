using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class LobbyServer
    {
        public static TcpListener LobbyListener { get; } = new TcpListener(IPAddress.Parse("127.0.0.1"), 54994);
        private static Log LogMsg { get; set; } = Log.Instance;
        private static bool _lobbyActive = false;

        public static void Initialize()
        {
            if (!_lobbyActive)
            {
                LogMsg.LogMessage(LogMsg.MSG, "Lobby server started @ port 54997");
                LobbyListener.Start();
                _lobbyActive = true;
                LobbyDaemon();
            }
        }

        public static void Shutdown()
        {
            if (_lobbyActive)
            {
                LogMsg.LogMessage(LogMsg.MSG, "Shutting down lobby server.");
                LobbyListener.Stop();
                _lobbyActive = false;
            }
        }

        public static void LobbyDaemon()
        {
            
            while (_lobbyActive)
            {
                try
                {
                    using (TcpClient client = LobbyListener.AcceptTcpClient())
                    {
                        LogMsg.LogMessage(LogMsg.OK, "Received game client connection! ");
                        HttpServer.Shutdown();

                        while (true)
                        {
                            NetworkStream ns = client.GetStream();
                            string request = Packet.ToText(ns);
                            
                            

                            if (request.IndexOf("Test Ticket Data") > 0)
                            {
                                byte[] clientTime = Packet.GetClientTimeStamp(request);
                                Packet.Send(ns, "Handshake packet sent.", Packet.AckPacket);
                                File.WriteAllText("packets/1_ticket.txt", request);
                                File.WriteAllBytes("packets/1_timestamp.txt", clientTime);
                            }
                            else
                            {
                                Packet.Send(ns, "Login packet sent.", Packet.LoginAckPacket);
                                byte[] packet = Packet.GetBytes(ns);// Packet.Decrypt(ns);
                                LogMsg.LogMessage(LogMsg.WNG, "received packet size: " + Packet.GetSizeByte(packet));

                                //AppendAllBytes("packets/1_incoming.txt", packet);
                                //AppendAllBytes("packets/1_incoming.txt", new byte[] {0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, });

                                //byte opcode = Packet.GetOpcode(packet);

                                //switch (opcode)
                                //{
                                //    case 0x03:
                                //        LogMsg.LogMessage(LogMsg.MSG, "Command 0x03 received.");
                                //        break;
                                //    case 0x04:
                                //        LogMsg.LogMessage(LogMsg.MSG, "Command 0x04 received.");
                                //        break;
                                //    case 0x05:
                                //        LogMsg.LogMessage(LogMsg.MSG, "Command 0x05 received.");
                                //        break;
                                //    case 0x0B:
                                //        LogMsg.LogMessage(LogMsg.MSG, "Command 0x0b received.");
                                //        break;
                                //    default:
                                //        LogMsg.LogMessage(LogMsg.ERR, "Unknown command 0x" + opcode + " received.");
                                //        break;
                                //}
                            }

                        }
                    }
                }
                catch (Exception) { }
            }           
        }

        private void ProcessSession()
        {

        }

        public static void AppendAllBytes(string path, byte[] bytes)
        {            
            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

    }
}
