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
        private User _user;
        private World _world;
        private int count = 0;
        private uint _emoteId = 0x05013001;
        private uint _emoteLabel = 0x52be;

        private Queue<Packet> DataRequestResponseQueue = new Queue<Packet>();
        private Queue<Packet> EventRequestResponseQueue = new Queue<Packet>();

        public GameServer(World world, User user)
        {
            Task.Run(() =>
            {
                while (_listening)
                {
                    ProcessIncoming(ref _connection);
                }
            });

            _user = user;
            _world = world;
            _world.ZoneList = new ZoneList(); //we don't need the zone list to be part of obj serializarion. 
            _world.CharacterId = _user.Character.Id;
            Start(world.Name, world.Port);
           
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
                            File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x02.txt", sp.Data);
                            break;

                        case 0x03:
                            ProcessCommand(sp);
                            break;

                        case 0x07:
                             _log.Warning("[" + _world.Name + "] Received: 0x07");
                            File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x07.txt", sp.Data);
                            break;

                        case 0x08:                           
                            _log.Warning("[" + _world.Name + "] Received: 0x08");
                            File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\type_0x08.txt", sp.Data);
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
        private void ProcessCommand(SubPacket subpacket)
        {
            //_world.Sender = _connection.socket;
            ushort command = (ushort)(subpacket.Data[0x03] << 8 | subpacket.Data[0x02]);           

            switch (command)
            {
                case 0x01: //ping
                    Pong(subpacket);
                    break;

                case 0x02: //unknown                  
                    Packet unknown = new Packet(subpacket);
                    _connection.Send(unknown.ToBytes());                   
                    break;

                case 0x03: //chat message received
                    ProcessChatMessage(subpacket.Data);                    
                    break;

                case 0x06: //world & player spawning        
                    SendMessage(MessageType.GeneralInfo, "Welcome to " + _world.Name + "!");
                    SendMessage(MessageType.GeneralInfo, "Welcome to Eorzea!");  
                    SendMessage(MessageType.GeneralInfo, @"To get a list of available commands, type \help in the chat window and hit enter.");

                    _user.Character.SendGroupPackets(_connection.socket);                    

                    _world.BeginWorldInit(_connection.socket);
                    _world.SetDalamudPhase(_connection.socket);
                    _world.SetMusic(_connection.socket, (byte)_world.ZoneList.GetZone(_user.Character.Position.ZoneId).MusicSet.DayMusic);
                    _world.SetWeather(_connection.socket, Weather.Clear);
                    _world.SetMap(_connection.socket, _user.Character.Position.ZoneId);

                    _user.Character.Spawn(_connection.socket);
                   
                    //Spawn zone where player character is located
                    _world.ZoneList.Zones[_user.Character.Position.ZoneId].Spawn(_connection.socket);

                    _world.SpawnDebugActor(_connection.socket);
                    _world.FinishWorldInit(_connection.socket);
                    
                    break;

                case 0x01ce: //friend list request 
                    //_log.Warning("[" + _world.Name + "] Friend list request....");
                    break;

                case 0x01cb: //black list request
                    //_log.Warning("[" + _world.Name + "] Black list request....");
                    break;

                case 0xca: 
                    UpdatePlayerPosition(subpacket.Data);
                    break;

                case 0x7: //unknown
                    break;

                case 0x133: //group created
                    //File.WriteAllBytes(@"C: \users\4nd0r\desktop\0x133.txt", subpacket.Data);
                    break;

                case 0x12d: //event start request
                    _log.Warning("Received event request: 0x" + command.ToString("X"));
                    File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\event_0x" + command.ToString("X") + ".txt", subpacket.Data);
                    EventRequest(subpacket.Data);
                    break;

                case 0x12f:       
                    uint targetId = (uint)(subpacket.Data[0x13] << 24 | subpacket.Data[0x12] << 16 | subpacket.Data[0x11] << 8 | subpacket.Data[0x10]);
                    string request = Encoding.ASCII.GetString(subpacket.Data).Substring(0x14, 0x20).Trim(new[] { '\0' });
                    uint sequence = (uint)(subpacket.Data[0x37] << 24 | subpacket.Data[0x36] << 16 | subpacket.Data[0x35] << 8 | subpacket.Data[0x34]);

                    //Implemented a queue of packets to send in order. if this doesnt work in the future, implement a dictionary with sequence number as key.
                    switch (request)
                    {
                        case "charaWork/exp":
                            _log.Warning("Request: " + request + " for target: 0x" + targetId.ToString("X") + " sequence: 0x" +sequence.ToString("X"));

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
                                Packet packet = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                DataRequestResponseQueue.Enqueue(packet);

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
                                packet = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                DataRequestResponseQueue.Enqueue(packet);

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
                                packet = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                DataRequestResponseQueue.Enqueue(packet);
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
                                packet = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
                                //_connection.Send(packet.ToBytes());
                                DataRequestResponseQueue.Enqueue(packet);
                            }
                            else
                            {
                               //maybe verify if sequence number is the same? after the second packet, the remaining sequence numbers are the same.
                            }

                            _connection.socket.Send(DataRequestResponseQueue.Dequeue().ToBytes());

                            break;
                    }
                    break;

                default:
                    _log.Error("[" + _world.Name + "] Unknown command: 0x" + command.ToString("X"));
                    File.WriteAllBytes(@"C: \users\4nd0r\desktop\received\unknown_0x" + command.ToString("X") + ".txt", subpacket.Data);
                    break;
            }
        }

        private void UpdatePlayerPosition(byte[] data)
        {
            _user.Character.Position.X = BitConverter.ToSingle(new byte[] { data[0x18], data[0x19], data[0x1a], data[0x1b] }, 0);
            _user.Character.Position.Y = BitConverter.ToSingle(new byte[] { data[0x1c], data[0x1d], data[0x1e], data[0x1f] }, 0);
            _user.Character.Position.Z = BitConverter.ToSingle(new byte[] { data[0x20], data[0x21], data[0x22], data[0x23] }, 0);
            _user.Character.Position.R = BitConverter.ToSingle(new byte[] { data[0x24], data[0x25], data[0x26], data[0x27] }, 0);
                       
            _user.AccountList[0].CharacterList[_user.Character.Slot] = _user.Character;
            UserRepository.UpdateUser(_user);

            //uint unknown0 = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x14]);
            //ushort moveState = (ushort)(data[0x29] << 8 | data[0x28]);
            //ushort unknown1 = (ushort)(data[0x2b] << 8 | data[0x2a]);
            //uint unknown2 = (uint)(data[0x2f] << 24 | data[0x2e] << 16 | data[0x2d] << 8 | data[0x2c]);

            //_log.Warning("Player position: X=" + _user.Character.Position.X + ", Y=" + _user.Character.Position.Y + ", Z=" + _user.Character.Position.Z + ", R=" + _user.Character.Position.R);
            //if(unknown1 != 0x19 && unknown1 != 0xed)
            //_log.Warning("U0=0x" + unknown0.ToString("X") + ", U1=0x" + unknown1.ToString("X") + ", U2=0x" + unknown2.ToString("X") + ", MS=0x"+ moveState.ToString("X"));
        }

        public void EventRequest(byte[] data)
        {
            string eventType = Encoding.ASCII.GetString(data, 0x21, 14);

            if(eventType.IndexOf("commandForced") >= 0)
            {
                
                ushort command = (ushort)(data[0x15] << 8 | data[0x14]);

                _log.Warning("event: " + eventType + ", command: " + command);

                switch (command)
                {
                    case 0x5209: //battle stance
                        _user.Character.SetMainState(_connection.socket, Actor.MainState.Active, 0xbf);
                        _user.Character.BattleActionResult(_connection.socket, command);
                        _user.Character.EndClientOrderEvent(_connection.socket, eventType);
                        break;
                    case 0x520a: //normal stance
                        _user.Character.SetMainState(_connection.socket, Actor.MainState.Passive, 0xbf);
                        _user.Character.BattleActionResult(_connection.socket, command);
                        _user.Character.EndClientOrderEvent(_connection.socket, eventType);
                        break;
                } 
            }
            else if (eventType.IndexOf("commandRequest") >= 0)
            {
                //emotes
            }

            //GamePacket gp = new GamePacket
            //{
            //    Opcode = 0x134,
            //    Data = new byte[]
            //    {
            //        0x02, 0xBF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            //    }
            //};

            //Packet p = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
            //_connection.Send(p.ToBytes());


            //data = new byte[]
            //    {
            //        0x41, 0x29, 0x9B, 0x02, 0x62, 0x00, 0x00, 0x7C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x01, 0x00, 0x00, 0x00, 0x09, 0x52, 0x10, 0x08, 0x41, 0x29, 0x9B, 0x02, 0x00, 0x00, 0x00, 0x00,
            //        0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00
            //    };

            //Buffer.BlockCopy(BitConverter.GetBytes(_user.Character.Id), 0, data, 0, 4);

            // gp = new GamePacket
            //{
            //    Opcode = 0x139,
            //    Data = data
            //};

            // p = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
            //_connection.Send(p.ToBytes());

            //data = new byte[]
            //    {
            //        0x41, 0x29, 0x9B, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64,
            //        0x46, 0x6F, 0x72, 0x63, 0x65, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF2, 0xD4, 0x09, 0x30, 0xA9, 0x09, 0x0A,
            //    };
            //Buffer.BlockCopy(BitConverter.GetBytes(_user.Character.Id), 0, data, 0, 4);

            //gp = new GamePacket
            //{
            //    Opcode = 0x131,
            //    Data = data
            //};

            // p = new Packet(new SubPacket(gp) { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
            //_connection.Send(p.ToBytes());



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
                        //will try to spawn the doo at limsa init
                        //529	1090025	 exit_door	193	 	0	0	10	-18	0	0	0	 null


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

                    case @"\setemote":
                        _emoteId = Convert.ToUInt32(value);
                        break;

                    case @"\setmap": //teleport

                        //get map entry point
                        if(!value.Equals("") && value != null)
                        {
                            Position entry = ZoneList.EntryPoints.Find(x => x.ZoneId == Convert.ToUInt32(value));
                            _user.Character.Position.ZoneId = entry.ZoneId;
                            _user.Character.Position.X = entry.X;
                            _user.Character.Position.Y = entry.Y;
                            _user.Character.Position.Z = entry.Z;
                            _user.Character.Position.R = entry.R;
                            _user.Character.Position.SpawnType = entry.SpawnType;

                            Packet packet = new Packet(new SubPacket(new GamePacket
                            { Opcode = 0xe2, Data = new byte[] { 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } })
                            { SourceId = _user.Character.Id, TargetId = _user.Character.Id });
                            _connection.Send(packet.ToBytes());

                            // packet = new Packet(new SubPacket(new GamePacket
                            //{Opcode = 0x17b,Data = new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,}}){ SourceId = _user.Character.Id, TargetId = _user.Character.Id });
                            //_connection.Send(packet.ToBytes());

                            //_user.Character.Spawn(_connection.socket);

                            _world.BeginWorldInit(_connection.socket);
                            _world.SetDalamudPhase(_connection.socket);
                            _world.SetMusic(_connection.socket, (byte)_world.ZoneList.GetZone(_user.Character.Position.ZoneId).MusicSet.DayMusic);
                            _world.SetWeather(_connection.socket, Weather.Clear);
                            _world.SetMap(_connection.socket, _user.Character.Position.ZoneId);



                            _user.Character.Spawn(_connection.socket);

                            //Spawn zone where player character is located
                            _world.ZoneList.Zones[_user.Character.Position.ZoneId].Spawn(_connection.socket);

                            _world.SpawnDebugActor(_connection.socket);
                            _world.FinishWorldInit(_connection.socket);
                        }
                        

                        break;

                    case @"\getposition":
                        Position current = _user.Character.Position;

                        if(value != "" && value == "hex")
                            SendMessage(MessageType.System, "Current positoin: X: " + current.X.ToString("X") + ", Y: " + current.Y.ToString("X") + ", Z: " + current.Z.ToString("X") + ", R: " + current.R.ToString("X"));
                        else
                            SendMessage(MessageType.System, "Current positoin: X: " + current.X + ", Y: " + current.Y + ", Z: " + current.Z + ", R: " + current.R);
                        break;

                    case @"\play":

                        GamePacket gp = new GamePacket
                        {
                            Opcode = 0xd9,
                            Data = new byte[]
                            {
                                0x73, 0x74, 0x74, 0x30, 0x00, 0x00, 0x00, 0x00
                            }
                        };

                        Packet p = new Packet(new SubPacket(gp) { SourceId = 0x44d80036, TargetId = _user.Character.Id });

                        _connection.Send(p.ToBytes());

                        break;

                    case @"\spawn":
                        _world.ZoneList.GetZone(_user.Character.Position.ZoneId).SpawnActors(_connection.socket);
                        break;

                    case @"\teleport":
                        if(split.Length > 2)
                        {
                            //Position pos = Aetheryte.AetheryteList.Find(x => x.Value.Id == 1280040).Value.Position;
                            Position pos = _user.Character.Position;
                            pos.X = Convert.ToSingle(split[1]);
                            pos.Y = Convert.ToSingle(split[2]);
                            pos.Z = Convert.ToSingle(split[3]);
                            //_user.Character.Position.R = value[4];
                            _user.Character.SetPosition(_connection.socket, pos, 2);
                        }                        
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
                TargetId = _user.SessionId,
                Message = message
            };

            _log.Chat("[" + _world.Name + "] " + message); //show sent message on launcher output window (make it optional?)

            Packet packet = new Packet(new SubPacket(messagePacket) { SourceId = _user.SessionId, TargetId = _user.SessionId });
            _connection.Send(packet.ToBytes());
        }    

        /// <summary>
        /// Performs security check within the client.
        /// </summary> 
        private void CreateGameSession(SubPacket sp)
        {
            _user.SessionId = uint.Parse(Encoding.ASCII.GetString(sp.Data).TrimStart(new[] { '\0' }).Substring(0, 12));

            byte[] handshakeData = new byte[0x18];
            Buffer.BlockCopy(BitConverter.GetBytes(0x0E016EE5), 0, handshakeData, 0, 0x4);
            Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);

            SubPacket handshake = new SubPacket
            {
                Type = 0x07,               
                TargetId = _user.SessionId,
                Data = handshakeData
            };

            Packet handshakePacket = new Packet(handshake);           
            _connection.Send(handshakePacket.ToBytes());

            //login packet sequence from SU g_client0_login2 byte array. It was compressed.
            byte[] data =  {
                0x00, 0x00, 0x00, 0x00, 0xc8, 0xd6, 0xaf, 0x2b, 0x38, 0x2b, 0x5f, 0x26, 0xb8, 0x8d,
                0xf0, 0x2b, 0xc8, 0xfd, 0x85, 0xfe, 0xa8, 0x7c, 0x5b, 0x09, 0x38, 0x2b, 0x5f, 0x26,
                0xc8, 0xd6, 0xaf, 0x2b, 0xb8, 0x8d, 0xf0, 0x2b, 0x88, 0xaf, 0x5e, 0x26
            };

            Buffer.BlockCopy(BitConverter.GetBytes(_user.Character.Id), 0, data, 0, 0x04);

            SubPacket session = new SubPacket
            {
                Type = 0x02,
                TargetId = _user.SessionId,
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
