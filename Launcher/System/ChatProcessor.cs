using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public static class ChatProcessor
    {       

        /// <summary>
        /// Analyzes text sent by the player through the chat window. If it matches what is expected as a command, process the command.
        /// </summary>
        /// <param name="data">The data from the received message packet.</param>
        public static void Incoming(Socket sender, byte[] data)
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
                        SendMessage(sender, MessageType.System, "Available commands:");
                        SendMessage(sender, MessageType.System, @"\setweather {weather name}");
                        SendMessage(sender, MessageType.System, @"\setmusic {music id}");
                        break;

                    case @"\setweather":
                        string wheatherName = parameters[0].First().ToString().ToUpper() + parameters[0].Substring(1);

                        if (Enum.IsDefined(typeof(Weather), wheatherName))
                        {
                            World.Instance.SetWeather(sender, (Weather)Enum.Parse(typeof(Weather), wheatherName));

                            switch (wheatherName)
                            {
                                case "Dalamudthunder":
                                    World.Instance.SetMusic(sender, 29); //set music to "Answers", I THINK it was the original track for this weather.
                                    break;
                            }
                        }
                        else
                            SendMessage(sender, MessageType.System, "Requested weather not found.");
                        break;

                    case @"\setmusic":
                        if (byte.TryParse(parameters[0], out byte id))
                            World.Instance.SetMusic(sender, id);
                        else
                            SendMessage(sender, MessageType.System, "Invalid music id.");
                        break;

                    case @"\setemote":
                        byte[] emote = new byte[] { 0x00, 0xB0, 0x00, 0x05, 0x41, 0x29, 0x9B, 0x02, 0x6E, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, emote, 0x04, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToByte(parameters[1])), 0, emote, Convert.ToByte(parameters[0]), 1);
                        sender.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.DoEmote,
                            Data = emote
                        }).ToBytes());

                        break;

                    case @"\resetlevel":
                        short level = 1;

                        if (hasParameters)
                            Int16.TryParse(parameters[0], out level);

                        pc.LevelDown(sender, level);
                        break;
                    case @"\teleport":
                        if (parameters.Count > 0)
                            World.Instance.TeleportPlayer(sender, Convert.ToUInt32(parameters[0]));
                        break;

                    case @"\setposition":
                        if (parameters.Count > 0)
                        {
                            Position pos = pc.Position;
                            pos.X = Convert.ToSingle(parameters[0]);
                            pos.Y = Convert.ToSingle(parameters[1]);
                            pos.Z = Convert.ToSingle(parameters[2]);

                            if(parameters.Count > 3)
                                pos.R = Convert.ToSingle(parameters[3]);

                            pc.Position = pos;
                            pc.GetPosition(sender);
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
                            npc.SetPosition(sender);
                        }
                        break;
                    case @"\setnpcanimation":
                        if (parameters.Count > 0)
                        {
                            Actor npc = User.Instance.Character.GetCurrentZone().GetActorByClassId(Convert.ToUInt32(parameters[0]));
                            npc.SubState.MotionPack = Convert.ToUInt16(parameters[1]);
                            npc.SetSubState(sender);
                            npc.SetMainState(sender);
                        }
                        break;

                    case @"\text":
                        //World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessage38b, new byte[] {0x01, 0x00, 0xF8, 0x5F, 0xE2, 0x61, 0x20, 0x00, 0x0A, 0x00, 0x3D, 0xA7, 0x36, 0x00, 0x00, 0x00,
                        //0x01, 0x01, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00 });
                        //World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessage38b, new byte[] {0x01, 0x00, 0xF8, 0x5F, 0xAE, 0x62, 0x20, 0x00, 0x0A, 0x00, 0x2E, 0x17, 0x3B, 0x00, 0x00, 0x00,
                        //0x63, 0x01, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00 });
                        //World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0x01, 0x00, 0xF8, 0x5f, 0xc8, 0x61, 0x20, 0x00 });
                        //World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0xB6, 0xAD, 0xF1, 0xA0, 0x62, 0x01, 0x20, 0x00 });
                        
                        uint qids = Convert.ToUInt32(parameters[0]);


                        object[] toSend = new object[] { "attention", (uint)0x5FF80001, "", qids };

                        byte[] datat = new byte[0xC0];
                        LuaParameters parameterss = new LuaParameters();

                        foreach (object obj in toSend)
                            parameterss.Add(obj);

                        LuaParameters.WriteParameters(ref datat, parameterss, 0);
                        Packet.Send(sender, ServerOpcode.GeneralData, datat);
                        break;

                    case @"\questtext":
                        byte[] datass = new byte[0x18];
                        uint qid = Convert.ToUInt32(parameters[0]);

                        Buffer.BlockCopy(BitConverter.GetBytes(World.Instance.Id), 0, datass, 0, sizeof(uint));
                        Buffer.BlockCopy(BitConverter.GetBytes(qid), 0, datass, 0x04, sizeof(uint)); //sheet#
                        Buffer.BlockCopy(BitConverter.GetBytes(LuaParameters.SwapEndian((uint)110002)), 0, datass, 0x09, sizeof(uint));
                        Buffer.BlockCopy(BitConverter.GetBytes(0x0800000F), 0, datass, 0x0D, sizeof(uint)); //unknown

                        World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource38b, datass);

                        break;

                    case @"\addloot":
                        Inventory.AddLoot(sender);
                        break;

                    case @"\additem":
                        pc.Inventory.AddItem(InventoryType.Bag,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToInt32(parameters[1]) : 1),
                            sender);
                        break;
                    case @"\addgil":
                        pc.Inventory.AddGil(sender, Convert.ToInt32(parameters[0]));                            
                        break;

                    case @"\addkeyitem":
                        pc.Inventory.AddItem(InventoryType.KeyItems,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToInt32(parameters[1]) : 1), //TODO: should key items be always 1?
                            sender);
                        break;

                    case @"\addexp":
                        if (hasParameters)
                            User.Instance.Character.AddExp(sender, Convert.ToInt32(parameters[0]));
                        break;

                    case @"\removeactor":
                        GamePacket gps = new GamePacket
                        {
                            Opcode = 0x7,
                            Data = new byte[8]
                        };
                        Packet packet = new Packet(new SubPacket(gps) { SourceId = User.Instance.Character.Id, TargetId = User.Instance.Character.Id });
                        sender.Send(packet.ToBytes());
                        //UserFactory.Instance.User.Character.SetPosition(sender, ZoneList.EntryPoints.Find(x => x.ZoneId == Convert.ToUInt32(value)), 2, 1);
                        User.Instance.Character.Position = EntryPoints.List.Find(x => x.ZoneId == Convert.ToUInt32(parameters[0]));
                        World.Instance.Initialize(sender);
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

                        sender.Send(new Packet(new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.PlayAnimationEffect,
                            Data = anim
                        }).ToBytes());
                        break;
                   
                    case @"\advancequest":
                        if (hasParameters)
                        {
                            User.Instance.Character.Journal.AdvanceQuestHistory(Convert.ToUInt32(parameters[0]));
                        }
                        break;
                    case @"\resetquest":
                        if (hasParameters)
                        {
                            User.Instance.Character.Journal.ResetQuestHistory(Convert.ToUInt32(parameters[0]));
                        }
                        break;
                    case @"\forward":
                        if (hasParameters)  
                            User.Instance.Character.GoForward(sender, Convert.ToSingle(parameters[0]));                        
                        break;
                    case @"\turnback":
                        if (hasParameters)
                            User.Instance.Character.TurnBack(sender, Convert.ToSingle(parameters[0]));
                        break;
                    
                    case @"\changetalk":
                        if (hasParameters)
                        {
                            Actor actor = User.Instance.Character.GetCurrentZone().GetActorByClassId(1000807);
                            actor.Events.Find(x => x.Name == "talkDefault").Action = "processEvent000_" + parameters[0];
                        }
                        break;                   
                    case @"\target":                        
                        User.Instance.Character.GetTargetData(sender);
                        
                        break;
                    case @"\spawn":
                        if (hasParameters)
                        {
                            Actor actor = ActorRepository.Instance.CreateTestActor(Convert.ToUInt32(parameters[0]));
                            actor.Id = 4 << 28 | User.Instance.Character.Position.ZoneId << 19 | (uint)(User.Instance.Character.GetCurrentZone().Actors.Count + 1);
                            actor.Spawn(sender);
                            User.Instance.Character.GetCurrentZone().Actors.Add(actor);
                        }

                        break;
                    case @"\despawn":
                        if (hasParameters)
                        {
                            Actor actor = User.Instance.Character.GetCurrentZone().Actors.Find(x => x.ClassId == Convert.ToUInt32(parameters[0]));

                            if (actor != null)
                                actor.Despawn(sender);
                            else
                                SendMessage(sender, MessageType.System, "Actor " + parameters[0] + " not found.");
                        }

                        break;
                    case @"\setmap":
                        if (hasParameters)
                        {
                            ZoneSwitch.ChangeMap(sender, parameters[0]);
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
                            User.Instance.Character.SetIcon(sender);
                        }

                        break;

                    default:
                        SendMessage(sender, MessageType.System, "Unknown command.");
                        break;
                }
            }
            else
                SendMessage(sender, MessageType.System, "Sorry, nobody is listening. What a lonely life...");
        }

        /// <summary>
        /// Sends a message to the game client. This message will appear in the game chat window.
        /// </summary>
        /// <param name="message">A string containing the message to be sent.</param>
        public static void SendMessage(Socket sender, MessageType type, string message)
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
            sender.Send(packet.ToBytes());
        }
    }
}
