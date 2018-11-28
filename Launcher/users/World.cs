using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.users
{
    [Serializable]
    public class World
    {
        #region For packet build
        public static int SIZE = 0x50; //account data chunk size
        public static byte OPCODE = 0x15;
        #endregion

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        #endregion
    }
}
