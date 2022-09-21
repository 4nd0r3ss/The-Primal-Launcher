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

namespace PrimalLauncher
{
    public static class ChatProcessor
    {       

        /// <summary>
        /// Analyzes text sent by the player through the chat window. If it matches what is expected as a command, process the command.
        /// </summary>
        /// <param name="data">The data from the received message packet.</param>
        public static void Incoming(byte[] data)
        {
            //get full packet data as string
            string message = Encoding.ASCII.GetString(data);
            //get message 
            message = message.Substring(0x2c, 0x1fb).Trim(new[] { '\0' }).ToLower(); //0x1fb = max message size

            if (message.Substring(0, 1).Equals(@"\")) //is command
            {
                PlayerCharacter pc = User.Instance.Character;
                string command;
                bool hasParameters = false;
                List<string> parameters = new List<string>();

                if (message.IndexOf(' ') > 0)
                {
                    parameters.AddRange(message.Split(' '));
                    command = parameters[0];
                    parameters.RemoveAt(0);
                }
                else
                    command = message;

                if (parameters.Count > 0)
                    hasParameters = true;

                switch (command)
                {
                    case @"\help":
                        SendMessage(MessageType.System, "Available commands:");
                        SendMessage(MessageType.System, @"\setweather {weather name}");
                        SendMessage(MessageType.System, @"\setmusic {music id}");
                        SendMessage(MessageType.System, @"\setposition {x} {y} {z}");
                        SendMessage(MessageType.System, @"\additem {item name}");
                        SendMessage(MessageType.System, @"\addgil {amount}");
                        SendMessage(MessageType.System, @"\forward {distance}");
                        break;

                    case @"\setweather":
                        string wheatherName = parameters[0].First().ToString().ToUpper() + parameters[0].Substring(1);

                        if (Enum.IsDefined(typeof(Weather), wheatherName))
                        {
                            World.Instance.SetWeather((Weather)Enum.Parse(typeof(Weather), wheatherName));

                            switch (wheatherName)
                            {
                                case "Dalamudthunder":
                                    World.Instance.SetMusic(29); //set music to "Answers", I THINK it was the original track for this weather.
                                    break;
                            }
                        }
                        else
                            SendMessage(MessageType.System, "Requested weather not found.");
                        break;

                    case @"\setmusic":
                        if (byte.TryParse(parameters[0], out byte id))
                            World.Instance.SetMusic(id);
                        else
                            SendMessage(MessageType.System, "Invalid music id.");
                        break;

                    case @"\linkadd":
                        if (parameters.Count > 0)
                        {
                            User.Instance.Character.Linkshell.NpcAddLinkpearl(Convert.ToInt32(parameters[0]));
                        }
                        break;
                    case @"\linknew":
                        if (parameters.Count > 0)
                        {
                            User.Instance.Character.Linkshell.NpcNewMessage(Convert.ToInt32(parameters[0]));
                        }
                        break;

                    case @"\resetlevel":
                        short level = 1;

                        if (hasParameters)
                            Int16.TryParse(parameters[0], out level);

                        pc.LevelDown(level);
                        break;
                    case @"\teleport":
                        if (parameters.Count > 0)
                            World.Instance.TeleportPlayer(Convert.ToUInt32(parameters[0]));
                        break;

                    case @"\setposition":
                        if (parameters.Count > 0)
                        {
                            Position pos = pc.Position;
                            pos.X = Convert.ToSingle(parameters[0]);
                            pos.Y = Convert.ToSingle(parameters[1]);
                            pos.Z = Convert.ToSingle(parameters[2]);

                            if (parameters.Count > 3)
                                pos.R = Convert.ToSingle(parameters[3]);

                            pc.Position = pos;
                            pc.GetPosition(0x11);
                        }
                        break;
                    case @"\setnpcposition":
                        if (parameters.Count > 0)
                        {
                            Actor npc = User.Instance.Character.GetCurrentZone().GetActorByClassId(Convert.ToUInt32(parameters[0]));
                            Position pos = npc.Position;
                            pos.X = Convert.ToSingle(parameters[1]);
                            pos.Y = Convert.ToSingle(parameters[2]);
                            pos.Z = Convert.ToSingle(parameters[3]);

                            if (parameters.Count > 3)
                                pos.R = Convert.ToSingle(parameters[4]);

                            npc.Position = pos;
                            npc.SetPosition();
                        }
                        break;
                    case @"\setnpcanimation":
                        if (parameters.Count > 0)
                        {
                            Actor npc = User.Instance.Character.GetCurrentZone().GetActorByClassId(Convert.ToUInt32(parameters[0]));
                            npc.SubState.MotionPack = Convert.ToUInt16(parameters[1]);
                            npc.Despawn();
                            npc.Spawn();
                        }
                        break;

                    case @"\addloot":
                        Inventory.AddLoot();
                        break;

                    case @"\additem":
                        pc.Inventory.AddItem(InventoryType.Bag,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToInt32(parameters[1]) : 1));
                        break;
                    case @"\addgil":
                        pc.Inventory.AddGil(Convert.ToInt32(parameters[0]));
                        break;

                    case @"\addkeyitem":
                        pc.Inventory.AddItem(InventoryType.KeyItems,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToInt32(parameters[1]) : 1) //TODO: should key items be always 1?
                            );
                        break;

                    case @"\addexp":
                        if (hasParameters)
                            User.Instance.Character.AddExp(Convert.ToInt32(parameters[0]));
                        break;

                    case @"\anim":
                        short animid = 0x29;
                        byte another = 0x04;
                        if (hasParameters)
                            animid = Convert.ToInt16(parameters[0]);

                        if (parameters.Count > 1)
                            another = Convert.ToByte(parameters[1]);

                        byte[] anim = new byte[] { 0x29, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };

                        Buffer.BlockCopy(BitConverter.GetBytes(animid), 0, anim, 0, 2);
                        anim[3] = another;

                        GameServer.Instance.Sender.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.PlayAnimationEffect,
                            Data = anim
                        }).ToBytes());
                        break;

