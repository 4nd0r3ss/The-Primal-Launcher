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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    class LobbyServer : Server
    {
        private static LobbyServer _instance = null;
        private static readonly object _padlock = new object();        

        private LobbyServer() { }

        public Socket Sender
        {
            get
            {
                return _connection.socket;
            }
        }

        public Blowfish Blowfish
        {
            get
            {
                return _blowfish;
            }
        }

        public static LobbyServer Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new LobbyServer();

                    return _instance;
                }
            }
        }

        public void Initialize()
        {
            Task.Run(() => { ProcessIncoming(); });
            Start("Lobby", 54994);
        }

        public override void ProcessIncoming()
        {
            while (_listening)
            {
                if (_connection.bufferQueue.Count > 0)
                {
                    Packet packet = new Packet(_connection.bufferQueue.Dequeue());

                    if (packet.Size == 0x288 && packet.RawData[0x34] == 'T')
                    {
                        StartSession(packet);
                    }
                    else
                    {
                        packet.ProcessSubPackets(_blowfish);

                        while (packet.SubPacketQueue.Count > 0)
                        {
                            SubPacket sp = packet.SubPacketQueue.Dequeue();

                            switch (sp.Opcode())
                            {
                                case 0x03:
                                    GetCharacters(sp);
                                    break;
                                case 0x04:
                                    SelectCharacter(sp);
                                    break;
                                case 0x05:
                                    User.Instance.SendAccountList();
                                    break;
                                case 0x0B:
                                    ModifyCharacter(sp);
                                    break;
                                default:
                                    UnknownPacketDebug(sp);
                                    break;
                            }
                        }
                    }
                }                
            }
        }               

        private void UnknownPacketDebug(SubPacket sp)
        {                 
            Log.Instance.Error("[Lobby Server] Unknown packet");
        }

        private void StartSession(Packet packet)
        {           
            _blowfish = new Blowfish(
                Blowfish.GenerateKey(
                    packet.RawData.ReadNullTeminatedString(0x34), 
                    BitConverter.ToUInt32(packet.RawData, 0x74)
                )
            );  

            _connection.Send(Packet.AckPacket);
            Log.Instance.Info("Security handshake packet sent.");
        }
              
        private void GetCharacters(SubPacket subPacket)
        {           
            GameServer.SendWorldList(_blowfish);         
            User.Instance.SendUserCharacterList(_blowfish);
        }

        private void ModifyCharacter(SubPacket subPacket)
        {          
            byte[] responseData = new byte[0x50];
            byte command = subPacket.Data[0x21];
            byte worldId = subPacket.Data[0x22];

            responseData.Write(new Dictionary<int, object>
            {
                {0x00, subPacket.Data[0x10]}, //sequence
                {0x08, (byte)1}, //unknown
                {0x09, (byte)2}, //unknown
                {0x0A, command},
                {0x1C, (byte)1}, //ticket
            });

            switch (command)
            {
                case 0x01: //Reserve name
                    User.Instance.GameAccount.ReserveName(subPacket.Data, worldId);
                    break;

                case 0x02: //Create character
                    User.Instance.GameAccount.CreateCharacter(subPacket.Data);
                    worldId = User.Instance.GameAccount.SelectedCharacter.WorldId;
                    responseData.Write(0x14, User.Instance.GameAccount.SelectedCharacter.Id);   
                    break;

                case 0x03: //Rename character
                    User.Instance.GameAccount.RenameCharacter(subPacket.Data);                    
                    break;

                case 0x04: //Delete character                   
                    File.WriteAllBytes("delete.txt", subPacket.Data);
                    uint id = BitConverter.ToUInt32(subPacket.Data, 0x18);                                        
                    worldId = User.Instance.GameAccount.GetCharacterById(id).WorldId;  
                    User.Instance.GameAccount.DeleteCharacter(subPacket.Data[0x20]);                                        
                    break;

                case 0x05: //Unknown
                    Log.Instance.Error("Unknown Modifycharacter() command: 0x05");
                    break;

                case 0x06: //Rename retainer
                    Log.Instance.Info("Rename retainer");
                    break;
            }

            responseData.Write(new Dictionary<int, object>
            {
                {0x40, GameServer.GetNameBytes(worldId)},
                {0x20, User.Instance.GameAccount.SelectedCharacter.Name}
            });

            Packet packet = new Packet(new SubPacket(new GamePacket(0x0e, responseData)) 
            { 
                SourceId = User.Instance.GameAccount.SelectedCharacter.Id, 
                TargetId = User.Instance.GameAccount.SelectedCharacter.Id 
            });            
            _connection.Send(packet.ToBytes(_blowfish));           
        }

        private void SelectCharacter(SubPacket subPacket)
        {                      
            uint characterId = subPacket.Data.GetUInt32(0x18);
            byte[] response = new byte[0x98];

            response.Write(new Dictionary<int, object>
            {
                {0, subPacket.Data[0x10]}, //sequence
                {0x08, characterId.GetBytes()},
                {0x0C, characterId.GetBytes()},
                {0x14, 0x01},
                {0x56, GameServer.Port},
                {0x58, GameServer.Address},
                {0x90, subPacket.Data.GetInt64Bytes(0x20)}
            }) ;

            Packet characterSelectedPacket = new Packet(new GamePacket(0x0f, response));
            _connection.Send(characterSelectedPacket.ToBytes(_blowfish));
            Log.Instance.Info("Character selected.");
        }

        public override void ServerTransition()
        {
            //throw new NotImplementedException();
        }
    }
}
