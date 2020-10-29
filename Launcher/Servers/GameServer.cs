using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class GameServer : Server
    {                          
        //check the usefulness of this variable
        private Queue<Packet> DataRequestResponseQueue = new Queue<Packet>();        

        public GameServer()
        {
            Task.Run(() =>
            {
                while (_listening)
                {
                    ProcessIncoming(ref _connection);
                }
            });            
                    
            Start(World.Name, World.Port);           
        }      

        public override void ProcessIncoming(ref StateObject _connection)
        {          
            while (_connection.bufferQueue.Count > 0)
            {
                byte[] buffer = _connection.bufferQueue.Dequeue();

                Packet packet = new Packet(buffer);

                if (packet.IsEncoded == 1)
                {
                    //decompressing is implemented but not needed so far.
                }

                packet.ProcessSubPackets(null); //no decrypting here            

                while (packet.SubPacketQueue.Count > 0)
                {
                    SubPacket sp = packet.SubPacketQueue.Dequeue();

                    switch (sp.Type)
                    {
                        case 0x01: 
                            CreateGameSession(sp);                           
                            break;

                        case 0x02:
                            _log.Warning("[" + World.Name + "] Received: 0x02");
                            //File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x02.txt", sp.Data);
                            break;

                        case 0x03:
                            ProcessGamePacket(sp);
                            break;

                        case 0x07: //delete actor request?
                             _log.Warning("[" + World.Name + "] Received: 0x07");
                            
                            break;

                        case 0x08:                           
                            _log.Warning("[" + World.Name + "] Received: 0x08");
                            //File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x08.txt", sp.Data);
                            break;

                        case 0x1000:
                            _log.Warning("[" + World.Name + "] Received: 0x1000");
                            break;

                        default:
                            _log.Error("[" + World.Name + "] Unknown packet type: " + sp.Type.ToString("X"));                           
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Processes the command for the gamepacket inside the current subpacket.
        /// </summary> 
        ///<param name="subpacket">The received subpacket.</param>
        private void ProcessGamePacket(SubPacket subpacket)
        {
            //_world.Sender = _connection.socket;
            ushort opcode = (ushort)(subpacket.Data[0x03] << 8 | subpacket.Data[0x02]);           

            switch (opcode)
            {
                case (ushort)ClientOpcode.Ping:
                    Pong(subpacket);
                    break;

                case (ushort)ClientOpcode.Unknown0x02: //unknown                  
                    SendPlayerCharacterId();
                    break;

                case (ushort)ClientOpcode.ChatMessage: //chat message received
                    ProcessChatMessage(subpacket.Data);                    
                    break;

                case 0x06: //world & player spawning        
                    SendMessage(MessageType.GeneralInfo, "Welcome to " + World.Name + "!");
                    SendMessage(MessageType.GeneralInfo, "Welcome to Eorzea!");  
                    SendMessage(MessageType.GeneralInfo, @"To get a list of available commands, type \help in the chat window and hit enter.");
                    World.Instance.Initialize(_connection.socket);                   
                    break;

                case 0x01ce: //friend list request 
                    //_log.Warning("[" + _world.Name + "] Friend list request....");
                    break;

                case 0x01cb: //black list request
                    //_log.Warning("[" + _world.Name + "] Black list request....");
                    break;

                case (ushort)ClientOpcode.PlayerPosition: 
                    UpdatePlayerPosition(subpacket.Data);
                    break;

                case 0x07: //unknown
                    //_log.Warning("Received command 0x07");
                    //File.WriteAllBytes(@"type_0x07.txt", subpacket.Data);
                    break;

                case 0x133: //group created                    
                    break;

                case (ushort)ClientOpcode.EventRequest:                                    
                    ProcessEventRequest(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.DataRequest:                           
                    string request = Encoding.ASCII.GetString(subpacket.Data).Substring(0x14, 0x20).Trim(new[] { '\0' }); 

                    switch (request)
                    {
                        case "charaWork/exp":                           
                            _connection.Send(UserRepository.Instance.User.Character.CharaWorkExp(_connection.socket));   
                         break;
                    }
                    break;

                case (ushort)ClientOpcode.SelectTarget:
                    TargetSelected(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.LockOnTarget:
                    _log.Warning("Target locked");
                    break;

                case (ushort)ClientOpcode.EventResult:
                    _log.Error("[" + World.Name + "] Event Result packet: 0x" + opcode.ToString("X"));
                    File.WriteAllBytes("client_eventresult_" + DateTime.Now.Ticks.ToString() + ".txt", subpacket.Data);
                    World.Instance.TeleportMenuLevel2(_connection.socket, subpacket.Data);
                    break;

                default:
                    _log.Error("[" + World.Name + "] Unknown command: 0x" + opcode.ToString("X"));                    
                    break;
            }
        }

        private void SendPlayerCharacterId()
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(UserRepository.Instance.User.Character.Id), 0, data, 0x08, 0x04);
            _connection.Send(new Packet(new GamePacket { Opcode = (ushort)ServerOpcode.Unknown0x02, Data = data }).ToBytes());
        }

        private void TargetSelected(byte[] data)
        {
            uint targetId = (uint)(data[0x13] << 24 | data[0x12] << 16 | data[0x11] << 8 | data[0x10]);
            uint unknown = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x014]);
            UserRepository.Instance.User.Character.LockTargetActor(_connection.socket, targetId);
        }

        private void UpdatePlayerPosition(byte[] data)
        {
            //get player character
            PlayerCharacter playerCharacter = UserRepository.Instance.User.Character;

            //get player position from packet
            playerCharacter.Position.X = BitConverter.ToSingle(new byte[] { data[0x18], data[0x19], data[0x1a], data[0x1b] }, 0);
            playerCharacter.Position.Y = BitConverter.ToSingle(new byte[] { data[0x1c], data[0x1d], data[0x1e], data[0x1f] }, 0);
            playerCharacter.Position.Z = BitConverter.ToSingle(new byte[] { data[0x20], data[0x21], data[0x22], data[0x23] }, 0);
            playerCharacter.Position.R = BitConverter.ToSingle(new byte[] { data[0x24], data[0x25], data[0x26], data[0x27] }, 0);

            byte[] unknown = new byte[] { data[0x2a], data[0x2b], data[0x2c], data[0x2d], data[0x2e], data[0x2e] };

            //save player position 
            UserRepository.Instance.User.AccountList[0].CharacterList[playerCharacter.Slot] = playerCharacter;
            UserRepository.Instance.UpdateUser();

            //print player position on PL ui.
            ucLogWindow.Instance.PrintPlayerPosition(playerCharacter.Position, unknown);

            //check eorzea time and change BGM if necessary
            //ActorRepository.Instance.Zones.Find(x => x.Id == playerCharacter.Position.ZoneId).GetCurrentBGM();

            //uint unknown0 = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x14]);
            //ushort moveState = (ushort)(data[0x29] << 8 | data[0x28]);
            //ushort unknown1 = (ushort)(data[0x2b] << 8 | data[0x2a]);
            //uint unknown2 = (uint)(data[0x2f] << 24 | data[0x2e] << 16 | data[0x2d] << 8 | data[0x2c]);

            //_log.Warning("Player position: X=" + _user.Character.Position.X + ", Y=" + _user.Character.Position.Y + ", Z=" + _user.Character.Position.Z + ", R=" + _user.Character.Position.R);
            //if(unknown1 != 0x19 && unknown1 != 0xed)
            //_log.Warning("U0=0x" + unknown0.ToString("X") + ", U1=0x" + unknown1.ToString("X") + ", U2=0x" + unknown2.ToString("X") + ", MS=0x"+ moveState.ToString("X"));
        }

        public void ProcessEventRequest(byte[] data)
        {
            PlayerCharacter playerCharacter = UserRepository.Instance.User.Character;
            string eventType = Encoding.ASCII.GetString(data, 0x21, 14);
            ushort command = (ushort)(data[0x15] << 8 | data[0x14]);

            //File.WriteAllBytes(command + ".txt", data);

            //_log.Warning("Received event request: 0x12d");
            _log.Warning("event: " + eventType + ", command: " + command.ToString("X2"));
            File.WriteAllBytes("eventrequest_" + DateTime.Now.Ticks.ToString() + ".txt", data);

            if (eventType.IndexOf("commandForced") >= 0)
            {
                switch (command)
                {
                    case (ushort)Command.BattleStance:
                        playerCharacter.SetMainState(_connection.socket, MainState.Active, 0xbf);
                        playerCharacter.SendActionResult(_connection.socket, Command.BattleStance);
                        playerCharacter.EndClientOrderEvent(_connection.socket, eventType);
                        break;
                    case (ushort)Command.NormalStance:
                        playerCharacter.SetMainState(_connection.socket, MainState.Passive, 0xbf);
                        playerCharacter.SendActionResult(_connection.socket, Command.NormalStance);
                        playerCharacter.EndClientOrderEvent(_connection.socket, eventType);
                        break;
                    case (ushort)Command.MountChocobo:
                        playerCharacter.ToggleMount(_connection.socket, Command.MountChocobo);
                        _log.Success("Player is now mounted.");
                        break;
                   
                } 
            }
            else if (eventType.IndexOf("commandRequest") >= 0)
            {
                switch (command)
                {
                    case (ushort)Command.UmountChocobo:
                        playerCharacter.ToggleMount(_connection.socket, Command.UmountChocobo);
                        _log.Success("Player is now dismounted.");
                        break;

                    case (ushort)Command.DoEmote:
                        _log.Warning("emote id:" + data[0x45].ToString("X2"));
                        playerCharacter.DoEmote(_connection.socket);
                        break;

                    case (ushort)Command.ChangeEquipment:
                        playerCharacter.ChangeGear(_connection.socket, data);
                        break;
                    case (ushort)Command.EquipSouldStone:
                        playerCharacter.EquipSoulStone(_connection.socket, data);
                        break;
                }
            }
            else if (eventType.IndexOf("commandContent") >= 0)
            {
                switch (command)
                {
                    case (ushort)Command.Teleport:
                        //creates teleport widget
                        byte[] startserverorder =
                        {
                            0x41, 0x29, 0x9B, 0x02, 0x9C, 0x5E, 0xF0, 0xA0, 0x00, 0x63, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64,
                            0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x65, 0x6C, 0x65, 0x67, 0x61, 0x74,
                            0x65, 0x43, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0xA0, 0xF0, 0x5E, 0x9C, 0x02, 0x65,
                            0x76, 0x65, 0x6E, 0x74, 0x52, 0x65, 0x67, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64,
                            0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7A, 0xB4, 0x53, 0x40, 0x00, 0x00, 0x00
                        };

                        Buffer.BlockCopy(BitConverter.GetBytes(playerCharacter.Id), 0, startserverorder, 0, 4);

                        _connection.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.StartEventRequest,
                            Data = startserverorder
                        }).ToBytes());
                        break;
                   
                }
            }
        }

        /// <summary>
        /// Analyzes text sent by the player through the chat window. If it matches what is expected as a command, process the command.
        /// </summary>
        /// <param name="data">The data from the received message packet.</param>
        private void ProcessChatMessage(byte[] data)
        {
            //get full packet data as string
            string message = Encoding.ASCII.GetString(data);
            //get message 
            message = message.Substring(0x2c,0x1fb).Trim(new[] { '\0' }).ToLower(); //0x1fb = max message size

            if (message.Substring(0, 1).Equals(@"\")) //is command
            {
                PlayerCharacter pc = UserRepository.Instance.User.Character;
                string command;
                bool hasParameters = false;
                List<string> parameters = new List<string>();                

                if (message.IndexOf(' ') > 0)
                {
                    parameters.AddRange(message.Split(' '));                   
                    command = parameters[0];
                    parameters.RemoveAt(0);
                    
                }
                else                
                    command = message;

                if (parameters.Count > 0)
                    hasParameters = true;

                switch (command)
                {                  
                    case @"\help":
                        SendMessage(MessageType.System, "Available commands:");
                        SendMessage(MessageType.System, @"\setweather {weather name}");
                        SendMessage(MessageType.System, @"\setmusic {music id}");
                        break;

                    case @"\setweather":
                        string wheatherName = parameters[0].First().ToString().ToUpper() + parameters[0].Substring(1);

                        if (Enum.IsDefined(typeof(Weather), wheatherName))
                        {
                            World.Instance.SetWeather(_connection.socket, (Weather)Enum.Parse(typeof(Weather), wheatherName));

                            switch (wheatherName)
                            {
                                case "Dalamudthunder":
                                    World.Instance.SetMusic(_connection.socket, 29); //set music to "Answers", I THINK it was the original track for this weather.
                                    break;
                            }
                        }
                        else
                            SendMessage(MessageType.System, "Requested weather not found.");                        
                        break;

                    case @"\setmusic":
                        if(byte.TryParse(parameters[0], out byte id))
                            World.Instance.SetMusic(_connection.socket, id);
                        else
                            SendMessage(MessageType.System, "Invalid music id.");
                        break;

                    case @"\setemote":
                        byte[] emote = new byte[] { 0x00, 0xB0, 0x00, 0x05, 0x41, 0x29, 0x9B, 0x02, 0x6E, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        Buffer.BlockCopy(BitConverter.GetBytes(UserRepository.Instance.User.Character.Id), 0, emote, 0x04, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToByte(parameters[1])), 0, emote, Convert.ToByte(parameters[0]), 1);       
                        _connection.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.DoEmote,
                            Data = emote
                        }).ToBytes());

                        break;                   

                    case @"\resetlevel":
                        short level = 1;

                        if (hasParameters) 
                            Int16.TryParse(parameters[0], out level);                           
                       
                        pc.LevelDown(_connection.socket, level);
                        break;

                    //case @"\spawn":
                    //    //_world.ZoneList.GetZone(_user.Character.Position.ZoneId).SpawnActors(_connection.socket);
                    //    break;

                    case @"\teleport":
                        if (parameters.Count > 0)
                            World.Instance.TeleportPlayer(_connection.socket, Convert.ToUInt32(parameters[0]));
                        break;

                    case @"\setposition":
                        if (parameters.Count > 0)
                        {
                            Position pos = pc.Position;
                            pos.X = Convert.ToSingle(parameters[0]);
                            pos.Y = Convert.ToSingle(parameters[1]);
                            pos.Z = Convert.ToSingle(parameters[2]);

                            pc.Position = pos;
                            pc.SetPosition(_connection.socket, pos);
                        }
                        break;

                    case @"\spawn":
                        if (hasParameters)
                        {
                            if(parameters[0] == "antelope")
                                TestPackets.Antelope(pc.Id, pc.Position, _connection.socket);
                            else if(parameters[0] == "populace")
                                TestPackets.Populace(pc.Id, pc.Position, _connection.socket);
                            else if (parameters[0] == "company")
                                TestPackets.CompanyWarp(pc.Id, pc.Position, _connection.socket);

                        }
                        
                        //TestPackets.Antelope(pc.Id, pc.Position, _connection.socket);
                        //TestPackets.TeleportInn(UserFactory.Instance.User.Character.Id, UserFactory.Instance.User.Character.Position, _connection.socket);                      
                        //Aetheryte ae = new Aetheryte(1280007, 20925, new Position(128, 582.47f, 54.52f, -1.2f, 0f, 0));
                        
                        _log.Info("sent test");

                        break;

                    case @"\text":

                        data = new byte[] { 0x41, 0x29, 0x9B, 0x02, 0x01, 0x00, 0xF8, 0x5F, 0x89, 0x77, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x02, 0x00, 0x00, 0x6B, 0x1E, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                        if(parameters.Count>0)
                            Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToUInt32(parameters[0])), 0, data, 0x08, 4);

                        Buffer.BlockCopy(BitConverter.GetBytes(pc.Id), 0, data, 0, 4);  

                        GamePacket g = new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.TextSheetMessage70b,
                            Data = data
                        };

                        SubPacket sb = new SubPacket(g)
                        {
                            SourceId = 0x5ff80001
                        };

                        Packet pk = new Packet(sb);                        

                        _connection.Send(pk.ToBytes());
                        break;

                    case @"\addloot":
                        Inventory.AddLoot(_connection.socket);
                        break;

                    case @"\additem":
                        pc.Inventory.AddItem(ref pc.Inventory.Bag,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToUInt32(parameters[1]) : 1),
                            _connection.socket);
                        break;

                    case @"\addkeyitem":   
                        pc.Inventory.AddItem(ref pc.Inventory.KeyItems, 
                            parameters[0].Replace("_", " ").Replace("'", "''"), 
                            (parameters.Count > 2 ? Convert.ToUInt32(parameters[1]) : 1), //TODO: should key items be always 1?
                            _connection.socket);
                        break;

                    case @"\addexp":
                        if (hasParameters)  
                            UserRepository.Instance.User.Character.AddExp(_connection.socket, Convert.ToInt32(parameters[0]));   
                        break;
                   
                    case @"\removeactor":
                        GamePacket gps = new GamePacket
                        {
                            Opcode = 0x7,
                            Data = new byte[8]
                        };
                        Packet packet = new Packet(new SubPacket(gps) { SourceId = UserRepository.Instance.User.Character.Id, TargetId = UserRepository.Instance.User.Character.Id });
                        _connection.Send(packet.ToBytes());
                        //UserFactory.Instance.User.Character.SetPosition(_connection.socket, ZoneList.EntryPoints.Find(x => x.ZoneId == Convert.ToUInt32(value)), 2, 1);
                        UserRepository.Instance.User.Character.Position = EntryPoints.List.Find(x => x.ZoneId == Convert.ToUInt32(parameters[0]));
                        World.Instance.Initialize(_connection.socket);
                        break;

                    case @"\anim":
                        short animid = 0x29;
                        byte another = 0x04;
                        if (hasParameters)
                            animid = Convert.ToInt16(parameters[0]);

                        if (parameters.Count > 1)
                            another = Convert.ToByte(parameters[1]);

                        byte[] anim = new byte[] { 0x29, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };

                        Buffer.BlockCopy(BitConverter.GetBytes(animid), 0, anim, 0, 2);
                        anim[3] = another;

                        _connection.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.PlayAnimationEffect,
                            Data = anim
                        }).ToBytes());
                        break;

                    default:
                        SendMessage(MessageType.System, "Unknown command.");
                        break;
                }
            }
            else            
                SendMessage(MessageType.System, "Sorry, no one is listening. What a lonely life...");            
        }

        /// <summary>
        /// Answer to a ping request from the client.
        /// </summary>
        /// <param name="subpacket">The received subpacket with the information to be sent back with the flag 0x14d (unknown meaning).</param>        
        private void Pong(SubPacket subpacket)
        {
            GamePacket pong = new GamePacket
            {
                Opcode = 0x01,
                Data = new byte[0x20]
            };

            ushort interval = 0x004d;

            Buffer.BlockCopy(subpacket.Data, 0x10, pong.Data, 0, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(interval), 0, pong.Data, 0x04, 0x02);

            Packet pongPacket = new Packet(pong);
            _connection.Send(pongPacket.ToBytes());            
        }

        /// <summary>
        /// Sends a message to the game client. This message will appear in the game chat window.
        /// </summary>
        /// <param name="message">A string containing the message to be sent.</param>
        private void SendMessage(MessageType type, string message)
        {
            MessagePacket messagePacket = new MessagePacket
            {                
                MessageType = type,
                TargetId = UserRepository.Instance.User.SessionId,
                Message = message
            };

            _log.Chat("[" + World.Name + "] " + message); //show sent message on launcher output window (make it optional?)

            Packet packet = new Packet(new SubPacket(messagePacket) { SourceId = UserRepository.Instance.User.SessionId, TargetId = UserRepository.Instance.User.SessionId });
            _connection.Send(packet.ToBytes());
        }    

        /// <summary>
        /// Performs security check within the client.
        /// </summary> 
        private void CreateGameSession(SubPacket sp)
        {
            UserRepository.Instance.User.SessionId = uint.Parse(Encoding.ASCII.GetString(sp.Data).TrimStart(new[] { '\0' }).Substring(0, 12));

            byte[] handshakeData = new byte[0x18];
            Buffer.BlockCopy(BitConverter.GetBytes(0x0E016EE5), 0, handshakeData, 0, 0x4);
            Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);

            SubPacket handshake = new SubPacket
            {
                Type = 0x07,               
                TargetId = UserRepository.Instance.User.SessionId,
                Data = handshakeData
            };

            Packet handshakePacket = new Packet(handshake);           
            _connection.Send(handshakePacket.ToBytes());

            //login packet sequence from SU g_client0_login2 byte array. It was originally compressed.
            byte[] data =  {
                0x00, 0x00, 0x00, 0x00, 0xc8, 0xd6, 0xaf, 0x2b, 0x38, 0x2b, 0x5f, 0x26, 0xb8, 0x8d,
                0xf0, 0x2b, 0xc8, 0xfd, 0x85, 0xfe, 0xa8, 0x7c, 0x5b, 0x09, 0x38, 0x2b, 0x5f, 0x26,
                0xc8, 0xd6, 0xaf, 0x2b, 0xb8, 0x8d, 0xf0, 0x2b, 0x88, 0xaf, 0x5e, 0x26
            };

            Buffer.BlockCopy(BitConverter.GetBytes(UserRepository.Instance.User.Character.Id), 0, data, 0, 0x04);

            SubPacket session = new SubPacket
            {
                Type = 0x02,
                TargetId = UserRepository.Instance.User.SessionId,
                Data = data
            };

            Packet sessionPacket = new Packet(session);
            _connection.Send(sessionPacket.ToBytes());

            _log.Info("[" + World.Name + "] Session created.");
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
