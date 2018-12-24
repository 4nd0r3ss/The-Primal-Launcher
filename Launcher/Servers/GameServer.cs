using Launcher.Characters;
using Launcher.packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Servers
{
    class GameServer : Server
    {           
        private User _user;
        private World _world;

        public GameServer(Blowfish blowfish, World world, User user)
        {
            _user = user;
            _world = world;
            _blowfish = blowfish;
            Start(world.Name, world.Port);
        }

        int count = 0;

        public override void ProcessIncoming(StateObject state)
        {
            Packet packet = new Packet(state.buffer);

            if (packet.IsEncoded == 1)
            {
                //decompress
            }

            packet.ProcessSubPackets(null); //no decrypting here
            //File.WriteAllBytes(@"C:\users\4nd0r\desktop\packet_" + count + ".txt", state.buffer);
            count++;
            //

            while (packet.SubPacketQueue.Count > 0)
            {
                SubPacket sp = packet.SubPacketQueue.Dequeue();               

                switch (sp.Type)
                {
                    case 0x01: //handshake

                        uint sessionId = uint.Parse(Encoding.ASCII.GetString(sp.Data).TrimStart(new[] {'\0'}).Substring(0,12));
                        //File.WriteAllText(@"C:\users\4nd0r\desktop\sessionId.txt", Encoding.ASCII.GetString(sp.Data).Trim(new[] { '\0' }));

                        if (packet.ConnType == 1) //zone
                        {
                            GamePacket zoneHello = new GamePacket
                            {
                                Opcode = 0x1000,
                                Data = new byte[] { 0x00, 0x00, 0x00, 0x01 }
                            };

                            Packet zoneHelloPacket = new Packet(zoneHello);
                            zoneHelloPacket.SubPacketList[0].SourceId = sessionId;

                            byte[] packets = zoneHelloPacket.Build();
                            state.workSocket.Send(packets);
                            //File.WriteAllBytes(@"C:\users\4nd0r\desktop\zoneHello.txt", packets);

                            _log.Warning("[" + _world.Name + "] Zone hello sent.");
                        }
                        else //2 = chat
                        {
                            _log.Warning("Type Chat");
                        }

                        SendHandShake(state.workSocket);
                        CreateSession(state.workSocket, sessionId);
                        break;

                    case 0x03:
                        _log.Warning("[" + _world.Name + "] Received: 0x03");
                        break;

                    case 0x07:
                        _log.Warning("[" + _world.Name + "] Received: 0x07");
                        break;

                    case 0x08:
                        _log.Warning("[" + _world.Name + "] Received: 0x08");
                        break;

                    case 0x1000:
                        _log.Warning("[" + _world.Name + "] Received: 0x100");
                        break;

                    default:
                        _log.Error("[" + _world.Name + "] Unknown packet type: " + sp.Type.ToString("X"));
                        break;
                }
            }
        }

        private void CreateSession(Socket handler, uint sessionId)
        {
            //Have no idea what this could be. Just c/p from IonCannon code.
            byte[] data =  {
                0x6c, 0x00, 0x00, 0x00, 0xC8, 0xD6, 0xAF, 0x2B, 0x38, 0x2B, 0x5F, 0x26, 0xB8, 0x8D, 0xF0, 0x2B,
                0xC8, 0xFD, 0x85, 0xFE, 0xA8, 0x7C, 0x5B, 0x09, 0x38, 0x2B, 0x5F, 0x26, 0xC8, 0xD6, 0xAF, 0x2B,
                0xB8, 0x8D, 0xF0, 0x2B, 0x88, 0xAF, 0x5E, 0x26
            };

            Buffer.BlockCopy(BitConverter.GetBytes(sessionId), 0, data, 0, 0x04);

            GamePacket session = new GamePacket
            {
                Opcode = 0x0002,
                Data = data
            };            

            Packet sessionPacket = new Packet(session);

            byte[] packet = sessionPacket.Build();
            handler.Send(packet);
            //File.WriteAllBytes(@"C:\users\4nd0r\desktop\createSession.txt", packet);

            _log.Message("[" + _world.Name + "] Session packet sent.");
        }

        private void SendHandShake(Socket handler)
        {
            byte[] handshakeData = new byte[0x18];
            Buffer.BlockCopy(BitConverter.GetBytes(0x0e016ee5), 0, handshakeData, 0, 0x4);
            Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);

            GamePacket handshake = new GamePacket
            {
                Opcode = 0x0007,
                Data = handshakeData
            };

            Packet handshakePacket = new Packet(handshake);

            byte[] packet = handshakePacket.Build();
            handler.Send(packet);
            //File.WriteAllBytes(@"C:\users\4nd0r\desktop\handshake.txt", packet);
            _log.Message("[" + _world.Name + "] Handshake packet sent.");
        }

        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
