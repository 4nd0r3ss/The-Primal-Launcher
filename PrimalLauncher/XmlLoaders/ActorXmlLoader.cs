/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Xml;
using System.Xml.Linq;

namespace PrimalLauncher
{
    public class ActorXmlLoader
    {
        public static List<Aetheryte> GetAetherytes()
        {
            XmlDocument aetheryteFile = new XmlDocument();
            aetheryteFile.LoadFromResource("AetheryteList.xml");
            List<Aetheryte> result = new List<Aetheryte>();        

            if (aetheryteFile.HasChildNodes)
            {
                try
                {
                    XmlElement root = aetheryteFile.DocumentElement;
                    XmlNode list = root;
                   
                    foreach (XmlNode node in list.ChildNodes)
                        result.Add(new Aetheryte(node));                    
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }            

            return result;
        } 

        public static List<Actor> GetZoneNpcs(uint zoneId, string fileName = "npc.xml")
        {
            //in case file name is passed null or empty for some reason.
            if (string.IsNullOrEmpty(fileName))
                fileName = "npc.xml";

            string zoneDir = @"" + zoneId.ToString("X") ;       
            string fileNamePath = zoneDir + "." + fileName;
            List<Actor> zoneNpcs = new List<Actor>();
            XmlDocument npcFile = new XmlDocument();
            npcFile.LoadFromResource("zones.x" + fileNamePath);

            if (npcFile.HasChildNodes)
            {
                try
                {          
                    XmlElement root = npcFile.DocumentElement;
                    XmlNode chara = root.FirstChild;
                    
                    foreach (XmlNode node in chara.ChildNodes)
                    {                       
                        Actor actor = LoadActor(node, zoneId);

                        if(actor != null)
                            zoneNpcs.Add(actor);
                    }                                                    
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }

            return zoneNpcs;
        }

        public static List<Actor> GetCompanyWarp(uint zoneId)
        {
            List<Actor> npcs = new List<Actor>();
            XmlDocument npcFile = new XmlDocument();
            npcFile.LoadFromResource("CompanyWarp.xml");            

            if (npcFile.HasChildNodes)
            {
                try
                {                    
                    XmlElement root = npcFile.DocumentElement;
                    XmlNode chara = root.FirstChild;
                    
                    foreach (XmlNode node in chara.ChildNodes)
                    {
                        if (Convert.ToUInt32(node.Attributes["zone"].Value) == zoneId)
                        {                            
                            CompanyWarp companyWarp = (CompanyWarp)LoadActor(node, zoneId);
                            companyWarp.Region = Convert.ToUInt32(node.Attributes["region"].Value);
                            companyWarp.Zone = Convert.ToUInt32(node.Attributes["zone"].Value);
                            companyWarp.WarpId = Convert.ToInt32(node.Attributes["id"].Value);
                            npcs.Add(companyWarp);
                        }
                    }                        
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }

            return npcs;
        }

        public static Actor LoadActor(XmlNode node, uint zoneId = 0)
        {         
            Type type = Type.GetType("PrimalLauncher." + node.Name);            

            string className = node.Attributes["className"] != null ? node.Attributes["className"].Value : "";

            if (type != null
                && (
                className == "" ||
                className == "PopulaceCutScenePlayer" ||
                className == "PopulaceStandard" ||
                className == "PopulaceCompanyWarp" ||
                className == "PopulaceFlyingShip" ||
                className == "PopulaceLinkshellManager" ||
                className == "PopulacePassiveGLPublisher" ||
                className == "RetainerFurniture" ||
                className == "PopulaceGuildlevePublisher" ||
                className == "PopulaceRetainerManager" ||
                className == "MapObjShipPort" ||
                className == "PopulaceSpecialEventCryer" ||
                className == "PopulaceBlackMarketeer" ||
                className == "PopulaceCompanySupply" ||
                className == "PopulaceCompanyShop" ||
                className == "ObjectItemStorage" ||
                type.Name == "Monster" ||
                type.Name == "Object" ||
                type.Name == "MapObj" ||
                type.Name == "ElevatorStandard" ||
                type.Name == "RetainerFurniture" ||
                type.Name == "PopulaceFlyingShip" ||
                type.Name == "MarketEntrance" ||
                type.Name == "PopulaceCompanyGLPublisher"
                )
            )
            {
                uint classId = Convert.ToUInt32(node.SelectSingleNode("classId").InnerText);
                uint state = node.GetNodeAsUint("state"); //TODO: fix this as it is 2 bytes. so far it's alaways 0 so it's ok.
                ushort animation = node.GetNodeAsUshort("animation");
                int questIcon = node.GetNodeAsInt("questIcon", -1);
                ActorGameFilesData.Instance.LoadActorData(classId);
     
                Actor actor = (Actor)Activator.CreateInstance(type);
                actor.Family = node.GetAttributeAsString("family");
                actor.ClassId = classId;
                actor.ClassName = node.GetAttributeAsString("className", actor.ClassName);
                actor.NameId = ActorGameFilesData.Instance.NameId;
                actor.Appearance = new Appearance(ActorGameFilesData.Instance.Graphics);
                actor.Position = new Position(node.SelectSingleNode("position"), zoneId);
                actor.QuestIcon = questIcon;
                actor.SubState = new SubState { MotionPack = animation };
                actor.Events.AddRange(SetEvents(node.SelectSingleNode("events")));
                actor.TalkFunctions = GetTalkFunctions(classId, node.SelectSingleNode("talkFunctions"));

                switch (node.Name)
                {
                    case "PopulaceShopSalesman":
                        return LoadPopulaceShopSalesman(actor, node);
                    case "MapObj":
                        return LoadMapObj(actor, node);
                    case "Monster":
                        return LoadMonster(actor, node, zoneId);
                    case "ElevatorStandard":
                        return LoadElevatorStandard(actor, node);
                    case "RetainerFurniture":
                        return (RetainerFurniture)actor;
                    case "PopulaceFlyingShip":
                        return (PopulaceFlyingShip)actor;
                    case "PopulaceLinkshellManager":
                        return (PopulaceLinkshellManager)actor;
                    case "PopulaceGuildlevePublisher":
                        PopulaceGuildlevePublisher publisher = (PopulaceGuildlevePublisher)actor;
                        publisher.GuildLevePackSet = GuildLeveXmlLoader.GetGuildLevePackSet(21);
                        return publisher;
                    case "PopulacePassiveGLPublisher":
                        PopulacePassiveGLPublisher pasiveGl = (PopulacePassiveGLPublisher)actor;
                        pasiveGl.LoadPassiveGls();
                        return pasiveGl;
                    case "PopulaceRetainerManager":
                        return (PopulaceRetainerManager)actor;
                    default:
                        return actor;                       
                }   
            }
            else
            {
                return null;
            }
        }

        private static ElevatorStandard LoadElevatorStandard(Actor actor, XmlNode node)
        {
            ElevatorStandard elevator = (ElevatorStandard)actor;
            elevator.Floor = node.GetAttributeAsInt("floor");
            var destinations = node.SelectSingleNode("destinations");

            if(destinations != null)            
                foreach (XmlNode destination in destinations) 
                    elevator.Destinations.Add(destination.GetAttributeAsInt("option"), destination.GetAttributeAsString("position"));

            return elevator;
        }

        private static Monster LoadMonster(Actor actor, XmlNode node, uint zoneId)
        {
            Monster monster = (Monster)actor;
            monster.RespawnDelay = node.GetAttributeAsInt("respawnDelay");
            monster.Immobile = node.GetAttributeAsBool("immobile");
            monster.DisableAutoAttack = node.GetAttributeAsBool("disableAutoAttack");
            monster.SpawnPoint = new Position(node.SelectSingleNode("position"), zoneId);
            return monster;
        }

        private static MapObj LoadMapObj(Actor actor, XmlNode node)
        {
            MapObj mapObj = (MapObj)actor;
            mapObj.LayoutId = node.GetAttributeAsInt("layoutId");
            mapObj.ObjectId = node.GetAttributeAsInt("objectId");
            mapObj.SpawnAnimation = node.GetAttributeAsString("spawnAnimation");
            mapObj.DespawnAnimation = node.GetAttributeAsString("despawnAnimation");
            return mapObj;
        }

        private static PopulaceShopSalesman LoadPopulaceShopSalesman(Actor actor, XmlNode node)
        {
            PopulaceShopSalesman shopSalesman = (PopulaceShopSalesman)actor;
            shopSalesman.WelcomeTalk = node.GetAttributeAsInt("welcomeTalk");
            shopSalesman.MenuId = node.GetAttributeAsInt("menuId");
            shopSalesman.ShopType = node.Attributes["shopType"] != null ? (ShopType)Enum.Parse(typeof(ShopType), node.Attributes["shopType"].Value) : 0;
            shopSalesman.ItemSet = node.GetAttributeAsInt("itemSet");
            return shopSalesman;
        }

        private static TalkFunction? GenerateDefaultTalkFunction(uint classId)
        {                      
            if (classId < 3000000 && ActorGameFilesData.Instance.NameId > 0) //< 3000000 is NPC. > is monster.
            {
                string displayName = ActorGameFilesData.Instance.Name;
                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1, displayName.Length - 1);

                displayName = displayName.Replace(" ", "")
                    .Replace("`", "")
                    .Replace("\0", "")
                    .Replace("'", "");

                return new TalkFunction(0,"defaultTalkWith" + displayName + "_001", "");               
            }
            else
            {
                return null; 
            }
        }

        private static List<TalkFunction> GetTalkFunctions(uint classId, XmlNode talkFunctionsNode)
        {
            List<TalkFunction> functions = new List<TalkFunction>();
            TalkFunction? defaultTalkFunction = GenerateDefaultTalkFunction(classId);

            //if it was able to generate a default talk functions, add it.
            if (defaultTalkFunction != null)
                functions.Add((TalkFunction)defaultTalkFunction);

            //get talk functions from xml file
            if (talkFunctionsNode != null && talkFunctionsNode.ChildNodes != null && talkFunctionsNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode node in talkFunctionsNode.ChildNodes)
                {
                    TalkFunction function = new TalkFunction(0, node.GetAttributeAsString("name"), node.GetAttributeAsString("parameters"));
                    
                    //I don't think this will ever return anything. need to check.
                    uint questId = node.GetAttributeAsUint("questId");

                    //here we check if the function in the xml is a replacement for the default talk function
                    //created by the GenerateDefaultTalkFunction method. It's useful in cases where the actual
                    //default talk function for an actor in the lua script has a different naming scheme or when 
                    //the default function name has a typo (there is a bunch of them).
                    if (functions.Any(x => x.TalkCode == 0) && questId == 0)
                    {                      
                        functions.Remove((TalkFunction)defaultTalkFunction);                        
                    }

                    functions.Add(function);
                }
            }

            return functions;
        }

        private static List<Event> SetEvents(XmlNode eventNode)
        {
            List<Event> eventList = new List<Event>();

            if (eventNode != null && eventNode.ChildNodes != null && eventNode.ChildNodes.Count > 0)
            {              
                foreach (XmlNode node in eventNode.ChildNodes)
                {
                    switch (node.Name)
                    {                       
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
                        
                        case "exit":
                        case "caution":
                        case "pushCommandIn":
                        case "pushCommandOut":
                        case "pushDefault":
                            eventList.Add(new Event
                            {
                                Opcode = ServerOpcode.PushEventCircle,
                                Name = node.Name,
                                Radius = node.Attributes["radius"] != null ? float.Parse(node.Attributes["radius"].Value) : 0,
                                Silent = node.Attributes["silent"] != null ? Convert.ToByte(node.Attributes["silent"].Value == "false" ? 0 : 1) : (byte)0,
                                Enabled = node.Attributes["enabled"] != null ? Convert.ToByte(node.Attributes["enabled"].Value) : (byte)0,
                                Direction = node.Attributes["outwards"] != null ? Convert.ToByte(node.Attributes["outwards"].Value == "false" ? 1 : 0x11) : (byte)0,
                                Action = node.Attributes["action"] != null ? node.Attributes["action"].Value : ""
                            });
                            break;

                        case "pushCommand":

                            break;
                        case "in":
                            //pushWithBoxEventConditions
                            break;
                        default:
                            Log.Instance.Warning("ActorRepository.SetEvents: Unhandled event '" + node.Name + "' received.");
                            break;
                    }
                }
            }
            else
            {
                //default events
                eventList.Add(new Event{Opcode = ServerOpcode.TalkEvent,Name = "talkDefault",Priority = 4,Enabled = 1});
                eventList.Add(new Event{Opcode = ServerOpcode.NoticeEvent,Name = "noticeEvent",});
            }            

            return eventList;
        }  

        public static Actor CreateActorObj(XmlNode node)
        {          
            return LoadActor(node);
        }

        public static Actor CreateTestActor(uint classId)
        {
            return new Actor
            {
                ClassName = "PopulaceStandard",
                ClassId = classId,
                Position = User.Instance.Character.Position
            };
        }

        public static List<Actor> GetZoneMonsters(uint zoneId)
        {
            XmlDocument monsterFile = new XmlDocument();
            List<Actor> monsters = new List<Actor>();
            string zoneDir = @"" + zoneId.ToString("X");
            string fileNamePath = zoneDir + ".monster.xml";
            monsterFile.LoadFromResource("zones.x" + fileNamePath);

            if (monsterFile.HasChildNodes)
            {
                try
                {
                    XmlElement root = monsterFile.DocumentElement;

                    foreach(XmlNode groupNode in root.ChildNodes)
                    {
                        MonsterGroup group = new MonsterGroup(zoneId);

                        foreach(XmlNode monsterNode in groupNode.ChildNodes)
                        {
                            Actor actor = LoadActor(monsterNode, zoneId);                            

                            if (actor != null)
                            {
                                //crappy modeling just for now until I think of a better solution.
                                ((Monster)actor).BattleGroup = group;                               
                                group.MemberList.Add((Monster)actor);
                                monsters.Add(actor);
                            }                                
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.Warning(e.Message);
                }
            }

            return monsters;
        }    
    }
}
