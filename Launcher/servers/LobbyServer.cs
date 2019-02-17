using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class LobbyServer : Server
    {        
        private const int PORT = 54994;       
        private static WorldRepository _worldRepo = WorldRepository.Instance;
        private User _user;
        private PlayerCharacter _newCharacter = new PlayerCharacter();

        public LobbyServer(User user)
        {
            Task.Run(() =>
            {
                while (_listening)                
                    ProcessIncoming(ref _connection);                
            });

            _user = user;
            Start("Lobby", PORT);
                                  
        }       

        public override void ProcessIncoming(ref StateObject _connection)
        {
            while (_connection.bufferQueue.Count > 0)
            {
                Packet packet = new Packet(_connection.bufferQueue.Dequeue());         

                if (packet.Size == 0x288 && packet.Data[0x34] == 'T')
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
                                SendAccountList();
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

        private void UnknownPacketDebug(SubPacket sp)
        {
            //File.WriteAllBytes(counter + "unknown_packet.txt", sp.Data);       
            _log.Error("[Lobby Server] Unknown packet");
        }

        private void StartSession(Packet packet)
        {
            byte[] ticketPhrase = new byte[0x40];
            byte[] clientNumber = new byte[0x04];

            Buffer.BlockCopy(packet.Data, 0x34, ticketPhrase, 0, ticketPhrase.Length);
            Buffer.BlockCopy(packet.Data, 0x74, clientNumber, 0, clientNumber.Length);            
            
            _blowfish = new Blowfish(
                Blowfish.GenerateKey(
                    Encoding.ASCII.GetString(ticketPhrase).Trim(new[] { '\0' }), 
                    BitConverter.ToUInt32(clientNumber, 0)
                )
            );  

            _connection.Send(Packet.AckPacket);
            _log.Info("Security handshake packet sent.");
        }

        private void SendAccountList()
        {                           
            GamePacket gamePacket = new GamePacket
            {
                Opcode = GameAccount.OPCODE,
                Data = _user.GetAccountListData()
            };

            Packet packet = new Packet(gamePacket);
            _connection.Send(packet.ToBytes(_blowfish));
            _log.Info("Account list sent.");
        }

        private void GetCharacters(SubPacket subPacket)
        {
            GamePacket worldList = new GamePacket
            {
                Opcode = World.OPCODE,
                Data = _worldRepo.GetWorldListData()
            };

            Packet worldListPacket = new Packet(worldList);            
            _connection.Send(worldListPacket.ToBytes(_blowfish));
            _log.Info("World list sent.");

            //GamePacket importList = new GamePacket
            //{
            //    Opcode = 0x16,
            //    Data = new byte[]{} //SIZE: 0X1F0
            //};

            //Packet importListPacket = new Packet(importList);
            //handler.Send(importListPacket.Build(_blowfish));
            //_log.Message("Import list sent.");

            //GamePacket retainerList = new GamePacket
            //{
            //    Opcode = 0x17,
            //    Data = new byte[] { }
            //};

            //Packet retainerListPacket = new Packet(retainerList);
            //handler.Send(retainerListPacket.Build(_blowfish));
            //_log.Message("Retainer list sent.");

            GamePacket characterList = new GamePacket
            {
                Opcode = 0x0d,
                Data = _user.GetCharacters(0) //only one account supported so far.          
            };            

            Packet characterListPacket = new Packet(characterList);
            File.WriteAllBytes(@"c:\users\4nd0r\desktop\getchars_out.txt", characterListPacket.ToBytes());
            _connection.Send(characterListPacket.ToBytes(_blowfish));
            _log.Info("Character list sent.");   
        }

        private void ModifyCharacter(SubPacket subPacket)
        {
            File.WriteAllBytes("characreation_in.txt", subPacket.Data);

            byte[] responseData = new byte[0x50];
            byte command = subPacket.Data[0x21];
            byte worldId = subPacket.Data[0x22];

            responseData[0x00] = subPacket.Data[0x10]; //sequence
            responseData[0x08] = (byte)1; //unknown
            responseData[0x09] = (byte)2; //unknown
            responseData[0x0a] = command;
            
            //Buffer.BlockCopy(BitConverter.GetBytes(0x400017), 0, responseData, 0x18, 0x03); //type ??        
            responseData[0x1c] = (byte)1; //ticket

            switch (command)
            {
                case 0x01:
                    //As this is intended to be a single player experience, no name reserving is needed.                    
                    Buffer.BlockCopy(subPacket.Data, 0x24, _newCharacter.CharacterName, 0, 0x20);
                    _newCharacter.Slot = subPacket.Data[0x20];                                        
                    _newCharacter.WorldId = worldId;                   
                    _log.Info("Character name reserved.");
                    break;

                case 0x02:
                    _newCharacter.Setup(subPacket.Data);
                    _user.AccountList[0].CharacterList.Add(_newCharacter);
                    UserRepository.UpdateUser(_user);
                    worldId = _newCharacter.WorldId;
                    //Buffer.BlockCopy(BitConverter.GetBytes(_newCharacter.Id), 0, responseData, 0x10, 0x04);
                    Buffer.BlockCopy(BitConverter.GetBytes(_newCharacter.Id), 0, responseData, 0x14, 0x04);
                    _log.Success("Character ID#"+_newCharacter.Id.ToString("X")+": \"" + Encoding.ASCII.GetString(_newCharacter.CharacterName) + "\" created!");
                    break;

                case 0x03:
                    _log.Info("Rename character");
                    break;

                case 0x04:
                    //get character ID
                    byte[] idBuffer = new byte[0x4];
                    Array.Copy(subPacket.Data, 0x18, idBuffer, 0, 0x4);
                    uint id = BitConverter.ToUInt32(idBuffer, 0);
                    //get character name
                    byte[] nameBuffer = new byte[0x20];
                    Array.Copy(subPacket.Data, 0x24, nameBuffer, 0, 0x20);
                    string name = Encoding.ASCII.GetString(nameBuffer).Trim(new[] { '\0' });
                    //get world id
                    worldId = _user.AccountList[0].CharacterList.Find(x => x.Id == id).WorldId;
                    //get character index in list
                    int i = _user.AccountList[0].CharacterList.FindIndex(x => x.Id == id);
                    //delete from list
                    _user.AccountList[0].CharacterList.RemoveAt(i);
                    //Update user info
                    UserRepository.UpdateUser(_user);
                    _log.Warning("Character ID#" + id.ToString("X") + ": \"" + name + "\" was deleted.");                    
                    break;

                case 0x05:
                    _log.Error("Unknown Modifycharacter() command: 0x05");
                    break;

                case 0x06:
                    _log.Info("Rename retainer");
                    break;
            }

            byte[] worldName = Encoding.ASCII.GetBytes(_worldRepo.GetWorld(worldId).Name);
            Buffer.BlockCopy(worldName, 0, responseData, 0x40, worldName.Length);   
            Buffer.BlockCopy(_newCharacter.CharacterName, 0, responseData, 0x20, 0x20);            

            GamePacket response = new GamePacket
            {
                Opcode = 0x0e,
                Data = responseData
            };           

            Packet packet = new Packet(new SubPacket(response) { SourceId = _newCharacter.Id, TargetId = _newCharacter.Id });
            File.WriteAllBytes(@"c:\users\4nd0r\desktop\createchar_out.txt", packet.ToBytes());
            _connection.Send(packet.ToBytes(_blowfish));           
        }

        private void SelectCharacter(SubPacket subPacket)
        {
            //File.WriteAllBytes(@"c:\users\4nd0r\desktop\selectcharacter_in.txt", subPacket.Data);

            byte sequence = subPacket.Data[0x10];
            byte[] characterId = new byte[0x04];
            byte[] ticket = new byte[0x08];

            Buffer.BlockCopy(subPacket.Data, 0x18, characterId, 0, 0x04);
            Buffer.BlockCopy(subPacket.Data, 0x20, ticket, 0, 0x08);

            uint selectedCharacterId = BitConverter.ToUInt32(characterId, 0);

            //Keep selected character in the User obj 
            _user.Character = _user.AccountList[0].CharacterList.Find(x => x.Id == selectedCharacterId);

            //when you create a new character, the game client calls SelectCharacter right after,
            //however the received packet does not contain the character id for some reason. 
            //the code below fix this as we need it to get the world id.
            if (_user.Character == null)
                _user.Character = _newCharacter;
            //if (selectedCharacterId == 0)
                //selectedCharacterId = _newCharacter.Id;

            //Get world info
            byte worldId = _user.Character.WorldId;
            World world = _worldRepo.GetWorld(worldId);            

            byte[] response = new byte[0x98];
            response[0] = sequence;
            Buffer.BlockCopy(characterId, 0, response, 0x8, characterId.Length);
            Buffer.BlockCopy(characterId, 0, response, 0xc, characterId.Length);
            response[0x14] = 0x1;
            Buffer.BlockCopy(BitConverter.GetBytes(world.Port), 0, response, 0x56, 0x2);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(world.Address), 0, response, 0x58, world.Address.Length);
            Buffer.BlockCopy(ticket, 0, response, 0x90, ticket.Length);

            GamePacket characterSelected = new GamePacket
            {
                Opcode = 0x0f,
                Data = response
            };

            ServerTransition(world);

            Packet characterSelectedPacket = new Packet(characterSelected);
            _connection.Send(characterSelectedPacket.ToBytes(_blowfish));
            _log.Info("Character selected.");
        }

        public override void ServerTransition()
        {            
            throw new NotImplementedException();
        }

        public void ServerTransition(World world) => Task.Run(() => { GameServer game = new GameServer(world, _user); });
        
    }
}
