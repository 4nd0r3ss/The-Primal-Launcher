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
        
        public List<Zone> Zones { get; set; }
        public List<Aetheryte> Aetherytes { get; set; }

        public static ActorRepository Instance
        {
            get
            {              
              if (_instance == null)
                  _instance = new ActorRepository();

              return _instance;   
            }
        }

        public ActorRepository()
        {
            ZoneList zoneList = new ZoneList();
            Zones = zoneList.Zones;

                    
            //Aetherytes = aetheryteList.AllAetherytes;

            //add aetherytes to zone actor list
            foreach (Zone z in Zones)
                z.Actors.AddRange(AllAetherytes.Where(x => x.Position.ZoneId == z.Id));
        }

        public List<Aetheryte> AllAetherytes = new List<Aetheryte>()
        {
            //Red Aetherytes
            //new Aetheryte(1280127, 20902, new Position(0,-400f, 19f, 338f, 0f, 0), 2048),                  // 
            //new Aetheryte(1280057, 20902, new Position(0,33f, 201f, -482f, 0f, 0), 2048),                  // 
            //new Aetheryte(1280058, 20902, new Position(0,-1315f, 57f, -147f, 0f, 0), 2048),                // 
            //new Aetheryte(1280059, 20902, new Position(0,-165f, 281f, -1699f, 0f, 0), 2048),               //
            //new Aetheryte(1280088, 20902, new Position(0,-1054f, 21f, -1761f, 0f, 0), 2048),               // 
            //new Aetheryte(1280089, 20902, new Position(0,-1568f, -11f, -552f, 0f, 0), 2048),               // 
            //new Aetheryte(1280117, 20902, new Position(0,216f, 303f, -258f, 0f, 0), 2048),                 // 
            //new Aetheryte(1280118, 20902, new Position(0,1498f, 207f, 767f, 0f, 0), 2048),                 // 
            //new Aetheryte(1280119, 20902, new Position(0,-163f, 223f, 1151f, 0f, 0), 2048),                //
            //new Aetheryte(1280126, 20902, new Position(0,484f, 19f, 672f, 0f, 0), 2048),                   // 
            //new Aetheryte(1280127, 20902, new Position(0,-400f, 19f, 338f, 0f, 0), 2048),                  // 

            //City State Aetherytes            
            new Aetheryte(1280001, 20902, new Position(230,-395.1f, 42.5f, 337.12f, 0f, 0)),       // limsa_aetheryte
            new Aetheryte(1280061, 20902, new Position(206,-130.63f, 16.08f, -1323.99f, 0f, 0)),   // gridania_aetheryte
            new Aetheryte(1280031, 20902, new Position(175,-240.45f, 185.93f, -9.56f, 0f, 0)),     // uldah_aetheryte 

            //Camp Aetherytes
            new Aetheryte(1280002, 20902, new Position(128,29.97f, 45.83f, -35.47f, 0f, 0)),       // beardedrock_aetheryte
            new Aetheryte(1280003, 20902, new Position(129,-991.88f, 61.71f, -1120.79f, 0f, 0)),   // skullvalley_aetheryte
            new Aetheryte(1280032, 20902, new Position(170,33f, 201f, -482f, 0f, 0)),              // blackbrush_aetheryte
            new Aetheryte(1280036, 20902, new Position(174,1686f, 297f, 995f, 0f, 0)),             // brokenwater_aetheryte   
            new Aetheryte(1280004, 20902, new Position(129,-1883.47f, 53.77f, -1372.68f, 0f, 0)),  // baldknoll_aetheryte
            new Aetheryte(1280005, 20902, new Position(130,1123.29f, 45.7f, -928.69f, 0f, 0)),     // bloodshore_aetheryte
            new Aetheryte(1280006, 20902, new Position(135,-278.181f, 77.63f, -2260.79f, 0f, 0)),  // ironlake_aetheryte
            new Aetheryte(1280033, 20902, new Position(171,1250.9f, 264f, -544.2f, 0f, 0)),        // drybone_aetheryte
            new Aetheryte(1280034, 20902, new Position(172,-1315f, 57f, -147f, 0f, 0)),            // horizon_aetheryte
            new Aetheryte(1280035, 20902, new Position(173,-165f, 281f, -1699f, 0f, 0)),           // bluefog_aetheryte
            new Aetheryte(1280062, 20902, new Position(150,288f, 4f, -543.928f, 0f, 0)),           // bentbranch_aetheryte
            new Aetheryte(1280063, 20902, new Position(151,1702f, 20f, -862f, 0f, 0)),             // nineivies_aetheryte
            new Aetheryte(1280092, 20902, new Position(143,216f, 303f, -258f, 0f, 0)),             // dragonhead_aetheryte
            new Aetheryte(1280093, 20902, new Position(144,1122f, 271f, -1149f, 0f, 0)),           // crookedfork_aetheryte
            new Aetheryte(1280094, 20902, new Position(145,1498f, 207f, 767f, 0f, 0)),             // glory_aetheryte
            new Aetheryte(1280095, 20902, new Position(147,-163f, 223f, 1151f, 0f, 0)),            // everlakes_aetheryte
            new Aetheryte(1280096, 20902, new Position(148,-1761f, 270f, -198f, 0f, 0)),           // riversmeet_aetheryte
            new Aetheryte(1280064, 20902, new Position(152,-1052f, 20f, -1760f, 0f, 0)),           // emeraldmoss_aetheryte
            new Aetheryte(1280065, 20902, new Position(153,-1566.04f, -11.89f, -550.51f, 0f, 0)),  // crimsonbark_aetheryte
            new Aetheryte(1280066, 20902, new Position(154,734f, -12f, 1126f, 0f, 0)),             // tranquil_aetheryte
            
            //Aetheryte Gates
            new Aetheryte(1280007, 20925, new Position(128,582.47f, 54.52f, -1.2f, 0f, 0)),        // cedarwood_aetherytegate
            new Aetheryte(1280008, 20925, new Position(128,966f, 50f, 833f, 0f, 0)),               // widowcliffs_aetherytegate                          
            new Aetheryte(1280009, 20925, new Position(128,318f, 25f, 581f, 0f, 0)),               // morabybay_aetherytegate
            new Aetheryte(1280010, 20925, new Position(129,-636f, 50f, -1287f, 0f, 0)),            // woadwhisper_aetherytegate
            new Aetheryte(1280011, 20925, new Position(129,-2018f, 61f, -763f, 0f, 0)),            // islesofumbra_aetherytegate
            new Aetheryte(1280012, 20925, new Position(130,1628f, 62f, -449f, 0f, 0)),             // tigerhelm_aetherytegate
            new Aetheryte(1280013, 20925, new Position(130,1522f, 3f, -669f, 0f, 0)),              // southbloodshore_aetherytegate
            new Aetheryte(1280014, 20925, new Position(130,1410f, 55f, -1650f, 0f, 0)),            // agelysswise_aetherytegate
            new Aetheryte(1280015, 20925, new Position(135,-125f, 61f, -1440f, 0f, 0)),            // zelmasrun_aetherytegate
            new Aetheryte(1280016, 20925, new Position(135,-320f, 53f, -1826f, 0f, 0)),            // bronzelake_aetherytegate
            new Aetheryte(1280017, 20925, new Position(135,-894f, 42f, -2188f, 0f, 0)),            // oakwood_aetherytegate
            new Aetheryte(1280018, 20925, new Position(131,-1694.5f, -19f, -1534f, 0f, 0)),        // mistbeardcove_aetherytegate                          
            new Aetheryte(1280020, 20925, new Position(132,1343.5f, -54.38f, -870.84f, 0f, 0)),    // cassiopeia_aetherytegate
            new Aetheryte(1280037, 20925, new Position(170,639f, 185f, 122f, 0f, 0)),              // cactusbasin_aetherytegate
            new Aetheryte(1280038, 20925, new Position(170,539f, 218f, -14f, 0f, 0)),              // foursisters_aetherytegate               
            new Aetheryte(1280039, 20925, new Position(171,1599f, 259f, -233f, 0f, 0)),            // halatali_aetherytegate                          
            new Aetheryte(1280040, 20925, new Position(171,2010f, 281f, -768f, 0f, 0)),            // burningwall_aetherytegate
            new Aetheryte(1280041, 20925, new Position(171,2015f, 248f, 64f, 0f, 0)),              // sandgate_aetherytegate
            new Aetheryte(1280042, 20925, new Position(172,-866f, 89f, 376f, 0f, 0)),              // nophicaswells_aetherytegate
            new Aetheryte(1280043, 20925, new Position(172,-1653f, 25f, -469f, 0f, 0)),            // footfalls_aetherytegate
            new Aetheryte(1280044, 20925, new Position(172,-1223f, 70f, 191f, 0f, 0)),             // scorpionkeep_aetherytegate
            new Aetheryte(1280045, 20925, new Position(173,-635f, 281f, -1797f, 0f, 0)),           // hiddengorge_aetherytegate
            new Aetheryte(1280046, 20925, new Position(173,447f, 260f, -2158f, 0f, 0)),            // seaofspires_aetherytegate
            new Aetheryte(1280047, 20925, new Position(173,-710f, 281f, -2212f, 0f, 0)),           // cutterspass_aetherytegate
            new Aetheryte(1280048, 20925, new Position(174,1797f, 249f, 1856f, 0f, 0)),            // redlabyrinth_aetherytegate
            new Aetheryte(1280049, 20925, new Position(174,1185f, 280f, 1407f, 0f, 0)),            // burntlizardcreek_aetherytegate
            new Aetheryte(1280050, 20925, new Position(174,2416f, 249f, 1535f, 0f, 0)),            // zanrak_aetherytegate
            new Aetheryte(1280052, 20925, new Position(176,80.5f, 169f, -1268.5f, 0f, 0)),         // nanawamines_aetherytegate
            new Aetheryte(1280054, 20925, new Position(178,-621f, 112f, -118f, 0f, 0)),            // copperbellmines_aetherytegate            
            new Aetheryte(1280067, 20925, new Position(150,-94.07f, 4f, -543.16f, 0f, 0)),         // humblehearth_aetherytegate
            new Aetheryte(1280068, 20925, new Position(150,-285f, -21f, -46f, 0f, 0)),             // sorrelhaven_aetherytegate
            new Aetheryte(1280069, 20925, new Position(150,636f, 17f, -324f, 0f, 0)),              // fivehangs_aetherytegate
            new Aetheryte(1280070, 20925, new Position(151,1529f, 27f, -1147f, 0f, 0)),            // verdantdrop_aetherytegate
            new Aetheryte(1280071, 20925, new Position(151,1296f, 48f, -1534f, 0f, 0)),            // lynxpeltpatch_aetherytegate
            new Aetheryte(1280072, 20925, new Position(151,2297f, 33f, -703f, 0f, 0)),             // larkscall_aetherytegate
            new Aetheryte(1280073, 20925, new Position(152,-888f, 40f, -2192f, 0f, 0)),            // treespeak_aetherytegate
            new Aetheryte(1280074, 20925, new Position(152,-1567f, 17f, -2593f, 0f, 0)),           // aldersprings_aetherytegate
            new Aetheryte(1280075, 20925, new Position(152,-801f, 32f, -2792f, 0f, 0)),            // lasthold_aetherytegate
            new Aetheryte(1280076, 20925, new Position(153,-1908f, 1f, -1042f, 0f, 0)),            // lichenweed_aetherytegate
            new Aetheryte(1280077, 20925, new Position(153,-2158f, -45f, -166f, 0f, 0)),           // mumurrills_aetherytegate
            new Aetheryte(1280078, 20925, new Position(153,-1333f, -13f, 324f, 0f, 0)),            // turningleaf_aetherytegate
            new Aetheryte(1280079, 20925, new Position(154,991f, -11f, 600f, 0f, 0)),              // silentarbor_aetherytegate
            new Aetheryte(1280080, 20925, new Position(154,1126f, 1f, 1440f, 0f, 0)),              // longroot_aetherytegate
            new Aetheryte(1280081, 20925, new Position(154,189f, 1f, 1337f, 0f, 0)),               // snakemolt_aetherytegate
            new Aetheryte(1280082, 20925, new Position(157,-689f, -15f, -2065f, 0f, 0)),           // muntuycellars_aetherytegate
            new Aetheryte(1280083, 20925, new Position(158,313f, -35f, -171f, 0f, 0)),             // tamtaradeeprcroft_aetherytegate            
            new Aetheryte(1280097, 20925, new Position(143,-517f, 210f, 543f, 0f, 0)),             // boulderdowns_aetherytegate
            new Aetheryte(1280098, 20925, new Position(143,190f, 368f, -662f, 0f, 0)),             // prominencepoint_aetherytegate
            new Aetheryte(1280099, 20925, new Position(143,960f, 288f, -22f, 0f, 0)),              // feathergorge_aetherytegate
            new Aetheryte(1280100, 20925, new Position(144,1737f, 177f, -1250f, 0f, 0)),           // maidenglen_aetherytegate
            new Aetheryte(1280101, 20925, new Position(144,1390f, 223f, -736f, 0f, 0)),            // hushedboughs_aetherytegate
            new Aetheryte(1280102, 20925, new Position(144,1788f, 166f, -829f, 0f, 0)),            // scarwingfall_aetherytegate
            new Aetheryte(1280103, 20925, new Position(145,1383f, 232f, 422f, 0f, 0)),             // weepingvale_aetherytegate
            new Aetheryte(1280104, 20925, new Position(145,2160f, 143f, 622f, 0f, 0)),             // clearwater_aetherytegate
            new Aetheryte(1280105, 20925, new Position(147,-1f, 145f, 1373f, 0f, 0)),              // teriggansstand_aetherytegate
            new Aetheryte(1280106, 20925, new Position(147,-64f, 186f, 1924f, 0f, 0)),             // shepherdpeak_aetherytegate
            new Aetheryte(1280107, 20925, new Position(147,-908f, 192f, 2162f, 0f, 0)),            // fellwood_aetherytegate
            new Aetheryte(1280108, 20925, new Position(148,-1738f, 286f, -844f, 0f, 0)),           // wyrmkingspearch_aetherytegate
            new Aetheryte(1280109, 20925, new Position(148,-2366f, 337f, -1058f, 0f, 0)),          // lance_aetherytegate
            new Aetheryte(1280110, 20925, new Position(148,-2821f, 257f, -290f, 0f, 0)),           // twinpools_aetherytegate             
            new Aetheryte(1280121, 20902, new Position(190,484f, 19f, 672f, 0f, 0)),               // camp_brittlebark_aetheryte
            new Aetheryte(1280122, 20902, new Position(190,-400f, 19f, 338f, 0f, 0)),              // camp_revenantstoll_aetheryte
            new Aetheryte(1280123, 20925, new Position(190,-458f, -40f, -318f, 0f, 0)),            // fogfens_aetherytegate
            new Aetheryte(1280124, 20925, new Position(190,580f, 59f, 206f, 0f, 0)),               // singingshards_aetherytegate
            new Aetheryte(1280125, 20925, new Position(190,-365f, -13f, -37f, 0f, 0)),             // jaggedcrestcave_aetherytegate            
        };
    }
}
