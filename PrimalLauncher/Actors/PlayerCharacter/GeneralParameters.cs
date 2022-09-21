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

using System.Collections.Generic;
using System.Collections.Specialized;

namespace PrimalLauncher
{
    public static class GeneralParameters
    {
        //static initial attribute values. 
        //TODO: try to find these values in the game data files.
        private static List<ushort[]> Initial = new List<ushort[]>
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

        /// <summary>
        /// This method creates a dictionary with all the attribute points alloted to a slot identified by name. It was modeled like that so the values can be edited.
        /// </summary>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public static OrderedDictionary Get(byte tribeId)
        {
            int index = 0; //midlander male, female

            switch (tribeId)
            {
                case 3: //highlander male
                    index = 1;
                    break;
                case 4: //wildwood male
                case 5: //wildwood female
                    index = 2;
                    break;
                case 6: //duskwight male 
                case 7: //duskwight female
                    index = 3;
                    break;
                case 8: //plainsfolk male
                case 9: //plainsfolk female
                    index = 4;
                    break;
                case 10: //dunesfolk male
                case 11: //dunesfolk female
                    index = 5;
                    break;
                case 12: //seeker of the sun
                    index = 6;
                    break;
                case 13: //keeper of the moon
                    index = 7;
                    break;
                case 14: //sea wolf
                    index = 8;
                    break;
                case 15: //hellsguard     
                    index = 9;
                    break;
            }

            return new OrderedDictionary
            {
                {"unknown1", Initial[index][0]},
                {"unknown2", Initial[index][1]},
                {"unknown3", Initial[index][2]},

                {"strength", Initial[index][3]},
                {"vitality", Initial[index][4]},
                {"dexterity", Initial[index][5]},
                {"intelligence", Initial[index][6]},
                {"mind", Initial[index][7]},
                {"piety", Initial[index][8]},

                {"fire", Initial[index][9]},
                {"ice", Initial[index][10]},
                {"wind", Initial[index][11]},
                {"earth", Initial[index][12]},
                {"lightning", Initial[index][13]},
                {"water", Initial[index][14]},

                {"accuracy", Initial[index][15]},
                {"evasion", Initial[index][16]},
                {"attack", Initial[index][17]},
                {"defense", Initial[index][18]},

                {"unknown4", Initial[index][19]},
                {"unknown5", Initial[index][20]},
                {"unknown6", Initial[index][21]},
                {"unknown7", Initial[index][22]},

                {"attack magic potency", Initial[index][23]},
                {"healing magic potency", Initial[index][24]},
                {"enhance magic potency", Initial[index][25]},
                {"enfeeble magic potency", Initial[index][26]},

                {"magic accuracy", Initial[index][27]},
                {"magic avasion", Initial[index][28]},
            };
        }
    }
}
