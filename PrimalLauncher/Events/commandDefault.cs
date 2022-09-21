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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class commandDefault : EventRequest
    {
        public short CommandId { get; set; }

        public commandDefault(byte[] data) : base(data)
        {
            CommandId = (short)(data[0x15] << 8 | data[0x14]);            
            InitLuaParameters();
        }

        public override void Execute()
        {
            Log.Instance.Warning("commandDefault: 0x" + CommandId.ToString("X2") + ", " + CommandId);

            
            User.Instance.Character.ExecuteBattleCommand(CommandId);
            Finish();
        }
    }
}
