using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace Launcher
{    
    [Serializable]
    public class Zone : Actor
    {
        private Log _log = Log.Instance;
        public uint RegionId { get; set; }
        public string MapName { get; set; }
        public string PlaceName { get; set; }      
        public bool MountAllowed { get; set; }       
        public string ContentFunction { get; set; }
        public MusicSet MusicSet { get; set; }
        public ZoneType Type { get; set; }
        public byte PrivLevel { get; set; }

        public List<Actor> Actors = new List<Actor>(); //list to keep all actors which are currentl in this zone.

        public Zone(uint regionId, uint zoneId, byte locationNameId, byte musicSetId, int classNameId = -1, bool isMountAllowed = true, ZoneType zoneType = ZoneType.Default, string mapName = null)
        {
            Id = zoneId; 
            Name = Encoding.ASCII.GetBytes("_areaMaster");
            RegionId = regionId;
            MapName = mapName;
            PlaceName = ZoneList.LocationName[locationNameId];
            ClassName = classNameId < 0 ? null : "ZoneMaster" + ZoneList.ClassName[classNameId];
            MusicSet = MusicSet.Get(musicSetId);           
            MountAllowed = isMountAllowed;
            Type = zoneType;

            Actors.AddRange(ActorRepository.Instance.Aetherytes.FindAll(x => x.Position.ZoneId == Id));
            Actors.AddRange(ActorRepository.Instance.GetZoneNpcs(Id));
        }

        public override void Prepare(ushort actorIndex = 0)
        {
            LuaParameters = new LuaParameters
            {
                ActorName = "_areaMaster" + "@0" + LuaParameters.SwapEndian(Id).ToString("X").Substring(0, 4),
                ClassName = ClassName,
                ClassCode = 0x30400000
            };

            LuaParameters.Add("/Area/Zone/" + ClassName);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(MapName);
            LuaParameters.Add((!string.IsNullOrEmpty(ContentFunction) ? ContentFunction : ""));
            LuaParameters.Add((!string.IsNullOrEmpty(ContentFunction) ? 1 : -1));
            LuaParameters.Add(Convert.ToByte(MountAllowed));

            for (int i = 7; i > -1; i--)
                LuaParameters.Add(((byte)Type & (1 << i)) != 0);
        }

        public override void Spawn(Socket handler, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0, ushort actorIndex = 0)
        {
            Prepare();
            CreateActor(handler);
            SetSpeeds(handler);
            SetPosition(handler, 1, isZoning);
            SetName(handler);
            SetMainState(handler);            
            SetIsZoning(handler);
            LoadScript(handler);
        }       

        public void SpawnActors(Socket sender, string allowedTypes = "all")
        {            
            List<Actor> actorsToSpawn = Actors; //all actors in zone

            try
            {
                switch (allowedTypes)
                {
                    case "monster":
                        actorsToSpawn = Actors.FindAll(x => x.GetType().Name == "Monster").ToList();
                        break;
                    case "populace":
                        actorsToSpawn = Actors.FindAll(x => x.GetType().Name == "Populace").ToList();
                        break;
                }

                for (int i = 0; i < actorsToSpawn.Count; i++)
                    actorsToSpawn[i].Spawn(sender, actorIndex: (ushort)(i + 1));
            }
            catch (Exception e) { _log.Error(e.Message); }

            _log.Success("Loaded " + actorsToSpawn.Count + " actors in zone " + PlaceName);
        }

        public uint GetCurrentBGM()
        {
            //if it's past 8 in the morning, play day music.
            if (Clock.Instance.Period == "AM" && Clock.Instance.Time >= new TimeSpan(5, 0, 0))
                return MusicSet.DayMusic;
            else
                return MusicSet.NightMusic;
        }
    }

    [Serializable]
    public class ZoneList
    {        
        public static string[] LocationName { get; } = new string[]
        {
            "Lower La Noscea",              //0
            "Western La Noscea",            //1
            "Eastern La Noscea",            //2
            "Mistbeard Cove",               //3
            "Cassiopeia Hollow",            //4
            "Limsa Lominsa",                //5
            "U'Ghamaro Mines",              //6
            "La Noscea",                    //7
            "Sailors Ward",                 //8
            "Upper La Noscea",              //9
            "Shposhae",                     //10
            "Locke's Lie",                  //11
            "Turtleback Island",            //12
            "Coerthas Central Highlands",   //13
            "Coerthas Eastern Highlands",   //14
            "Coerthas Eastern Lowlands",    //15
            "Coerthas",                     //16
            "Coerthas Central Lowlands",    //17
            "Coerthas Western Highlands",   //18 
            "Dzemael Darkhold",             //19
            "The Howling Eye",              //20
            "The Aurum Vale",               //21
            "Central Shroud",               //22
            "East Shroud",                  //23
            "North Shroud",                 //24
            "West Shroud",                  //25
            "South Shroud",                 //26
            "Gridania",                     //27
            "The Black Shroud",             //28
            "The Mun-Tuy Cellars",          //29
            "The Tam-Tara Deepcroft",       //30
            "The Thousand Maws of Toto-Rak",//31
            "Peasants Ward",                //32
            "Thornmarch",                   //33
            "Central Thanalan",             //34
            "Eastern Thanalan",             //35
            "Western Thanalan",             //36
            "Northern Thanalan",            //37
            "Southern Thanalan",            //38
            "Ul'dah",                       //39
            "Nanawa Mines",                 //40
            "Copperbell Mines",             //41
            "Thanalan",                     //42
            "Merchants Ward",               //43
            "The Bowl of Embers",           //44
            "Cutter's Cry",                 //45
            "Mor Dhona",                    //46
            "Transmission Tower",           //47
            "Rivenroad",                    //48
            "Rhotano Sea",                  //49
            "The Cieldalaes",               //50
            "Market Wards",                 //51
            "Maelstrom Command",            //52
            "Adders' Nest",                 //53
            "Hall of Flames",               //54
            "Inn Room",                     //55
            "Strait of Merlthor",           //56
            "Jail",                         //57
            "Cottage"                       //58
        };
        public static string[] ClassName { get; } = new string[]
        {
            "SeaS0",       //0
            "MarketSeaS0", //1
            "RocR0",       //2
            "FstF0",       //3
            "MarketFstF0", //4
            "BattleFstF0", //5
            "WilW0",       //6
            "MarketWilW0", //7
            "BattleWilW0", //8
            "LakL0",       //9
            "BattleOcnO1", //10
            "Jail",        //11
            "BattleOcnO0", //12
            "CruiseSeaS1", //13
            "CottagePrv00",//14
            "OfficeSeaS0", //15
            "OfficeWilW0", //16
            "OfficeFstF0", //17
            "SeaS1",       //18
            "PrvI0",       //19
            "RocR1"        //20
        };
       
        
    }

    
}


