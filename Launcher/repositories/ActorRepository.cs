using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrimalLauncher
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
            
            //Aetherytes - grouped by teleport menu pages
            new Aetheryte(1280001, AetheryteType.Crystal, new Position(230,-395.1f, 42.5f, 337.12f, 0f, 0), 1, 1),       // lanoscea_limsa
            new Aetheryte(1280002, AetheryteType.Crystal, new Position(128,29.97f, 45.83f, -35.47f, 0f, 0), 1, 2),       // lanoscea_beardedrock
            new Aetheryte(1280003, AetheryteType.Crystal, new Position(129,-991.88f, 61.71f, -1120.79f, 0f, 0), 1, 3),   // lanoscea_skullvalley
            new Aetheryte(1280004, AetheryteType.Crystal, new Position(129,-1883.47f, 53.77f, -1372.68f, 0f, 0), 1, 4),  // lanoscea_baldknoll
            new Aetheryte(1280005, AetheryteType.Crystal, new Position(130,1123.29f, 45.7f, -928.69f, 0f, 0), 1, 5),     // lanoscea_bloodshore
            new Aetheryte(1280006, AetheryteType.Crystal, new Position(135,-278.181f, 77.63f, -2260.79f, 0f, 0), 1, 6),  // lanoscea_ironlake

            new Aetheryte(1280092, AetheryteType.Crystal, new Position(143,216f, 303f, -258f, 0f, 0), 2, 1),             // coerthas_dragonhead
            new Aetheryte(1280093, AetheryteType.Crystal, new Position(144,1122f, 271f, -1149f, 0f, 0), 2, 2),           // coerthas_crookedfork
            new Aetheryte(1280094, AetheryteType.Crystal, new Position(145,1498f, 207f, 767f, 0f, 0), 2, 3),             // coerthas_glory
            new Aetheryte(1280095, AetheryteType.Crystal, new Position(147,-163f, 223f, 1151f, 0f, 0), 2, 4),            // coerthas_everlakes
            new Aetheryte(1280096, AetheryteType.Crystal, new Position(148,-1761f, 270f, -198f, 0f, 0), 2, 5),           // coerthas_riversmeet

            new Aetheryte(1280061, AetheryteType.Crystal, new Position(206,-130.63f, 16.08f, -1323.99f, 0f, 0), 3, 1),   // blackshroud_gridania
            new Aetheryte(1280062, AetheryteType.Crystal, new Position(150,288f, 4f, -543.928f, 0f, 0), 3, 2),           // blackshroud_bentbranch
            new Aetheryte(1280063, AetheryteType.Crystal, new Position(151,1702f, 20f, -862f, 0f, 0), 3, 3),             // blackshroud_nineivies
            new Aetheryte(1280064, AetheryteType.Crystal, new Position(152,-1052f, 20f, -1760f, 0f, 0), 3, 4),           // blackshroud_emeraldmoss
            new Aetheryte(1280065, AetheryteType.Crystal, new Position(153,-1566.04f, -11.89f, -550.51f, 0f, 0), 3, 5),  // blackshroud_crimsonbark
            new Aetheryte(1280066, AetheryteType.Crystal, new Position(154,734f, -12f, 1126f, 0f, 0), 3, 6),             // blackshroud_tranquil

            new Aetheryte(1280031, AetheryteType.Crystal, new Position(175,-240.45f, 185.93f, -9.56f, 0f, 0), 4, 1),     // thanalan_uldah
            new Aetheryte(1280032, AetheryteType.Crystal, new Position(170,33f, 201f, -482f, 0f, 0), 4, 2),              // thanalan_blackbrush
            new Aetheryte(1280033, AetheryteType.Crystal, new Position(171,1250.9f, 264f, -544.2f, 0f, 0), 4, 3),        // thanalan_drybone
            new Aetheryte(1280034, AetheryteType.Crystal, new Position(172,-1315f, 57f, -147f, 0f, 0), 4, 4),            // thanalan_horizon
            new Aetheryte(1280035, AetheryteType.Crystal, new Position(173,-165f, 281f, -1699f, 0f, 0), 4, 5),           // thanalan_bluefog
            new Aetheryte(1280036, AetheryteType.Crystal, new Position(174,1686f, 297f, 995f, 0f, 0), 4, 6),             // thanalan_brokenwater      

            new Aetheryte(1280121, AetheryteType.Crystal, new Position(190,484f, 19f, 672f, 0f, 0), 5, 1),               // mordhona_brittlebark
            new Aetheryte(1280122, AetheryteType.Crystal, new Position(190,-400f, 19f, 338f, 0f, 0), 5, 2),              // mordhona_revenantstoll
            
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

        public List<Actor> GetZoneNpcs(uint zoneId, string excludeOfType = "")
        {
            string npcListPath = @"" + zoneId.ToString("X") ;
            string fileNamePath = npcListPath + @".npc.xml";
            XmlDocument npcFile = new XmlDocument();
            string file = GetXmlResource(fileNamePath);
            List<Actor> zoneNpcs = new List<Actor>();           

            if (file != "")
            {
                try
                {
                    //get game data tables with actors data
                    DataTable actorsGraphics = GameData.Instance.GetGameData("actorclass_graphic");
                    DataTable actorsNameIds = GameData.Instance.GetGameData("actorclass");
                    DataTable actorsNames = GameData.Instance.GetGameData("xtx/displayName");

                    //prepare xml nodes
                    npcFile.LoadXml(file);
                    XmlElement root = npcFile.DocumentElement;
                    XmlNode chara = root.FirstChild;                    

                    //each npc node in xml 
                    foreach (XmlNode node in chara.ChildNodes)
                    {
                        if (node.Name == excludeOfType) continue;

                        Type type = Type.GetType("PrimalLauncher." + node.Name);

                        if (type != null 
                            && (node.Attributes["className"].Value == "PopulaceStandard" || type.Name == "Monster" || type.Name == "Object")
                        )
                        {
                            //XmlNode node = objNode.SelectSingleNode("PopulaceStandard");
                            uint classId = Convert.ToUInt32(node.SelectSingleNode("classId").InnerText);
                            uint state = node.SelectSingleNode("state") != null ? Convert.ToUInt32(node.SelectSingleNode("state").InnerText) : 0; //TODO: fix this as it is 2 bytes. so far it's alaways 0 so it's ok.
                            ushort animation = node.SelectSingleNode("animation") != null ? Convert.ToUInt16(node.SelectSingleNode("animation").InnerText) : (ushort)0;
                            int questIcon = node.SelectSingleNode("questIcon") != null ? Convert.ToInt32(node.SelectSingleNode("questIcon").InnerText) : -1;
                            string talkFunction = node.SelectSingleNode("talkFunction") != null ? node.SelectSingleNode("talkFunction").InnerText : "";

                            //get table lines with npc data
                            //had to separate the selects as it throws an exception when the actor have no appearance and/or nameid data.
                            DataRow[] actorsGraphicsSelect = actorsGraphics.Select("id = '" + classId + "'");
                            DataRow[] actorsNameIdsSelect = actorsNameIds.Select("id = '" + classId + "'");
                            DataRow actorGraphics = actorsGraphicsSelect != null && actorsGraphicsSelect.Length > 0 ? actorsGraphicsSelect[0] : null;
                            DataRow actorNameId = actorsNameIdsSelect != null && actorsNameIdsSelect.Length > 0 ? actorsNameIdsSelect[0] : null;

                            Actor actor = (Actor)Activator.CreateInstance(type);

                            //if there is a talk function node, it will just add the function and skip the block below. Useful for cases when the function name
                            //does not match the NPC name, like 'Noncomenanco has a typo in the function name, Noncomananco
                            if (string.IsNullOrEmpty(talkFunction) && classId < 3000000 && actorNameId != null && (int)actorNameId.ItemArray[1] > 0) //< 3000000 is NPC. > is monster.
                            {
                                DataRow displayNameRow = actorsNames.Select("id = '" + actorNameId.ItemArray[1] + "'")[0];
                                string displayName = (displayNameRow.ItemArray[1] + ""); //just to parse to string.
                                displayName = displayName
                                    .Replace(" ", "")
                                    .Replace("`", "")
                                    .Replace("\0", "")
                                    .Replace("'", "");
                                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1, displayName.Length - 1).ToLower();

                                talkFunction = "DelegateEvent:defaultTalkWith" + displayName + "_001";
                            }

                            actor.Family = node.Attributes["family"] != null ? node.Attributes["family"].Value : "";
                            actor.ClassId = classId;
                            actor.ClassName = node.Attributes["className"].Value;
                            actor.NameId = actorNameId != null ? Convert.ToInt32(actorNameId.ItemArray[1]) : 0;                            
                            actor.Appearance = SetAppearance(actorGraphics);                            
                            actor.Position = SetPosition(zoneId, node.SelectSingleNode("position"));
                            actor.QuestIcon = questIcon;
                            actor.SubState = new SubState { MotionPack = animation };
                            actor.Events = SetEvents(node.SelectSingleNode("events"));
                            actor.TalkFunction = talkFunction;                            
                           
                            zoneNpcs.Add(actor);
                        }                        
                    }
                }
                catch (Exception e)
                {
                    _log.Warning(e.Message);
                }
            }

            return zoneNpcs;
        }

        private List<Event> SetEvents(XmlNode eventNode)
        {
            List<Event> eventList = new List<Event>();

            if (eventNode != null && eventNode.ChildNodes != null && eventNode.ChildNodes.Count > 0)
            {              
                foreach (XmlNode node in eventNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "pushDefault":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.PushEventCircle,
                                Name = "pushDefault",
                                Radius = float.Parse(node.Attributes["radius"].Value),
                                Direction = Convert.ToByte(node.Attributes["outwards"].Value == "false" ? 1 : 0),
                                Silent = Convert.ToByte(node.Attributes["silent"].Value == "false" ? 0 : 1)
                            });
                            break;
                        case "talkDefault":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.TalkEvent,
                                Name = "talkDefault",
                                Priority = Convert.ToByte(node.Attributes["priority"].Value),
                                Enabled = Convert.ToByte(node.Attributes["enabled"].Value)
                            });
                            break;

                        case "noticeEvent":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.NoticeEvent,
                                Name = "noticeEvent",
                                Priority = Convert.ToByte(node.Attributes["priority"].Value),
                                Enabled = Convert.ToByte(node.Attributes["enabled"].Value),
                                Silent = node.Attributes["silent"] != null ? Convert.ToByte(node.Attributes["silent"].Value) : (byte)0
                            });
                            break;
                        case "in":
                            //pushWithBoxEventConditions
                            break;
                        case "exit":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.PushEventCircle,
                                Name = "exit",
                                Radius = node.Attributes["radius"] != null ? float.Parse(node.Attributes["radius"].Value) : 0,
                                Silent = Convert.ToByte(node.Attributes["silent"].Value == "false" ? 0 : 1),
                                Enabled = Convert.ToByte(node.Attributes["enabled"].Value),
                                Direction = Convert.ToByte(node.Attributes["outwards"].Value == "false" ? 1 : 0x11),
                                Action = node.Attributes["action"] != null ? node.Attributes["action"].Value : ""
                            });
                            break;
                        case "caution":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.PushEventCircle,
                                Name = "caution",
                                Radius = node.Attributes["radius"] != null ? float.Parse(node.Attributes["radius"].Value) : 0,
                                Silent = Convert.ToByte(node.Attributes["silent"].Value == "false" ? 0 : 1),
                                Enabled = Convert.ToByte(node.Attributes["enabled"].Value),
                                Direction = Convert.ToByte(node.Attributes["outwards"].Value == "false" ? 1 : 0x11),
                                Action = node.Attributes["action"] != null ? node.Attributes["action"].Value : ""
                            });
                            break;
                        case "pushCommand":

                            break;
                        case "pushCommandIn":

                            break;
                        case "pushCommandOut":

                            break;
                        default:
                            Log.Instance.Warning("ActorRepository.SetEvents: Unhandled event '" + node.Name + "' received.");
                            break;
                    }
                }
            }            

            return eventList;
        }

        private Appearance SetAppearance(DataRow actorGraphics)
        {
            if(actorGraphics != null)
            {
                return new Appearance
                {
                    BaseModel = Convert.ToUInt32(actorGraphics.ItemArray[1]),
                    Size = Convert.ToUInt32(actorGraphics.ItemArray[2]),
                    MainWeapon = Convert.ToUInt32(actorGraphics.ItemArray[20]),
                    SecondaryWeapon = Convert.ToUInt32(actorGraphics.ItemArray[21]),
                    SPMainWeapon = Convert.ToUInt32(actorGraphics.ItemArray[22]),
                    SPSecondaryWeapon = Convert.ToUInt32(actorGraphics.ItemArray[23]),
                    Throwing = Convert.ToUInt32(actorGraphics.ItemArray[24]),
                    Pack = Convert.ToUInt32(actorGraphics.ItemArray[25]),
                    Pouch = Convert.ToUInt32(actorGraphics.ItemArray[26]),
                    Head = Convert.ToUInt32(actorGraphics.ItemArray[27]),
                    Body = Convert.ToUInt32(actorGraphics.ItemArray[28]),
                    Legs = Convert.ToUInt32(actorGraphics.ItemArray[29]),
                    Hands = Convert.ToUInt32(actorGraphics.ItemArray[30]),
                    Feet = Convert.ToUInt32(actorGraphics.ItemArray[31]),
                    Waist = Convert.ToUInt32(actorGraphics.ItemArray[32]),
                    Neck = Convert.ToUInt32(actorGraphics.ItemArray[33]),
                    RightEar = Convert.ToUInt32(actorGraphics.ItemArray[34]),
                    LeftEar = Convert.ToUInt32(actorGraphics.ItemArray[35]),
                    RightIndex = Convert.ToUInt32(actorGraphics.ItemArray[36]),
                    LeftIndex = Convert.ToUInt32(actorGraphics.ItemArray[37]),
                    RightFinger = Convert.ToUInt32(actorGraphics.ItemArray[38]),
                    LeftFinger = Convert.ToUInt32(actorGraphics.ItemArray[39]),
                    Voice = Convert.ToUInt32(actorGraphics.ItemArray[18]),
                    HairStyle = Convert.ToUInt16(actorGraphics.ItemArray[3]),
                    HairHighlightColor = Convert.ToUInt16(actorGraphics.ItemArray[4]),
                    HairColor = Convert.ToUInt16(actorGraphics.ItemArray[16]),
                    SkinColor = Convert.ToUInt16(actorGraphics.ItemArray[17]),
                    EyeColor = Convert.ToUInt16(actorGraphics.ItemArray[18]),
                    Face = SetFace(actorGraphics)
                };
            }
            else
            {
                //this is necessary for actors who have no appearance data.
                return new Appearance
                {
                    Face = new Face()
                };
            }            
        }

        private Face SetFace(DataRow actorGraphics)
        {
            return new Face
            {
                Characteristics = Convert.ToByte(actorGraphics.ItemArray[7]),
                CharacteristicsColor = Convert.ToByte(actorGraphics.ItemArray[8]),
                Type = Convert.ToByte(actorGraphics.ItemArray[6]),
                Ears = Convert.ToByte(actorGraphics.ItemArray[15]),
                Mouth = Convert.ToByte(actorGraphics.ItemArray[14]),
                Features = Convert.ToByte(actorGraphics.ItemArray[13]),
                Nose = Convert.ToByte(actorGraphics.ItemArray[12]),
                EyeShape = Convert.ToByte(actorGraphics.ItemArray[11]),
                IrisSize = Convert.ToByte(actorGraphics.ItemArray[10]),
                EyeBrows = Convert.ToByte(actorGraphics.ItemArray[9])
            };
        }

        private Position SetPosition(uint zoneId, XmlNode positionNode)
        {
            return new Position
            {
                ZoneId = zoneId,
                X = Convert.ToSingle(positionNode.Attributes["x"].Value),
                Y = Convert.ToSingle(positionNode.Attributes["y"].Value),
                Z = Convert.ToSingle(positionNode.Attributes["z"].Value),
                R = Convert.ToSingle(positionNode.Attributes["r"].Value)
            };
        }

        /// <summary>
        /// Get the contents of a XML file configured as a resource.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetXmlResource(string fileName)
        {
            //From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
            string result = string.Empty;
            Stream stream = typeof(ActorRepository).Assembly.GetManifestResourceStream("Launcher.Resources.xml.zones.x" + fileName);
            if(stream != null)
                using (stream)            
                    using (StreamReader sr = new StreamReader(stream))                
                        result = sr.ReadToEnd();               
           
            return result;
        }
    }
}
