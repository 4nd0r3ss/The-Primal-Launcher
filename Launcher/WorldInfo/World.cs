using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace Launcher
{
    [Serializable]
    public class World : Actor
    {
        #region Properties   
        public ushort Port { get; set; }
        public int ServerId { get; set; }
        public int Population { get; set; }
        public string Name { get; set; }        
        public string Address { get; set; }           
        public List<Zone> Zones { get; set; }
        private Actor WorldMaster { get; set; }       
        public PlayerCharacter PlayerCharacter { get; set; }
        #endregion
        public World() {}

        public void Initialize(Socket handler)
        {
            PlayerCharacter playerCharacter = UserFactory.Instance.User.Character;

            Zones = ActorRepository.Instance.Zones;
            Debug debug = new Debug();

            WorldMaster = new Actor { Id = 0x5ff80001, TargetId = playerCharacter.Id };
            WorldMaster.LuaParameters = new LuaParameters { ActorName = "worldMaster", ClassName = "WorldMaster" };
            WorldMaster.LuaParameters.Add("/World/WorldMaster_event");
            WorldMaster.LuaParameters.Add(false);
            WorldMaster.LuaParameters.Add(false);
            WorldMaster.LuaParameters.Add(false);
            WorldMaster.LuaParameters.Add(false);
            WorldMaster.LuaParameters.Add(false);
            WorldMaster.LuaParameters.Add(null);
            WorldMaster.Speeds = new uint[] { 0, 0, 0, 0 };
            WorldMaster.Position = new Position();

            uint currentZone = playerCharacter.Position.ZoneId;
            Zone zone = Zones.Find(x => x.Id == currentZone);            

            SetIsZoning(handler);
            SetDalamudPhase(handler);
            SetMusic(handler, zone.GetCurrentBGM());
            SetWeather(handler, Weather.Clear);
            SetMap(handler, currentZone);
            WorldMaster.Spawn(handler, 0x01);
            debug.Spawn(handler);          
            zone.Spawn(handler);//spawn zone      
            playerCharacter.Inventory = new Inventory();
            playerCharacter.Spawn(handler);//spawn player character
            zone.SpawnActors(handler);
        }

        public void SetDalamudPhase(Socket sender)
        {
            byte[] data = new byte[0x08];
            data[0] = 0xff; //test other values and make enum later

            SendPacket(sender, Opcode.SetDalamud, data);
        }       

        public void SetMusic(Socket sender, uint musicId)
        {
            byte[] data = new byte[0x08];            
            data[0] = (byte)musicId; //these numbers will not vary, so no need for blockcopy
            data[0x02] = (byte)MusicMode.Play;
            SendPacket(sender, Opcode.SetMusic, data);
        }

        public void SetWeather(Socket sender, Weather weather)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)weather), 0, data, 0, sizeof(ushort));
            data[0x02] = 0x02;
            SendPacket(sender, Opcode.SetWeather, data);           
        }    

        public void SetMap(Socket sender, uint zoneId)
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(Zones.Find(x => x.Id == zoneId).RegionId), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(zoneId), 0, data, 0x04, sizeof(uint));            
            data[0x08] = 0x28;
            SendPacket(sender, Opcode.SetMap, data);           
        }       

        public void TeleportPlayer(Socket handler, uint zoneId)
        {
            Debug debug = new Debug();
            Zone zone = Zones.Find(x => x.Id == zoneId);
            UserFactory.Instance.User.Character.Position = ZoneList.EntryPoints.Find(x => x.ZoneId == zoneId);

            SetIsZoning(handler);
            SetDalamudPhase(handler);
            SetMusic(handler, zone.GetCurrentBGM());
            SetWeather(handler, Weather.Clear);
            SetMap(handler, zoneId);
            WorldMaster.Spawn(handler, 0x01);
            debug.Spawn(handler);
            zone.Spawn(handler);//spawn zone             
            UserFactory.Instance.User.Character.Spawn(handler);//spawn player character
        }


    }

    [Serializable]
    public enum Weather
    {
        Clear = 8001,
        Fine = 8002,
        Cloudy = 8003,
        Foggy = 8004,
        Windy = 8005,
        Blustery = 8006,
        Rainy = 8007,
        Showery = 8008,
        Thundery = 8009,
        Stormy = 8010,
        Dusty = 8011,
        Sandy = 8012,
        Hot = 8013,
        Blistering = 8014,
        Snowy = 8015,
        Wintry = 8016,
        Gloomy = 8017,
        Seasonal = 8027,
        Primal = 8028,
        Fireworks = 8029,
        Dalamud = 8030,
        Aurora = 8031,
        Dalamudthunder = 8032,
        Day = 8065,
        Twilight = 8066
    }
}
