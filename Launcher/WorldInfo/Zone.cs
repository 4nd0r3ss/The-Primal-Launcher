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
        public byte[] PlaceName { get; set; }       
        public bool IsIsolated { get; set; }
        public bool IsInn { get; set; }
        public bool MountAllowed { get; set; }
        public bool CanStealth { get; set; }
        public bool IsInstance { get; set; }
        public MusicSet MusicSet { get; set; }

        public List<Actor> LoadedActors = new List<Actor>(); //list to keep all actors which are currentl in this zone.

        public Zone(uint zoneId, uint regionId, string mapName, string placeName, string className, MusicSet musicSet, bool isIsolated, bool isInn, bool mountAllowed, bool canStealth, bool isInstance)
        {
            Id = zoneId;
            //Name = "_areaMaster";
            RegionId = regionId;
            MapName = mapName;
            PlaceName = Encoding.ASCII.GetBytes(placeName);
            ClassPath = className;
            MusicSet = musicSet;
            IsInn = isInn;
            MountAllowed = mountAllowed;
            CanStealth = canStealth;
            IsInstance = isInstance;

            LuaParameters = new LuaParameters
            {
                ActorName = "_areaMaster" + "@0" + LuaParameters.SwapEndian(zoneId).ToString("X").Substring(0, 4),
                ClassName = className
            };

            LuaParameters.Add("/Area/Zone/" + className);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(mapName);
            LuaParameters.Add("");
            LuaParameters.Add(-1);
            LuaParameters.Add((byte)0);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(true);
            LuaParameters.Add(false);
            LuaParameters.Add(false);

            //get aetherytes from list
            //    foreach(var eth in Aetheryte.AetheryteList)
            //    {
            //        if (eth.Key == zoneId)
            //            LoadedActors.Add(eth.Value);
            //    }
            //    //try this for performance
            //    //var selected = Aetheryte.AetheryteList.Select(x => x.Key == zoneId).ToList();
            //    //foreach(var a in selected)
            //    //{
            //    //    LoadedActors.Add((Aetheryte)a);
            //    //}
        }

        public void SpawnActors(Socket sender)
        {
            //foreach (var a in LoadedActors)
            //{
            //    a.Spawn(sender);
            //}
        }
    }

    [Serializable]
    public class ZoneList
    {
        public Dictionary<uint, Zone> Zones = new Dictionary<uint, Zone>
        {
            //Convert booleans to bitfield?
            { 128, new Zone(128, 101, "sea0Field01", "Lower La Noscea", ClassName[0], MusicSet.Get(1), false, false, true, false, false)},
            { 129, new Zone(129, 101, "sea0Field02", "Western La Noscea", ClassName[0], MusicSet.Get(1), false, false, true, false, false)},
            { 130, new Zone(130, 101, "sea0Field03", "Eastern La Noscea", ClassName[0], MusicSet.Get(1), false, false, true, false, false)},
            { 131, new Zone(131, 101, "sea0Dungeon01", "Mistbeard Cove", ClassName[0], MusicSet.Get(0), false, false, false, false, false)},
            { 132, new Zone(132, 101, "sea0Dungeon03", "Cassiopeia Hollow", ClassName[0], MusicSet.Get(0), false, false, false, false, false)},
            { 133, new Zone(133, 101, "sea0Town01", "Limsa Lominsa", ClassName[0], MusicSet.Get(2), false, false, false, false, false)},
            { 134, new Zone(134, 202, "sea0Market01", "Market Wards", ClassName[1], MusicSet.Get(0), false, false, false, false, false)},
            { 135, new Zone(135, 101, "sea0Field04", "Upper La Noscea", ClassName[0], MusicSet.Get(1), false, false, true, false, false)},
            { 137, new Zone(137, 101, "sea0Dungeon06", "U'Ghamaro Mines", ClassName[0], MusicSet.Get(0), false, false, false, true, false)},
            { 138, new Zone(138, 101, null, "La Noscea", "", MusicSet.Get(1), false, false, false, false, false)},

            { 139, new Zone(139, 112, "sea0Field01a", "The Cieldalaes", ClassName[0], MusicSet.Get(0), false, false, false, false, false)},
            { 140, new Zone(140, 101, null, "Sailors Ward", "", MusicSet.Get(0), false, false, false, false, false)},
            { 141, new Zone(141, 101, "sea0Field01a", "Lower La Noscea", ClassName[0], MusicSet.Get(1), false, false, false, false, false)},
            { 143, new Zone(143, 102, "roc0Field01", "Coerthas Central Highlands", ClassName[2], MusicSet.Get(3), false, false, true, false, false)},
            { 144, new Zone(144, 102, "roc0Field02", "Coerthas Eastern Highlands", ClassName[2], MusicSet.Get(3), false, false, true, false, false)},
            { 145, new Zone(145, 102, "roc0Field03", "Coerthas Eastern Lowlands", ClassName[2], MusicSet.Get(3), false, false, true, false, false)},
            { 146, new Zone(146, 102, null, "Coerthas", "", MusicSet.Get(3), false, false, false, false, false)},
            { 147, new Zone(147, 102, "roc0Field04", "Coerthas Central Lowlands", ClassName[2], MusicSet.Get(3), false, false, true, false, false)},
            { 148, new Zone(148, 102, "roc0Field05", "Coerthas Western Highlands", ClassName[2], MusicSet.Get(3), false, false, true, false, false)},
            { 150, new Zone(150, 103, "fst0Field01", "Central Shroud", ClassName[3], MusicSet.Get(4), false, false, true, false, false)},

            { 151, new Zone(151, 103, "fst0Field02", "East Shroud", ClassName[3], MusicSet.Get(4), false, false, true, false, false)},
            { 152, new Zone(152, 103, "fst0Field03", "North Shroud", ClassName[3], MusicSet.Get(4), false, false, true, false, false)},
            { 153, new Zone(153, 103, "fst0Field04", "West Shroud", ClassName[3], MusicSet.Get(4), false, false, true, false, false)},
            { 154, new Zone(154, 103, "fst0Field05", "South Shroud", ClassName[3], MusicSet.Get(4), false, false, true, false, false)},
            { 155, new Zone(155, 103, "fst0Town01", "Gridania", ClassName[3], MusicSet.Get(5), false, false, false, false, false)},
            { 156, new Zone(156, 103, null, "The Black Shroud", "", MusicSet.Get(0), false, false, false, false, false)},
            { 157, new Zone(157, 103, "fst0Dungeon01", "The Mun-Tuy Cellars", ClassName[3], MusicSet.Get(0), false, false, false, false, false)},
            { 158, new Zone(158, 103, "fst0Dungeon02", "The Tam-Tara Deepcroft", ClassName[3], MusicSet.Get(0), false, false, false, false, false)},
            { 159, new Zone(159, 103, "fst0Dungeon03", "The Thousand Maws of Toto-Rak", ClassName[3], MusicSet.Get(0), false, false, false, false, false)},
            { 160, new Zone(160, 204, "fst0Market01", "Market Wards", ClassName[4], MusicSet.Get(0), false, false, false, false, false)},

            { 161, new Zone(161, 103, null, "Peasants Ward", "", MusicSet.Get(0), false, false, false, false, false)},
            { 162, new Zone(162, 103, "fst0Field01a", "Central Shroud", ClassName[3], MusicSet.Get(4), false, false, false, false, false)},
            { 164, new Zone(164, 106, "fst0Battle01", "Central Shroud", ClassName[5], MusicSet.Get(6), false, false, false, false, false)},
            { 165, new Zone(165, 106, "fst0Battle02", "Central Shroud", ClassName[5], MusicSet.Get(6), false, false, false, false, false)},
            { 166, new Zone(166, 106, "fst0Battle03", "Central Shroud", ClassName[5], MusicSet.Get(6), false, false, false, false, false)},
            { 167, new Zone(167, 106, "fst0Battle04", "Central Shroud", ClassName[5], MusicSet.Get(6), false, false, false, false, false)},
            { 168, new Zone(168, 106, "fst0Battle05", "Central Shroud", ClassName[5], MusicSet.Get(6), false, false, false, false, false)},
            { 170, new Zone(170, 104, "wil0Field01", "Central Thanalan", ClassName[6], MusicSet.Get(7), false, false, true, false, false)},
            { 171, new Zone(171, 104, "wil0Field02", "Eastern Thanalan", ClassName[6], MusicSet.Get(7), false, false, true, false, false)},
            { 172, new Zone(172, 104, "wil0Field03", "Western Thanalan", ClassName[6], MusicSet.Get(7), false, false, true, false, false)},

            { 173, new Zone(173, 104, "wil0Field04", "Northern Thanalan", ClassName[6], MusicSet.Get(7), false, false, true, false, false)},
            { 174, new Zone(174, 104, "wil0Field05", "Southern Thanalan", ClassName[6], MusicSet.Get(7), false, false, true, false, false)},
            { 175, new Zone(175, 104, "wil0Town01", "Ul'dah", ClassName[6], MusicSet.Get(11), false, false, false, false, false)},
            { 176, new Zone(176, 104, "wil0Dungeon02", "Nanawa Mines", ClassName[6], MusicSet.Get(0), false, false, false, false, false)},
            { 177, new Zone(177, 207, "_jail", "-", ClassName[11], MusicSet.Get(1), false, false, false, false, false)},
            { 178, new Zone(178, 104, "wil0Dungeon04", "Copperbell Mines", ClassName[6], MusicSet.Get(0), false, false, false, false, false)},
            { 179, new Zone(179, 104, "null", "Thanalan", "", MusicSet.Get(0), false, false, false, false, false)},
            { 180, new Zone(180, 205, "wil0Market01", "Market Wards", ClassName[7], MusicSet.Get(0), false, false, false, false, false)},
            { 181, new Zone(181, 104, null, "Merchants Ward", "", MusicSet.Get(0), false, false, false, false, false)},
            { 182, new Zone(182, 104, null, "Central Thanalan", "", MusicSet.Get(0), false, false, false, false, false)},

            { 184, new Zone(184, 107, "wil0Battle01", "Ul'dah", ClassName[8], MusicSet.Get(0), false, false, false, false, false)},
            { 185, new Zone(185, 107, "wil0Battle01", "Ul'dah", ClassName[8], MusicSet.Get(0), false, false, false, false, false)},
            { 186, new Zone(186, 104, "wil0Battle02", "Ul'dah", ClassName[8], MusicSet.Get(0), false, false, false, false, false)},
            { 187, new Zone(187, 104, "wil0Battle03", "Ul'dah", ClassName[8], MusicSet.Get(0), false, false, false, false, false)},
            { 188, new Zone(188, 104, "wil0Battle04", "Ul'dah", ClassName[8], MusicSet.Get(0), false, false, false, false, false)},
            { 190, new Zone(190, 105, "lak0Field01", "Mor Dhona", ClassName[9], MusicSet.Get(8), false, false, true, false, false)},
            { 192, new Zone(192, 112, "ocn1Battle01", "Rhotano Sea", ClassName[10], MusicSet.Get(0), false, false, false, false, false)},
            { 193, new Zone(193, 111, "ocn0Battle02", "Rhotano Sea", ClassName[12], MusicSet.Get(9), false, false, false, false, false)},
            { 194, new Zone(194, 112, "ocn1Battle03", "Rhotano Sea", ClassName[10], MusicSet.Get(0), false, false, false, false, false)},
            { 195, new Zone(195, 112, "ocn1Battle04", "Rhotano Sea", ClassName[10], MusicSet.Get(0), false, false, false, false, false)},

            { 196, new Zone(196, 112, "ocn1Battle05", "Rhotano Sea", ClassName[10], MusicSet.Get(0), false, false, false, false, false)},
            { 198, new Zone(198, 112, "ocn1Battle06", "Rhotano Sea", ClassName[10], MusicSet.Get(0), false, false, false, false, false)},
            { 200, new Zone(200, 805, "sea1Cruise01", "Strait of Merlthor", ClassName[13], MusicSet.Get(10), false, false, false, false, false)},
            { 201, new Zone(201, 208, "prv0Cottage00", "-", ClassName[14], MusicSet.Get(0), false, false, false, false, false)},
            { 204, new Zone(204, 101, "sea0Field02a", "Western La Noscea", "", MusicSet.Get(1), false, false, false, false, false)},
            { 205, new Zone(205, 101, "sea0Field03a", "Eastern La Noscea", "", MusicSet.Get(1), false, false, false, false, false)},
            { 206, new Zone(206, 103, "fst0Town01a", "Gridania", ClassName[3], MusicSet.Get(16), false, false, false, false, false)},
            { 207, new Zone(207, 103, "fst0Field03a", "North Shroud", ClassName[3], MusicSet.Get(4), false, false, false, false, false)},
            { 208, new Zone(208, 103, "fst0Field05a", "South Shroud", ClassName[3], MusicSet.Get(4), false, false, false, false, false)},
            { 209, new Zone(209, 104, "wil0Town01a", "Ul'dah", ClassName[5], MusicSet.Get(11), false, false, false, false, false)},

            { 210, new Zone(210, 104, null, "Eastern Thanalan", "", MusicSet.Get(7), false, false, false, false, false)},
            { 211, new Zone(211, 104, null, "Western Thanalan", "", MusicSet.Get(7), false, false, false, false, false)},
            { 230, new Zone(230, 101, "sea0Town01a", "Limsa Lominsa", ClassName[0], MusicSet.Get(2), false, false, false, false, false)},
            { 231, new Zone(231, 102, "roc0Dungeon01", "Dzemael Darkhold", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},
            { 232, new Zone(232, 202, "sea0Office01", "Maelstrom Command", ClassName[15], MusicSet.Get(13), false, false, false, false, false)},
            { 233, new Zone(233, 205, "wil0Office01", "Hall of Flames", ClassName[16], MusicSet.Get(14), false, false, false, false, false)},
            { 234, new Zone(234, 204, "fst0Office01", "Adders' Nest", ClassName[17], MusicSet.Get(12), false, false, false, false, false)},
            { 235, new Zone(235, 101, null, "Shposhae", "", MusicSet.Get(0), false, false, false, false, false)},
            { 236, new Zone(236, 101, "sea1Field01", "Locke's Lie", ClassName[18], MusicSet.Get(0), false, false, false, false, false)},
            { 237, new Zone(237, 101, null, "Turtleback Island", "", MusicSet.Get(0), false, false, false, false, false)},

            { 238, new Zone(238, 103, "fst0Field04", "Thornmarch", "", MusicSet.Get(0), false, false, false, false, false)},
            { 239, new Zone(239, 102, "roc0Field02a", "The Howling Eye", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},
            { 240, new Zone(240, 104, "wil0Field05a", "The Bowl of Embers", "", MusicSet.Get(0), false, false, false, false, false)},
            { 244, new Zone(244, 209, "prv0Inn01", "Inn Room", ClassName[19], MusicSet.Get(15), false, true, false, false, false)},
            { 245, new Zone(245, 102, "roc0Dungeon04", "The Aurum Vale", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},
            { 246, new Zone(246, 104, null, "Cutter's Cry", "", MusicSet.Get(0), false, false, false, false, false)},
            { 247, new Zone(247, 103, null, "North Shroud", "", MusicSet.Get(0), false, false, false, false, false)},
            { 248, new Zone(248, 101, null, "Western La Noscea", "", MusicSet.Get(0), false, false, false, false, false)},
            { 249, new Zone(249, 104, null, "Eastern Thanalan", "", MusicSet.Get(0), false, false, false, false, false)},
            { 250, new Zone(250, 102, "roc0Field02a", "The Howling Eye", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},

            { 251, new Zone(251, 105, null, "Transmission Tower", "", MusicSet.Get(0), false, false, false, false, false)},
            { 252, new Zone(252, 102, "roc0Dungeon04", "The Aurum Vale", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},
            { 253, new Zone(253, 102, "roc0Dungeon04", "The Aurum Vale", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},
            { 254, new Zone(254, 104, null, "Cutter's Cry", "", MusicSet.Get(0), false, false, false, false, false)},
            { 255, new Zone(255, 104, null, "Cutter's Cry", "", MusicSet.Get(0), false, false, false, false, false)},
            { 256, new Zone(256, 102, "roc0Field02a", "The Howling Eye", ClassName[2], MusicSet.Get(0), false, false, false, false, false)},
            { 257, new Zone(257, 109, "roc1Field01", "Rivenroad", ClassName[20], MusicSet.Get(0), false, false, false, false, false)},
            { 258, new Zone(258, 103, null, "North Shroud", "", MusicSet.Get(0), false, false, false, false, false)},
            { 259, new Zone(259, 103, null, "North Shroud", "", MusicSet.Get(0), false, false, false, false, false)},
            { 260, new Zone(260, 101, null, "Western La Noscea", "", MusicSet.Get(0), false, false, false, false, false)},

            { 261, new Zone(261, 101, null, "Western La Noscea", "", MusicSet.Get(0), false, false, false, false, false)},
            { 262, new Zone(262, 104, null, "Eastern Thanalan", "", MusicSet.Get(0), false, false, false, false, false)},
            { 263, new Zone(263, 104, null, "Eastern Thanalan", "", MusicSet.Get(0), false, false, false, false, false)},
            { 264, new Zone(264, 105, "lak0Field01", "Transmission Tower", "", MusicSet.Get(0), false, false, true, false, false)},
            { 265, new Zone(265, 104, null, "The Bowl of Embers", "", MusicSet.Get(0), false, false, false, false, false)},
            { 266, new Zone(266, 105, "lak0Field01a", "Mor Dhona", ClassName[9], MusicSet.Get(8), false, false, false, false, false)},
            { 267, new Zone(267, 109, "roc1Field02", "Rivenroad", ClassName[20], MusicSet.Get(0), false, false, false, false, false)},
            { 268, new Zone(268, 109, "roc1Field03", "Rivenroad", ClassName[20], MusicSet.Get(0), false, false, false, false, false)},
            { 269, new Zone(269, 101, null, "Locke's Lie", "", MusicSet.Get(0), false, false, false, false, false)},
            { 270, new Zone(270, 101, null, "Turtleback Island", "", MusicSet.Get(0), false, false, false, false, false)},
        };

        public static string[] ClassName { get; } = new string[]
        {
            "ZoneMasterSeaS0",       //0
            "ZoneMasterMarketSeaS0", //1
            "ZoneMasterRocR0",       //2
            "ZoneMasterFstF0",       //3
            "ZoneMasterMarketFstF0", //4
            "ZoneMasterBattleFstF0", //5
            "ZoneMasterWilW0",       //6
            "ZoneMasterMarketWilW0", //7
            "ZoneMasterBattleWilW0", //8
            "ZoneMasterLakL0",       //9
            "ZoneMasterBattleOcnO1", //10
            "ZoneMasterJail",        //11
            "ZoneMasterBattleOcnO0", //12
            "ZoneMasterCruiseSeaS1", //13
            "ZoneMasterCottagePrv00",//14
            "ZoneMasterOfficeSeaS0", //15
            "ZoneMasterOfficeWilW0", //16
            "ZoneMasterOfficeFstF0", //17
            "ZoneMasterSeaS1",       //18
            "ZoneMasterPrvI0",       //19
            "ZoneMasterRocR1"        //20
        };

        public Zone GetZone(uint id) => Zones[id];

        public static List<Position> EntryPoints { get; } = new List<Position>
        {
            new Position(128, -8.48f, 45.36f, 139.5f, 2.02f, 15),
            new Position(133, -444.266f, 39.518f, 191f, 1.9f, 15),
            new Position(133, -8.062f, 45.429f, 139.364f, 2.955f, 15),
            new Position(150, 333.271f, 5.889f, -943.275f, 0.794f, 15),
            new Position(155, 58.92f, 4f, -1219.07f, 0.52f, 15),
            new Position(166, 356.09f, 3.74f, -701.62f, -1.4f, 15),
            new Position(166, 356.09f, 3.74f, -701.62f, -1.4f, 16),
            new Position(170, -27.015f, 181.798f,-79.72f, 2.513f, 15),
            new Position(175, -110.157f, 202f, 171.345f, 0f, 15),
            new Position(184, 5.36433f, 196f, 133.656f, -2.84938f, 15),
            new Position(184, -24.34f, 192f, 34.22f, 0.78f, 16),
            new Position(184, -24.34f, 192f, 34.22f, 0.78f, 15),
            new Position(184, -22f, 196f, 87f, 1.8f, 15),
            new Position(193, 0.016f, 10.35f, -36.91f, 0.025f, 15),
            new Position(193, -5f, 16.35f, 6f, 0.5f, 16),
            new Position(230, -838.1f, 6f, 231.94f, 1.1f, 15),
            new Position(244, 0.048f, 0f, -5.737f, 0f, 15),
            new Position(244, -160.048f, 0f, -165.737f, 0f, 15),
            new Position(244, 160.048f, 0f, 154.263f, 0f, 15)
        };      
    }
}

