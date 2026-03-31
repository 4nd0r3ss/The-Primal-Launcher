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
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class Monster : ActorBattle
    {
        public int RespawnDelay { get; set; }
        public bool IsHostile { get; set; } //determine if monster will attack on sight.

        //rewards
        //find a better place for this.
        public int RewardExp { get; set; }
        public int RewardGil { get; set; }
        public int[] RewardItems { get; set; }
        public MonsterGroup Group { get; set; }            
        public Position SpawnPoint { get; set; }

        //from npc base class, only battle npcs will have values.
        public int PartsName { get; set; } //still not clear what this does. maybe should match the number of parts enabled? always 1 so far.
        public bool[] PartsExists { get; set; } = new bool[8]; //enables 
        public int Aggro { get; set; } //it's a byte in lua script but passes an int in function call.
        public int ParameterIndex { get; set; } //index used to unpack parameter lists. Always 0x0A for battle actors, 0 for everything else.
        
        //from chara base class
        public bool EnableBattle { get; set; } //if true, intializes battle stuff for actor
        public bool GameParameter { get; set; } //enable/disable common parameters sync and work sync
        public int EventCommonParameter { get; set; } //this is present but its always 0 and never used. It's passed as a param to NpcBaseClass.initForEventCommon

        public Monster()
        {
            ClassPath = "/chara/npc/monster/";
            ClassCode = 0x30400000;
            State.Type = MainStateType.Monster;
            AutoAttackDelay = 5000;
            Speeds.Walking = ActorSpeed.Walking;
            Speeds.Running = ActorSpeed.Running;
            CharaWork.AddProperties(new byte[] { 0, 1, 2, 3 });
            RewardExp = 49;

            //from base npc
            PartsName = 1;
            PartsExists[0] = true;
            //from base chara
            EnableBattle = true;
            GameParameter = true;

            SpawnPoint = Position;
        }

        public override void Prepare()
        {
            string actorName = GenerateName();
            actorName = actorName.Substring(0, actorName.IndexOf("_") - 1) + actorName.Substring(actorName.IndexOf("_")); //dirty way of removing extra character...           

            LuaParameters = new LuaParameters
            {
                ActorName = actorName,
                ClassName = ClassName,
                ClassCode = ClassCode
            };

            LuaParameters.AddRange(new object[]{
                ClassPath + (!string.IsNullOrEmpty(Family) ? Family + "/" : "") + ClassName,
                false,
                false,
                false,
                false,
                false,
                (int)ClassId,
                GameParameter,
                EnableBattle,
                (int)0x0A,
                Aggro,
                PartsName,
                PartsExists,
                EventCommonParameter
            });
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {            
            Prepare();
            CreateActor(0x08);
            SetEventConditions();
            SetSpeeds();
            SetPosition(spawnType, isZoning);
            SetAppearance();
            SetName();
            SetMainState();
            SetSubState();
            SetAllStatus();
            SetIcon();
            SetIsZoning(false);
            SetLuaScript();
            Init();
            SetEventStatus();
            Spawned = true;
            TargetId = 0;
        }



        public override void AutoAttack()
        {
            //if actor is close enough to target, attack.
            if (TargetId > 0 && GetTargetDistance() <= CharaWork.CurrentJob.AutoAttackMaxDistance)
            {
                ActorBattle target = (ActorBattle)GetCurrentZone().GetActorById(TargetId);

                if (target != null && !target.IsDead() && !IsDead())
                {
                    TurnToTarget();

                    short damageDealt = 10;

                    //0x08000608 - normal hit
                    //0x0800060C - strong hit
                    //0x0800060F - critical hit (shows word critical)
                    //0x0800064C - target has protect

                    CommandResult cr = new CommandResult
                    {
                        TargetId = target.Id,
                        TotalPoints = damageDealt, //damage
                        TextSheetId = 0x765D,
                        EffectId = EffectId.HitNormal,
                        HitPosition = 1,
                        HitSequence = 1
                    };

                    SendCommandResult((Command)0x59DD, new List<CommandResult> { cr }, 0x11001000, senderId: Id);
                    Thread.Sleep(500);//added this here to wait for attack animation to finish before applying damage.
                    AddTp(200);
                    target.TakeDamage(this, damageDealt);
                }
                else
                {
                    Disengage();
                }
            }
            else
            {
                //need to check if actor is caster, is yes, can keep distance.
                //if (!(this is PlayerCharacter)

            }
        }

        public override void Die()
        {
            Thread.Sleep(500);
            SetSubState();
            State.Main = MainState.Dead2;
            SetMainState();
            SetEnmity(-1);

            //this command result makes monster die instantly after killing blow.
            List<CommandResult> commandresults = new List<CommandResult>
            {
                new CommandResult(Id, 0, 0, (EffectId)0x08080604, 1, 1), //die animation
                new CommandResult(Id, 0, 0x75A4, 0, 0, 1), //enemy defeated message                
            };

            SendCommandResult(0, commandresults, 0x7C000062/*, senderId: attacker*/);
            Disengage();           

            if (RespawnDelay > 0) Respawn();
        }

        public override void Engage(uint attacker)
        {
            TargetId = attacker;
            IsEngaged = true;

            if(Group != null)
                Group.Send();

            SetHeadToAttacker();
            ToggleStance(Command.BattleStance);
            SetHateType(3);
            SetEnmity(30);
        }

        public override void Disengage()
        {
            //ToggleStance(Command.NormalStance);
            TargetId = 0;
            IsEngaged = false;
        }

        public void Respawn()
        {
            if(RespawnDelay > 0)
            {
                Task.Run(() =>
                {
                    Thread.Sleep(RespawnDelay * 1000);
                    CharaWork.CurrentClass.Hp = CharaWork.CurrentClass.MaxHp;
                    CharaWork.CurrentClass.Mp = CharaWork.CurrentClass.MaxMp;
                    Despawn();
                    State.Main = MainState.Passive;
                    Position = SpawnPoint.Clone();
                    Spawn();
                    return;
                });
            }
            
        }

        private void SetHeadToAttacker()
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(TargetId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(0x3F800000), 0, data, 0x04, 4); //0x3F800000 always the same number so far
            Packet.Send(ServerOpcode.SetHeadToActor, data, Id);
        }

        private void CastAbility(short id)
        {
            //each ability has its own animation
            //hoofkick = 0x08001E0C

            //figure out cast bar 
            Task.Run(() =>
            {
                Thread.Sleep(3000);
                SubState.Chant = 0xF0;
                SetSubState();
                SendCommandResult((Command)id, new List<CommandResult> { new CommandResult(TargetId, 0, 0x75AE, EffectId.Default, 0, 1) }, 0x6F00000B);
                Thread.Sleep(2000);
                SubState.Chant = 0;
                SetSubState();
                SendCommandResult((Command)id, new List<CommandResult> { new CommandResult(TargetId, 0x31, 0x765D, (EffectId)0x08001E0C, 0, 1) }, 0x21003000);
            });

        }

        public Dictionary<uint, int> GetLoot()
        {
            //itemId, quantity
            return new Dictionary<uint, int> { };
        }
    }
}
