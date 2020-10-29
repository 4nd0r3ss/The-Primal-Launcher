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
        public static string Name { get; set; } = "CHANGE NAME"; //change this
        public List<Zone> Zones { get; set;}       
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
        public World()
        {
            Id = 0x5ff80001;
            Zones = ActorRepository.Instance.ZoneList();
        }

        public static byte[] GetNameBytes() => Encoding.ASCII.GetBytes(Name);

        public override void Spawn(Socket handler, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare(actorIndex);
            CreateActor(handler, 0x08);
            SetSpeeds(handler, Speeds);
            SetPosition(handler, Position, spawnType, isZoning);
            SetName(handler);
            SetIsZoning(handler, false);
            LoadActorScript(handler, LuaParameters);
            ActorInit(handler);
        }

        public override void Prepare(ushort actorIndex = 0)
        {
            LuaParameters = new LuaParameters { ActorName = "worldMaster", ClassName = "WorldMaster" };
            LuaParameters.Add("/World/WorldMaster_event");
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(null);
            Speeds = new uint[] { 0, 0, 0, 0 };
            Position = new Position();
        }

        public void Initialize(Socket handler)
        {
            PlayerCharacter playerCharacter = UserRepository.Instance.User.Character;
            uint currentZone = playerCharacter.Position.ZoneId;
            Zone zone = Zones.Find(x => x.Id == currentZone);

            Debug debug = new Debug();

            SetIsZoning(handler);
            SetDalamudPhase(handler);
            SetMusic(handler, zone.GetCurrentBGM());
            SetWeather(handler, Weather.Clear);
            SetMap(handler, zone);
            Spawn(handler, 0x01);
            debug.Spawn(handler);          
            zone.Spawn(handler);//spawn zone               
            playerCharacter.Spawn(handler);//spawn player character
            zone.SpawnActors(handler);
        }

        public void SetDalamudPhase(Socket sender)
        {
            byte[] data = new byte[0x08];
            data[0] = 0xff; //test other values and make enum later

            SendPacket(sender, ServerOpcode.SetDalamud, data);
        }       

        public void SetMusic(Socket sender, uint musicId)
        {
            byte[] data = new byte[0x08];            
            data[0] = (byte)musicId; //these numbers will not vary, so no need for blockcopy
            data[0x02] = (byte)MusicMode.Play;
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
            SendPacket(sender, ServerOpcode.SetMap, data);           
        }       

        public void TeleportPlayer(Socket sender, uint zoneId)
        {
            Position entryPoint = EntryPoints.List.Find(x => x.ZoneId == zoneId);

            if (entryPoint == null)
                entryPoint = new Position() { ZoneId = zoneId };

            PlayerCharacter playerCharacter = UserRepository.Instance.User.Character;
            playerCharacter.Position = entryPoint;

            uint currentZone = playerCharacter.Position.ZoneId;
            Zone zone = Zones.Find(x => x.Id == currentZone);
            Debug debug = new Debug();

            SendPacket(sender, ServerOpcode.MassDeleteEnd, new byte[0x08], playerCharacter.Id, playerCharacter.Id);

            byte[] e2 = new byte[0x08];
            e2[0] = 0x15;// (0x02 & 0xff);
            SendPacket(sender, ServerOpcode.MapUiChange, e2, playerCharacter.Id, playerCharacter.Id);

            SetIsZoning(sender);
            SetDalamudPhase(sender);
            SetMusic(sender, zone.GetCurrentBGM());
            SetWeather(sender, Weather.Clear);
            SetMap(sender, zone);
            playerCharacter.Spawn(sender, 2, 1, -1);//spawn player character
            zone.Spawn(sender);//spawn zone 
            debug.Spawn(sender);
            Spawn(sender, 0x01);
            playerCharacter.SetPosition(sender, entryPoint, 2, 1, -1);
            zone.SpawnActors(sender);
        }

        public static XmlNodeList GetWorldListXml()
        {
            List<Actor> zoneNpcs = new List<Actor>();
            XmlNodeList rootNode = null;
            XmlDocument worldListFile = new XmlDocument();
            string worldListPath = @"world\";
            string fileNamePath = worldListPath + @"\WorldList.xml";
            string file = "";
            string regionId = "NA"; //TODO: add option to choose game server region 
            string defaultXml = @"<?xml version=""1.0"" encoding=""UTF - 8""?><servers><region id = """ + regionId + @"""><world id = ""1"" name = ""Primal Launcher"" population = ""10""></world></region></servers>";

            if (Directory.Exists(worldListPath) && File.Exists(fileNamePath))
                file = File.ReadAllText(fileNamePath);
            else
                Log.Instance.Warning("Server list file not found!");

            if (file == "")
                file = defaultXml;

            try
            {
                //prepare xml nodes
                worldListFile.LoadXml(file);
                rootNode = worldListFile.SelectNodes("servers/region[@id = '" + regionId + "']/world");
            }
            catch (Exception e) { Log.Instance.Error(e.Message); }

            return rootNode;
        }

        public string GetName(byte id)
        {
            XmlNodeList worldListXml = GetWorldListXml();
            string worldName = "Primal Launcher";

            foreach (XmlNode node in worldListXml)
                if (node.Attributes["id"].InnerText == id.ToString())
                    worldName = node.Attributes["name"].InnerText;

            Name = worldName;

            return worldName;
        }

        public static void SendWorldList(Socket handler, Blowfish blowfish)
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

        public void TeleportMenuLevel2(Socket sender, byte[] data)
        {
            byte pageSelected = data[0x25];

            byte[] teleportmenulevel2 =
            {
                0x41, 0x29, 0x9B, 0x02, 0x9C, 0x5E, 0xF0, 0xA0, 0x00, 0x63, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64,
                0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x65, 0x6C, 0x65, 0x67, 0x61, 0x74,
                0x65, 0x43, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0xA0, 0xF0, 0x5E, 0x9C, 0x02, 0x65,
                0x76, 0x65, 0x6E, 0x74, 0x41, 0x65, 0x74, 0x68, 0x65, 0x72, 0x79, 0x74, 0x65, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00,
                0x04, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x57, 0xE0, 0x3F, 0x40, 0x00, 0x00, 0x00,
            };

            teleportmenulevel2[0x62] = pageSelected;
            Buffer.BlockCopy(BitConverter.GetBytes(UserRepository.Instance.User.Character.Id), 0, teleportmenulevel2, 0, 4);

            SendPacket(sender, ServerOpcode.StartEventRequest, teleportmenulevel2);
        }
    }    
}
