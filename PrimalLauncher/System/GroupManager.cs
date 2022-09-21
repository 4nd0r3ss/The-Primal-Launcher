using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class GroupManager
    {
        private static GroupManager _instance = null;
        private static readonly object _padlock = new object();
        public GroupDuty DutyGroup { get; set; }
        public GroupParty PartyGroup { get; set; }
        public GroupMob MobGroup { get; set; }

        private Queue<Tuple<uint, string, int>> ActionQueue { get; set; }

        public bool IsEngaged { get; set; }

        public static GroupManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new GroupManager();

                    return _instance;
                }
            }
        }
    }
}
