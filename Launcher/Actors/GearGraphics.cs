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
    }
}