                    case @"\advancequest":
                        if (hasParameters)
                        {
                            User.Instance.Character.Journal.UpdateQuest(Convert.ToUInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                        }
                        break;
                    case @"\map":
                        if (hasParameters)
                        {
                            World.SendTextSheet(25083, new object[] { Convert.ToInt32(parameters[0]) });
                        }
                        break;

                    case @"\forward":
                        if (hasParameters)
                            User.Instance.Character.GoForward(Convert.ToSingle(parameters[0]));
                        break;

                    case @"\turnback":
                        if (hasParameters)
                            User.Instance.Character.TurnBack(Convert.ToSingle(parameters[0]));
                        break;

                    case @"\target":
                        User.Instance.Character.GetTargetData();

                        break;

                    case @"\despawn":
                        if (hasParameters)
                        {
                            Actor actor = User.Instance.Character.GetCurrentZone().Actors.Find(x => x.ClassId == Convert.ToUInt32(parameters[0]));

                            if (actor != null)
                                actor.Despawn();
                            else
                                SendMessage(MessageType.System, "Actor " + parameters[0] + " not found.");
                        }

                        break;

                    case @"\icon":
                        if (hasParameters)
                        {
                            byte b1 = Convert.ToByte(parameters[0]);
                            byte b2 = Convert.ToByte(parameters[1]);
                            byte b3 = Convert.ToByte(parameters[2]);
                            byte b4 = Convert.ToByte(parameters[3]);
                            User.Instance.Character.Icon = (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
                            User.Instance.Character.SetIcon();
                        }
                        break;

                    case @"\dialog":
                        if (hasParameters)
                        {
                            World.Instance.ShowAttentionDialog(new object[] { Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]) });
                            //World.Instance.SendDialogData(new List<object> { "attention", Convert.ToUInt32(parameters[0]), "" }, new object[] { Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]) });
                        }
                        break;

                    case @"\achieve":
                        //if (hasParameters)
                        //{
                        Achievements.UnlockedDialog(0x64);
                        //}
                        break;
                    case @"\questicon":
                        if (hasParameters)
                        {
                            Actor actor = User.Instance.Character.GetCurrentZone().GetActorByClassId(Convert.ToUInt32(parameters[0]));
                            actor.QuestIcon = Convert.ToInt32(parameters[1]);
                            actor.SetQuestIcon();
                        }
                        break;

