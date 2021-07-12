using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    [Serializable]
    public class Job
    {
        public byte Id { get; set; }
        public string Name { get; set; }       
        public bool IsCurrent { get; set; } //the class the player is using 

        public short Level { get; set; } 
        public short LevelCap { get; set; }         
        public long TotalExp { get; set; }      

        public ushort MaxHp { get; set; }
        public ushort MaxMp { get; set; }
        public ushort MaxTp { get; set; }

        public ushort Hp { get; set; }
        public ushort Mp { get; set; }
        public ushort Tp { get; set; }

        public ushort[] Hotbar { get; set; }
               
        public Job(byte id, string name, short maxLevel)
        {
            Id = id;
            Name = name;
            IsCurrent = false;            

            Level = 0; //if the player doesn't have a class, then level is 0.
            LevelCap = maxLevel; //put these default number somewhere else
           
            MaxHp = 300;
            MaxMp = 200;
            MaxTp = 3000; //this was the default max TP value for 1.x

            //can use these fields to start the character with less HP, MP or TP.
            Hp = MaxHp;
            Mp = MaxMp;
            Tp = 0;

            Hotbar = new ushort[0x1d];
            Hotbar[0] = 27039; //remove later
        }

        public static Dictionary<byte, Job> LoadAll()
        {
            Dictionary<byte, Job> jobs = new Dictionary<byte, Job>();

            try
            {
                //get job info from game data
                DataTable jobsTable = GameData.Instance.GetGameData("xtx/text_jobName");                

                //couldn't find a way in the game's files to say which ones of the jobs/classes are disabled, so using this for now
                List<uint> disabledJobs = new List<uint>
                {
                    1, 9, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 25, 26, 27, 28, 37, 38, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58
                };

                foreach (DataRow row in jobsTable.Rows)
                {
                    uint jobId = (uint)row.ItemArray[0];
                    short maxLevel = (short)(disabledJobs.Any(x => x == jobId) ? 255 : 50);
                    string jobName = (string)row.ItemArray[1];
                    jobs.Add((byte)jobId, new Job((byte)jobId, jobName, maxLevel));
                }
            }
            catch (Exception e)
            {
                Log.Instance.Equals(e.Message);                
            }

            return jobs;
        }       

        public static long[] ExpTable = {
                0, 570, 700, 880, 1100, 1500, 1800, 2300, 3200, 4300, 5000,
                5900, 6800, 7700, 8700, 9700, 11000, 12000, 13000, 15000, 16000,
                20000, 22000, 23000, 25000, 27000, 29000, 31000, 33000, 35000, 38000,
                45000, 47000, 50000, 53000, 56000, 59000, 62000, 65000, 68000, 71000,
                74000, 78000, 81000, 85000, 89000, 92000, 96000, 100000, 100000, 110000
        };

        public static Dictionary<byte, ushort> ExpTextIds = new Dictionary<byte, ushort>
        {
            { 2, 33934 },   //Pugilist
            { 3, 33935 },   //Gladiator
            { 4, 33936 },   //Marauder
            { 7, 33937 },   //Archer
            { 8, 33938 },   //Lancer
            { 10, 33939 },  //Sentinel, this doesn't exist anymore but it's still in the files so may as well put it here just in case
            { 22, 33940 },  //Thaumaturge
            { 23, 33941 },  //Conjurer
            { 29, 33945 },  //Carpenter, for some reason there's a a few different messages between 33941 and 33945
            { 30, 33946 },  //Blacksmith
            { 31, 33947 },  //Armorer
            { 32, 33948 },  //Goldsmith
            { 33, 33949 },  //Leatherworker
            { 34, 33950 },  //Weaver
            { 35, 33951 },  //Alchemist
            { 36, 33952 },  //Culinarian
            { 39, 33953 },  //Miner
            { 40, 33954 },  //Botanist
            { 41, 33955 }   //Fisher
        };

        //TODO: find a better way to do this.
        public static Dictionary<ushort, byte> Category = new Dictionary<ushort, byte>
        {
            {402, 2}, {403, 3},{404, 4},{407, 7},{408, 8},                                      //Disciples of War
            {502, 22},{503, 23},                                                                //Disciples of Magic
            {601, 29},{602, 30}, {603, 31},{604, 32},{605, 33},{606, 34},{607, 35},{608, 36},   //Disciples of the Hand
            {701, 39},{702, 40},{703, 41}                                                       //Disciples of the Land
        };

        public static AnimationEffect AnimationEffectId(byte jobId)
        {
            switch (jobId)
            {
                case 0x0f:
                    return AnimationEffect.ChangeTo_MNK;
                case 0x10:
                    return AnimationEffect.ChangeTo_PAL;
                case 0x11:
                    return AnimationEffect.ChangeTo_WAR;
                case 0x12:
                    return AnimationEffect.ChangeTo_BRD;
                case 0x13:
                    return AnimationEffect.ChangeTo_DRG;
                case 0x1a:
                    return AnimationEffect.ChangeTo_BLM;
                case 0x1b:
                    return AnimationEffect.ChangeTo_WHM;
                default:
                    return 0;
            }
        }
    }
}
