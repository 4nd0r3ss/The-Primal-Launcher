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

namespace PrimalLauncher
{
    [Serializable]
    public class MusicSet
    {
        public uint DayMusic { get; private set; }
        public uint NightMusic { get; private set; }
        public uint BattleMusic { get; private set; }

        private MusicSet(uint day, uint night, uint battle)
        {
            DayMusic = day;
            NightMusic = night;
            BattleMusic = battle;
        }

        private static Dictionary<int, MusicSet> MusicSets { get; } = new Dictionary<int, MusicSet>
        {
            {0, new MusicSet(0, 0, 0)}, //dummy
            {1, new MusicSet(53, 60, 21)},
            {2, new MusicSet(59, 59, 0)}, //limsa
            {3, new MusicSet(55, 55, 15)}, //coerthas
            {4, new MusicSet(52, 52, 13)},
            {5, new MusicSet(51, 51, 0)}, //gridania
            {6, new MusicSet(57, 52, 13)}, //black shroud
            {7, new MusicSet(68, 68, 25)},
            {8, new MusicSet(49, 49, 11)}, //mor dhona
            {9, new MusicSet(7, 0, 0)}, //silence
            {10, new MusicSet(65, 0, 0)},
            {11, new MusicSet(66, 66, 0)}, //uldah
            {12, new MusicSet(2, 2, 0)}, //twin adder
            {13, new MusicSet(3, 3, 0)}, //maelstrom
            {14, new MusicSet(4, 4, 0)}, //immortal flames
            {15, new MusicSet(61, 61, 0)}, //inn
            {16, new MusicSet(51, 51, 13)},
            {17, new MusicSet(62, 67, 25)}, //thanalan
            {18, new MusicSet(97, 97, 97)}, //thornmarch
            {19, new MusicSet(23, 23, 23)}, //bowl of embers
            {20, new MusicSet(104, 104, 104)}, //howling eye
        };

        public static MusicSet Get(int id)
        {
            MusicSet musicSet = new MusicSet(0, 0, 0);

            if (MusicSets.ContainsKey(id))
                musicSet = MusicSets[id];

            return musicSet;
        }
    }  
}
