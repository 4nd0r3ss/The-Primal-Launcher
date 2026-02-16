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
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;

namespace PrimalLauncher
{
    [Serializable]
    public class ActorBattle : Actor
    {
        public Dictionary<uint, short> EnmityTable { get; set; }
        public bool DisableAutoAttack { get; set; }
        public bool Immobile { get; set; }        
        public bool IsEngaged { get; set; }
        public bool LockOnTarget { get; set; }       

        public virtual GroupBase BattleGroup { get; set; } //did it like that so we can keep it generic, mobs will have a MonsterGroup or a DutyGroup, player will have a PartyGroup or a DutyGroup.
       
        public ActorBattle()
        {
            EnmityTable = new Dictionary<uint, short>();
        }

        public virtual void Engage(uint attacker) { }

        public virtual void Disengage() { }

        public virtual void AutoAttack()
        {
            if (!DisableAutoAttack)
            {
                var target = ((ActorBattle)base.GetCurrentZone().GetActorById(base.TargetId));

                //need to check if actor is caster, is yes, can keep distance.
                if (target != null && !(this is PlayerCharacter) && CharaWork.CurrentClass.GetCategory() != JobClassCategory.DoM)
                {
                    MoveToActor(target);
                }
            }            
        }


        public Actor GetTargetActor()
        {
            return base.GetCurrentZone().GetActorById(base.TargetId);
        }

        public void ExecuteActionCommand(short commandId)
        {
            ActionCommand action = CharaWork.CurrentClass.Actions.Find(x => x.Id == (Command)commandId);

            if (action != null)
            {
                if(action.CastTime > 0)
                {
                    SetCastBar((uint)commandId, action.CastTime);                    
                    SubState.Chant = 0xF0;
                    SetSubState();

                    Thread.Sleep((int)((action.CastTime - 1) * 1000));

                    SetCastBar();
                    SubState.Chant = 0;
                    SetSubState();
                }                

                action.Execute(this);
            }
            else
            {
                Log.Instance.Error("Command " + commandId + " not found.");
            }
        }

        public virtual void Die() { }

        public virtual void AddTp(ushort amount)
        {            
            CharaWork.CurrentClass.Tp += (short)amount;

            if (CharaWork.CurrentClass.Tp > 3000)
                CharaWork.CurrentClass.Tp = 3000;

            WorkProperties prop = new WorkProperties(Id, "charaWork/stateAtQuicklyForAll");
            prop.Add("charaWork.parameterTemp.tp", CharaWork.CurrentClass.Tp);
            prop.FinishWritingAndSend(Id);
        }

        public void TakeDamage(ActorBattle attacker, short amount)
        {
            AddEnmity(attacker);

            CharaWork.CurrentClass.Hp -= amount;
            CharaWork.CurrentClass.Hp = CharaWork.CurrentClass.Hp < 0 ? (short)0 : CharaWork.CurrentClass.Hp;
            CharaWork.CurrentClass.Tp += 10;

            WorkProperties prop = new WorkProperties(Id, "charaWork/stateAtQuicklyForAll");
            prop.Add("charaWork.parameterSave.hp[0]", CharaWork.CurrentClass.Hp);
            prop.Add("charaWork.parameterTemp.tp", CharaWork.CurrentClass.Tp);
            prop.FinishWritingAndSend(Id);

            if (CharaWork.CurrentClass.Hp <= 0) Die();           
        }

        public (short damage, EffectId effectId) CalculateDamage(ActorBattle attacker, short attackDamage)
        {
            (short damage, EffectId effectId) result = ((short)((attacker is PlayerCharacter) ? 100 : 10), EffectId.HitNormal);
            //temporary while I don't have damage calculations.
            TargetId = attacker.Id;
            LockOnTarget = true;

            return result;
        }

