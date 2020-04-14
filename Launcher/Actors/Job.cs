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
        public ushort LevelCap { get; set; } 
        public long TotalExp { get; set; }      

        public ushort MaxHp { get; set; }
        public ushort MaxMp { get; set; }
        public ushort MaxTp { get; set; }

        public ushort Hp { get; set; }
        public ushort Mp { get; set; }
        public ushort Tp { get; set; }
               
        public Job(byte id, string name)
        {
            Id = id;
            Name = name;
            IsCurrent = false;            

            Level = -1; //if the player doesn't have a class, then level is -1.
            LevelCap = 50; //put these default number somewhere else
           
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

            foreach (DataRow row in jobsTable.Rows)
            {
                uint jobId = (uint)row.ItemArray[0];
                string jobName = (string)row.ItemArray[1];
                jobs.Add((byte)jobId, new Job((byte)jobId, jobName));
            }

            return jobs;
        }
    }
}
