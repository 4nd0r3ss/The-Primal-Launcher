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
        public uint Class { get; set; }
        public uint LevelRequired { get; set; }
        public uint AnimationId { get; set; }
        public ushort TextSheet { get; set; }
        public uint ComboAction { get; set; }
        public int Range { get; set; }

        public ushort MpCost { get; set; }
        public ushort TpCost { get; set; }


        public float CastTime { get; set; } 
        public float RecastTime { get; set; }

        public bool IsActiveOnly { get; set; }

        public ActionCommand() { }
        public ActionCommand(XmlNode node, DataRow data)
        {
            Id = (Command)Convert.ToUInt32(data.ItemArray[0]);
            Class = node.GetAttributeAsUshort("class");
            LevelRequired = Convert.ToUInt32(data.ItemArray[3]);
            AnimationId = node.GetAttributeAsUint("animationId");
            TextSheet = node.GetAttributeAsUshort("textSheet");
            ComboAction = node.GetAttributeAsUint("comboAction");
            Range = node.GetAttributeAsInt("range");
            MpCost = Convert.ToUInt16(data.ItemArray[7]);
            TpCost = Convert.ToUInt16(data.ItemArray[8]);
            CastTime = (float)Convert.ToDouble(data.ItemArray[5]);
            RecastTime = (float)Convert.ToDouble(data.ItemArray[6]);
        }

        public void Execute(ActorBattle caster, int slot)
        {
            ActorBattle target = (ActorBattle)caster.GetTargetActor();
            int range = Range > 0 ? Range : caster.CharaWork.CurrentClass.AutoAttackMaxDistance;

            if(caster is PlayerCharacter)
            {
                //1. Check if command can be axecuted
                if (IsActiveOnly && User.Instance.Character.State.Main == MainState.Passive)
                {
                    World.SendTextSheet(32503); //That command can only be performed in active mode.
                    return;
                }

                //2. Check if target is in range
                if (caster.GetTargetDistance() > range)
                {
                    World.SendTextSheet(32537); //The target is out of range.
                    return;
                }

                //3. Check if target and caster are both alive
                if (target != null && (target.IsDead() || caster.IsDead()))
                {
                    caster.Disengage();
                    return;
                }
                    
            }
            else if(caster is Monster)
            {
                //1. Check if target and caster are both alive
                if (target.IsDead() || caster.IsDead())
                    caster.Disengage();

                //2. Check if target is in range
                if (caster.GetTargetDistance() > range)
                    caster.MoveToActor(target);                
            }
            else //allies?
            {

            }

            short attackDamage = 300;// caster.CalculateDamageInflicted();
            if (target == caster)
                attackDamage = (short)(attackDamage * -1); //if players are targeting themselves, we add hp instead of damage.

            var (damage, effectId) = target.CalculateDamageTaken(caster, attackDamage);

            CommandResult cr = new CommandResult
            {
                TargetId = target.Id,
                TotalPoints = damage,
                TextSheetId = TextSheet,
                EffectId = effectId,
                HitPosition = 1,
                HitSequence = 1
            };

            caster.SendCommandResult(Id, new List<CommandResult> { cr }, AnimationId, senderId: caster.Id);

            //if it wasn't a miss
            if (damage > 0)
            {
                if (!BattleManager.Instance.BattleEngaged)
                    BattleManager.Instance.StartBattle(caster);
                                
                //TODO: check if any action has bonus TP gain and add this case.
                if (TpCost > 0)
                    caster.AddTp((ushort)(TpCost * -1));
                else if (Id == Command.PlayerAutoAttack || Id == Command.MonsterAutoAttack)
                    caster.AddTp(100); //to be calculated

                if (MpCost > 0)
                    caster.AddMp((short)(MpCost * -1));

                caster.CharaWork.SetCommandRecast(caster.Id, RecastTime, slot);
                caster.CharaWork.SetComboAction(caster.Id, ComboAction);
                target.TakeDamage(caster, damage);
            }

            //if target dies, player gets loot rewards
            if (target.IsDead() && target is Monster monster && caster is PlayerCharacter)
            {
                User.Instance.Character.AddLoot(monster.GetLoot());
            }

            if (caster is PlayerCharacter player && !player.IsTutorialComplete) BattleTutorial.Instance.NextTutorial("tp");
          
        }  
        
   
    }
}