        public void SetEnmity(short amount)
        {
            byte[] data = new byte[0x08];
            if (amount > 0) Buffer.BlockCopy(BitConverter.GetBytes(base.TargetId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(amount), 0, data, 0x04, 2);
            Packet.Send(ServerOpcode.SetEnmity, data, Id);
        }

        public void SetHateType(byte type)
        {
            WorkProperties property = new WorkProperties(Id, @"npcWork/hate");
            property.Add("npcWork.hateType", type);
            property.FinishWritingAndSend(Id);
        }

        public virtual void AddEnmity(ActorBattle attacker)
        {
            if(!(this is PlayerCharacter))
            {
                short enmityAmount;

                //here we check the attackers role to determine the amount of enmity to be added.
                //for now we have hardcoded values only, I'm not considering any enmity calculations/buffs/debuffs.
                if (attacker is Monster monster)
                {
                    if (monster.ClassName.IndexOf("Healer") > 0)
                        enmityAmount = 5;
                    else if (monster.ClassName.IndexOf("Tank") > 0)
                        enmityAmount = 8;
                    else
                        enmityAmount = 3;
                }
                else
                {
                    enmityAmount = attacker.CharaWork.CurrentClass.EnmityGenerated();
                }

                //attacker is already in enmity table
                if (EnmityTable.ContainsKey(attacker.Id))
                    EnmityTable[attacker.Id] += enmityAmount;
                else
                    EnmityTable.Add(attacker.Id, enmityAmount);

                //check if current attacker has highest enmity in table. if yes, change target.
                uint highestEnmityId = EnmityTable.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

                if (highestEnmityId != attacker.Id)
                    base.TargetId = attacker.Id;

                //update enmity indicator
                //SetEnmity(EnmityTable[attacker.Id]); //this is crashing the game. maybe because the value being sent is too high?
            }
        }

        public bool IsDead() => State.Main == MainState.Dead || State.Main == MainState.Dead2;

        public void MoveToTarget()
        {
            if (!Immobile)
            {
                var target = ((ActorBattle)GetCurrentZone().GetActorById(TargetId));

                //if (GetTargetDistance() <= CharaWork.CurrentClass.AutoAttackMaxDistance)
                    MoveToActor(target);
            }                        
        }

        public void FaceTarget()
        {

        }

        public float GetTargetDistance()
        {
            float distance = 0;

            if(base.TargetId > 0)
            {                
                distance = GetActorDistance(base.GetCurrentZone().GetActorById(base.TargetId));
            }  
            
            return distance;
        }

        public float GetActorDistance(Actor actor)
        {
            if (actor != null)
                return (float)Math.Sqrt(Math.Pow(actor.Position.X - Position.X, 2) + Math.Pow(actor.Position.Z - Position.Z, 2));
            else
                return 0;
        }

        /// <summary>
        /// Move actor to another actor's position with a distance equal to maximum attack distance.
        /// From: https://math.stackexchange.com/questions/175896/finding-a-point-along-a-line-a-certain-distance-away-from-another-point
        /// </summary>
        /// <param name="actor"></param>
        public void MoveToActor(Actor actor)
        {
            if (actor != null)
            {
                if (!Immobile)
                {
                    float distance = GetActorDistance(actor);

                    //move to target only if current ditance is greater than attack distance.
                    if (distance > CharaWork.CurrentClass.AutoAttackMaxDistance)
                    {
                        float t = (distance - CharaWork.CurrentClass.AutoAttackMaxDistance) / distance;

                        Position.X = ((1 - t) * Position.X) + (t * actor.Position.X);
                        Position.Z = ((1 - t) * Position.Z) + (t * actor.Position.Z);
                    }
                }               

                //rotation has to be calculated regardless of movement.
                Position.R = CalculateRotation(actor.Position);
                MoveToPosition(Position, 2);
            }           
        }

        public float CalculateRotation(Position target)
        {
            float deltaX = target.X - Position.X;
            float deltaZ = target.Z - Position.Z;
            double radian = Math.Round(Math.Atan(deltaZ / deltaX), 1);
            float rotation = (float)Math.Round(radian + 1.6, 1);
            if (deltaX < 0) rotation = (float)Math.Round(rotation - 3.2, 1);

            return rotation;
        }

        private void TurnToAttacker()
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(base.TargetId), 0, data, 0, 4);
            Packet.Send(ServerOpcode.TurnToTarget, data, Id);
        }

        public void SetCastBar(uint commandId = 0, float castTime = 0)
        {
            WorkProperties properties = new WorkProperties(Id, @"playerWork/castState");

            if (commandId > 0)            
                properties.Add("playerWork.castEndClient", Server.GetTimeStamp(castTime));

            properties.Add("playerWork.castCommandClient", commandId);    
            properties.FinishWritingAndSend();
        }
    }
}
