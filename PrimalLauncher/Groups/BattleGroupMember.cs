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
    public class BattleGroupMember
    {
        public ActorBattle Actor { get; set; }
        public int ActionTimer { get; set; }
        public bool IsObjective { get; set; }

        public void UpdateActionTimer()
        {
            if (!Actor.IsDead())
            {
                ActionTimer += BattleManager.Instance.TickIntervalSeconds;

                if (!(Actor is PlayerCharacter)) //and actor is not ranged               
                    Actor.MoveToTarget();

                if (ActionTimer >= Actor.AutoAttackDelay)
                {
                    Actor.AutoAttack();
                    ActionTimer = new Random().Next(0, 300); //TODO: need to calculate this based on [weapon delay] and [char attr speed] (?)
                }
            }
        }
    }
}
