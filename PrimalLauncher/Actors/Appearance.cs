/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace PrimalLauncher
{
    /// <summary>
    /// Keeps a set of the character's currently equipped gear grahic ids.
    /// </summary>
    [Serializable]
    public class Appearance
    {
        #region Head       
        public ushort HairStyle { get; set; }
        public ushort HairColor { get; set; }
        public ushort HairHighlightColor { get; set; }
        public ushort HairVariation { get; set; }
        public ushort EyeColor { get; set; }
        public ushort SkinColor { get; set; }
        #endregion

        public Face Face { get; set; }

        public uint BaseModel { get; set; }
        public uint Size { get; set; }
        public uint Voice { get; set; }       

        public uint MainWeapon { get; set; }
        public uint SecondaryWeapon { get; set; }
        public uint SPMainWeapon { get; set; }
        public uint SPSecondaryWeapon { get; set; }
        public uint Throwing { get; set; }
        public uint Pack { get; set; }
        public uint Pouch { get; set; }
        public uint Head { get; set; }
        public uint Body { get; set; }
        public uint Undershirt { get; set; }
        public uint Hands { get; set; }
        public uint Legs { get; set; }
        public uint Undergarment { get; set; }
        public uint Feet { get; set; }
        public uint Waist { get; set; }
        public uint Neck { get; set; }
        public uint RightEar { get; set; }
        public uint LeftEar { get; set; }
        public uint Wrists { get; set; }
        public uint LeftIndex { get; set; }
        public uint RightIndex { get; set; }
        public uint RightFinger { get; set; }
        public uint LeftFinger { get; set; }

        public Appearance()
        {
            Face = new Face();
        }
        public Appearance(DataRow actorGraphics)
        {
            if (actorGraphics != null)
            {
                BaseModel = Convert.ToUInt32(actorGraphics.ItemArray[1]);
                Size = Convert.ToUInt32(actorGraphics.ItemArray[2]);
                MainWeapon = Convert.ToUInt32(actorGraphics.ItemArray[20]);
                SecondaryWeapon = Convert.ToUInt32(actorGraphics.ItemArray[21]);
                SPMainWeapon = Convert.ToUInt32(actorGraphics.ItemArray[22]);
                SPSecondaryWeapon = Convert.ToUInt32(actorGraphics.ItemArray[23]);
                Throwing = Convert.ToUInt32(actorGraphics.ItemArray[24]);
                Pack = Convert.ToUInt32(actorGraphics.ItemArray[25]);
                Pouch = Convert.ToUInt32(actorGraphics.ItemArray[26]);
                Head = Convert.ToUInt32(actorGraphics.ItemArray[27]);
                Body = Convert.ToUInt32(actorGraphics.ItemArray[28]);
                Legs = Convert.ToUInt32(actorGraphics.ItemArray[29]);
                Hands = Convert.ToUInt32(actorGraphics.ItemArray[30]);
                Feet = Convert.ToUInt32(actorGraphics.ItemArray[31]);
                Waist = Convert.ToUInt32(actorGraphics.ItemArray[32]);
                Neck = Convert.ToUInt32(actorGraphics.ItemArray[33]);
                RightEar = Convert.ToUInt32(actorGraphics.ItemArray[34]);
                LeftEar = Convert.ToUInt32(actorGraphics.ItemArray[35]);
                RightIndex = Convert.ToUInt32(actorGraphics.ItemArray[36]);
                LeftIndex = Convert.ToUInt32(actorGraphics.ItemArray[37]);
                RightFinger = Convert.ToUInt32(actorGraphics.ItemArray[38]);
                LeftFinger = Convert.ToUInt32(actorGraphics.ItemArray[39]);
                Voice = Convert.ToUInt32(actorGraphics.ItemArray[19]);
                HairStyle = Convert.ToUInt16(actorGraphics.ItemArray[3]);
                HairHighlightColor = Convert.ToUInt16(actorGraphics.ItemArray[4]);
                HairColor = Convert.ToUInt16(actorGraphics.ItemArray[16]);
                SkinColor = Convert.ToUInt16(actorGraphics.ItemArray[17]);
                EyeColor = Convert.ToUInt16(actorGraphics.ItemArray[18]);
                Face = new Face(actorGraphics);
            }
            else
            {
                Face = new Face();
            }            
        }

        /// <summary>
        /// Stores all default graphic id values to their respective slots. 
        /// </summary>
        /// <param name="graphicId"></param>
        public void SetToSlots(uint[] graphicId, uint underShirtId, uint underGarmentId)
        {
            MainWeapon = graphicId[1];
            SecondaryWeapon = graphicId[2];
            SPMainWeapon = graphicId[3];
            SPSecondaryWeapon = graphicId[4];
            Throwing = graphicId[5];
            //Pack = graphicId[6]; //this breaks the game
            Pouch = graphicId[7];
            Head = graphicId[8];
            Undershirt = ItemGraphics.Body.First(x => x.Key == underShirtId).Value;
            Body = graphicId[9];
            Undergarment = ItemGraphics.Legs.First(x => x.Key == underGarmentId).Value;
            Legs = graphicId[10];
            Hands = graphicId[11];
            Feet = graphicId[12];
            Waist = graphicId[13];
            Neck = graphicId[14];
            RightEar = graphicId[15];
            LeftEar = graphicId[16];
            Wrists = graphicId[17];
            LeftIndex = graphicId[18];
            RightIndex = graphicId[19];
            RightFinger = graphicId[20];
            LeftFinger = graphicId[21];            
            //Unknown2 = graphicId[22];
        }

        /// <summary>
        /// Write object as a byte array. Used in lobby server only.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] result = new byte[0x50];

            using (MemoryStream ms = new MemoryStream(result))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(MainWeapon);           //0
                    bw.Write(SecondaryWeapon);      //1
                    bw.Write(SPMainWeapon);         //2
                    bw.Write(SPSecondaryWeapon);    //3
                    bw.Write(Throwing);             //4
                    bw.Write(Pack);                 //5
                    bw.Write(Pouch);                //6
                    bw.Write(Head);                 //7
                    bw.Write(Body);                 //8
                    bw.Write(Legs);                 //9
                    bw.Write(Hands);                //10
                    bw.Write(Feet);                 //11
                    bw.Write(Waist);                //12
                    bw.Write(Neck);                 //13
                    bw.Write(RightEar);             //14
                    bw.Write(LeftEar);              //15
                    bw.Write(Wrists);               //16                    
                    bw.Write(0);                    //17 //TODO: include an user option to show bracelet graphics in both or one wrist only.
                    bw.Write(RightFinger);          //18
                    bw.Write(LeftFinger);           //19
                    //bw.Write(RightIndex);           //20
                    //bw.Write(LeftIndex);            //21                    
                    
                }
            }
            return result;
        }

        public void Set(byte gearSlot, uint equipId)
        {
            switch (gearSlot)
            {
                case 0:
                    MainWeapon = ItemGraphics.Weapon.First(x => x.Key == equipId).Value;
                    break;
                case 1:
                    SecondaryWeapon = ItemGraphics.Weapon.First(x => x.Key == equipId).Value;
                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:
                    Throwing = ItemGraphics.Throwing.First(x => x.Key == equipId).Value;
                    break;
                case 5:
                    
                    break;
                case 6:

                    break;
                case 7:

                    break;
                case 8:
                    Head = ItemGraphics.Head.First(x => x.Key == equipId).Value;
                    break;
                case 9:
                    Undershirt = ItemGraphics.Body.First(x => x.Key == equipId).Value;
                    break;
                case 10:
                    if (equipId == 0)
                        Body = Undershirt;
                    else
                        Body = ItemGraphics.Body.First(x => x.Key == equipId).Value;
                    break;
                case 11:
                    Undergarment = ItemGraphics.Legs.First(x => x.Key == equipId).Value;
                    break;
                case 12:
                    if (equipId == 0)
                        Legs = Undergarment;
                    else
                        Legs = ItemGraphics.Legs.First(x => x.Key == equipId).Value;
                    break;
                case 13:
                    Hands = ItemGraphics.Hands.First(x => x.Key == equipId).Value;
                    break;
                case 14:
                    Feet = ItemGraphics.Feet.First(x => x.Key == equipId).Value;
                    break;
                case 15:
                    Waist = ItemGraphics.Waist.First(x => x.Key == equipId).Value;
                    break;
                case 16:
                    Neck = ItemGraphics.Neck.First(x => x.Key == equipId).Value;
                    break;
                case 17:                   
                    uint graphId = ItemGraphics.Ears.First(x => x.Key == equipId).Value;
                    LeftEar = graphId;
                    RightEar = graphId;
                    break;
                case 18:
                    
                    break;
                case 19:                   
                    Wrists = ItemGraphics.Wrist.First(x => x.Key == equipId).Value;
                    break;
                case 20:

                    break;
                case 21:
                    RightFinger = ItemGraphics.Finger.First(x => x.Key == equipId).Value;
                    break;
                case 22:
                    LeftFinger = ItemGraphics.Finger.First(x => x.Key == equipId).Value;
                    break;
            }
        }

        public byte[] ToSlotBytes()
        {
            byte[] data = new byte[0x108];

            Dictionary<uint, uint> AppearanceSlots = new Dictionary<uint, uint>
            {
                //slot number, value
                { 0x00, BaseModel },
                { 0x01, Size },
                { 0x02, (uint)(SkinColor | HairColor << 10 | EyeColor << 20) },
                { 0x03, BitField.PrimitiveConversion.ToUInt32(Face) },
                { 0x04, (uint)(HairHighlightColor | HairStyle << 10) },
                { 0x05, Voice },
                { 0x06, MainWeapon },
                { 0x07, SecondaryWeapon },
                { 0x08, SPMainWeapon },
                { 0x09, SPSecondaryWeapon },
                { 0x0a, Throwing },
                { 0x0b, Pack },
                { 0x0c, Pouch },
                { 0x0d, Head },
                { 0x0e, Body },
                { 0x0f, Legs },
                { 0x10, Hands },
                { 0x11, Feet },
                { 0x12, Waist },
                { 0x13, Neck },
                { 0x14, RightEar },
                { 0x15, LeftEar },
                { 0x16, Wrists },
                { 0x17, 0 },
                { 0x18, LeftFinger },
                { 0x19, RightFinger },
                { 0x1a, RightIndex },
                { 0x1b, LeftIndex }
            };

            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (var slot in AppearanceSlots)
                    {
                        writer.Write(slot.Value);
                        writer.Write(slot.Key);
                    }
                }
            }

            data[0x100] = (byte)AppearanceSlots.Count;

            return data;
        }
    }

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

        public Face(byte[] data)
        {
            Characteristics = data[0x0f];
            CharacteristicsColor = data[0x10];
            Type = data[0x0e];
            Ears = data[0x1b];
            Mouth = data[0x1a];
            Features = data[0x19];
            Nose = data[0x18];
            EyeShape = data[0x17];
            IrisSize = data[0x16];
            EyeBrows = data[0x15];
            Unknown = 0;
        }

        public Face(DataRow actorGraphics)
        {
            Characteristics = Convert.ToByte(actorGraphics.ItemArray[7]);
            CharacteristicsColor = Convert.ToByte(actorGraphics.ItemArray[8]);
            Type = Convert.ToByte(actorGraphics.ItemArray[6]);
            Ears = Convert.ToByte(actorGraphics.ItemArray[15]);
            Mouth = Convert.ToByte(actorGraphics.ItemArray[14]);
            Features = Convert.ToByte(actorGraphics.ItemArray[13]);
            Nose = Convert.ToByte(actorGraphics.ItemArray[12]);
            EyeShape = Convert.ToByte(actorGraphics.ItemArray[11]);
            IrisSize = Convert.ToByte(actorGraphics.ItemArray[10]);
            EyeBrows = Convert.ToByte(actorGraphics.ItemArray[9]);
            Unknown = 0;
        }
    }
}
