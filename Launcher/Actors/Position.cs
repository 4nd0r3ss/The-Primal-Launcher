using System;
using System.Collections.Generic;

namespace PrimalLauncher
{   
    [Serializable]
    public class Position
    {
        public uint ZoneId {get;set;}
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float R { get; set; } 
        public float FloatingHeight { get; set; }
        public ushort SpawnType { get; set; }
        public ushort IsZoningPlayer { get; set; }

        public Position() { }
        public Position(uint zoneId, float x, float y, float z, float r, ushort spawnType)
        {
            ZoneId = zoneId;
            X = x;
            Y = y;
            Z = z;
            R = r;
            SpawnType = spawnType;
            FloatingHeight = 0;
            IsZoningPlayer = 0;
        }

        public byte[] ToBytes(bool isPlayer, uint actorId = 0)
        {
            byte[] toBytes = new byte[0x28];
            int idToPrint = (int)actorId;

            if (isPlayer)
                idToPrint = -1;

            Buffer.BlockCopy(BitConverter.GetBytes(idToPrint), 0, toBytes, 0x04, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, toBytes, 0x08, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, toBytes, 0x0c, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, toBytes, 0x10, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(R), 0, toBytes, 0x14, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(FloatingHeight), 0, toBytes, 0x1c, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(SpawnType), 0, toBytes, 0x24, 0x02);
            Buffer.BlockCopy(BitConverter.GetBytes(IsZoningPlayer), 0, toBytes, 0x26, 0x02);

            return toBytes;
        }
    }

    public static class EntryPoints
    {
        public static Position GetStartPosition(uint startTown)
        {
            switch (startTown)
            {
                case 1:
                    return new Position(193, 0.016f, 10.35f, -36.91f, 0.025f, 15);
                case 2:
                    return new Position(166, 369.5434f, 4.21f, -706.1074f, -1.26721f, 15);
                case 3:
                    return new Position(184, 5.364327f, 196.0f, 133.6561f, -2.849384f, 15);
                default:
                    throw new Exception("There was an error getting the initial town position.");
            }           
        }

        public static Position GetInnEntry(uint town)
        {
            switch (town)
            {
                case 1: //limsa
                    return new Position(244, 160.048f, 0f, 154.263f, 0f, 15);
                case 2: //gridania
                    return new Position(244, -160.048f, 0f, -165.737f, 0f, 15);
                case 3: //uldah
                    return new Position(244, 0.048f, 0f, -5.737f, 0f, 15);
                default:
                    throw new Exception("There was an error getting the inn entry.");
            }
        }

        public static Position GetInnExit(uint town)
        {
            switch (town)
            {
                case 1: //limsa
                    return new Position(133, -435.2f, 40f, 201.5f, -2.6f, 15);
                case 2: //gridania
                    return new Position(155, 58.92f, 4f, -1219.07f, 0.52f, 15);
                case 3: //uldah
                    return new Position(175, -112.8f, 202f, 172.6f, 2.2f, 15); //verify map number
                default:
                    throw new Exception("There was an error getting the inn entry.");
            }
        }

        public static List<Position> List { get; } = new List<Position>
        {          
            //Opening instances
            new Position(1, -20.9f, 196f, 87.4f, -1.6f, 15),                        //4 - uldah (pearl lane)
            new Position(8, -75.4f, 195f, 74.8f, 0f, 15),                           //5 - uldah (quicksand)
            new Position(2, -826.86f, 6f, 192.74f, -0.0083f, 15),                   //6 - limsa (docks)
            new Position(4, -459.61f, 40.00f, 196.37f, 2.01f, 15),                  //7 - limsa (drowning wrench)             
            new Position(5, 175.7f, -1.2f, -1156.1f, -2f, 15),                      //8 - gridania (city entrance)
            new Position(6, 65.3f, 4f, -1204.8f, -1.6f, 15),                        //9 - gridania (canopy)

            //Opening battle
            new Position(193, -5f, 16.35f, 6f, 0.5f, 16),                           //10 - limsa
            new Position(166, 364.0f, 3.9f, -703.8f, 1.5f, 16),                     //11 - gridania
            new Position(184, -21.4f, 192f, 36.3f, 1f, 16),                         //12 - uldah            

            //Opening after battle
            new Position(133, -444.266f, 39.518f, 191f, 1.9f, 15),                  //13 - drowning wrench
            new Position(155, 63.4f, 4f, -1203.5f, 1.7f, 15),                       //14 - carline canopy
            new Position(175, -75.4f, 195f, 74.8f, 0f, 15),                         //15 - quicksand

            //Market wards
            new Position(134, 160f, 0f, 204f, 3f, 15),                              //13 - limsa            
            new Position(160, 0f, 0f, 0f, 0f, 15),                                  //14 - gridania           
            new Position(180, 0f, 0f, 0f, 0f, 15),                                  //15 - uldah      

            //Extra
            new Position(177, 0f, 0f, 0f, 0f, 15),                                  //22 - jail
            new Position(201, 0f, 0f, 0f, 0f, 15),                                  //23 - tent (looks like tent from camp locations, but inside)
            new Position(190, 160.048f, 0f, 154.263f, 0f, 15),                      //24 - Mor Dhona - wrong position
            new Position(240, 160.048f, 0f, 154.263f, 0f, 15),                      //25 - the bowl of embers - gives a zone script error
            new Position(134, -185.9f, -2f, -221f, 0f, 15),                         //25 - limsa big warehouse-like place in markert wards map.
            new Position(134, -160f, 0f, -137.3f, 0f, 15),                          //25 - limsa 2 bed inn room in markert wards map.
            new Position(134, -133.3f, 1f, -160f, 1.4f, 15),                        //25 - limsa admiral room in markert wards map.

            //Dungeons
            new Position(147, 0f, 0f, 0f, 0f, 15),                                  //26 - dzemael darkhold 

            //remove if not useful
            new Position(128, -8.48f, 45.36f, 139.5f, 2.02f, 15),                   //27 - zephir gate     
            new Position(230, -838.1f, 6f, 231.94f, 1.1f, 15),                      //32 - remove                               
        };

        public static Position Get(uint zoneId, ushort spawnType = 15) => List.Find(x => x.ZoneId == zoneId && x.SpawnType == spawnType);

        public static Position GetTownWarpExit(uint region, uint id)
        {
            string index = region + "-" + id;

            Dictionary<string, Position> exits = new Dictionary<string, Position>
            {
                { "101-7", new Position(133, -8.0f, 45.4f, 139.3f, 2.9f, 15) },

                { "103-7", new Position(150, 333.2f, 5.8f, -943.2f, 0.7f, 15) },
                { "103-8", new Position(150, -185.3f, 5.4f, -977f, 0f, 15) },
                
                { "104-7", new Position(170, -27.0f, 181.7f,-79.7f, 2.5f, 15) },
                { "104-8", new Position(170, 176.6f, 184.3f, 227.3f, 1.5f, 15) },
            };

            return exits[index];
        }  
    }
 }
