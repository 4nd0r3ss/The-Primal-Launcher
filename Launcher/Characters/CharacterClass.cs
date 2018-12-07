using System;
using System.Collections.Generic;

namespace Launcher.Characters
{
    [Serializable]
    public struct Class
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint[] Gear { get; set; }        
    }

    [Serializable]
    public static class CharacterClass
    {
        public static readonly List<KeyValuePair<uint, uint>> TribeUndershirt = new List<KeyValuePair<uint, uint>>
        {
            new KeyValuePair<uint, uint>(1,1184),
            new KeyValuePair<uint, uint>(2,1186),
            new KeyValuePair<uint, uint>(3,1187),
            new KeyValuePair<uint, uint>(4,1184),
            new KeyValuePair<uint, uint>(5,1024),
            new KeyValuePair<uint, uint>(6,1187),
            new KeyValuePair<uint, uint>(7,1505),
            new KeyValuePair<uint, uint>(8,1184),
            new KeyValuePair<uint, uint>(9,1185),
            new KeyValuePair<uint, uint>(10,1504),
            new KeyValuePair<uint, uint>(11,1505),
            new KeyValuePair<uint, uint>(12,1216),
            new KeyValuePair<uint, uint>(13,1186),
            new KeyValuePair<uint, uint>(14,1184),
            new KeyValuePair<uint, uint>(15,1186),
        };

        public static readonly List<Class> InitialGear = new List<Class>
        {
            new Class
            {
                Id = 2,
                Name = "pug",
                Gear = new uint[0x16] {60818432, 60818432, 0, 0, 0, 0, 0, 0, 10656, 10560, 1024, 25824, 6144, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 3,
                Name = "gla",
                Gear = new uint[0x16] {79692890, 0, 0, 0, 0, 0, 0, 0, 31776, 4448, 1024, 25824, 6144, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 4,
                Name = "mrd",
                Gear = new uint[0x16] {147850310, 0, 0, 0, 0, 0, 0, 23713, 0, 10016, 5472, 1152, 6144, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 7,
                Name = "arc",
                Gear = new uint[0x16] {210764860, 236979210, 0, 0, 0, 231736320, 0, 0, 9888, 9984, 1024, 25824, 6144, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 8,
                Name = "lnc",
                Gear = new uint[0x16] {168823858, 0, 0, 0, 0, 0, 0, 0, 13920, 7200, 1024, 10656, 6144, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 22,
                Name = "thm",
                Gear = new uint[0x16] {294650980, 0, 0, 0, 0, 0, 0, 0, 7744, 5472, 1024, 5504, 4096, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 23,
                Name = "cnj",
                Gear = new uint[0x16] {347079700, 0, 0, 0, 0, 0, 0, 0, 4448, 2240, 1024, 4416, 4096, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 29,
                Name = "crp",
                Gear = new uint[0x16] {705692672, 0, 0, 0, 0, 0, 0, 0, 0, 10016, 10656, 9632, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 30,
                Name = "bsm",
                Gear = new uint[0x16] {721421372, 0, 0, 0, 0, 0, 0, 0, 0, 2241, 2336, 2304, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 31,
                Name = "arm",
                Gear = new uint[0x16] {737149962, 0, 0, 0, 0, 0, 0, 0, 32992, 2240, 1024, 2272, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 32,
                Name = "gsm",
                Gear = new uint[0x16] {752878592, 0, 0, 0, 0, 0, 0, 0, 2368, 3424, 1024, 10656, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 33,
                Name = "ltw",
                Gear = new uint[0x16] {768607252, 0, 0, 0, 0, 0, 0, 4448, 4449, 1792, 1024, 21888, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 34,
                Name = "wvr",
                Gear = new uint[0x16] {784335922, 0, 0, 0, 0, 0, 0, 0, 5505, 5473, 1024, 5505, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 35,
                Name = "alc",
                Gear = new uint[0x16] {800064522, 0, 0, 0, 0, 0, 0, 20509, 5504, 2241, 1024, 1152, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 36,
                Name = "cul",
                Gear = new uint[0x16] {815793192, 0, 0, 0, 0, 0, 0, 5632, 34848, 1792, 1024, 25825, 2048, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 39,
                Name = "min",
                Gear = new uint[0x16] {862979092, 0, 0, 0, 0, 0, 0, 0, 1184, 2242, 6464, 6528, 14336, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 40,
                Name = "btn",
                Gear = new uint[0x16] {878707732, 0, 0, 0, 0, 0, 0, 6304, 6624, 6560, 1024, 1152, 14336, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            },

            new Class
            {
                Id = 41,
                Name = "fsh",
                Gear = new uint[0x16] {894436372, 0, 0, 0, 0, 0, 0, 6400, 1184, 9984, 1024, 6529, 14336, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            }
        };

        public static Class GetClass(uint id) => InitialGear.Find(x => x.Id == id);
        public static uint GetTribeUndershirt(uint id) => TribeUndershirt.Find(x => x.Key == id).Value;
    }
}
