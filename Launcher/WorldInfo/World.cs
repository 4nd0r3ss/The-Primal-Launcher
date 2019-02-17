using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace Launcher
{
    [Serializable]
    public class World
    {
        #region Static members
        public static int SIZE = 0x50;//0x50; //account data chunk size
        public static byte OPCODE = 0x15;
        #endregion

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        public string Address { get; set; }
        public ushort Port {get;set;}
        public uint CharacterId { get; set; }
        public ZoneList ZoneList { get; set; }
        #endregion            

        private enum Opcode
        {
            SetDalamud = 0x10,
            SetMusic = 0x0c,
            SetWeather = 0x0d,
            SetMap = 0x05,
            BeginSpawning = 0x017b //this an actor opcode. don't want to turn World into an actor just bc of this...
        }     

        public void SpawnDebugActor(Socket sender)
        {
            Actor debug = new Actor
            {
                //Name = "debug",
                Id = 0x5ff80002, //id from hardcoded packet (just bc it works)     
                TargetId = CharacterId
            };

            debug.LuaParameters = new LuaParameters
            {
                ActorName = "debug",
                ClassName = "Debug"
            };

            debug.LuaParameters.Add("/System/Debug.prog");
            debug.LuaParameters.Add(false);
            debug.LuaParameters.Add(false);
            debug.LuaParameters.Add(false);
            debug.LuaParameters.Add(false);
            debug.LuaParameters.Add(true);
            debug.LuaParameters.Add((uint)0xc51f); //???
            debug.LuaParameters.Add(true);
            debug.LuaParameters.Add(true);

            debug.Speeds = new uint[] { 0, 0, 0, 0 };

            debug.Position = new Position();

            debug.Spawn(sender, 0x01);
        }

        public void FinishWorldInit(Socket sender)
        {
            Actor worldMaster = new Actor
            {
                //Name = "worldMaster",
                Id = 0x5ff80001, //id from hardcoded packet (just bc it works)     
                TargetId = CharacterId
            };

            worldMaster.LuaParameters = new LuaParameters
            {
                ActorName = "worldMaster",
                ClassName = "WorldMaster"
            };

            worldMaster.LuaParameters.Add("/World/WorldMaster_event");
            worldMaster.LuaParameters.Add(false);
            worldMaster.LuaParameters.Add(false);
            worldMaster.LuaParameters.Add(false);
            worldMaster.LuaParameters.Add(false);
            worldMaster.LuaParameters.Add(false);
            worldMaster.LuaParameters.Add(null);            

            worldMaster.Speeds = new uint[] { 0, 0, 0, 0 };

            worldMaster.Position = new Position();

            worldMaster.Spawn(sender, 0x01);
        }

        public void SetDalamudPhase(Socket sender)
        {
            byte[] data = new byte[0x08];
            data[0] = 0xff; //test other values and make enum later

            SendPacket(sender, Opcode.SetDalamud, data);
        }

        public void BeginWorldInit(Socket sender) => SendPacket(sender, Opcode.BeginSpawning, new byte[0x08]);

        public void SetMusic(Socket sender, byte musicId)
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
            Buffer.BlockCopy(BitConverter.GetBytes(ZoneList.GetZone(zoneId).RegionId), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(zoneId), 0, data, 0x04, sizeof(uint));            
            data[0x08] = 0x28;
            SendPacket(sender, Opcode.SetMap, data);           
        }

        private void SendPacket(Socket sender, Opcode opcode, byte[] data)
        {
            GamePacket gamePacket = new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            };

            Packet packet = new Packet(new SubPacket(gamePacket) { SourceId = CharacterId, TargetId = CharacterId });
            sender.Send(packet.ToBytes());
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
