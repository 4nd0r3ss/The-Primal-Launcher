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
using System.Threading;

namespace PrimalLauncher
{
    [Serializable]
    public class ActorBattle : Actor
    {
        public Dictionary<uint, short> EnmityTable { get; set; }
        public bool DisableAutoAttack { get; set; }
        public bool Immobile { get; set; }

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
                ActorBattle target = ((ActorBattle)GetCurrentZone().GetActorById(CurrentTargetId));

                //if actor is close enough to target, attack.
                if (CurrentTargetId > 0 && GetTargetDistance() <= CharaWork.CurrentJob.AutoAttackMaxDistance)
                {
                    if (target != null && !target.IsDead() && !IsDead())
                    {
                        short damageDealt = (short)((this is PlayerCharacter) ? 100 : 10); //to be calculated
                        uint effectId = 0x08000604; //also to be calculated

                        Command command = (this is PlayerCharacter) ? Command.PlayerAutoAttack : Command.MonsterAutoAttack;
                        uint animation = (uint)((this is PlayerCharacter) ? 0x19001000 : 0x11001000);

                        //0x08000608 - normal hit
                        //0x0800060C - strong hit
                        //0x0800060F - critical hit (shows word critical)
                        //0x0800064C - target has protect			

                        CommandResult cr = new CommandResult
                        {
                            TargetId = target.Id,
                            Amount = damageDealt,
                            TextId = 0x765D, //always the same  so far
                            EffectId = effectId, //target hit animation
                            Direction = 1,
                            HitSequence = 1
                        };

                        SendCommandResult(command, new List<CommandResult> { cr }, animation, senderId: Id);
                        Thread.Sleep(500);
                        AddTp(100);
                        target.TakeDamage(this, damageDealt);

                        //this should be in BM?
                        //if (target.CharaWork.CurrentJob.Hp <= 0)
                        //{
                        //    SendCommandResult(0, new List<CommandResult> { new CommandResult(Id, 1, 0x847F, 0, 50, 1) }, 0);
                        //    AddExp(target.Exp);
                        //    Inventory.AddGil(target.Gil);
                        //}            
                    }
                    else
                    {
                        //change target if current target is dead. if no more targets in enmity table, disengage.
                        Disengage();
                    }

                    if (this is PlayerCharacter player && !player.IsTutorialComplete) BattleTutorial.Instance.NextTutorial("tp");
                }
                else
                {
                    //need to check if actor is caster, is yes, can keep distance.
                    if (target != null && !(this is PlayerCharacter) && CharaWork.CurrentJob.GetCategory() != JobClassCategory.DoM)
                    {
                        MoveToActor(target);
                    }
                }
            }            
        }

        public virtual void Die() { }

        public virtual void AddTp(short amount)
        {
            CharaWork.CurrentJob.Tp += amount;
            WorkProperties prop = new WorkProperties(Id, "charaWork/stateAtQuicklyForAll");
            prop.Add("charaWork.parameterTemp.tp", CharaWork.CurrentJob.Tp);
            prop.FinishWritingAndSend(Id);
        }

        public void TakeDamage(ActorBattle attacker, short amount)
        {
            AddEnmity(attacker);

            CharaWork.CurrentJob.Hp -= amount;
            CharaWork.CurrentJob.Hp = CharaWork.CurrentJob.Hp < 0 ? (short)0 : CharaWork.CurrentJob.Hp;
            CharaWork.CurrentJob.Tp += 10;

            WorkProperties prop = new WorkProperties(Id, "charaWork/stateAtQuicklyForAll");
            prop.Add("charaWork.parameterSave.hp[0]", CharaWork.CurrentJob.Hp);
            prop.Add("charaWork.parameterTemp.tp", CharaWork.CurrentJob.Tp);
            prop.FinishWritingAndSend(Id);

            if (CharaWork.CurrentJob.Hp <= 0) Die();
        }

        public void SetEnmity(short amount)
        {
            byte[] data = new byte[0x08];
            if (amount > 0) Buffer.BlockCopy(BitConverter.GetBytes(CurrentTargetId), 0, data, 0, 4);
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
                    enmityAmount = attacker.CharaWork.CurrentJob.EnmityGenerated();
                }

                //attacker is already in enmity table
                if (EnmityTable.ContainsKey(attacker.Id))
                    EnmityTable[attacker.Id] += enmityAmount;
                else
                    EnmityTable.Add(attacker.Id, enmityAmount);

                //check if current attacker has highest enmity in table. if yes, change target.
                uint highestEnmityId = EnmityTable.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

                if (highestEnmityId != attacker.Id)
                    CurrentTargetId = attacker.Id;

                //update enmity indicator
                //SetEnmity(EnmityTable[attacker.Id]); //this is crashing the game. maybe because the value being sent is too high?
            }
        }

        public bool IsDead() => State.Main == MainState.Dead || State.Main == MainState.Dead2;

        public void MoveToTarget()
        {
            if (!Immobile)
            {
                ActorBattle target = ((ActorBattle)GetCurrentZone().GetActorById(CurrentTargetId));

                if (GetTargetDistance() <= CharaWork.CurrentJob.AutoAttackMaxDistance)
                    MoveToActor(target);
            }                        
        }

        public void FaceTarget()
        {

        }

        public float GetTargetDistance()
        {
            float distance = 0;

            if(CurrentTargetId > 0)
            {                
                distance = GetActorDistance(GetCurrentZone().GetActorById(CurrentTargetId));
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

                    //move to target only if current ditance is greter than attack distance.
                    if (distance > CharaWork.CurrentJob.AutoAttackMaxDistance)
                    {
                        float t = (distance - CharaWork.CurrentJob.AutoAttackMaxDistance) / distance;

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
            Buffer.BlockCopy(BitConverter.GetBytes(CurrentTargetId), 0, data, 0, 4);
            Packet.Send(ServerOpcode.TurnToTarget, data, Id);
        }
    }
}
