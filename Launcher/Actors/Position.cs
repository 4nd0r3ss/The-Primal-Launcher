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

        public static Position GetInitialPosition(uint town)
        {
            switch (town)
            {
                case 1:
                    return new Position((uint)CityState.Limsa, 0.016f, 10.35f, -36.91f, 0.025f, 15);
                case 2:
                    return new Position((uint)CityState.Gridania, 369.5434f, 4.21f, -706.1074f, -1.26721f, 15);                   
                case 3:
                    return new Position((uint)CityState.UlDah, 5.364327f, 196.0f, 133.6561f, -2.849384f, 15);
                default:
                    throw new Exception("There was an error getting the initial town value.");
            }            
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
        public static List<Position> List { get; } = new List<Position>
        {
            new Position(128, -8.48f, 45.36f, 139.5f, 2.02f, 15), //lanoscea

            new Position(133, -444.266f, 39.518f, 191f, 1.9f, 15),
            new Position(133, -8.062f, 45.429f, 139.364f, 2.955f, 15),

            new Position(150, 333.271f, 5.889f, -943.275f, 0.794f, 15),
            new Position(155, 58.92f, 4f, -1219.07f, 0.52f, 15),

            new Position(166, 356.09f, 3.74f, -701.62f, -1.4f, 15),// central shroud
            new Position(166, 364.0f, 4f, -703.8f, 1.5f, 16),// central shroud, gridania opening

            new Position(170, -27.015f, 181.798f,-79.72f, 2.513f, 15),
            new Position(175, -237.6312f, 184.8451f, -5.752599f, -2.536515f, 15),

            //Uldah
            new Position(184, 5.36433f, 196f, 133.656f, -2.84938f, 15),//uldah            
            new Position(184, -24.34f, 192f, 34.22f, 0.78f, 15),//uldah
            new Position(184, -24.34f, 192f, 34.22f, 0.78f, 16),//uldah, opening
            new Position(184, -22f, 196f, 87f, 1.8f, 15),//uldah

            new Position(193, 0.016f, 10.35f, -36.91f, 0.025f, 15),//rhotano sea
            new Position(193, -5f, 16.35f, 6f, 0.5f, 16), //rhotano sea    

            new Position(230, -838.1f, 6f, 231.94f, 1.1f, 15),

            new Position(244, 0.048f, 0f, -5.737f, 0f, 15),
            new Position(244, -160.048f, 0f, -165.737f, 0f, 15),
            new Position(244, 160.048f, 0f, 154.263f, 0f, 15),

            new Position(190, 160.048f, 0f, 154.263f, 0f, 15),
            new Position(240, 160.048f, 0f, 154.263f, 0f, 15),
            new Position(206, -124.852f, 15.920f, -1328.476f, 0f, 15),
            new Position(177, 0f, 0f, 0f, 0f, 15),
            new Position(160, 0f, 0f, 0f, 0f, 15),
            new Position(201, 0f, 0f, 0f, 0f, 15),
            new Position(147, 0f, 0f, 0f, 0f, 15),

            new Position(2, -826.86f, 6f, 192.74f, -0.0083f, 15), //limsa opening private area (docks)
            new Position(4, -459.61f, 40.00f, 196.37f, 2.01f, 15), //limsa opening private area (drowning wrench) 
            
            new Position(5, 175.7f, -1.2f, -1156.1f, -2f, 15), //gridania opening private area (city entrance)
            new Position(6, 65.3f, 4f, -1204.8f, -1.6f, 15), //gridania opening private area (canopy)
        };

        public static Position Get(uint zoneId, ushort spawnType = 15) => List.Find(x => x.ZoneId == zoneId && x.SpawnType == spawnType);
    }
}
