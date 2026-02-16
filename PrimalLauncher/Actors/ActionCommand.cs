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
using System.Xml;
using System.Xml.Linq;

namespace PrimalLauncher
{
    [Serializable]
    public class ActionCommand
    {
        public Command Id { get; set; }
        public uint LevelRequired { get; set; }
        public uint AnimationId { get; set; }
        public ushort TextSheet { get; set; }
        public uint ComboAction { get; set; }
        public int Range { get; set; }

        public ushort MpCost { get; set; }
        public ushort TpCost { get; set; }


        public float CastTime { get; set; } 
        public float RecastTime { get; set; }

        public ActionCommand() { }
        public ActionCommand(XmlNode node, DataRow data)
        {
            Id = (Command)Convert.ToUInt32(data.ItemArray[0]);
            LevelRequired = Convert.ToUInt16(data.ItemArray[3]);
            AnimationId = node.GetAttributeAsUint("animationId");
            TextSheet = node.GetNodeAsUshort("textSheet");
            ComboAction = node.GetAttributeAsUint("animationId");
            Range = node.GetAttributeAsInt("range");
            MpCost = Convert.ToUInt16(data.ItemArray[7]);
            TpCost = Convert.ToUInt16(data.ItemArray[8]);
            CastTime = (float)Convert.ToDouble(data.ItemArray[5]);
            RecastTime = (float)Convert.ToDouble(data.ItemArray[6]);
        }

        public void Execute(ActorBattle actor)
        {
            ActorBattle target = (ActorBattle)actor.GetTargetActor();

            int range = Range > 0 ? Range : actor.CharaWork.CurrentClass.AutoAttackMaxDistance;

            //if target is within range
            if (target != null && actor.GetTargetDistance() <= range && User.Instance.Character.State.Main == MainState.Active)
            {
                if (!target.IsDead() && !actor.IsDead())
                {
                    short attackDamage = 0;
                    var (damage, effectId) = target.CalculateDamage(actor, attackDamage);

                    CommandResult cr = new CommandResult
                    {
                        TargetId = target.Id,
                        TotalPoints = damage,
                        TextSheetId = TextSheet,
                        EffectId = effectId,
                        HitPosition = 1,
                        HitSequence = 1
                    };

                    actor.SendCommandResult(Id, new List<CommandResult> { cr }, AnimationId, senderId: actor.Id);

                    //if any damage was dealt
                    if (damage > 0)
                    {
                        if (!BattleManager.Instance.BattleEngaged)
                            BattleManager.Instance.StartBattle(actor);

                        //TP handling
                        //TODO: check if any action has bonus TP gain and add this case.
                        if (TpCost > 0)
                            actor.AddTp((ushort)(TpCost * -1));
                        else if (Id == Command.PlayerAutoAttack || Id == Command.MonsterAutoAttack)
                            actor.AddTp(100);

                        actor.CharaWork.SetCommandRecast(actor.Id, RecastTime);
                        actor.CharaWork.SetComboAction(actor.Id, ComboAction);
                        target.TakeDamage(actor, damage);
                    }

                    //if target dies, player gets loot rewards
                    if (target.IsDead() && target is Monster monster && actor is PlayerCharacter)
                    {
                        User.Instance.Character.AddLoot(monster.GetLoot());
                    }
                }
                else
                {
                    //change target if current target is dead. if no more targets in enmity table, disengage.
                    actor.Disengage();
                }

                Log.Instance.Info("Actor: 0x" + Id.ToString("X") + ", classid: " + actor.ClassId + " Command: " + Id.ToString());

                if (actor is PlayerCharacter player && !player.IsTutorialComplete) BattleTutorial.Instance.NextTutorial("tp");
            }
            else //target is not within range
            {
                //need to check if actor is caster, is yes, can keep distance.
                if (actor is PlayerCharacter)
                {
                    World.SendTextSheet(0x7F1B);
                }
                else
                {
                    if (target != null && actor.CharaWork.CurrentClass.GetCategory() != JobClassCategory.DoM)
                    {
                        actor.MoveToActor(target);
                    }
                }
            }
        }               
    }
}
