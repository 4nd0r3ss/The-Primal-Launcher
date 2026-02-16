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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace PrimalLauncher
{
    [Serializable]
    public class Achievements
    {
        //general
        public int TotalPoints { get; set; }
        public bool[] Unlocked { get; set; } //we store the index on the achievment on the 
        private Queue<int> Latest { get; set; }

        //enemies
        public int DefeatedMonstersTotal { get; set; }
        public List<uint> DefeatedNotoriousMonterIds { get; set; }
        public List<uint> DefeatedPrimalIds { get; set; } //check if it should be added to DefeatedNotoriousMonterIds or kept separate.

        //synth/gather

        //currency
        public int GilFromLeveQuestsTotal { get; set; }
        public int GilFromMonstersTotal { get; set; }
        public int CompanySealsTotal { get; set; }

        //materia
        public int MateriaAffixedTotal { get; set; }
        public int MateriaAssimilatedTotal { get; set; }

        public Achievements()
        {
            Unlocked = new bool[745];
            Latest = new Queue<int>();
            DefeatedNotoriousMonterIds = new List<uint>();
            DefeatedPrimalIds = new List<uint>();
        }

        public void Unlock(int id)
        {
            //get achievement index
            var table = GetAchievementsTable();
            var row = table.Rows.Find(id);
            int rowIndex = row != null ? table.Rows.IndexOf(row) : -1;

            //unlock achievement
            Unlocked[rowIndex] = true;
            //GetUnlocked(); //send packet to re-sync

            //add it to latest list
            AddLatest(id);

            //int a = (int)row.ItemArray[4];

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, data, 0, sizeof(int));
            Packet.Send(ServerOpcode.AchievementUnlocked, data);
        }

        public void CheckForAchieved()
        {
           
        }

        private void AddLatest(int id)
        {
            if (Latest.Count >= 5)
                Latest.Dequeue();

            Latest.Enqueue(id);

            //send packet to re-sync
            //GetLatest();
        }

        private void GetUnlocked()
        {
            byte[] data = new byte[0x80];
            var bs = new BitStream(data);
            Unlocked = new bool[745];

            foreach (bool item in Unlocked)
                bs.WriteBit(true);
           
            Packet.Send(ServerOpcode.AchievementsCompeted, data);
        }

        private void GetTotalPoints()
        {
            byte[] data = new byte[0x08];
            data.Write(0, TotalPoints);
            Packet.Send(ServerOpcode.AchievementPoints, data);
        }

        private void GetLatest()
        {
            if(Latest.Count > 0)
            {
                byte[] data = new byte[0x20];

                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryWriter writer = new BinaryWriter(ms))               
                    foreach (int item in Latest)                   
                        writer.Write(item);

                Packet.Send(ServerOpcode.AchievementsLatest, data);
            }            
        }

        public void Send()
        {
            GetLatest();
            GetTotalPoints();
            GetUnlocked();
        }

        private DataRow GetAchievementDataById(int id)
        {
            var table = GetAchievementsTable();
            return table.Rows.Find(id);
        }

        private DataTable GetAchievementsTable()
        {
            //get achievement data from game file
            DataTable table = GameData.Instance.GetGameData("achievement");
            table.PrimaryKey = new[] { table.Columns["id"] };

            return table;
        }
    }
}
