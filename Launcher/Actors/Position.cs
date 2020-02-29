using System;

namespace Launcher
{
    [Serializable]
    public enum CityState
    {
        Limsa = 193,
        Gridania = 166,
        UlDah = 184
    }  

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

        public byte[] ToBytes(uint actorId = 0)
        {
            byte[] toBytes = new byte[0x28];

            Buffer.BlockCopy(BitConverter.GetBytes(actorId), 0, toBytes, 0x04, 0x04);
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
}
