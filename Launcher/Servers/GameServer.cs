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
        private World _world = WorldFactory.GetWorld(UserFactory.Instance.User.Character.WorldId);
        
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
                    
            Start(_world.Name, _world.Port);           
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
                            _log.Warning("[" + _world.Name + "] Received: 0x02");
                            //File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x02.txt", sp.Data);
                            break;

                        case 0x03:
                            ProcessGamePacket(sp);
                            break;

                        case 0x07: //delete actor request?
                             _log.Warning("[" + _world.Name + "] Received: 0x07");
                            
                            break;

                        case 0x08:                           
                            _log.Warning("[" + _world.Name + "] Received: 0x08");
                            //File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x08.txt", sp.Data);
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
                    SendMessage(MessageType.GeneralInfo, "Welcome to " + _world.Name + "!");
                    SendMessage(MessageType.GeneralInfo, "Welcome to Eorzea!");  
                    SendMessage(MessageType.GeneralInfo, @"To get a list of available commands, type \help in the chat window and hit enter.");
                    _world.Initialize(_connection.socket);                   
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
                    uint targetId = (uint)(subpacket.Data[0x13] << 24 | subpacket.Data[0x12] << 16 | subpacket.Data[0x11] << 8 | subpacket.Data[0x10]);
                    string request = Encoding.ASCII.GetString(subpacket.Data).Substring(0x14, 0x20).Trim(new[] { '\0' });
                    uint sequence = (uint)(subpacket.Data[0x37] << 24 | subpacket.Data[0x36] << 16 | subpacket.Data[0x35] << 8 | subpacket.Data[0x34]);

                    //Implemented a queue of packets to send in order. if this doesnt work in the future, implement a dictionary with sequence number as key.
                    switch (request)
                    {
                        case "charaWork/exp":
                            _log.Warning("Request: " + request + " for target: 0x" + targetId.ToString("X") + " sequence: 0x" +sequence.ToString("X"));

                            UserFactory.Instance.User.Character.Inventory.Update(_connection.socket);

                            if (DataRequestResponseQueue.Count == 0)
                            {

                                byte[] data = new byte[]
                                {
                                0x72, 0x5F, 0x38, 0x43, 0x39, 0x2F, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x01, 0xB1, 0x63, 0x68, 0x61, 0x72, 0x61, 0x57, 0x6F, 0x72, 0x6B, 0x2F,
                                0x65, 0x78, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                                };

                                GamePacket gp = new GamePacket
                                {
                                    Opcode = 0x137,
                                    Data = data
                                };
                                Packet packet = new Packet(new SubPacket(gp) { SourceId = UserFactory.Instance.User.Character.Id, TargetId = UserFactory.Instance.User.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                //DataRequestResponseQueue.Enqueue(packet);

                                ////////////////////////////////////
                                ///
                                data = new byte[]
                               {
                                0x1E, 0x0B, 0x38, 0x43, 0x39, 0x2F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x03, 0xB1, 0x63, 0x68, 0x61, 0x72, 0x61, 0x57, 0x6F, 0x72, 0x6B, 0x2F, 0x65, 0x78, 0x70, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                               };

                                gp = new GamePacket
                                {
                                    Opcode = 0x137,
                                    Data = data
                                };
                                packet = new Packet(new SubPacket(gp) { SourceId = UserFactory.Instance.User.Character.Id, TargetId = UserFactory.Instance.User.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                //DataRequestResponseQueue.Enqueue(packet);

                                ///////////////////////////////////////////
                                ///
                                data = new byte[]
                               {
                                0x72, 0x5F, 0x80, 0xCE, 0xD1, 0x0A, 0xFF, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00,
                                0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0xFF, 0x00, 0x32, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
                                0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
                                0x32, 0x00, 0x32, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0x32, 0x00,
                                0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0xFF, 0x00,
                                0xFF, 0x00, 0x32, 0x00, 0x32, 0x00, 0x32, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
                                0xFF, 0x00, 0xFF, 0x00, 0x01, 0xB1, 0x63, 0x68, 0x61, 0x72, 0x61, 0x57, 0x6F, 0x72, 0x6B, 0x2F,
                                0x65, 0x78, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                               };

                                gp = new GamePacket
                                {
                                    Opcode = 0x137,
                                    Data = data
                                };
                                packet = new Packet(new SubPacket(gp) { SourceId = UserFactory.Instance.User.Character.Id, TargetId = UserFactory.Instance.User.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                //DataRequestResponseQueue.Enqueue(packet);
                                ///////////////////////////////////////
                                ///
                                data = new byte[]
                               {
                                0x1E, 0x0B, 0x80, 0xCE, 0xD1, 0x0A, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
                                0x03, 0x8F, 0x63, 0x68, 0x61, 0x72, 0x61, 0x57, 0x6F, 0x72, 0x6B, 0x2F, 0x65, 0x78, 0x70, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                               };

                                gp = new GamePacket
                                {
                                    Opcode = 0x137,
                                    Data = data
                                };
                                packet = new Packet(new SubPacket(gp) { SourceId = UserFactory.Instance.User.Character.Id, TargetId = UserFactory.Instance.User.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                //DataRequestResponseQueue.Enqueue(packet);
                            }
                            else
                            {
                               //maybe verify if sequence number is the same? after the second packet, the remaining sequence numbers are the same.
                            }

                            //_connection.socket.Send(DataRequestResponseQueue.Dequeue().ToBytes());

                            break;
                    }
                    break;

                case (ushort)ClientOpcode.SelectTarget:
                    TargetSelected(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.LockOnTarget:
                    _log.Warning("Target locked");
                    break;

                default:
                    _log.Error("[" + _world.Name + "] Unknown command: 0x" + opcode.ToString("X"));                    
                    break;
            }
        }

        private void SendPlayerCharacterId()
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(UserFactory.Instance.User.Character.Id), 0, data, 0x08, 0x04);
            _connection.Send(new Packet(new GamePacket { Opcode = (ushort)ServerOpcode.Unknown0x02, Data = data }).ToBytes());
        }

        private void TargetSelected(byte[] data)
        {
            uint targetId = (uint)(data[0x13] << 24 | data[0x12] << 16 | data[0x11] << 8 | data[0x10]);
            uint unknown = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x014]);
            UserFactory.Instance.User.Character.LockTargetActor(_connection.socket, targetId);
        }

        private void UpdatePlayerPosition(byte[] data)
        {
            //get player character
            PlayerCharacter playerCharacter = UserFactory.Instance.User.Character;

            //get player position from packet
            playerCharacter.Position.X = BitConverter.ToSingle(new byte[] { data[0x18], data[0x19], data[0x1a], data[0x1b] }, 0);
            playerCharacter.Position.Y = BitConverter.ToSingle(new byte[] { data[0x1c], data[0x1d], data[0x1e], data[0x1f] }, 0);
            playerCharacter.Position.Z = BitConverter.ToSingle(new byte[] { data[0x20], data[0x21], data[0x22], data[0x23] }, 0);
            playerCharacter.Position.R = BitConverter.ToSingle(new byte[] { data[0x24], data[0x25], data[0x26], data[0x27] }, 0);

            byte[] unknown = new byte[] { data[0x2a], data[0x2b], data[0x2c], data[0x2d], data[0x2e], data[0x2e] };

            //save player position 
            UserFactory.Instance.User.AccountList[0].CharacterList[playerCharacter.Slot] = playerCharacter;
            UserFactory.Instance.UpdateUser();

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
            PlayerCharacter playerCharacter = UserFactory.Instance.User.Character;
            string eventType = Encoding.ASCII.GetString(data, 0x21, 14);
            ushort command = (ushort)(data[0x15] << 8 | data[0x14]);

            //File.WriteAllBytes(command + ".txt", data);

            //_log.Warning("Received event request: 0x12d");
            //_log.Warning("event: " + eventType + ", command: " + command.ToString("X2"));

            if (eventType.IndexOf("commandForced") >= 0)
            {
                switch (command)
                {
                    case (ushort)Command.BattleStance:
                        playerCharacter.SetMainState(_connection.socket, MainState.Active, 0xbf);
                        playerCharacter.BattleActionResult(_connection.socket, command);
                        playerCharacter.EndClientOrderEvent(_connection.socket, eventType);
                        break;
                    case (ushort)Command.NormalStance:
                        playerCharacter.SetMainState(_connection.socket, MainState.Passive, 0xbf);
                        playerCharacter.BattleActionResult(_connection.socket, command);
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
                        playerCharacter.Inventory.SwitchGear(_connection.socket, data);
                        break;
                }
            }
            else if (eventType.IndexOf("commandContent") >= 0)
            {
                switch (command)
                {
                    case (ushort)Command.Teleport:
                        TestPackets.Teleport(UserFactory.Instance.User.Character.Id, _connection.socket);
                        _log.Success("Sent teleport sequence.");
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
                string command;
                string value = "";
                string[] split = null;

                if (message.IndexOf(' ') > 0)
                {
                    split = message.Split(' ');
                    command = split[0];
                    value = split[1];
                }
                else                
                    command = message;                              

                switch (command)
                {
                    case @"\setdoor":                        
                        break;

                    case @"\help":
                        SendMessage(MessageType.System, "Available commands:");
                        SendMessage(MessageType.System, @"\setweather {weather name}");
                        SendMessage(MessageType.System, @"\setmusic {music id}");

                        break;

                    case @"\setweather":
                        value = value.First().ToString().ToUpper() + value.Substring(1);

                        if (Enum.IsDefined(typeof(Weather), value))
                        {
                            _world.SetWeather(_connection.socket, (Weather)Enum.Parse(typeof(Weather), value));

                            switch (value)
                            {
                                case "Dalamudthunder":
                                    _world.SetMusic(_connection.socket, 29); //set music to "Answers", I THINK it was the original track for this weather.
                                    break;
                            }
                        }
                        else
                            SendMessage(MessageType.System, "Requested weather not found.");                        
                        break;

                    case @"\setmusic":
                        if(byte.TryParse(value, out byte id))
                            _world.SetMusic(_connection.socket, id);
                        else
                            SendMessage(MessageType.System, "Invalid music id.");
                        break;

                    case @"\spawngate":
                        //TestPackets.SpawnAetheryte(UserFactory.Instance.User.Character.Id, UserFactory.Instance.User.Character.Position, _connection.socket);
                        break;

                    case @"\setemote":
                        byte[] emote = new byte[] { 0x00, 0xB0, 0x00, 0x05, 0x41, 0x29, 0x9B, 0x02, 0x6E, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        Buffer.BlockCopy(BitConverter.GetBytes(UserFactory.Instance.User.Character.Id), 0, emote, 0x04, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToByte(split[2])), 0, emote, Convert.ToByte(value), 1);       
                        _connection.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.DoEmote,
                            Data = emote
                        }).ToBytes());

                        break;

                    case @"\setmap": //teleport
                        //get map entry point
                        if (!value.Equals("") && value != null)
                        {
                            _world.TeleportPlayer(_connection.socket, Convert.ToUInt32(value));
                        }

                        //if (!value.Equals("") && value != null)
                        //{
                        //    Position pos = Aetheryte.AetheryteList.Find(x => x.Value.Id == 1280040).Value.Position;
                        //    Position pos = UserFactory.Instance.User.Character.Position;
                        //    pos.X = 0;
                        //    pos.Y = 0;
                        //    pos.Z = 0;
                        //    _user.Character.Position.R = value[4];
                        //    pos.ZoneId = Convert.ToUInt32(value);

                        //    UserFactory.Instance.User.Character.SetPosition(_connection.socket, pos, 2);
                        //}
                        break;                        

                    case @"\getposition":
                        Position current = UserFactory.Instance.User.Character.Position;

                        if(value != "" && value == "hex")
                            SendMessage(MessageType.System, "Current position: X: " + current.X.ToString("X") + ", Y: " + current.Y.ToString("X") + ", Z: " + current.Z.ToString("X") + ", R: " + current.R.ToString("X"));
                        else
                            SendMessage(MessageType.System, "Current position: X: " + current.X + ", Y: " + current.Y + ", Z: " + current.Z + ", R: " + current.R);
                        break;

                    case @"\playbg":

                        GamePacket gp = new GamePacket
                        {
                            Opcode = 0xd9,
                            Data = new byte[]
                            {
                                0x73, 0x74, 0x74, 0x30, 0x00, 0x00, 0x00, 0x00
                            }
                        };

                        Packet p = new Packet(new SubPacket(gp) { SourceId = 0x44d80036, TargetId = UserFactory.Instance.User.Character.Id });

                        _connection.Send(p.ToBytes());

                        break;

                    //case @"\spawn":
                    //    //_world.ZoneList.GetZone(_user.Character.Position.ZoneId).SpawnActors(_connection.socket);
                    //    break;

                    case @"\teleport":
                        if(split.Length > 2)
                        {
                            //Position pos = Aetheryte.AetheryteList.Find(x => x.Value.Id == 1280040).Value.Position;
                            Position pos = UserFactory.Instance.User.Character.Position;
                            pos.X = Convert.ToSingle(split[1]);
                            pos.Y = Convert.ToSingle(split[2]);
                            pos.Z = Convert.ToSingle(split[3]);
                            //_user.Character.Position.R = value[4];
                            if (split.Length > 4)
                                pos.ZoneId = Convert.ToUInt32(split[4]);

                            UserFactory.Instance.User.Character.SetPosition(_connection.socket, pos, 2);
                        }                        
                        break;                    

                    case @"\spawn":
                        PlayerCharacter pc = UserFactory.Instance.User.Character;
                        TestPackets.SendTest(pc.Id, pc.Position, _connection.socket);
                        //TestPackets.TeleportInn(UserFactory.Instance.User.Character.Id, UserFactory.Instance.User.Character.Position, _connection.socket);                      
                        //Aetheryte ae = new Aetheryte(1280007, 20925, new Position(128, 582.47f, 54.52f, -1.2f, 0f, 0));
                        
                        _log.Info("sent test");

                        break;

                    case @"\text":
                        GamePacket g = new GamePacket
                        {
                            Opcode = 0x166,
                            Data = new byte[]
                           {
                               0x01, 0x00, 0xF8, 0x5F, 0x39, 0x85, 0x20, 0x00//, 0x00, 0x00, 0x00, 0x00, 0x28, 0x0F, 0x00, 0x00,
                                //0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                           }
                        };

                        Packet pk = new Packet(g);

                        _connection.Send(pk.ToBytes());
                        break;

                    case @"\addloot":
                        Inventory.AddLoot(_connection.socket);
                        break;

                    case @"\additem":
                        uint quantity = 1;

                        if (split.Length > 2)
                            quantity = Convert.ToUInt32(split[2]);

                        UserFactory.Instance.User.Character.Inventory.AddItem(ref UserFactory.Instance.User.Character.Inventory.Bag, value.Replace("_", " ").Replace("'","''"), quantity, _connection.socket);
                        break;

                    case @"\inventory":
                        UserFactory.Instance.User.Character.Inventory.Update(_connection.socket);
                        break;
                    case @"\equip":
                        UserFactory.Instance.User.Character.Inventory.EquipGear(_connection.socket, Convert.ToByte(value), Convert.ToByte(split[2]));
                        break;
                    case @"\unequip":
                        UserFactory.Instance.User.Character.Inventory.UnequipGear(_connection.socket, Convert.ToByte(value));
                        break;
                    case @"\removeactor":
                        GamePacket gps = new GamePacket
                        {
                            Opcode = 0x7,
                            Data = new byte[8]
                        };
                        Packet packet = new Packet(new SubPacket(gps) { SourceId = UserFactory.Instance.User.Character.Id, TargetId = UserFactory.Instance.User.Character.Id });
                        _connection.Send(packet.ToBytes());
                        //UserFactory.Instance.User.Character.SetPosition(_connection.socket, ZoneList.EntryPoints.Find(x => x.ZoneId == Convert.ToUInt32(value)), 2, 1);
                        UserFactory.Instance.User.Character.Position = ZoneList.EntryPoints.Find(x => x.ZoneId == Convert.ToUInt32(value));
                        _world.Initialize(_connection.socket);
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
                TargetId = UserFactory.Instance.User.SessionId,
                Message = message
            };

            _log.Chat("[" + _world.Name + "] " + message); //show sent message on launcher output window (make it optional?)

            Packet packet = new Packet(new SubPacket(messagePacket) { SourceId = UserFactory.Instance.User.SessionId, TargetId = UserFactory.Instance.User.SessionId });
            _connection.Send(packet.ToBytes());
        }    

        /// <summary>
        /// Performs security check within the client.
        /// </summary> 
        private void CreateGameSession(SubPacket sp)
        {
            UserFactory.Instance.User.SessionId = uint.Parse(Encoding.ASCII.GetString(sp.Data).TrimStart(new[] { '\0' }).Substring(0, 12));

            byte[] handshakeData = new byte[0x18];
            Buffer.BlockCopy(BitConverter.GetBytes(0x0E016EE5), 0, handshakeData, 0, 0x4);
            Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);

            SubPacket handshake = new SubPacket
            {
                Type = 0x07,               
                TargetId = UserFactory.Instance.User.SessionId,
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

            Buffer.BlockCopy(BitConverter.GetBytes(UserFactory.Instance.User.Character.Id), 0, data, 0, 0x04);

            SubPacket session = new SubPacket
            {
                Type = 0x02,
                TargetId = UserFactory.Instance.User.SessionId,
                Data = data
            };

            Packet sessionPacket = new Packet(session);
            _connection.Send(sessionPacket.ToBytes());

            _log.Info("[" + _world.Name + "] Session created.");
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
