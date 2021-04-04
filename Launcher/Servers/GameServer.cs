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
                    ProcessIncoming(ref _connection);                
            });            
                    
            Start("Game", World.Port);           
        }      

        public override void ProcessIncoming(ref StateObject _connection)
        {          
            while (_connection.bufferQueue.Count > 0)
            {
                byte[] buffer = _connection.bufferQueue.Dequeue();

                Packet packet = new Packet(buffer);

                if (packet.IsCompressed == 1)               
                    packet.Unzip();      

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
                            Log.Instance.Warning("[" + World.Instance.ServerName + "] Received: 0x02");                            
                            break;
                        case 0x03:
                            ProcessGamePacket(sp);
                            break;
                        case 0x07: //delete actor request?
                             Log.Instance.Warning("[" + World.Instance.ServerName + "] Received: 0x07");                            
                            break;
                        case 0x08:                           
                            Log.Instance.Warning("[" + World.Instance.ServerName + "] Received: 0x08");                            
                            break;
                        case 0x1000:
                            Log.Instance.Warning("[" + World.Instance.ServerName + "] Received: 0x1000");
                            break;
                        default:
                            Log.Instance.Error("[" + World.Instance.ServerName + "] Unknown packet type: " + sp.Type.ToString("X"));                           
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

                case (ushort)ClientOpcode.Unknown0x02:
                    User.Instance.Character.Unknown0x02(_connection.socket);
                    break;

                case (ushort)ClientOpcode.ChatMessage:
                    ChatProcessor.Incoming(_connection.socket, subpacket.Data);
                    break;

                case (ushort)ClientOpcode.Initialize:      
                    World.Instance.Initialize(_connection.socket);                   
                    break;

                case (ushort)ClientOpcode.FriendListRequest:
                    User.Instance.Character.GetFriendlist(_connection.socket);
                    break;

                case (ushort)ClientOpcode.BlacklistRequest:
                    User.Instance.Character.GetBlackList(_connection.socket);
                    break;

                case (ushort)ClientOpcode.PlayerPosition:
                    User.Instance.Character.UpdatePosition(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.Unknown0x07:
                    //Log.Instance.Warning("Received command 0x07");                    
                    break;

                case (ushort)ClientOpcode.InitGroupWork:
                    User.Instance.Character.SetGroupInitWork(_connection.socket, subpacket.Data);
                    break;

                case (ushort)ClientOpcode.EventRequest:                     
                    EventManager.Instance.ProcessIncoming(_connection.socket, subpacket.Data);
                    break;

                case (ushort)ClientOpcode.DataRequest:                           
                    string request = Encoding.ASCII.GetString(subpacket.Data).Substring(0x14, 0x20).Trim(new[] { '\0' }); 

                    switch (request)
                    {
                        case "charaWork/exp":                           
                            _connection.Send(User.Instance.Character.CharaWorkExp(_connection.socket));   
                         break;
                    }
                    break;

                case (ushort)ClientOpcode.SelectTarget:
                    User.Instance.Character.SelectTarget(_connection.socket, subpacket.Data);
                    break;

                case (ushort)ClientOpcode.LockOnTarget:
                    //Log.Instance.Warning("Target locked.");
                    break;

                case (ushort)ClientOpcode.EventResult:
                    EventManager.Instance.CurrentEvent.EndClientOrder(_connection.socket, "");                   
                    break;

                case (ushort)ClientOpcode.GMTicketActiveRequest:
                    World.Instance.GMActiveRequest(_connection.socket);
                    break;

                case (ushort)ClientOpcode.CutSceneFinished:
                    Log.Instance.Success("Cutscene finished.");
                    break;

                default:
                    Log.Instance.Error("[" + World.Instance.ServerName + "] Unknown command: 0x" + opcode.ToString("X"));                    
                    break;
            }
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
        /// Performs security check within the client.
        /// </summary> 
        private void CreateGameSession(SubPacket sp)
        {
            User.Instance.SessionId = uint.Parse(Encoding.ASCII.GetString(sp.Data).TrimStart(new[] { '\0' }).Substring(0, 12));

            byte[] handshakeData = new byte[0x18];
            Buffer.BlockCopy(BitConverter.GetBytes(0x0E016EE5), 0, handshakeData, 0, 0x4);
            Buffer.BlockCopy(GetTimeStampHex(), 0, handshakeData, 0x4, 0x4);

            SubPacket handshake = new SubPacket
            {
                Type = 0x07,               
                TargetId = User.Instance.SessionId,
                Data = handshakeData
            };

            Packet handshakePacket = new Packet(handshake);           
            _connection.Send(handshakePacket.ToBytes());

            //login packet sequence from SU g_client0Log.Instancein2 byte array. It was originally compressed.
            byte[] data =  {
                0x00, 0x00, 0x00, 0x00, 0xc8, 0xd6, 0xaf, 0x2b, 0x38, 0x2b, 0x5f, 0x26, 0xb8, 0x8d,
                0xf0, 0x2b, 0xc8, 0xfd, 0x85, 0xfe, 0xa8, 0x7c, 0x5b, 0x09, 0x38, 0x2b, 0x5f, 0x26,
                0xc8, 0xd6, 0xaf, 0x2b, 0xb8, 0x8d, 0xf0, 0x2b, 0x88, 0xaf, 0x5e, 0x26
            };

            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 0x04);

            SubPacket session = new SubPacket
            {
                Type = 0x02,
                TargetId = User.Instance.SessionId,
                Data = data
            };

            Packet sessionPacket = new Packet(session);
            _connection.Send(sessionPacket.ToBytes());

            Log.Instance.Info("[" + World.Instance.ServerName + "] Session created.");
        }

        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