                    case @"\reloadphase":
                        if (hasParameters)
                        {
                            User.Instance.Character.Journal.ReloadQuestPhase(Convert.ToUInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                        }
                        break;

                    case @"\reloadquest":
                        if (hasParameters)
                        {
                            User.Instance.Character.Journal.ReloadQuest(Convert.ToUInt32(parameters[0]));
                            User.Instance.Character.Journal.ReloadQuestPhase(Convert.ToUInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                            Quest quest = User.Instance.Character.Journal.GetQuestById(Convert.ToUInt32(parameters[0]));
                            SendMessage(MessageType.System, "Phase: " + quest.PhaseIndex);
                            int count = 0;

                            foreach (var step in quest.CurrentPhase.Steps)
                            {
                                SendMessage(MessageType.System, "Step " + count + "" + ", done: " + step.Done + ", " + step.ActorClassId + ", " + step.Event);
                                count++;
                            }
                        }
                        break;

                    case @"\reward":
                        //if (hasParameters)
                        //{
                        byte[] reward = new byte[0x20];
                        Buffer.BlockCopy(BitConverter.GetBytes(110828), 0, reward, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(5), 0, reward, 4, 4);
                        GameServer.Instance.Sender.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)0x1a2,
                            Data = reward
                        }).ToBytes());
                        //}
                        break;

                    case @"\pos":
                        Position mypos = User.Instance.Character.Position;
                        SendMessage(MessageType.System, mypos.X + ", " + mypos.Y + ", " + mypos.Z + ", " + mypos.R);
                        break;

                    case @"\posx":
                        Position myposx = User.Instance.Character.Position;
                        int xx = BitConverter.ToInt32(BitConverter.GetBytes(myposx.X), 0);
                        int yy = BitConverter.ToInt32(BitConverter.GetBytes(myposx.Y), 0);
                        int zz = BitConverter.ToInt32(BitConverter.GetBytes(myposx.Z), 0);
                        int rr = BitConverter.ToInt32(BitConverter.GetBytes(myposx.R), 0);

                        SendMessage(MessageType.System, "0x" + xx.ToString("X2") + ", 0x" + yy.ToString("X2") + ", 0x" + zz.ToString("X2") + ", 0x" + rr.ToString("X2"));
                        break;

                    case @"\spawn":
                        if (hasParameters)
                        {
                            Actor actor = ActorRepository.CreateTestActor(Convert.ToUInt32(parameters[0]));
                            actor.Id = 4 << 28 | User.Instance.Character.Position.ZoneId << 19 | (uint)(User.Instance.Character.GetCurrentZone().Actors.Count + 1);
                            actor.Position = User.Instance.Character.Position;
                            actor.Spawn();
                            User.Instance.Character.GetCurrentZone().Actors.Add(actor);
                        }
                        break;

                    case @"\instance":
                        if (hasParameters)
                        {
                            World.Instance.ToInstance(Convert.ToUInt32(parameters[0]));
                        }
                        break;
                    case @"\head":
                        User.Instance.Character.ToggleHeadDirection();
                        break;
                    case @"\queststep":
                        if (hasParameters)
                        {
                            Quest quest = User.Instance.Character.Journal.GetQuestById(Convert.ToUInt32(parameters[0]));
                            SendMessage(MessageType.System, "Phase: " + quest.PhaseIndex);
                            int count = 0;

                            foreach (var step in quest.CurrentPhase.Steps)
                            {
                                SendMessage(MessageType.System, "Step " + count + "" + ", done: " + step.Done + ", " + step.ActorClassId + ", " + step.Event);
                                count++;
                            }
                        }
                        break;
                    case @"\status":
                        //if (hasParameters)
                        //{
                        byte[] stat = { 0x00, 0x00, 0x36, 0x5A, 0x00, 0x00, 0x00, 0x00 };
                        Packet.Send(ServerOpcode.SetStatus, stat);
                        //}
                        break;
                    case @"\distance":
                        double distance = User.Instance.Character.GetTargetDistance();
                        SendMessage(MessageType.System, distance.ToString());
                        break;
                    case @"\come":
                        ActorBattle bactor = (ActorBattle)User.Instance.Character.GetCurrentZone().GetActorById(User.Instance.Character.CurrentTargetId);

                        if (bactor != null)
                        {
                            bactor.MoveToTarget();
                            Position myposs = User.Instance.Character.Position;
                            SendMessage(MessageType.System, "C: " + myposs.X + ", " + myposs.Z + ", " + myposs.R);
                            SendMessage(MessageType.System, "M: " + bactor.Position.X + ", " + bactor.Position.Z + ", " + bactor.Position.R);
                        }
                        else
                        {
                            SendMessage(MessageType.System, "Select target!");
                        }

                        break;
                    case @"\setr":
                        ActorBattle bactors = (ActorBattle)User.Instance.Character.GetCurrentZone().GetActorById(User.Instance.Character.CurrentTargetId);
                        bactors.Position.R = (float)Convert.ToDouble(parameters[0]);
                        bactors.MoveToPosition(bactors.Position, 2);

                        SendMessage(MessageType.System, "M: " + bactors.Position.X + ", " + bactors.Position.Z + ", " + bactors.Position.R);
                        break;

                    case @"\setcr":
                        if (hasParameters)
                        {
                            Position pos = pc.Position;
                            pos.R = Convert.ToSingle(parameters[0]);
                            pc.Position = pos;
                            pc.GetPosition();
                        }
                        break;
                    case @"\talk":
                        if (hasParameters)
                        {
                            Actor a = World.Instance.GetCurrentZone().GetActorByClassId(Convert.ToUInt32(parameters[0]));

                            if (a != null)
                                a.StartEvent("talkDefault");
                        }
                        break;
                    case @"\dirnotice":
                        var qq = ((QuestDirector)User.Instance.Character.GetCurrentZone().GetDirector("Quest"));

                        if (qq != null)
                            qq.StartEvent("noticeEvent");

                        break;
                    case @"\dir":
                        foreach (Director d in User.Instance.Character.GetCurrentZone().Directors)
                        {
                            if (d is QuestDirector)
                                SendMessage(MessageType.System, d.ClassName + " " + ((QuestDirector)d).QuestName + " 0x" + d.Id.ToString("X2"));
                        }
                        break;
                    case @"\obj":
                        if (hasParameters)
                        {
                            MapObj o = (MapObj)User.Instance.Character.GetCurrentZone().GetActorByClassId(5900001);
                            o.Despawn();
                            o.ObjectId = Convert.ToInt32(parameters[0]);
                            o.Spawn();
                        }
                        break;
                    case @"\objpos":
                        {
                            Actor npc = User.Instance.Character.GetCurrentZone().GetActorByClassId(5900001);
                            Position pos2 = User.Instance.Character.Position;
                            Position pos = npc.Position;
                            pos.X = pos2.X;
                            pos.Y = pos2.Y;
                            pos.Z = pos2.Z;

                            npc.Position = pos;
                            npc.SetPosition();
                            SendMessage(MessageType.System, npc.Position.X + ", " + npc.Position.Y + ", " + npc.Position.Z + ", " + npc.Position.R);
                        }
                        break;
                    case @"\diag":
                        {
                            World.Instance.SendDialogData(new List<object> { "attention", World.Instance.Id, "" }, new object[] { "this is a test" });
                        }
                        break;
                    default:
                        SendMessage(MessageType.System, "Unknown command.");
                        break;
                }
            }
            else
                SendMessage(MessageType.System, "Sorry, nobody is listening. What a lonely life...");
        }

        /// <summary>
        /// Sends a message to the game client. This message will appear in the game chat window.
        /// </summary>
        /// <param name="message">A string containing the message to be sent.</param>
        public static void SendMessage(MessageType type, string message)
        {
            MessagePacket messagePacket = new MessagePacket
            {
                MessageType = type,
                TargetId = User.Instance.SessionId,
                Message = message,
                Opcode = (ushort)ServerOpcode.ChatMessage
            };

            //Log.Instance.Chat("[" + World.Instance.ServerName + "] " + message); //show sent message on launcher output window (TODO: make it optional?)

            Packet packet = new Packet(new SubPacket(messagePacket) { SourceId = User.Instance.SessionId, TargetId = User.Instance.SessionId });
            GameServer.Instance.Sender.Send(packet.ToBytes());
        }
    }
}
