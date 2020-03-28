using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    [Serializable]
    public class CharacterClass
    {
        public byte Id { get; set; }
        public string Name { get; set; }       
        public bool IsCurrent { get; set; } //the class player is using with now

        public ushort Level { get; set; }
        public long TotalExp { get; set; }

        public uint[] InitialGear { get; set; }

        public ushort MaxHp { get; set; }
        public ushort MaxMp { get; set; }
        public ushort MaxTp { get; set; }

        public ushort Hp { get; set; }
        public ushort Mp { get; set; }
        public ushort Tp { get; set; }

        //Initial tribe attributes will work as multipliers for these?
        //will have to collect equipment attr to calculate these. 
        //I think these properties wont be necessary.
        public ushort AttackPower { get; set; }
        public ushort Defense { get; set; }
        public ushort AttacMagicPotency { get; set; }
        public ushort HealingMagicPotency { get; set; }
        public ushort MagicAccuracy { get; set; }        

        public ushort Accuracy { get; set; }
        public ushort Evasion { get; set; }
        public ushort EnfeeblingMagicPotency { get; set; }
        public ushort EnhancementMagicPotency { get; set; }
        public ushort MagicEvasion { get; set; }

        //For craft classes
        public uint Processing { get; set; }
        public uint MagicProcessing { get; set; }
        public uint ProcessControl { get; set; }

        //For gathering classes
        public uint HarvesPotency { get; set; }
        public uint HarvesLimit { get; set; }
        public uint HarvesRate { get; set; }
               
        public CharacterClass(byte id, string name, uint[] initialGear = null)
        {
            Id = id;
            Name = name;
            IsCurrent = false;            

            Level = 0; //if the player doesn't have a class, then level is 0.
            InitialGear = initialGear;
            MaxHp = 300;
            MaxMp = 200;
            MaxTp = 3000; //this was the default TP value for 1.x

            //can use these fields to start the character with less HP, MP or TP.
            Hp = MaxHp;
            Mp = MaxMp;
            Tp = MaxTp;
        }

        public static Dictionary<byte, CharacterClass> GetClassList()
        {
            return new Dictionary<byte, CharacterClass>
            {
                {1, new CharacterClass(1, "adv") }, //Adventurer class!
                {2, new CharacterClass(2, "pug", new uint[0x09] {60818432, 60818432, 0, 0, 10656, 10560, 1024, 25824, 6144}) },
                {3, new CharacterClass(3, "gla", new uint[0x09] {79692890, 0, 0, 0, 31776, 4448, 1024, 25824, 6144}) },
                {4, new CharacterClass(4, "mrd", new uint[0x09] {147850310, 0, 0, 23713, 0, 10016, 5472, 1152, 6144}) },
                {5, new CharacterClass(5, "fnc") },   //Fencer class!
                {6, new CharacterClass(6, "enf") },   //Enforcer class!
                {7, new CharacterClass(7, "arc", new uint[0x09] {210764860, 236979210, 231736320, 0, 9888, 9984, 1024, 25824, 6144}) },
                {8, new CharacterClass(8, "lnc", new uint[0x09] {168823858, 0, 0, 0, 13920, 7200, 1024, 10656, 6144}) },
                {9, new CharacterClass(9, "msk") },   //Musketeer class!
                {10, new CharacterClass(10, "sen") }, //Sentinel class! (this one has an icon!)
                {11, new CharacterClass(11, "sam") }, //Samurai class!
                {12, new CharacterClass(12, "stv") }, //Stavesman class!
                {13, new CharacterClass(13, "asn") }, //Assassin class!
                {14, new CharacterClass(14, "flr") }, //Flayer class!
                {15, new CharacterClass(15, "mnk") }, //Monk
                {16, new CharacterClass(16, "pld") }, //Paladin
                {17, new CharacterClass(17, "war") }, //Warrior
                {18, new CharacterClass(18, "brd") }, //Bard
                {19, new CharacterClass(19, "drg") }, //Dragoon
                {20, new CharacterClass(20, "???") }, //Written in Japanese
                {21, new CharacterClass(21, "mst") }, //Mystic class!
                {22, new CharacterClass(22, "thm", new uint[0x09] {294650980, 0, 0, 0, 7744, 5472, 1024, 5504, 4096}) },
                {23, new CharacterClass(23, "cnj", new uint[0x09] {347079700, 0, 0, 0, 4448, 2240, 1024, 4416, 4096}) },
                {24, new CharacterClass(24, "arc") }, //Arcanist class!
                {25, new CharacterClass(25, "brd") }, //Bard (again?? - this one has no icon)
                {26, new CharacterClass(26, "blm") }, //Black Mage
                {27, new CharacterClass(27, "whm") }, //White Mage
                {28, new CharacterClass(28, "???") }, //Written in Japanese
                {29, new CharacterClass(29, "crp", new uint[0x09] {705692672, 0, 0, 0, 0, 10016, 10656, 9632, 2048}) },
                {30, new CharacterClass(30, "bsm", new uint[0x09] {721421372, 0, 0, 0, 0, 2241, 2336, 2304, 2048}) },
                {31, new CharacterClass(31, "arm", new uint[0x09] {737149962, 0, 0, 0, 32992, 2240, 1024, 2272, 2048}) },
                {32, new CharacterClass(32, "gsm", new uint[0x09] {752878592, 0, 0, 0, 2368, 3424, 1024, 10656, 2048}) },
                {33, new CharacterClass(33, "ltw", new uint[0x09] {768607252, 0, 0, 4448, 4449, 1792, 1024, 21888, 2048}) },
                {34, new CharacterClass(34, "wvr", new uint[0x09] {784335922, 0, 0, 0, 5505, 5473, 1024, 5505, 2048}) },
                {35, new CharacterClass(35, "alc", new uint[0x09] {800064522, 0, 0, 20509, 5504, 2241, 1024, 1152, 2048}) },
                {36, new CharacterClass(36, "cul", new uint[0x09] {815793192, 0, 0, 5632, 34848, 1792, 1024, 25825, 2048}) },
                {37, new CharacterClass(37, "???") }, //Written in Japanese
                {38, new CharacterClass(38, "???") }, //Written in Japanese
                {39, new CharacterClass(39, "min", new uint[0x09] {862979092, 0, 0, 0, 1184, 2242, 6464, 6528, 14336}) },
                {40, new CharacterClass(40, "btn", new uint[0x09] {878707732, 0, 0, 6304, 6624, 6560, 1024, 1152, 14336}) },
                {41, new CharacterClass(41, "fsh", new uint[0x09] {894436372, 0, 0, 6400, 1184, 9984, 1024, 6529, 14336}) },
                {42, new CharacterClass(42, "shd") }, //Shepherd class!
                {43, new CharacterClass(43, "???") }, //Written in Japanese
                {44, new CharacterClass(44, "???") }, //Written in Japanese
                {45, new CharacterClass(45, "???") }, //Subskill 1(?) writen in katakana
                {46, new CharacterClass(46, "???") }, //Subskill 2(?) writen in katakana
                {47, new CharacterClass(47, "???") }, //Subskill 3(?) writen in katakana
                {48, new CharacterClass(48, "???") }, //Subskill 4(?) writen in katakana
                {49, new CharacterClass(49, "???") }, //to check
                {50, new CharacterClass(50, "???") }, //to check
                {51, new CharacterClass(51, "???") }, //to check
                {52, new CharacterClass(52, "???") }, //to check
            };
        }        

        public static  GearGraphics GetInitialGearSet(CharacterClass cClass, uint tribe)
        {
            return new GearGraphics
            {
                MainWeapon = cClass.InitialGear[0],
                SecondaryWeapon = cClass.InitialGear[0x01],
                Head = cClass.InitialGear[0x03],
                Body = cClass.InitialGear[0x04] == 0 ? Model.List.Find(x => x.Id == tribe).Undershirt : cClass.InitialGear[0x04],
                Hands = cClass.InitialGear[0x06],
                Legs = cClass.InitialGear[0x05],
                Feet = cClass.InitialGear[0x07],
                Waist = cClass.InitialGear[0x08]
            };
        }
    }
}
