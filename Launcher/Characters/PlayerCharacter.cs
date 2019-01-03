using System;
using System.IO;
using System.Text;

namespace Launcher.Characters
{  
    [Serializable]
    public class PlayerCharacter
    {
        #region Packet build
        public static readonly ushort OPCODE = 0xd;
        public static readonly byte MAX_SLOTS = 0x03;//0x08; //01 = ?? 08 = num slots
        public static readonly int SLOT_SIZE = 0x1d0;
        #endregion

        #region Info
        public uint Id { get; set; }
        public byte[] Name { get; set; } = new byte[0x20];
        public byte WorldId { get; set; }
        public byte Slot { get; set; }
        #endregion

        #region General
        public uint Size { get; set; }
        public uint Voice { get; set; }
        public ushort SkinColor { get; set; }
        #endregion

        #region Head       
        public ushort HairStyle { get; set; }
        public ushort HairColor { get; set; }
        public ushort HairHighlightColor { get; set; }
        public ushort HairVariation { get; set; }
        public ushort EyeColor { get; set; } //oddly not part of face bitfield values. Maybe it was added at a later time in development?
        #endregion

        public Face Face { get; set; }

        #region Background
        public byte Guardian { get; set; }
        public byte BirthMonth { get; set; }
        public byte BirthDay { get; set; }       
        public uint InitialTown { get; set; }
        public byte Tribe { get; set; }
        #endregion        

        #region Class/Job
        public byte CurrentClass { get; set; }
        public byte CurrentJob { get; set; }
        public ushort CurrentLevel { get; set; } = 1;
        #endregion
                
        public GearSet GearSet { get; set; }      
        public Position Position { get; set; }
        public Inventory Inventory { get; set; }       

        public void Setup(byte[] data)
        {
            //Character ID
            Id = NewId();

            //prepare packet info for decoding
            byte[] info = new byte[0x90];
            Buffer.BlockCopy(data, 0x30, info, 0, info.Length);
            string tmp = Encoding.ASCII.GetString(info).Trim(new[] { '\0' }).Replace('-', '+').Replace('_', '/');

            //decoded packet info
            data = Convert.FromBase64String(tmp);

            //File.WriteAllBytes("decodedchara.txt", data);

            //General
            Size = data[0x09];
            Voice = data[0x26];
            SkinColor = (ushort)(data[0x23] >> 8 | data[0x22]);

            //Head
            HairStyle = (ushort)(data[0x0b] >> 8 | data[0x0a]);
            HairColor = (ushort)(data[0x1d] >> 8 | data[0x1c]);
            HairHighlightColor = data[0x0c];
            HairVariation = data[0x0d];            
            EyeColor = (ushort)(data[0x25] >> 8 | data[0x24]);

            //Face
            Face = new Face
            {
                Characteristics = data[0x0f],
                CharacteristicsColor = data[0x10],
                Type = data[0x0e],
                Ears = data[0x1b],
                Mouth = data[0x1a],
                Features = data[0x19],
                Nose = data[0x18],
                EyeShape = data[0x17],
                IrisSize = data[0x16],
                EyeBrows = data[0x15],
                Unknown = 0
            };   

            //Background
            Guardian = data[0x27];
            BirthMonth = data[0x28];
            BirthDay = data[0x29];
            InitialTown = data[0x48];
            Tribe = data[0x08];             
            
            CurrentClass = data[0x2a];            
            GearSet = Appearance.GetInitialGearSet(CurrentClass, Tribe);  
            Position = InitialPosition.Get(InitialTown);
        }

        private uint NewId()
        {
            Random rnd = new Random();
            byte[] id = new byte[0x4];
            rnd.NextBytes(id);
            return BitConverter.ToUInt32(id, 0);
        }
    }
}
