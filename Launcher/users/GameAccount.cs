using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.users
{
    [Serializable]
    public class GameAccount
    {
        #region Info for packet build
        public static int SIZE = 0x50; //account data chunk size
        public static int OPCODE = 0x0C;
        #endregion

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Character> CharacterList { get; set; } = new List<Character>();
        #endregion
    }
}
