using Launcher.Characters;
using Launcher.packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public override void ProcessIncoming(StateObject state)
        {
            Packet packet = new Packet(state.buffer);

            if (packet.IsEncoded == 1)
            {
                //decompress
            }

            packet.ProcessSubPackets(null); //no decrypting here

            //

            while (packet.SubPacketQueue.Count > 0)
            {
                SubPacket sp = packet.SubPacketQueue.Dequeue();

                //File.WriteAllBytes("packet_" + packet.IsEncoded + ".txt", sp.Data);

                switch (sp.Type)
                {
                    case 0x01: //handshake
                        _log.Warning("Create Session");
                        if (packet.ConnType == 1) //zone
                        {
                            _log.Warning("Type zone");
                        }
                        else //2 = chat
                        {

                        }

                        byte[] handshakeData = new byte[0x18];
                        Buffer.BlockCopy(BitConverter.GetBytes(0x0e016ee5), 0, handshakeData, 0, 0x4);
                        Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);

                        GamePacket handshake = new GamePacket
                        {
                            Opcode = 0x0007,
                            TimeStamp = GetTimeStampHex(),
                            Data = handshakeData
                        };

                        Packet handshakePacket = new Packet(handshake);
                        state.workSocket.Send(handshakePacket.Build(ref _blowfish));
                        _log.Message("["+_world.Name + "] Handshake packet sent.");

                        break;

                    case 0x03:

                        break;
                    default:
                        break;
                }
            }
        }

        private static byte[] PackPage(string file) => new HtmlPacket(file).ToBytes();

        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
