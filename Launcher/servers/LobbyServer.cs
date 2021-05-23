using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class LobbyServer : Server
    {        
        private PlayerCharacter _newCharacter = new PlayerCharacter();       

        public LobbyServer()
        {
            Task.Run(() =>
            {
                while (_listening)                
                    ProcessIncoming(ref _connection);                
            });
           
            Start("Lobby", 54994);                                  
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
            Log.Instance.Error("[Lobby Server] Unknown packet");
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
            Log.Instance.Info("Security handshake packet sent.");
        }

        private void SendAccountList()
        {                           
            GamePacket gamePacket = new GamePacket
            {
                Opcode = GameAccount.OPCODE,
                Data = User.Instance.GetAccountListData()
            };

            Packet packet = new Packet(gamePacket);
            _connection.Send(packet.ToBytes(_blowfish));
            Log.Instance.Info("Account list sent.");
        }

        private void GetCharacters(SubPacket subPacket)
        {           
            GameServer.SendWorldList(_connection.socket, _blowfish);         
            User.Instance.SendUserCharacterList(_connection.socket, _blowfish);
        }

        private void ModifyCharacter(SubPacket subPacket)
        {
            List<PlayerCharacter> userCharacters = User.Instance.AccountList[0].CharacterList;
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
                case 0x01: //Reserve name
                    //As this is intended to be a single player experience, no name reserving is needed.                    
                    Buffer.BlockCopy(subPacket.Data, 0x24, _newCharacter.Name, 0, 0x20);
                    _newCharacter.Slot = subPacket.Data[0x20];                                        
                    _newCharacter.WorldId = worldId;                   
                    Log.Instance.Info("Character name reserved.");
                    break;

                case 0x02: //Create character
                    _newCharacter.Setup(subPacket.Data);
                    userCharacters.Add(_newCharacter);
                    User.Instance.Save();
                    worldId = _newCharacter.WorldId;
                    //Buffer.BlockCopy(BitConverter.GetBytes(_newCharacter.Id), 0, responseData, 0x10, 0x04);
                    Buffer.BlockCopy(BitConverter.GetBytes(_newCharacter.Id), 0, responseData, 0x14, 0x04);
                    Log.Instance.Success("Character ID#"+_newCharacter.Id.ToString("X")+": \"" + Encoding.ASCII.GetString(_newCharacter.Name) + "\" created!");
                    break;

                case 0x03: //Rename character
                    Log.Instance.Info("Rename character");
                    break;

                case 0x04: //Delete character
                    //get character ID
                    byte[] idBuffer = new byte[0x4];
                    Array.Copy(subPacket.Data, 0x18, idBuffer, 0, 0x4);
                    uint id = BitConverter.ToUInt32(idBuffer, 0);
                    //get character name
                    byte[] nameBuffer = new byte[0x20];
                    Array.Copy(subPacket.Data, 0x24, nameBuffer, 0, 0x20);
                    string name = Encoding.ASCII.GetString(nameBuffer).Trim(new[] { '\0' });
                    //get world id
                    worldId = userCharacters.Find(x => x.Id == id).WorldId;
                    //get character index in list
                    int i = userCharacters.FindIndex(x => x.Id == id);
                    //delete from list
                    userCharacters.RemoveAt(i);
                    //Update user info
                    User.Instance.Save();
                    Log.Instance.Warning("Character ID#" + id.ToString("X") + ": \"" + name + "\" was deleted.");                    
                    break;

                case 0x05: //Unknown
                    Log.Instance.Error("Unknown Modifycharacter() command: 0x05");
                    break;

                case 0x06: //Rename retainer
                    Log.Instance.Info("Rename retainer");
                    break;
            }

            byte[] worldName = GameServer.GetNameBytes(worldId); 
            Buffer.BlockCopy(worldName, 0, responseData, 0x40, worldName.Length);   
            Buffer.BlockCopy(_newCharacter.Name, 0, responseData, 0x20, 0x20);            

            GamePacket response = new GamePacket
            {
                Opcode = 0x0e,
                Data = responseData
            };           

            Packet packet = new Packet(new SubPacket(response) { SourceId = _newCharacter.Id, TargetId = _newCharacter.Id });            
            _connection.Send(packet.ToBytes(_blowfish));           
        }

        private void SelectCharacter(SubPacket subPacket)
        {          
            byte sequence = subPacket.Data[0x10];
            byte[] characterId = new byte[0x04];
            byte[] ticket = new byte[0x08];

            Buffer.BlockCopy(subPacket.Data, 0x18, characterId, 0, 0x04);
            Buffer.BlockCopy(subPacket.Data, 0x20, ticket, 0, 0x08);

            uint selectedCharacterId = BitConverter.ToUInt32(characterId, 0);

            //Keep selected character in the User obj 
            User.Instance.Character = User.Instance.AccountList[0].CharacterList.Find(x => x.Id == selectedCharacterId);

            //when you create a new character, the game client calls SelectCharacter right after,
            //however the received packet does not contain the character id for some reason. 
            //the code below fix this as we need it to get the world id.
            if (User.Instance.Character == null)
                User.Instance.Character = _newCharacter;
            //if (selectedCharacterId == 0)
                //selectedCharacterId = _newCharacter.Id;

            //Get world info
            byte worldId = User.Instance.Character.WorldId;
            //World world = WorldFactory.GetWorld(worldId);            

            byte[] response = new byte[0x98];
            response[0] = sequence;
            Buffer.BlockCopy(characterId, 0, response, 0x8, characterId.Length);
            Buffer.BlockCopy(characterId, 0, response, 0xc, characterId.Length);
            response[0x14] = 0x1;
            Buffer.BlockCopy(BitConverter.GetBytes(GameServer.Port), 0, response, 0x56, 0x2);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(GameServer.Address), 0, response, 0x58, GameServer.Address.Length);
            Buffer.BlockCopy(ticket, 0, response, 0x90, ticket.Length);

            GamePacket characterSelected = new GamePacket
            {
                Opcode = 0x0f,
                Data = response
            };

            Packet characterSelectedPacket = new Packet(characterSelected);
            _connection.Send(characterSelectedPacket.ToBytes(_blowfish));
            Log.Instance.Info("Character selected.");
        }

        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
