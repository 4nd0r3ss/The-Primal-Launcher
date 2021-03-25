using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
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
            {0, new MusicSet(66, 67, 0)},
            {1, new MusicSet(60, 60, 21)},
            {2, new MusicSet(59, 59, 0)},
            {3, new MusicSet(55, 55, 15)},
            {4, new MusicSet(52, 52, 13)},
            {5, new MusicSet(51, 51, 0)},
            {6, new MusicSet(57, 52, 13)},
            {7, new MusicSet(68, 68, 25)},
            {8, new MusicSet(49, 49, 11)},
            {9, new MusicSet(7, 0, 0)},
            {10, new MusicSet(65, 0, 0)},
            {11, new MusicSet(66, 66, 0)},
            {12, new MusicSet(2, 0, 0)},
            {13, new MusicSet(3, 0, 0)},
            {14, new MusicSet(4, 0, 0)},
            {15, new MusicSet(61, 61, 0)},
            {16, new MusicSet(51, 51, 13)}
        };

        public static MusicSet Get(int id)
        {
            MusicSet musicSet = new MusicSet(0, 0, 0);

            if (MusicSets.ContainsKey(id))
                musicSet = MusicSets[id];

            return musicSet;
        }
    }

    [Serializable]
    public enum MusicMode
    {
        Play = 0x01, //change music immediately
        Crossfade = 0x02,
        Layer = 0x03,
        FadeStart = 0x04,
        ChannelOne = 0x05,
        ChannelTwo = 0x06
    }
}
