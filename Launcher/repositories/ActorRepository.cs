using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class ActorRepository
    {
        private static ActorRepository _instance = null;           
        private static readonly Log _log = Log.Instance;

        public static ActorRepository Instance
        {
            get
            {              
              if (_instance == null)
                  _instance = new ActorRepository();

              return _instance;   
            }
        }

        private ActorRepository()
        {  
            //Aetherytes = aetheryteList.AllAetherytes;

            //add aetherytes to zone actor list
            //foreach (Zone z in Zones)
            //    z.Actors.AddRange(AllAetherytes.Where(x => x.Position.ZoneId == z.Id));
        }

        public List<Aetheryte> Aetherytes { get; } = new List<Aetheryte>()
        {
            //Red Aetherytes
            //new Aetheryte(1280127, AetheryteType.Crystal, new Position(0,-400f, 19f, 338f, 0f, 0), 2048),                  // 
            //new Aetheryte(1280057, AetheryteType.Crystal, new Position(0,33f, 201f, -482f, 0f, 0), 2048),                  // 
            //new Aetheryte(1280058, AetheryteType.Crystal, new Position(0,-1315f, 57f, -147f, 0f, 0), 2048),                // 
            //new Aetheryte(1280059, AetheryteType.Crystal, new Position(0,-165f, 281f, -1699f, 0f, 0), 2048),               //
            //new Aetheryte(1280088, AetheryteType.Crystal, new Position(0,-1054f, 21f, -1761f, 0f, 0), 2048),               // 
            //new Aetheryte(1280089, AetheryteType.Crystal, new Position(0,-1568f, -11f, -552f, 0f, 0), 2048),               // 
            //new Aetheryte(1280117, AetheryteType.Crystal, new Position(0,216f, 303f, -258f, 0f, 0), 2048),                 // 
            //new Aetheryte(1280118, AetheryteType.Crystal, new Position(0,1498f, 207f, 767f, 0f, 0), 2048),                 // 
            //new Aetheryte(1280119, AetheryteType.Crystal, new Position(0,-163f, 223f, 1151f, 0f, 0), 2048),                //
            //new Aetheryte(1280126, AetheryteType.Crystal, new Position(0,484f, 19f, 672f, 0f, 0), 2048),                   // 
            //new Aetheryte(1280127, AetheryteType.Crystal, new Position(0,-400f, 19f, 338f, 0f, 0), 2048),                  // 
            
            //Aetherytes - grouped by teleport menu pages
            new Aetheryte(1280001, AetheryteType.Crystal, new Position(230,-395.1f, 42.5f, 337.12f, 0f, 0)),       // lanoscea_limsa
            new Aetheryte(1280002, AetheryteType.Crystal, new Position(128,29.97f, 45.83f, -35.47f, 0f, 0)),       // lanoscea_beardedrock
            new Aetheryte(1280003, AetheryteType.Crystal, new Position(129,-991.88f, 61.71f, -1120.79f, 0f, 0)),   // lanoscea_skullvalley
            new Aetheryte(1280004, AetheryteType.Crystal, new Position(129,-1883.47f, 53.77f, -1372.68f, 0f, 0)),  // lanoscea_baldknoll
            new Aetheryte(1280005, AetheryteType.Crystal, new Position(130,1123.29f, 45.7f, -928.69f, 0f, 0)),     // lanoscea_bloodshore
            new Aetheryte(1280006, AetheryteType.Crystal, new Position(135,-278.181f, 77.63f, -2260.79f, 0f, 0)),  // lanoscea_ironlake

            new Aetheryte(1280092, AetheryteType.Crystal, new Position(143,216f, 303f, -258f, 0f, 0)),             // coerthas_dragonhead
            new Aetheryte(1280093, AetheryteType.Crystal, new Position(144,1122f, 271f, -1149f, 0f, 0)),           // coerthas_crookedfork
            new Aetheryte(1280094, AetheryteType.Crystal, new Position(145,1498f, 207f, 767f, 0f, 0)),             // coerthas_glory
            new Aetheryte(1280095, AetheryteType.Crystal, new Position(147,-163f, 223f, 1151f, 0f, 0)),            // coerthas_everlakes
            new Aetheryte(1280096, AetheryteType.Crystal, new Position(148,-1761f, 270f, -198f, 0f, 0)),           // coerthas_riversmeet

            new Aetheryte(1280061, AetheryteType.Crystal, new Position(206,-130.63f, 16.08f, -1323.99f, 0f, 0)),   // blackshroud_gridania
            new Aetheryte(1280062, AetheryteType.Crystal, new Position(150,288f, 4f, -543.928f, 0f, 0)),           // blackshroud_bentbranch
            new Aetheryte(1280063, AetheryteType.Crystal, new Position(151,1702f, 20f, -862f, 0f, 0)),             // blackshroud_nineivies
            new Aetheryte(1280064, AetheryteType.Crystal, new Position(152,-1052f, 20f, -1760f, 0f, 0)),           // blackshroud_emeraldmoss
            new Aetheryte(1280065, AetheryteType.Crystal, new Position(153,-1566.04f, -11.89f, -550.51f, 0f, 0)),  // blackshroud_crimsonbark
            new Aetheryte(1280066, AetheryteType.Crystal, new Position(154,734f, -12f, 1126f, 0f, 0)),             // blackshroud_tranquil

            new Aetheryte(1280031, AetheryteType.Crystal, new Position(175,-240.45f, 185.93f, -9.56f, 0f, 0)),     // thanalan_uldah
            new Aetheryte(1280032, AetheryteType.Crystal, new Position(170,33f, 201f, -482f, 0f, 0)),              // thanalan_blackbrush
            new Aetheryte(1280036, AetheryteType.Crystal, new Position(174,1686f, 297f, 995f, 0f, 0)),             // thanalan_brokenwater              
            new Aetheryte(1280033, AetheryteType.Crystal, new Position(171,1250.9f, 264f, -544.2f, 0f, 0)),        // thanalan_drybone
            new Aetheryte(1280034, AetheryteType.Crystal, new Position(172,-1315f, 57f, -147f, 0f, 0)),            // thanalan_horizon
            new Aetheryte(1280035, AetheryteType.Crystal, new Position(173,-165f, 281f, -1699f, 0f, 0)),           // thanalan_bluefog  

            new Aetheryte(1280121, AetheryteType.Crystal, new Position(190,484f, 19f, 672f, 0f, 0)),               // mordhona_brittlebark
            new Aetheryte(1280122, AetheryteType.Crystal, new Position(190,-400f, 19f, 338f, 0f, 0)),              // mordhona_revenantstoll
            
            //Aetheryte Gates
            new Aetheryte(1280007, AetheryteType.Gate, new Position(128,582.47f, 54.52f, -1.2f, 0f, 0)),        // cedarwood
            new Aetheryte(1280008, AetheryteType.Gate, new Position(128,966f, 50f, 833f, 0f, 0)),               // widowcliffs                          
            new Aetheryte(1280009, AetheryteType.Gate, new Position(128,318f, 25f, 581f, 0f, 0)),               // morabybay
            new Aetheryte(1280010, AetheryteType.Gate, new Position(129,-636f, 50f, -1287f, 0f, 0)),            // woadwhisper
            new Aetheryte(1280011, AetheryteType.Gate, new Position(129,-2018f, 61f, -763f, 0f, 0)),            // islesofumbra
            new Aetheryte(1280012, AetheryteType.Gate, new Position(130,1628f, 62f, -449f, 0f, 0)),             // tigerhelm
            new Aetheryte(1280013, AetheryteType.Gate, new Position(130,1522f, 3f, -669f, 0f, 0)),              // southbloodshore
            new Aetheryte(1280014, AetheryteType.Gate, new Position(130,1410f, 55f, -1650f, 0f, 0)),            // agelysswise
            new Aetheryte(1280015, AetheryteType.Gate, new Position(135,-125f, 61f, -1440f, 0f, 0)),            // zelmasrun
            new Aetheryte(1280016, AetheryteType.Gate, new Position(135,-320f, 53f, -1826f, 0f, 0)),            // bronzelake
            new Aetheryte(1280017, AetheryteType.Gate, new Position(135,-894f, 42f, -2188f, 0f, 0)),            // oakwood
            new Aetheryte(1280018, AetheryteType.Gate, new Position(131,-1694.5f, -19f, -1534f, 0f, 0)),        // mistbeardcove                          
            new Aetheryte(1280020, AetheryteType.Gate, new Position(132,1343.5f, -54.38f, -870.84f, 0f, 0)),    // cassiopeia
            new Aetheryte(1280037, AetheryteType.Gate, new Position(170,639f, 185f, 122f, 0f, 0)),              // cactusbasin
            new Aetheryte(1280038, AetheryteType.Gate, new Position(170,539f, 218f, -14f, 0f, 0)),              // foursisters               
            new Aetheryte(1280039, AetheryteType.Gate, new Position(171,1599f, 259f, -233f, 0f, 0)),            // halatali                          
            new Aetheryte(1280040, AetheryteType.Gate, new Position(171,2010f, 281f, -768f, 0f, 0)),            // burningwall
            new Aetheryte(1280041, AetheryteType.Gate, new Position(171,2015f, 248f, 64f, 0f, 0)),              // sandgate
            new Aetheryte(1280042, AetheryteType.Gate, new Position(172,-866f, 89f, 376f, 0f, 0)),              // nophicaswells
            new Aetheryte(1280043, AetheryteType.Gate, new Position(172,-1653f, 25f, -469f, 0f, 0)),            // footfalls
            new Aetheryte(1280044, AetheryteType.Gate, new Position(172,-1223f, 70f, 191f, 0f, 0)),             // scorpionkeep
            new Aetheryte(1280045, AetheryteType.Gate, new Position(173,-635f, 281f, -1797f, 0f, 0)),           // hiddengorge
            new Aetheryte(1280046, AetheryteType.Gate, new Position(173,447f, 260f, -2158f, 0f, 0)),            // seaofspires
            new Aetheryte(1280047, AetheryteType.Gate, new Position(173,-710f, 281f, -2212f, 0f, 0)),           // cutterspass
            new Aetheryte(1280048, AetheryteType.Gate, new Position(174,1797f, 249f, 1856f, 0f, 0)),            // redlabyrinth
            new Aetheryte(1280049, AetheryteType.Gate, new Position(174,1185f, 280f, 1407f, 0f, 0)),            // burntlizardcreek
            new Aetheryte(1280050, AetheryteType.Gate, new Position(174,2416f, 249f, 1535f, 0f, 0)),            // zanrak
            new Aetheryte(1280052, AetheryteType.Gate, new Position(176,80.5f, 169f, -1268.5f, 0f, 0)),         // nanawamines
            new Aetheryte(1280054, AetheryteType.Gate, new Position(178,-621f, 112f, -118f, 0f, 0)),            // copperbellmines            
            new Aetheryte(1280067, AetheryteType.Gate, new Position(150,-94.07f, 4f, -543.16f, 0f, 0)),         // humblehearth
            new Aetheryte(1280068, AetheryteType.Gate, new Position(150,-285f, -21f, -46f, 0f, 0)),             // sorrelhaven
            new Aetheryte(1280069, AetheryteType.Gate, new Position(150,636f, 17f, -324f, 0f, 0)),              // fivehangs
            new Aetheryte(1280070, AetheryteType.Gate, new Position(151,1529f, 27f, -1147f, 0f, 0)),            // verdantdrop
            new Aetheryte(1280071, AetheryteType.Gate, new Position(151,1296f, 48f, -1534f, 0f, 0)),            // lynxpeltpatch
            new Aetheryte(1280072, AetheryteType.Gate, new Position(151,2297f, 33f, -703f, 0f, 0)),             // larkscall
            new Aetheryte(1280073, AetheryteType.Gate, new Position(152,-888f, 40f, -2192f, 0f, 0)),            // treespeak
            new Aetheryte(1280074, AetheryteType.Gate, new Position(152,-1567f, 17f, -2593f, 0f, 0)),           // aldersprings
            new Aetheryte(1280075, AetheryteType.Gate, new Position(152,-801f, 32f, -2792f, 0f, 0)),            // lasthold
            new Aetheryte(1280076, AetheryteType.Gate, new Position(153,-1908f, 1f, -1042f, 0f, 0)),            // lichenweed
            new Aetheryte(1280077, AetheryteType.Gate, new Position(153,-2158f, -45f, -166f, 0f, 0)),           // mumurrills
            new Aetheryte(1280078, AetheryteType.Gate, new Position(153,-1333f, -13f, 324f, 0f, 0)),            // turningleaf
            new Aetheryte(1280079, AetheryteType.Gate, new Position(154,991f, -11f, 600f, 0f, 0)),              // silentarbor
            new Aetheryte(1280080, AetheryteType.Gate, new Position(154,1126f, 1f, 1440f, 0f, 0)),              // longroot
            new Aetheryte(1280081, AetheryteType.Gate, new Position(154,189f, 1f, 1337f, 0f, 0)),               // snakemolt
            new Aetheryte(1280082, AetheryteType.Gate, new Position(157,-689f, -15f, -2065f, 0f, 0)),           // muntuycellars
            new Aetheryte(1280083, AetheryteType.Gate, new Position(158,313f, -35f, -171f, 0f, 0)),             // tamtaradeeprcroft            
            new Aetheryte(1280097, AetheryteType.Gate, new Position(143,-517f, 210f, 543f, 0f, 0)),             // boulderdowns
            new Aetheryte(1280098, AetheryteType.Gate, new Position(143,190f, 368f, -662f, 0f, 0)),             // prominencepoint
            new Aetheryte(1280099, AetheryteType.Gate, new Position(143,960f, 288f, -22f, 0f, 0)),              // feathergorge
            new Aetheryte(1280100, AetheryteType.Gate, new Position(144,1737f, 177f, -1250f, 0f, 0)),           // maidenglen
            new Aetheryte(1280101, AetheryteType.Gate, new Position(144,1390f, 223f, -736f, 0f, 0)),            // hushedboughs
            new Aetheryte(1280102, AetheryteType.Gate, new Position(144,1788f, 166f, -829f, 0f, 0)),            // scarwingfall
            new Aetheryte(1280103, AetheryteType.Gate, new Position(145,1383f, 232f, 422f, 0f, 0)),             // weepingvale
            new Aetheryte(1280104, AetheryteType.Gate, new Position(145,2160f, 143f, 622f, 0f, 0)),             // clearwater
            new Aetheryte(1280105, AetheryteType.Gate, new Position(147,-1f, 145f, 1373f, 0f, 0)),              // teriggansstand
            new Aetheryte(1280106, AetheryteType.Gate, new Position(147,-64f, 186f, 1924f, 0f, 0)),             // shepherdpeak
            new Aetheryte(1280107, AetheryteType.Gate, new Position(147,-908f, 192f, 2162f, 0f, 0)),            // fellwood
            new Aetheryte(1280108, AetheryteType.Gate, new Position(148,-1738f, 286f, -844f, 0f, 0)),           // wyrmkingspearch
            new Aetheryte(1280109, AetheryteType.Gate, new Position(148,-2366f, 337f, -1058f, 0f, 0)),          // lance
            new Aetheryte(1280110, AetheryteType.Gate, new Position(148,-2821f, 257f, -290f, 0f, 0)),           // twinpools             
            new Aetheryte(1280123, AetheryteType.Gate, new Position(190,-458f, -40f, -318f, 0f, 0)),            // fogfens
            new Aetheryte(1280124, AetheryteType.Gate, new Position(190,580f, 59f, 206f, 0f, 0)),               // singingshards
            new Aetheryte(1280125, AetheryteType.Gate, new Position(190,-365f, -13f, -37f, 0f, 0)),             // jaggedcrestcave          
            
            //Aetheryte shards in cities
            new Aetheryte(1200288, AetheryteType.Shard, new Position(206,-112.1921f, 16.274f, -1337.151f, 0f, 0)),    // gridania_
        };

        public List<Aetheryte> GetZoneAetherytes(uint zoneId) => Aetherytes.FindAll(x => x.Position.ZoneId == zoneId);

        public List<Zone> ZoneList()
        {
            return new List<Zone>
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
                new Zone(208, 201, locationNameId: 58, classNameId: 14, musicSetId: 0, mapName: "prv0Cottage00"),
                new Zone(209, 244, locationNameId: 55, classNameId: 19, musicSetId: 15, mapName: "prv0Inn01"),
                new Zone(805, 200, locationNameId: 56, classNameId: 13, musicSetId: 10, mapName: "sea1Cruise01"),
            };
        }
    }
}
