using System;

namespace Launcher.Characters
{
    [Serializable]
    public enum CityState
    {
        Limsa = 193,
        Gridania = 166,
        UlDah = 184
    }

    [Serializable]
    struct InitialPosition
    {      
        public static Position Get(uint town)
        {
            switch (town)
            {
                case 1:
                    return new Position //Limsa 
                    {
                        ZoneId = (uint)CityState.Limsa,
                        X = 0.016f,
                        Y = 10.35f,
                        Z = -36.91f,
                        Rotation = 0.025f
                    };
                case 2:
                    return new Position //Gridania
                    {
                        ZoneId = (uint)CityState.Gridania,
                        X = 369.5434f,
                        Y = 4.21f,
                        Z = -706.1074f,
                        Rotation = -1.26721f
                    };
                case 3:
                    return new Position //Ul'Dah
                    {
                        ZoneId = (uint)CityState.UlDah,
                        X = 5.364327f,
                        Y = 196.0f,
                        Z = 133.6561f,
                        Rotation = -2.849384f
                    };
            }

            return new Position();
        }
    }

    [Serializable]
    public struct Position
    {
        public uint ZoneId {get;set;}
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rotation { get; set; } 
        public float FloatingHeight { get; set; }
        public ushort SpawnType { get; set; }
        public ushort IsZoningPlayer { get; set; }

        public byte[] ToBytes(uint actorId = 0)
        {
            byte[] toBytes = new byte[0x28];

            Buffer.BlockCopy(BitConverter.GetBytes(actorId), 0, toBytes, 0x04, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, toBytes, 0x08, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, toBytes, 0x0c, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, toBytes, 0x10, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(Rotation), 0, toBytes, 0x14, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(FloatingHeight), 0, toBytes, 0x1c, 0x04);
            Buffer.BlockCopy(BitConverter.GetBytes(SpawnType), 0, toBytes, 0x24, 0x02);
            Buffer.BlockCopy(BitConverter.GetBytes(IsZoningPlayer), 0, toBytes, 0x26, 0x02);

            return toBytes;
        }
    }
}
