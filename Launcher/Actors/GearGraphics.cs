using System;
using System.IO;
using System.Linq;

namespace Launcher
{
    /// <summary>
    /// Keeps a set of the character's currently equipped gear grahic ids.
    /// </summary>
    [Serializable]
    public class GearGraphics
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

        /// <summary>
        /// Stores all default graphic id values to their respective slots. 
        /// </summary>
        /// <param name="graphicId"></param>
        public void SetToSlots(uint[] graphicId)
        {
            MainWeapon = graphicId[1];
            SecondaryWeapon = graphicId[2];
            SPMainWeapon = graphicId[3];
            SPSecondaryWeapon = graphicId[4];
            Throwing = graphicId[5];
            //Pack = graphicId[6];
            Pouch = graphicId[7];
            Head = graphicId[8];
            Body = graphicId[9];
            Legs = graphicId[10];
            Hands = graphicId[11];
            Feet = graphicId[12];
            Waist = graphicId[13];
            Neck = graphicId[14];
            RightEar = graphicId[15];
            LeftEar = graphicId[16];
            LeftIndex = graphicId[17];
            RightIndex = graphicId[18];
            RightFinger = graphicId[19];
            LeftFinger = graphicId[20];
            //Unknown1 = graphicId[21];
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

                    break;
                case 9:
                    Body = ItemGraphics.Body.First(x => x.Key == equipId).Value;
                    break;
                case 10:
                    Body = ItemGraphics.Body.First(x => x.Key == equipId).Value;
                    break;
                case 11:
                    Legs = ItemGraphics.Legs.First(x => x.Key == equipId).Value;
                    break;
                case 12:
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

                    break;
                case 20:

                    break;              
            }



        }
    }
}
