using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace Launcher
{
    public enum ZoneType
    {
        Default = 4,
        Inn = 68,
        Instance = 6,
        CanStealth = 132
    }
    [Serializable]
    public class Zone : Actor
    {
        private Log _log = Log.Instance;
        public uint RegionId { get; set; }
        public string MapName { get; set; }
        public string PlaceName { get; set; }      
        public bool MountAllowed { get; set; }       
        public MusicSet MusicSet { get; set; }

        public List<Actor> Actors = new List<Actor>(); //list to keep all actors which are currentl in this zone.

        public Zone(uint regionId, uint zoneId, byte locationNameId, byte musicSetId, int classNameId = -1, bool isMountAllowed = true, ZoneType zoneType = ZoneType.Default, string mapName = null)
        {
            Id = zoneId;            
            RegionId = regionId;
            MapName = mapName;
            PlaceName = ZoneList.LocationName[locationNameId];
            ClassPath = classNameId < 0 ? null : "ZoneMaster" + ZoneList.ClassName[classNameId];
            MusicSet = MusicSet.Get(musicSetId);           
            MountAllowed = isMountAllowed;
            
            LuaParameters = new LuaParameters
            {
                ActorName = "_areaMaster" + "@0" + LuaParameters.SwapEndian(zoneId).ToString("X").Substring(0, 4),
                ClassName = ClassPath,
                ServerCodes = 0x30400000
            };

            LuaParameters.Add("/Area/Zone/" + ClassPath);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(MapName);
            LuaParameters.Add("");
            LuaParameters.Add(-1);
            LuaParameters.Add(Convert.ToByte(isMountAllowed));
            
            for (int i = 7; i > -1; i--)          
                LuaParameters.Add(((byte)zoneType & (1 << i)) != 0);
        }
       
        public void SpawnActors(Socket sender)
        {
            try
            {
                for(int i=0;i<Actors.Count;i++)
                    Actors[i].Spawn(sender, actorIndex: (ushort)i);
            }
            catch(Exception e)
            {
                _log.Error(e.Message);
            }

            _log.Success("Loaded " + Actors.Count + " actors in zone " + PlaceName);
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
        public List<Zone> Zones = new List<Zone>
        {
            //Convert booleans to bitfield?
            new Zone(101, 128, locationNameId: 0, classNameId: 0, musicSetId: 1, mapName: "sea0Field01"),
            new Zone(101, 129, locationNameId: 1, classNameId: 0, musicSetId: 1, mapName: "sea0Field02"),
            new Zone(101, 130, locationNameId: 2, classNameId: 0, musicSetId: 1, mapName: "sea0Field03"),
            new Zone(101, 131, locationNameId: 3, classNameId: 0, musicSetId: 0, mapName: "sea0Dungeon01"),
            new Zone(101, 132, locationNameId: 4, classNameId: 0, musicSetId: 0, mapName: "sea0Dungeon03"),
            new Zone(101, 133, locationNameId: 5, classNameId: 0, musicSetId: 2, mapName: "sea0Town01"),
            new Zone(101, 137, locationNameId: 6, classNameId: 0, musicSetId: 0, mapName: "sea0Dungeon06"),
            //new Zone(101, 138, locationNameId: 7, musicSetId: 1),
            //new Zone(101, 140, locationNameId: 8, musicSetId: 0),
            new Zone(101, 141, locationNameId: 0, classNameId: 0, musicSetId: 1, mapName: "sea0Field01a"),
            new Zone(101, 135, locationNameId: 9, classNameId: 0, musicSetId: 1, mapName: "sea0Field04"),
            //new Zone(101, 204, locationNameId: 1, musicSetId: 1, mapName: "sea0Field02a"),
            //new Zone(101, 205, locationNameId: 2, musicSetId: 1, mapName: "sea0Field03a"),
            new Zone(101, 230, locationNameId: 5, classNameId: 0, musicSetId: 2, mapName: "sea0Town01a"),
            //new Zone(101, 235, locationNameId: 10, musicSetId: 0),
            new Zone(101, 236, locationNameId: 11, classNameId: 18, musicSetId: 0, mapName: "sea1Field01"),
            //new Zone(101, 237, locationNameId: 12, musicSetId: 0),
            //new Zone(101, 248, locationNameId: 1, musicSetId: 0),
            //new Zone(101, 260, locationNameId: 1, musicSetId: 0),
            //new Zone(101, 261, locationNameId: 1, musicSetId: 0),
            //new Zone(101, 269, locationNameId: 11, musicSetId: 0),
            //new Zone(101, 270, locationNameId: 12, musicSetId: 0),
            new Zone(102, 143, locationNameId: 13, classNameId: 2, musicSetId: 3, mapName: "roc0Field01"),
            new Zone(102, 144, locationNameId: 14, classNameId: 2, musicSetId: 3, mapName: "roc0Field02"),
            new Zone(102, 145, locationNameId: 15, classNameId: 2, musicSetId: 3, mapName: "roc0Field03"),
            //new Zone(102, 146, locationNameId: 16, musicSetId: 3),
            new Zone(102, 147, locationNameId: 17, classNameId: 2, musicSetId: 3, mapName: "roc0Field04"),
            new Zone(102, 148, locationNameId: 18, classNameId: 2, musicSetId: 3, mapName: "roc0Field05"),
            new Zone(102, 231, locationNameId: 19, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon01"),
            new Zone(102, 239, locationNameId: 20, classNameId: 2, musicSetId: 0, mapName: "roc0Field02a"),
            new Zone(102, 245, locationNameId: 21, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon04"),
            new Zone(102, 250, locationNameId: 20, classNameId: 2, musicSetId: 0, mapName: "roc0Field02a"),
            new Zone(102, 252, locationNameId: 21, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon04"),
            new Zone(102, 253, locationNameId: 21, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon04"),
            new Zone(102, 256, locationNameId: 20, classNameId: 2, musicSetId: 0, mapName: "roc0Field02a"),
            new Zone(103, 150, locationNameId: 22, classNameId: 3, musicSetId: 4, mapName: "fst0Field01"),
            new Zone(103, 151, locationNameId: 23, classNameId: 3, musicSetId: 4, mapName: "fst0Field02"),
            new Zone(103, 152, locationNameId: 24, classNameId: 3, musicSetId: 4, mapName: "fst0Field03"),
            new Zone(103, 153, locationNameId: 25, classNameId: 3, musicSetId: 4, mapName: "fst0Field04"),
            new Zone(103, 154, locationNameId: 26, classNameId: 3, musicSetId: 4, mapName: "fst0Field05"),
            new Zone(103, 155, locationNameId: 27, classNameId: 3, musicSetId: 5, mapName: "fst0Town01"),
            //new Zone(103, 156, locationNameId: 28, musicSetId: 0),
            new Zone(103, 157, locationNameId: 29, classNameId: 3, musicSetId: 0, mapName: "fst0Dungeon01"),
            new Zone(103, 158, locationNameId: 30, classNameId: 3, musicSetId: 0, mapName: "fst0Dungeon02"),
            new Zone(103, 159, locationNameId: 31, classNameId: 3, musicSetId: 0, mapName: "fst0Dungeon03"),
            //new Zone(103, 161, locationNameId: 32, musicSetId: 0),
            new Zone(103, 162, locationNameId: 22, classNameId: 3, musicSetId: 4, mapName: "fst0Field01a"),
            new Zone(103, 206, locationNameId: 27, classNameId: 3, musicSetId: 16, mapName: "fst0Town01a"),
            new Zone(103, 207, locationNameId: 24, classNameId: 3, musicSetId: 4, mapName: "fst0Field03a"),
            new Zone(103, 208, locationNameId: 26, classNameId: 3, musicSetId: 4, mapName: "fst0Field05a"),
            //new Zone(103, 238, locationNameId: 33, musicSetId: 0, mapName: "fst0Field04"),
            //new Zone(103, 247, locationNameId: 24, musicSetId: 0),
            //new Zone(103, 258, locationNameId: 24, musicSetId: 0),
            //new Zone(103, 259, locationNameId: 24, musicSetId: 0),
            new Zone(104, 170, locationNameId: 34, classNameId: 6, musicSetId: 7, mapName: "wil0Field01"),
            new Zone(104, 171, locationNameId: 35, classNameId: 6, musicSetId: 7, mapName: "wil0Field02"),
            new Zone(104, 172, locationNameId: 36, classNameId: 6, musicSetId: 7, mapName: "wil0Field03"),
            new Zone(104, 173, locationNameId: 37, classNameId: 6, musicSetId: 7, mapName: "wil0Field04"),
            new Zone(104, 174, locationNameId: 38, classNameId: 6, musicSetId: 7, mapName: "wil0Field05"),
            new Zone(104, 175, locationNameId: 39, classNameId: 6, musicSetId: 11, mapName: "wil0Town01"),
            new Zone(104, 176, locationNameId: 40, classNameId: 6, musicSetId: 0, mapName: "wil0Dungeon02"),
            new Zone(104, 178, locationNameId: 41, classNameId: 6, musicSetId: 0, mapName: "wil0Dungeon04"),
            //new Zone(104, 179, locationNameId: 42, musicSetId: 0),
            //new Zone(104, 181, locationNameId: 43, musicSetId: 0),
            //new Zone(104, 182, locationNameId: 34, musicSetId: 0),
            new Zone(104, 186, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle02"),
            new Zone(104, 187, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle03"),
            new Zone(104, 188, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle04"),
            new Zone(104, 209, locationNameId: 39, classNameId: 5, musicSetId: 11, mapName: "wil0Town01a"),
            //new Zone(104, 210, locationNameId: 35, musicSetId: 7),
            //new Zone(104, 211, locationNameId: 36, musicSetId: 7),
            //new Zone(104, 240, locationNameId: 44, musicSetId: 0, mapName: "wil0Field05a"),
            //new Zone(104, 246, locationNameId: 45, musicSetId: 0),
            //new Zone(104, 249, locationNameId: 35, musicSetId: 0),
            //new Zone(104, 254, locationNameId: 45, musicSetId: 0),
            //new Zone(104, 255, locationNameId: 45, musicSetId: 0),
            //new Zone(104, 262, locationNameId: 35, musicSetId: 0),
            //new Zone(104, 263, locationNameId: 35, musicSetId: 0),
            //new Zone(104, 265, locationNameId: 44, musicSetId: 0),
            new Zone(105, 190, locationNameId: 46, classNameId: 9, musicSetId: 8, mapName: "lak0Field01"),
            //new Zone(105, 251, locationNameId: 47, musicSetId: 0),
            //new Zone(105, 264, locationNameId: 47, musicSetId: 0, mapName: "lak0Field01"),
            new Zone(105, 266, locationNameId: 46, classNameId: 9, musicSetId: 8, mapName: "lak0Field01a"),
            new Zone(106, 164, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle01"),
            new Zone(106, 165, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle02"),
            new Zone(106, 166, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle03"),
            new Zone(106, 167, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle04"),
            new Zone(106, 168, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle05"),
            new Zone(107, 184, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle01"),
            new Zone(107, 185, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle01"),
            new Zone(109, 257, locationNameId: 48, classNameId: 20, musicSetId: 0, mapName: "roc1Field01"),
            new Zone(109, 267, locationNameId: 48, classNameId: 20, musicSetId: 0, mapName: "roc1Field02"),
            new Zone(109, 268, locationNameId: 48, classNameId: 20, musicSetId: 0, mapName: "roc1Field03"),
            new Zone(111, 193, locationNameId: 49, classNameId: 12, musicSetId: 9, mapName: "ocn0Battle02"),
            new Zone(112, 139, locationNameId: 50, classNameId: 0, musicSetId: 0, mapName: "sea0Field01a"),
            new Zone(112, 192, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle01"),
            new Zone(112, 194, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle03"),
            new Zone(112, 195, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle04"),
            new Zone(112, 196, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle05"),
            new Zone(112, 198, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle06"),
            new Zone(202, 134, locationNameId: 51, classNameId: 1, musicSetId: 0, mapName: "sea0Market01"),
            new Zone(202, 232, locationNameId: 52, classNameId: 15, musicSetId: 13, mapName: "sea0Office01"),
            new Zone(204, 234, locationNameId: 53, classNameId: 17, musicSetId: 12, mapName: "fst0Office01"),
            new Zone(204, 160, locationNameId: 51, classNameId: 4, musicSetId: 0, mapName: "fst0Market01"),
            new Zone(205, 233, locationNameId: 54, classNameId: 16, musicSetId: 14, mapName: "wil0Office01"),
            new Zone(205, 180, locationNameId: 51, classNameId: 7, musicSetId: 0, mapName: "wil0Market01"),
            new Zone(207, 177, locationNameId: 57, classNameId: 11, musicSetId: 1, mapName: "_jail"),
            new Zone(208, 201, locationNameId: 57, classNameId: 14, musicSetId: 0, mapName: "prv0Cottage00"),
            new Zone(209, 244, locationNameId: 55, classNameId: 19, musicSetId: 15, mapName: "prv0Inn01"),
            new Zone(805, 200, locationNameId: 56, classNameId: 13, musicSetId: 10, mapName: "sea1Cruise01"),
        };
        public static string[] LocationName { get; } = new string[]
        {
            "Lower La Noscea",      //0
            "Western La Noscea",    //1
            "Eastern La Noscea",    //2
            "Mistbeard Cove",       //3
            "Cassiopeia Hollow",    //4
            "Limsa Lominsa",        //5
            "U'Ghamaro Mines",      //6
            "La Noscea",            //7
            "Sailors Ward",         //8
            "Upper La Noscea",      //9
            "Shposhae",             //10
            "Locke's Lie",          //11
            "Turtleback Island",    //12
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
            "-",                            //57
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
        public Zone GetZone(uint id) => Zones.Find(x=>x.Id==id);
        public static List<Position> EntryPoints { get; } = new List<Position>
        {
            new Position(128, -8.48f, 45.36f, 139.5f, 2.02f, 15),
            new Position(133, -444.266f, 39.518f, 191f, 1.9f, 15),
            new Position(133, -8.062f, 45.429f, 139.364f, 2.955f, 15),
            new Position(150, 333.271f, 5.889f, -943.275f, 0.794f, 15),
            new Position(155, 58.92f, 4f, -1219.07f, 0.52f, 15),
            new Position(166, 356.09f, 3.74f, -701.62f, -1.4f, 15),//
            new Position(166, 356.09f, 3.74f, -701.62f, -1.4f, 16),//
            new Position(170, -27.015f, 181.798f,-79.72f, 2.513f, 15),
            new Position(175, -110.157f, 202f, 171.345f, 0f, 15),
            new Position(184, 5.36433f, 196f, 133.656f, -2.84938f, 15),//
            new Position(184, -24.34f, 192f, 34.22f, 0.78f, 16),
            new Position(184, -24.34f, 192f, 34.22f, 0.78f, 15),
            new Position(184, -22f, 196f, 87f, 1.8f, 15),
            new Position(193, 0.016f, 10.35f, -36.91f, 0.025f, 15),//
            new Position(193, -5f, 16.35f, 6f, 0.5f, 16),
            new Position(230, -838.1f, 6f, 231.94f, 1.1f, 15),
            new Position(244, 0.048f, 0f, -5.737f, 0f, 15),
            new Position(244, -160.048f, 0f, -165.737f, 0f, 15),
            new Position(244, 160.048f, 0f, 154.263f, 0f, 15),
            new Position(190, 160.048f, 0f, 154.263f, 0f, 15),
            new Position(240, 160.048f, 0f, 154.263f, 0f, 15),
            new Position(206, -124.852f, 15.920f, -1328.476f, 0f, 15),
            new Position(177, 0f, 0f, 0f, 0f, 15),

        };
    }
}


