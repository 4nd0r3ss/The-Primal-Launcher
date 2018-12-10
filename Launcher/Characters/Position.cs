using System;

namespace Launcher.Characters
{
    [Serializable]
    public struct Position
    {       
        public int ZoneId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rotation { get; set; }

        public static Position GetInitialPosition(uint initialTown)
        {
            switch (initialTown)
            {
                case 1:
                    return new Position //Limsa 
                    {
                        ZoneId = 193,
                        X = 0.016f,
                        Y = 10.35f,
                        Z = -36.91f,
                        Rotation = 0.025f
                    };
                case 2:
                    return new Position //Gridania
                    {
                        ZoneId = 166,
                        X = 369.5434f,
                        Y = 4.21f,
                        Z = -706.1074f,
                        Rotation = -1.26721f
                    };
                case 3:
                    return new Position //Ul'Dah
                    {
                        ZoneId = 184,
                        X = 5.364327f,
                        Y = 196.0f,
                        Z = 133.6561f,
                        Rotation = -2.849384f
                    };               
            }

            return new Position();
        }
    }
}
