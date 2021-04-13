using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Launcher
{
    [Serializable]
    public class World : Actor
    {
        #region Properties 
        private static World _instance = null;
        public static ushort Port { get; set; } = 54992;
        public static string Address { get; set; } = "127.0.0.1";
        public int ServerId { get; set; }
        public int Population { get; set; }
        public string ServerName { get; set; } = "CHANGE NAME"; //change this
        public List<Zone> Zones { get; set;}
        public List<OpeningDirector> Actors { get; set; } = new List<OpeningDirector>();
        public static World Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new World();

                return _instance;
            }
        }
        #endregion

        private World()
        {
            Id = 0x5ff80001;
            Zones = ActorRepository.Instance.ZoneList();
            Name = Encoding.ASCII.GetBytes("worldMaster");     
        }

        public byte[] GetNameBytes(byte id) => Encoding.ASCII.GetBytes(GetServerName(id));

        public override void Spawn(Socket handler, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare(actorIndex);
            CreateActor(handler);
            SetSpeeds(handler);
            SetPosition(handler, 1, isZoning);
            SetName(handler);
            SetMainState(handler);
            SetIsZoning(handler);
            LoadScript(handler);
        }

        public override void Prepare(ushort actorIndex = 0)
        {
            LuaParameters = new LuaParameters { ActorName = "worldMaster", ClassName = "WorldMaster", ClassCode = 0x30400000 };
            LuaParameters.Add("/World/WorldMaster_event");
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(null);            
        }

        public void Initialize(Socket sender)
        {
            PlayerCharacter playerCharacter = User.Instance.Character;
            Debug debug = new Debug();           
            ServerName = GetServerName(playerCharacter.WorldId);
            uint currentZone = playerCharacter.Position.ZoneId;
            Zone zone = Zones.Find(x => x.Id == currentZone);

            //login welcome messages
            ChatProcessor.SendMessage(sender, MessageType.GeneralInfo, "Welcome to " + ServerName + "!");
            ChatProcessor.SendMessage(sender, MessageType.GeneralInfo, "Welcome to Eorzea!");
            ChatProcessor.SendMessage(sender, MessageType.GeneralInfo, @"To get a list of available commands, type \help in the chat window and hit enter.");            
            
            playerCharacter.SendGroupPackets(sender);
            playerCharacter.IsNew = true;
            playerCharacter.OpeningSequence(sender);

            SetMapEnvironment(sender, zone);

            //spawn actors
            playerCharacter.Spawn(sender, spawnType: 0x01, isZoning: 0);//spawn player character
            zone.Spawn(sender);//spawn zone
            debug.Spawn(sender);
            Spawn(sender, 0x01);            
            zone.SpawnActors(sender);           
        }

        private void SetMapEnvironment(Socket sender, Zone zone)
        {
            SetIsZoning(sender);
            SetDalamudPhase(sender);
            SetMusic(sender, zone.GetCurrentBGM());
            SetWeather(sender, Weather.Clear);
            SetMap(sender, zone);
        }

        public void SetDalamudPhase(Socket sender)
        {
            byte[] data = new byte[0x08];
            data[0] = 0xff; //test other values and make enum later
            SendPacket(sender, ServerOpcode.SetDalamud, data, sourceId: User.Instance.Character.Id);
        }       

        public void SetMusic(Socket sender, uint musicId, MusicMode mode = MusicMode.Play)
        {
            byte[] data = new byte[0x08];            
            data[0] = (byte)musicId; //these numbers will not vary, so no need for blockcopy
            data[0x02] = (byte)mode;
            SendPacket(sender, ServerOpcode.SetMusic, data);
        }

        public void SetWeather(Socket sender, Weather weather)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)weather), 0, data, 0, sizeof(ushort));
            data[0x02] = 0x02;
            SendPacket(sender, ServerOpcode.SetWeather, data);           
        }    

        public void SetMap(Socket sender, Zone zone)
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(zone.RegionId), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(zone.Id), 0, data, 0x04, sizeof(uint));            
            data[0x08] = 0x28;
            SendPacket(sender, ServerOpcode.SetMap, data, sourceId: User.Instance.Character.Id);           
        }       

        public void TeleportPlayer(Socket sender, uint zoneId)
        {
            Position entryPoint = EntryPoints.List.Find(x => x.ZoneId == zoneId);

            if (entryPoint == null)
                entryPoint = new Position() { ZoneId = zoneId };

            ChangeZone(sender, entryPoint);
        }

        public void TeleportPlayer(Socket sender, Position position)
        {
            position.X = position.X + 3;
            ChangeZone(sender, position);
        }        

        private void ChangeZone(Socket sender, Position position)
        {
            User.Instance.Character.Position = position;
            PlayerCharacter playerCharacter = User.Instance.Character;            

            uint currentZone = playerCharacter.Position.ZoneId;
            Zone zone = Zones.Find(x => x.Id == currentZone);
            Debug debug = new Debug();

            SendPacket(sender, ServerOpcode.MassDeleteEnd, new byte[0x08], playerCharacter.Id, playerCharacter.Id);

            playerCharacter.MapUIChange(sender, 0x02);

            SetIsZoning(sender);
            SetDalamudPhase(sender);
            SetMusic(sender, zone.GetCurrentBGM());
            SetWeather(sender, Weather.Clear);
            SetMap(sender, zone);

            playerCharacter.Spawn(sender, 2, 1, -1);//spawn player character
            zone.Spawn(sender);//spawn zone 
            debug.Spawn(sender);
            Spawn(sender, 0x01);
            playerCharacter.SetPosition(sender, 2, 1, -1);
            zone.SpawnActors(sender);
        }

        public XmlNodeList GetWorldListXml()
        {
            List<Actor> zoneNpcs = new List<Actor>();
            XmlNodeList rootNode = null;
            XmlDocument worldListFile = new XmlDocument();

            //From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
            string file = string.Empty;          
           
            using (Stream stream = GetType().Assembly.GetManifestResourceStream("Launcher.Resources.xml.WorldList.xml"))
            using (StreamReader sr = new StreamReader(stream))
                file = sr.ReadToEnd();

            try
            {
                //prepare xml nodes
                worldListFile.LoadXml(file);
                rootNode = worldListFile.SelectNodes("servers/region[@id = '" + "NA" + "']/world");
            }
            catch (Exception e) { Log.Instance.Error(e.Message); }

            return rootNode;
        }

        public string GetServerName(byte id)
        {
            XmlNodeList worldListXml = GetWorldListXml();
            string worldName = "Primal Launcher";

            foreach (XmlNode node in worldListXml)
                if (node.Attributes["id"].InnerText == id.ToString())
                {
                    worldName = node.Attributes["name"].InnerText;
                    break;
                }                    

            ServerName = worldName;

            return worldName;
        }

        public void SendWorldList(Socket handler, Blowfish blowfish)
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
                    handler.Send(worldListPacket.ToBytes(blowfish));
                    Log.Instance.Info("World list sent.");
                }
                catch (Exception e) { Log.Instance.Error(e.Message); }
            }
            else
            {
                Log.Instance.Error("An error ocurred when loading the world list.");
            }
        }

        public void SendTextSheetMessage(Socket sender, ServerOpcode opcode, byte[] data) => SendPacket(sender, opcode, data);

        public void GMActiveRequest(Socket sender)
        {
            byte[] data = new byte[0x08];
            SendPacket(sender, ServerOpcode.GMTicketActiveRequest, data);
        }
       
    }    
}
