using Launcher.Characters;
using Launcher.Characters;
using System;
using System.IO;
using System.Text;

namespace Launcher.Characters
{
    [Serializable]
    public class Character
    {
        #region Packet build
        public static readonly ushort OPCODE = 0xd;
        public static readonly byte MAX_SLOTS = 0x03;//0x08; //01 = ?? 08 = num slots
        public static readonly int SLOT_SIZE = 0x3b0;
        #endregion

        #region Info
        public byte Id { get; set; }
        public byte[] Name { get; set; } = new byte[0x20];
        public byte WorldId { get; set; }
        public byte Slot { get; set; }
        #endregion

        #region General
        public byte Size { get; set; }
        public byte Voice { get; set; }
        public ushort SkinColor { get; set; }
        #endregion

        #region Head
        public byte Characteristics { get; set; }
        public byte CharacteristicsColor { get; set; }
        public ushort HairStyle { get; set; }
        public ushort HairColor { get; set; }
        public ushort HairHighlightColor { get; set; }
        public ushort HairVariation { get; set; }
        #endregion

        #region Face 
        public byte Type { get; set; }
        public byte Ears { get; set; }
        public byte Mouth { get; set; }
        public byte Features { get; set; }
        public byte Nose { get; set; }
        public byte EyeShape { get; set; }
        public ushort EyeColor { get; set; }
        public byte IrisSize { get; set; }
        public byte EyeBrows { get; set; }
        public uint Unknown { get; set; }
        #endregion

        #region Background
        public uint Guardian { get; set; }
        public uint BirthMonth { get; set; }
        public uint BirthDay { get; set; }       
        public uint InitialTown { get; set; }
        public uint Tribe { get; set; }
        #endregion        

        #region Class/Job
        public uint CurrentClass { get; set; }
        public uint CurrentJob { get; set; }
        public uint CurrentLevel { get; set; } = 1;
        #endregion

        #region Gear
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
        #endregion

        public CharacterPosition Position { get; set; }

        public void Setup(byte[] data)
        {
            //prepare packet info for decoding
            byte[] info = new byte[0x90];
            Buffer.BlockCopy(data, 0x30, info, 0, info.Length);
            string tmp = Encoding.ASCII.GetString(info).Trim(new[] {'\0'}).Replace('-', '+').Replace('_', '/');

            //decoded packet info
            data = Convert.FromBase64String(tmp);

            File.WriteAllBytes("decodedchara.txt", data);

            //General
            Size = data[0x09];
            Voice = data[0x26];
            SkinColor = (ushort)(data[0x23] >> 8 | data[0x22]);

            //Head
            Characteristics = data[0x0f];
            CharacteristicsColor = data[0x10];
            HairStyle = (ushort)(data[0x0b] >> 8 | data[0x0a]);
            HairHighlightColor = data[0x0c];
            HairVariation = data[0x0d];
            HairColor = (ushort)(data[0x1d] >> 8 | data[0x1c]);

            //Face
            Type = data[0x0e];
            Ears = data[0x1b];
            Mouth = data[0x1a];
            Features = data[0x19];
            Nose = data[0x18];
            EyeShape = data[0x17];
            EyeColor = (ushort)(data[0x25] >> 8 | data[0x24]);
            IrisSize = data[0x16];
            EyeBrows = data[0x15];

            //Background
            Guardian = data[0x27];
            BirthMonth = data[0x28];
            BirthDay = data[0x29];
            InitialTown = data[0x48];
            Tribe = data[0x08];
             
            //Class/Job
            CurrentClass = data[0x2a];

            //Gear
            uint[] initialGear = CharacterClass.GetClass(CurrentClass).Gear;
            MainWeapon = initialGear[0];
            SecondaryWeapon = initialGear[0x01];
            Head = initialGear[0x07];
            Body = initialGear[0x08] == 0 ? CharacterClass.GetTribeUndershirt(Tribe) : initialGear[0x08];
            Legs = initialGear[0x09];
            Hands = initialGear[0x0a];
            Feet = initialGear[0x0b];
            Waist = initialGear[0x0c];
            
            //Position
            Position = CharacterPosition.GetInitialPosition(InitialTown);
        }
    }
}
