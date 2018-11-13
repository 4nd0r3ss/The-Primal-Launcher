using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public struct Opcodes
    {
        public static byte GET_CHARACTERS { get; } = 0x03;
        public static byte SELECT_CHARACTER { get; } = 0x03;
    }
}
