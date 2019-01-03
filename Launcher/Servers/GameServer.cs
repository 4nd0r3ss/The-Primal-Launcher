using Launcher.Characters;
using Launcher.Packets;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

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

        public override void ProcessIncoming()
        {
            Packet packet = new Packet(_connection.buffer);

            if (packet.IsEncoded == 1)
            {
                //decompress
            }           

            packet.ProcessSubPackets(null); //no decrypting here
            //File.WriteAllBytes(@"C:\users\4nd0r\desktop\packet_" + count + ".txt", state.buffer);
           
            while (packet.SubPacketQueue.Count > 0)
            {
                SubPacket sp = packet.SubPacketQueue.Dequeue();               

                switch (sp.Type)
                {
                    case 0x01: //handshake

                        string connType = "Zone";
                        _user.SessionId = uint.Parse(Encoding.ASCII.GetString(sp.Data).TrimStart(new[] {'\0'}).Substring(0,12));                        

                        if (packet.ConnType == 1) //zone
                        {
                            GamePacket zoneHello = new GamePacket
                            {
                                Opcode = 0x1000,
                                Data = new byte[] { 0x01, 0x00, 0x00, 0x00 }
                            };

                            Packet zoneHelloPacket = new Packet(zoneHello);
                            zoneHelloPacket.SubPacketList[0].SourceId = _user.SessionId;                           
                            _connection.Send(zoneHelloPacket.Build());

                            ///////////////////////////////////////////////////////////
                            GamePacket setMap = new GamePacket
                            {
                                Opcode = 0x05,
                                Data = new byte[] { 0xD1, 0x00, 0x00, 0x00, 0xF4, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
                            };
                            Packet setMapPacket = new Packet(setMap);
                            _connection.socket.Send(setMapPacket.Build());
                            
                            



                        }
                        else //2 = chat
                        {
                            connType = "Chat";
                        }

                        SendHandShake();   
                        CreateSession();
                        _log.Message("[" + _world.Name + "] " + connType + " session created.");
                        break;

                    case 0x03:
                        ProcessCommand(sp);
                        break;

                    case 0x07:
                        _log.Warning("[" + _world.Name + "] Received: 0x07");
                        break;

                    case 0x08:
                        _log.Warning("[" + _world.Name + "] Received: 0x08");
                        break;

                    case 0x1000:
                        _log.Warning("[" + _world.Name + "] Received: 0x1000");
                        break;

                    default:
                        _log.Error("[" + _world.Name + "] Unknown packet type: " + sp.Type.ToString("X"));
                        break;
                }
            }
        }

        /// <summary>
        /// Processes the command for the gamepacket inside the current subpacket.
        /// </summary> 
        ///<param name="subpacket">The received subpacket.</param>
        private void ProcessCommand(SubPacket subpacket)
        {
            ushort command = (ushort)(subpacket.Data[0x03] << 8 | subpacket.Data[0x02]);

            switch (command)
            {
                case 0x01: //ping
                    Pong(subpacket);
                    break;

                case 0x02: //unknown
                    Packet unknown2 = new Packet(subpacket);
                    _connection.Send(unknown2.Build());
                    _log.Warning("[" + _world.Name + "]  Unknown command: " + command.ToString("X"));
                    break;

                case 0x06: //lots of thing to send
                    SendMessage("Welcome to " + _world.Name + "!");
                    SendMessage("Welcome to Eorzea!");
                    SendMessage("Here is a test message of the day from the world server!");

                   

                    Group group = new Group
                    {
                        Character = _user.AccountList[0].CharacterList.Find(x => x.Id == _user.SelectedCharacterId),
                        ActorId = _user.SessionId
                    };

                    //File.WriteAllBytes(@"c:\users\4nd0r\desktop\party.txt", group.GetPartyPacket().Build());
                    File.WriteAllBytes(@"c:\users\4nd0r\desktop\retainer.txt", group.GetRetainerPacket().Build());
                    //File.WriteAllBytes(@"c:\users\4nd0r\desktop\linkshell.txt", group.GetLinkshellPacket().Build());

                    //_connection.Send(group.GetPartyPacket().Build());
                    _connection.Send(group.GetRetainerPacket().Build());
                    //_connection.Send(group.GetLinkshellPacket().Build());

                    TestPackets tp = new TestPackets(_user.SessionId, _connection.socket);

                   

                    break;

                case 0x01ce: //friend list request 
                    _log.Warning("[" + _world.Name + "] Friend list request....");
                    break;

                case 0x01cb: //black list request
                    _log.Warning("[" + _world.Name + "] Black list request....");
                    break;

                default:
                    _log.Error("[" + _world.Name + "] Unknown command: " + command.ToString("X"));
                    break;
            }



            //Packet packet = new Packet(subPacket);
            //handler.Send(packet.Build());
            //_log.Warning("[" + _world.Name + "] ConnectToZoneServer() SPType:" + subPacket.Type.ToString("X") + " GPOpcode: " + subPacket.Data[0x03].ToString("X") + subPacket.Data[0x02].ToString("X"));

            //File.WriteAllBytes(@"C: \users\4nd0r\desktop\ConnectToZoneServer.txt", subPacket.Data);
        }

        /// <summary>
        /// Answers to a ping request from the client.
        /// </summary>
        /// <param name="subpacket">The received subpacket with the information to be sent back with the flag 0x14d.</param>        
        private void Pong(SubPacket subpacket)
        {
            GamePacket pong = new GamePacket
            {
                Opcode = 0x01,
                Data = new byte[0x20]
            };

            Buffer.BlockCopy(subpacket.Data, 0x10, pong.Data, 0, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(0x014d), 0, pong.Data, 0x04, 0x02);

            Packet pongPacket = new Packet(pong);
            _connection.Send(pongPacket.Build());
            //_log.Warning("[" + _world.Name + "] Pong packet sent to client.");
        }

        /// <summary>
        /// Sends a message to the game client. This message will appear in the game chat window.
        /// </summary>
        /// <param name="message">A string containing the message to be sent.</param>
        private void SendMessage(string message)
        {
            MessagePacket messagePacket = new MessagePacket
            {                
                MessageType = MessageType.GeneralInfo,
                TargetId = _user.SessionId,
                Message = message
            };

            _log.Message("[" + _world.Name + "] " + message); //show sent message on launcher output window (make it optional?)

            Packet packet = new Packet(messagePacket);
            _connection.Send(packet.Build());
        }

        /// <summary>
        /// Tells the game client that a user session has been created.
        /// </summary>       
        private void CreateSession()
        {
            //login packet sequence from SU g_client0_login2 byte array. It was compressed.
            byte[] data =  {
                0x00, 0x00, 0x00, 0x00, 0xC8, 0xD6, 0xAF, 0x2B, 0x38, 0x2B, 0x5F, 0x26, 0xB8, 0x8D, 0xF0, 0x2B,
                0xC8, 0xFD, 0x85, 0xFE, 0xA8, 0x7C, 0x5B, 0x09, 0x38, 0x2B, 0x5F, 0x26, 0xC8, 0xD6, 0xAF, 0x2B,
                0xB8, 0x8D, 0xF0, 0x2B, 0x88, 0xAF, 0x5E, 0x26
            };

            Buffer.BlockCopy(BitConverter.GetBytes(_user.SessionId), 0, data, 0, 0x04);            

            SubPacket session = new SubPacket
            {
                Type = 0x02,
                Size = (ushort)(0x10 + data.Length), //TODO: figure out a way to automate this.
                Data = data
            };

            Packet sessionPacket = new Packet(session);          
            _connection.Send(sessionPacket.Build());           
        }

        /// <summary>
        /// Performs security check within the client.
        /// </summary> 
        private void SendHandShake()
        {
            byte[] handshakeData = new byte[0x18];
            Buffer.BlockCopy(BitConverter.GetBytes(0x0e016ee5), 0, handshakeData, 0, 0x4);
            Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);
           
            SubPacket handshake = new SubPacket
            {
                Type = 0x07,
                Size = (ushort)(0x10 + handshakeData.Length), //TODO: figure out a way to automate this.
                Data = handshakeData
            };

            Packet handshakePacket = new Packet(handshake);           
            _connection.Send(handshakePacket.Build());
        }

        /// <summary>
        /// Start the next server, if any.
        /// </summary> 
        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
