using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    [Serializable]
    public class Job
    {
        public byte Id { get; set; }
        public string Name { get; set; }       
        public bool IsCurrent { get; set; } //the class the player is using 

        public short Level { get; set; } 
        public short MaxLevel { get; set; } 
        public long TotalExp { get; set; }      

        public ushort MaxHp { get; set; }
        public ushort MaxMp { get; set; }
        public ushort MaxTp { get; set; }

        public ushort Hp { get; set; }
        public ushort Mp { get; set; }
        public ushort Tp { get; set; }
               
        public Job(byte id, string name, short maxLevel)
        {
            Id = id;
            Name = name;
            IsCurrent = false;            

            Level = 0; //if the player doesn't have a class, then level is 0.
            MaxLevel = maxLevel; //put these default number somewhere else
           
            MaxHp = 300;
            MaxMp = 200;
            MaxTp = 3000; //this was the default max TP value for 1.x

            //can use these fields to start the character with less HP, MP or TP.
            Hp = MaxHp;
            Mp = MaxMp;
            Tp = 0;
        }

        public static Dictionary<byte, Job> LoadAll()
        {
            //get job infor from game data
            DataTable jobsTable = GameData.Instance.GetGameData("xtx/text_jobName");
            Dictionary<byte, Job> jobs = new Dictionary<byte, Job>();

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

            return jobs;
        }
    }
}
