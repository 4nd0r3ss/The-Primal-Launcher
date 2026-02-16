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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    /// <summary>
    /// This class loads data from actors in the games files. This is so related game data files are accessed only once during the 
    /// program execution.
    /// </summary>
    public class ActorGameFilesData
    {
        private static ActorGameFilesData _instance { get; set; }
        public readonly DataTable _actorsGraphics = GameData.Instance.GetGameData("actorclass_graphic");
        public readonly DataTable _actorsNameIds = GameData.Instance.GetGameData("actorclass");
        public readonly DataTable _actorsNames = GameData.Instance.GetGameData("xtx/displayName");

        public DataRow Graphics { get; set; }
        public int NameId { get; set; }
        public string Name { get; set; }

        public static ActorGameFilesData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ActorGameFilesData();

                return _instance;
            }
        }

        private ActorGameFilesData() { }

        public void LoadActorData(uint classId)
        {
            DataRow[] actorsGraphicsSelect = _actorsGraphics.Select("id = '" + classId + "'");
            Graphics = actorsGraphicsSelect != null && actorsGraphicsSelect.Length > 0 ? actorsGraphicsSelect[0] : null;

            DataRow[] actorsNameIdsSelect = _actorsNameIds.Select("id = '" + classId + "'");
            DataRow actorNameId = actorsNameIdsSelect != null && actorsNameIdsSelect.Length > 0 ? actorsNameIdsSelect[0] : null;
            NameId = actorNameId != null ? Convert.ToInt32(actorNameId.ItemArray[1]) : 0;

            DataRow[] actorsNameSelect = _actorsNames.Select("id = '" + NameId + "'");
            DataRow actorNames = actorsNameSelect != null && actorsNameSelect.Length > 0 ? actorsNameSelect[0] : null;
            Name = (actorNames.ItemArray[1] + "");
        }


    }
}
