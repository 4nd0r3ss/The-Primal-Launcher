using System;
using System.Collections.Generic;
using System.IO;

namespace Launcher.Characters
{
    [Serializable]
    public struct Face
    {
        [BitField.BitfieldLength(5)]
        public uint Characteristics;
        [BitField.BitfieldLength(3)]
        public uint CharacteristicsColor;
        [BitField.BitfieldLength(6)]
        public uint Type;
        [BitField.BitfieldLength(2)]
        public uint Ears;
        [BitField.BitfieldLength(2)]
        public uint Mouth;
        [BitField.BitfieldLength(2)]
        public uint Features;
        [BitField.BitfieldLength(3)]
        public uint Nose;
        [BitField.BitfieldLength(3)]
        public uint EyeShape;
        [BitField.BitfieldLength(1)]
        public uint IrisSize;
        [BitField.BitfieldLength(3)]
        public uint EyeBrows;
        [BitField.BitfieldLength(2)]
        public uint Unknown;
    }

    [Serializable]
    public struct Class
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint[] InitialGear { get; set; }    
        
        public Class(uint id, string name, uint[] initialGear)
        {
            Id = id;
            Name = name;
            InitialGear = initialGear;
        }
    }

    [Serializable]
    struct Tribe
    {
        public uint Id { get; set; }
        public uint Undershirt { get; set; }
        public uint Model { get; set; }

        public Tribe(uint id, uint undershirt, uint model)
        {
            Id = id;
            Undershirt = undershirt;
            Model = model;
        }
    }

    [Serializable]
    public struct GearSet
    {
        public uint MainWeapon { get; set; }
        public uint SecondaryWeapon { get; set; }
        public uint Head { get; set; }
        public uint Body { get; set; }
        public uint Hands { get; set; }
        public uint Legs { get; set; }
        public uint Feet { get; set; }
        public uint Waist { get; set; }
        public uint RightEar { get; set; }
        public uint LeaftEar { get; set; }
        public uint RightFinger { get; set; }
        public uint LeftFinger { get; set; }

        public byte[] ToBytes()
        {
            byte[] result = new byte[0x50];

            using(MemoryStream ms = new MemoryStream(result))
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(MainWeapon);
                    bw.Write(SecondaryWeapon);
                    bw.Write((ulong)0);
                    bw.Write((ulong)0);
                    bw.Write((uint)0);
                    bw.Write(Head);
                    bw.Write(Body);
                    bw.Write(Legs);
                    bw.Write(Hands);
                    bw.Write(Feet);
                    bw.Write(Waist);
                    bw.Write((uint)0);
                    bw.Write(RightEar);
                    bw.Write(LeaftEar);
                    bw.Write((ulong)0);
                    bw.Write(RightFinger);
                    bw.Write(LeftFinger);                   
                }

                //result = ms.GetBuffer();
            }
            return result;
        }
    }   

    [Serializable]
    public static class Appearance
    {
        private static readonly List<Tribe> Tribes = new List<Tribe>
        {
            new Tribe(1, 1184, 1), //id, undershirt, model
            new Tribe(2, 1186, 2),
            new Tribe(3, 1187, 9),
            new Tribe(4, 1184, 3),
            new Tribe(5, 1024, 4),
            new Tribe(6, 1187, 3),
            new Tribe(7, 1505, 4),
            new Tribe(8, 1184, 5),
            new Tribe(9, 1185, 6),
            new Tribe(10, 1504, 5),
            new Tribe(11, 1505, 6),
            new Tribe(12, 1216, 8),
            new Tribe(13, 1186, 8),
            new Tribe(14, 1184, 7),
            new Tribe(15, 1186, 7),
        };

        private static readonly List<Class> Classes = new List<Class>
        {
            new Class(2, "pug", new uint[0x09] {60818432, 60818432, 0, 0, 10656, 10560, 1024, 25824, 6144}),
            new Class(3, "gla", new uint[0x09] {79692890, 0, 0, 0, 31776, 4448, 1024, 25824, 6144}),
            new Class(4, "mrd", new uint[0x09] {147850310, 0, 0, 23713, 0, 10016, 5472, 1152, 6144}),
            new Class(7, "arc", new uint[0x09] {210764860, 236979210, 231736320, 0, 9888, 9984, 1024, 25824, 6144}),
            new Class(8, "lnc", new uint[0x09] {168823858, 0, 0, 0, 13920, 7200, 1024, 10656, 6144}),
            new Class(22, "thm", new uint[0x09] {294650980, 0, 0, 0, 7744, 5472, 1024, 5504, 4096}),
            new Class(23, "cnj", new uint[0x09] {347079700, 0, 0, 0, 4448, 2240, 1024, 4416, 4096}),
            new Class(29, "crp", new uint[0x09] {705692672, 0, 0, 0, 0, 10016, 10656, 9632, 2048}),
            new Class(30, "bsm", new uint[0x09] {721421372, 0, 0, 0, 0, 2241, 2336, 2304, 2048}),
            new Class(31, "arm", new uint[0x09] {737149962, 0, 0, 0, 32992, 2240, 1024, 2272, 2048}),
            new Class(32, "gsm", new uint[0x09] {752878592, 0, 0, 0, 2368, 3424, 1024, 10656, 2048}),
            new Class(33, "ltw", new uint[0x09] {768607252, 0, 0, 4448, 4449, 1792, 1024, 21888, 2048}),
            new Class(34, "wvr", new uint[0x09] {784335922, 0, 0, 0, 5505, 5473, 1024, 5505, 2048}),
            new Class(35, "alc", new uint[0x09] {800064522, 0, 0, 20509, 5504, 2241, 1024, 1152, 2048}),
            new Class(36, "cul", new uint[0x09] {815793192, 0, 0, 5632, 34848, 1792, 1024, 25825, 2048}),
            new Class(39, "min", new uint[0x09] {862979092, 0, 0, 0, 1184, 2242, 6464, 6528, 14336}),
            new Class(40, "btn", new uint[0x09] {878707732, 0, 0, 6304, 6624, 6560, 1024, 1152, 14336}),
            new Class(41, "fsh", new uint[0x09] {894436372, 0, 0, 6400, 1184, 9984, 1024, 6529, 14336}),
        };             
        public static GearSet GetInitialGearSet(uint id, uint tribe)
        {
            Class cClass = Classes.Find(x => x.Id == id);

            return new GearSet
            {                
                MainWeapon = cClass.InitialGear[0],
                SecondaryWeapon = cClass.InitialGear[0x01],
                Head = cClass.InitialGear[0x03],
                Body = cClass.InitialGear[0x04] == 0 ? Tribes.Find(x => x.Id == tribe).Undershirt : cClass.InitialGear[0x04],
                Hands = cClass.InitialGear[0x05],
                Legs = cClass.InitialGear[0x06],
                Feet = cClass.InitialGear[0x07],
                Waist = cClass.InitialGear[0x08]
            };          
        }
        public static uint GetTribeModel(uint tribe) => Tribes.Find(x => x.Id == tribe).Model;
    }
}
