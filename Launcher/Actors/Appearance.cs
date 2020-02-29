using System;
using System.Collections.Generic;
using System.IO;

namespace Launcher
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

    /// <summary>
    /// This class contains basic information for character creation like character model, initial status and initial gear.
    /// </summary>
    [Serializable]
    public struct Model
    {
        public byte Id { get; }
        public uint Undershirt { get; }
        public uint Type { get; set; }  
        public ushort[] InitialParameters { get; set; }

        public Model(byte id, uint undershirt, uint type, ushort[] initialParameters = null)
        {
            Id = id;
            Undershirt = undershirt;
            Type = type;
            InitialParameters = initialParameters;
        }

        //I modeled this differently until I figured out how it was implemented in the game. It is a MESS.
        //From indexes 15 forward it will be calculated based on equiment.
        private static List<ushort[]> InitialParametersList = new List<ushort[]>
        {
            //--, --, --, str, vit, dex, int, mnd, pie, fir, ice, wnd, ear, lit, wat, acc, eva, att, def, --, --, --, --, attMagP, heaMagP, enhMagP, enfMagP, magAcc, magEva
            new ushort[]{0,0,0,16,15,14,16,13,16,16,13,15,15,15,16,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //midlander
            new ushort[]{0,0,0,18,17,15,13,15,12,15,16,14,14,18,13,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //highlander
            new ushort[]{0,0,0,14,13,18,17,12,16,12,14,18,17,14,15,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //wildwood
            new ushort[]{0,0,0,15,14,15,18,15,13,14,16,12,17,15,16,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //duskwight
            new ushort[]{0,0,0,13,13,17,16,15,16,14,13,15,16,17,15,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //plainsfolk
            new ushort[]{0,0,0,12,12,15,16,17,18,17,12,16,18,15,12,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //dunesfolk
            new ushort[]{0,0,0,16,15,17,13,14,15,18,15,13,15,12,17,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //seeker of the sun
            new ushort[]{0,0,0,13,12,16,14,18,17,13,18,15,14,16,14,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //keeper of the moon
            new ushort[]{0,0,0,17,18,13,12,16,14,13,17,17,12,13,18,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //sea wolf
            new ushort[]{0,0,0,15,16,12,15,17,15,18,14,16,13,14,18,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, //hellsguard
        };

        public static readonly List<Model> List = new List<Model>
        {
            //id, undershirt, model
            new Model(0, 0, 0, null),  //default
            new Model(1, 1184, 1, InitialParametersList[0]),  //midlander male
            new Model(2, 1186, 2, InitialParametersList[0]),  //midlander female
            new Model(3, 1187, 9, InitialParametersList[1]),  //highlander male
            new Model(4, 1184, 3, InitialParametersList[2]),  //wildwood male
            new Model(5, 1024, 4, InitialParametersList[2]),  //wildwood female
            new Model(6, 1187, 3, InitialParametersList[3]),  //duskwight male 
            new Model(7, 1505, 4, InitialParametersList[3]),  //duskwight female
            new Model(8, 1184, 5, InitialParametersList[4]),  //plainsfolk male
            new Model(9, 1185, 6, InitialParametersList[4]),  //plainsfolk female
            new Model(10, 1504, 5, InitialParametersList[5]), //dunesfolk male
            new Model(11, 1505, 6, InitialParametersList[5]), //dunesfolk female
            new Model(12, 1216, 8, InitialParametersList[6]), //seeker of the sun
            new Model(13, 1186, 8, InitialParametersList[7]), //keeper of the moon
            new Model(14, 1184, 7, InitialParametersList[8]), //sea wolf
            new Model(15, 1186, 7, InitialParametersList[9]), //hellsguard                    
        };               

        public static Model GetTribe(uint tribe) => List.Find(x => x.Id == tribe);
        public static uint GetModel(uint tribe) => List.Find(x => x.Id == tribe).Type;
    }

    /// <summary>
    /// Keeps a set of the character's currently equipped gear.
    /// </summary>
    [Serializable]
    public struct GearSet
    {
        public uint MainWeapon { get; set; }
        public uint SecondaryWeapon { get; set; }
        public uint SPMainWeapon { get; set; }
        public uint SPSecondaryWeapon { get; set; }
        public uint Throwing { get; set; }
        public uint Pack { get; set; }
        public uint Pouch { get; set; }
        public uint Head { get; set; }
        public uint Body { get; set; }
        public uint Hands { get; set; }
        public uint Legs { get; set; }
        public uint Feet { get; set; }
        public uint Waist { get; set; }
        public uint Neck { get; set; }
        public uint RightEar { get; set; }
        public uint LeftEar { get; set; }
        public uint LeftIndex { get; set; }
        public uint RightIndex { get; set; }
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
                    bw.Write(SPMainWeapon);
                    bw.Write(SPSecondaryWeapon);
                    bw.Write(Throwing);
                    bw.Write(Pack);
                    bw.Write(Pouch);
                    bw.Write(Head);
                    bw.Write(Body);
                    bw.Write(Legs);
                    bw.Write(Hands);
                    bw.Write(Feet);
                    bw.Write(Waist);
                    bw.Write(Neck);
                    bw.Write(RightEar);
                    bw.Write(LeftEar);
                    bw.Write(LeftIndex);
                    bw.Write(RightIndex);
                    bw.Write(RightFinger);
                    bw.Write(LeftFinger);                   
                }               
            }
            return result;
        }
    }   
       
}
