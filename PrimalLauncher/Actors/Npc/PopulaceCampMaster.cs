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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    /// <summary>
    /// TODO: implement favored destination registering;take gil from registering; check if there is any text sheet to be sent;
    /// </summary>
    public class PopulaceCampMaster : PopulaceStandard
    {

        public PopulaceCampMaster() {
            ClassName = GetType().Name;
        }

        public override void talkDefault()
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse("defTalk", new List<object> 
            { 
                User.Instance.Character.Id,
                0, //favored destination aetheryte #
                0, //favored destination
                0, //favored destination
                20, //control what speech npc will say. probably changes after some game event? need to further investigate.
                true //same as above.
            });
        }
    }
}
