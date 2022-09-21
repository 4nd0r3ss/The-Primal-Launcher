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
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrimalLauncher
{
    class GameServer : Server
    {
        private static GameServer _instance = null;
        private static readonly object _padlock = new object();

        public static ushort Port { get; set; } = 54992;
        public static string Address { get; set; } = "127.0.0.1";
        public string Name { get; set; }
        public Socket Sender
        {
            get
            {
                return _connection.socket;
            }
        }

        public static GameServer Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new GameServer();

                    return _instance;
                }
            }
        }

        private GameServer() { }

        public void Initialize()
        {
            Task.Run(() => { ProcessIncoming(); });
            Start("Game", Port);
        }

        #region World List Methods
        public static byte[] GetNameBytes(byte id) => Encoding.ASCII.GetBytes(GetServerName(id));

        public static XmlNodeList GetWorldListXml()
        {
            List<Actor> zoneNpcs = new List<Actor>();
            XmlNodeList rootNode = null;
            XmlDocument worldListFile = new XmlDocument();
            worldListFile.LoadFromResource("WorldList.xml");            

            try
            {               
                rootNode = worldListFile.SelectNodes("servers/region[@id = '" + Preferences.Instance.Options.ServerRegion + "']/world");
            }
            catch (Exception e) { Log.Instance.Error(e.Message); throw e; }

            return rootNode;
        }

        public static string GetServerName(byte id)
        {
            XmlNodeList worldListXml = GetWorldListXml();
            string worldName = "Primal Launcher";

            foreach (XmlNode node in worldListXml)
                if (node.Attributes["id"].InnerText == id.ToString())
                {
                    worldName = node.Attributes["name"].InnerText;
                    break;
                }

            return worldName;
        }

        public static void SendWorldList(Blowfish blowfish)
        {
            XmlNodeList rootNode = GetWorldListXml();

            if (rootNode != null)
            {
                try
                {
                    byte[] serverListData = new byte[(0x50 * rootNode.Count) + 0x10];
                    int index = 0;

                    //read nodes
                    foreach (XmlNode node in rootNode)
                    {
                        byte[] name = Encoding.ASCII.GetBytes(node.Attributes["name"].Value);
                        byte[] server = new byte[0x50];

                        server[0x00] = Convert.ToByte(node.Attributes["id"].Value);
                        server[0x02] = (byte)index;
                        server[0x04] = Convert.ToByte(node.Attributes["population"].Value);
                        Buffer.BlockCopy(name, 0, server, 0x10, name.Length);

                        Buffer.BlockCopy(server, 0, serverListData, ((index * 0x50) + 0x10), server.Length);

                        index++;
                    }

                    serverListData[0x09] = (byte)index;

                    GamePacket worldList = new GamePacket
                    {
                        Opcode = 0x15,
                        Data = serverListData
                    };

                    Packet worldListPacket = new Packet(worldList);
                    LobbyServer.Instance.Sender.Send(worldListPacket.ToBytes(blowfish));
                    Log.Instance.Info("World list sent.");
                }
                catch (Exception e) { Log.Instance.Error(e.Message); throw e; }
            }
            else
            {
                Log.Instance.Error("An error ocurred when loading the world list.");
            }
        }
        #endregion

        public override void ProcessIncoming()
        {          
            while (_listening)
            {
                MainWindow.Window.ClockTime = "Eorzea time: " + Clock.Instance.StringTime;

                if (_connection.bufferQueue.Count > 0)
                {
                    try
                    {
                        byte[] buffer = _connection.bufferQueue.Dequeue();

                        Packet packet = new Packet(buffer);

                        if (packet.IsCompressed == 1)
                            packet.Unzip();

                        packet.ProcessSubPackets(null); //no decrypting here

                        if (Preferences.Instance.Options.PrintPacketsToFile)
                            packet.OutputToFile();

                        while (packet.SubPacketQueue.Count > 0)
                        {
                            SubPacket sp = packet.SubPacketQueue.Dequeue();

                            switch (sp.Type)
                            {
                                case 0x01:
                                    CreateGameSession(sp);
                                    break;
                                case 0x02:
                                    Log.Instance.Warning("[" + Name + "] Received: 0x02");
                                    break;
                                case 0x03:
                                    ProcessGamePacket(sp);
                                    break;
                                case 0x07: //delete actor request?
                                    Log.Instance.Warning("[" + Name + "] Received: 0x07");
                                    break;
                                case 0x08:
                                    Log.Instance.Warning("[" + Name + "] Received: 0x08");
                                    break;
                                case 0x1000:
                                    Log.Instance.Warning("[" + Name + "] Received: 0x1000");
                                    break;
                                default:
                                    Log.Instance.Error("[" + Name + "] Unknown packet type: " + sp.Type.ToString("X"));
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Instance.Error("Game server exception: " + e.Message);
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
                    User.Instance.Character.Unknown0x02();
                    break;

                case (ushort)ClientOpcode.ChatMessage:
                    ChatProcessor.Incoming(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.Initialize:
                    Name = GetServerName(User.Instance.Character.WorldId);
                    ChatProcessor.SendMessage(MessageType.GeneralInfo, "Welcome to " + Name + "!");
                    ChatProcessor.SendMessage(MessageType.GeneralInfo, "Welcome to Eorzea!");
                    ChatProcessor.SendMessage(MessageType.GeneralInfo, @"To get a list of custom commands, type \help in the chat window and hit enter.");
                    World.Instance.Initialize();                   
                    break;

                case (ushort)ClientOpcode.FriendListRequest:
                    User.Instance.Character.GetFriendlist();
                    break;

                case (ushort)ClientOpcode.BlacklistRequest:
                    User.Instance.Character.GetBlackList();
                    break;

                case (ushort)ClientOpcode.PlayerPosition:
                    User.Instance.Character.UpdatePosition(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.Unknown0x07:
                    //Log.Instance.Warning("Received command 0x07");                    
                    break;

                case (ushort)ClientOpcode.InitGroupWork:
                    BattleManager.Instance.GetGroupInitWork(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.EventRequest:                     
                    EventManager.Instance.ProcessIncoming(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.DataRequest:     //TODO: should be in event manager?                      
                    string request = Encoding.ASCII.GetString(subpacket.Data).Substring(0x14, 0x20).Trim(new[] { '\0' }); 

                    switch (request)
                    {
                        case "charaWork/exp":                           
                            _connection.Send(User.Instance.Character.ClassExp());   
                         break;
                    }
                    break;

                case (ushort)ClientOpcode.SelectTarget:
                    User.Instance.Character.SelectTarget(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.LockOnTarget:
                    User.Instance.Character.LockTarget(subpacket.Data);
                    break;

                case (ushort)ClientOpcode.EventResult:
                    EventManager.Instance.CurrentEvent.ProcessEventResult(subpacket.Data);                   
                    break;

                case (ushort)ClientOpcode.GMTicketActiveRequest:
                    World.Instance.GMActiveRequest();
                    break;

                case (ushort)ClientOpcode.CutScene:
                    CutsceneLog(subpacket.Data);
                    break;
                case (ushort)ClientOpcode.ItemSearchRequest:
                    ItemSearch(subpacket.Data);
                    break;
                case (ushort)ClientOpcode.RetainerSearchRequest:
                    RetainerSearch(subpacket.Data);
                    break;
                case (ushort)ClientOpcode.PurchaseHistoryRequest:
                    PurchaseHistory(subpacket.Data);
                    break;

                default:
                    Log.Instance.Error("[" + Name + "] Unknown command: 0x" + opcode.ToString("X"));
                    File.WriteAllBytes("unknowncommand.txt", subpacket.Data);
                    break;
            }
        }


        private void ItemSearch(byte[] requestData)
        {            
            string searchString = requestData.ReadNullTeminatedString(0x28);
            DataTable itemNames = GameData.Instance.GetGameData("xtx/itemName");
            DataTable itemsInfo = GameData.Instance.GetGameData("_item");
            DataRow[] selectedItems = itemNames.Select("strc0 like '%" + searchString + "%'");            

            if (selectedItems.Length > 0)
            {
                int dataIndex = 0;
                int counter = 0;
                byte[] data = new byte[0x230];

                Packet.Send(ServerOpcode.ItemSearchStart, new byte[8]);

                foreach (DataRow item in selectedItems)
                {              
                    DataRow itemInfo = itemsInfo.Select("id = '" + item[0] + "'")[0];

                    if ((string)itemInfo[1] != "Normal/DummyItem" &&
                        (string)itemInfo[1] != "Money/MoneyStandard" &&
                        (string)itemInfo[1] != "Important/ImportantItemStandard" &&
                        !(bool)itemInfo[4])
                    {
                        dataIndex += 0x04;
                        counter++;

                        //write item id
                        Buffer.BlockCopy(BitConverter.GetBytes((uint)itemInfo[0]), 0, data, dataIndex, 0x04);
                        //write item stack#
                        Buffer.BlockCopy(BitConverter.GetBytes((int)itemInfo[2]), 0, data, dataIndex + 0x100, 0x04);
                    }

                    if (counter == 64 || Array.IndexOf(selectedItems, item) == selectedItems.Length - 1)
                    {                       
                        //write number of items being sent
                        Buffer.BlockCopy(BitConverter.GetBytes(dataIndex / 0x04), 0, data, 0, 0x04);
                        //send packet
                        Packet.Send(ServerOpcode.ItemSearchresult, data);
                        //reset buffer
                        Array.Clear(data, 0, data.Length);
                        dataIndex = 0;
                        counter = 0;
                    }                    
                }

                Packet.Send(ServerOpcode.ItemSearchEnd, new byte[8]);
            }            
        }

        private void RetainerSearch(byte[] requestData)
        {
            byte[] data = new byte[0xA10];
            int numRetainers = 20;
            int index = 0x10;
            Random random = new Random();
            DataTable table = GameData.Instance.GetGameData("xtx/displayName");
            DataRow[] dataRows = table.Select("id >= '1000001' and id < '3000001'");

            Buffer.BlockCopy(BitConverter.GetBytes(numRetainers), 0, data, 0x0C, 0x04);

            for(int i = 0; i < numRetainers; i++)
            {
                byte[] retainerData = new byte[0x80];

                using(MemoryStream ms = new MemoryStream(retainerData))
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Seek(0x08, SeekOrigin.Begin);
                    bw.Write(BitConverter.ToInt32(requestData, 0x08));//item id
                    bw.Write(0);//market row
                    bw.Write(200);//price
                    bw.Write(random.Next(1,10));//quantity
                    bw.Write(true);//stacked
                    bw.Write((byte)random.Next(1,3));//quality
                    bw.Write((short)0);//nothing
                    bw.Write(Encoding.ASCII.GetBytes((string)dataRows[random.Next(0, dataRows.Length-1)][1]));//name
                    bw.Seek(0x60, SeekOrigin.Begin);
                    bw.Write(0);//materia
                    bw.Write(0);//materia grade
                }
             
                //write retainer to main packet
                Buffer.BlockCopy(retainerData, 0, data, index, retainerData.Length);
                index += 0x80;
            }

            Packet.Send(ServerOpcode.RetainerSearchResult, data);
            Packet.Send(ServerOpcode.RetainerSearchUpdate, new byte[0x08]);

            data = new byte[0x20];
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, data, 0x10, 0x04);
            Packet.Send(ServerOpcode.RetainerSearchEnd, data);
        }

        private void PurchaseHistory(byte[] requestData)
        {

        }

        /// <summary>
        /// Indicate is a cutscene is starting or finishing. Might be used in the future to fire events after cutscenes.
        /// </summary>
        /// <param name="data"></param>
        private void CutsceneLog(byte[] data)
        {
            string status = "started";
            string cutsceneName = Encoding.ASCII.GetString(data, 0x14, 12).Replace("\0", "");

            if (data[0x10] > 0)
                status = "finished";

            Log.Instance.Success("Cutscene " + cutsceneName + " " + status +".");

            if(EventManager.Instance.CurrentEvent != null)
                EventManager.Instance.CurrentEvent.OnCutscene(status);
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
                0x00, 0x00, 0x00, 0x00, 
                0xc8, 0xd6, 0xaf, 0x2b, 
                0x38, 0x2b, 0x5f, 0x26, 
                0xb8, 0x8d, 0xf0, 0x2b, 
                0xc8, 0xfd, 0x85, 0xfe, 
                0xa8, 0x7c, 0x5b, 0x09, 
                0x38, 0x2b, 0x5f, 0x26,
                0xc8, 0xd6, 0xaf, 0x2b, 
                0xb8, 0x8d, 0xf0, 0x2b, 
                0x88, 0xaf, 0x5e, 0x26
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
          
            Log.Instance.Info("Session created.");
        }

        public override void ServerTransition()
        {
            //throw new NotImplementedException();
        }
    }
}
