using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
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

                    //case @"\spawn":
                    //    //_world.ZoneList.GetZone(_user.Character.Position.ZoneId).SpawnActors(sender);
                    //    break;

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

                            pc.Position = pos;
                            pc.GetPosition(sender);
                        }
                        break;

                    case @"\spawn":
                        if (hasParameters)
                        {
                            if (parameters[0] == "antelope")
                                TestPackets.Antelope(pc.Id, pc.Position, sender);
                            else if (parameters[0] == "populace")
                                TestPackets.Populace(pc.Id, pc.Position, sender);
                            else if (parameters[0] == "company")
                                TestPackets.CompanyWarp(pc.Id, pc.Position, sender);

                        }

                        //TestPackets.Antelope(pc.Id, pc.Position, sender);
                        //TestPackets.TeleportInn(UserFactory.Instance.User.Character.Id, UserFactory.Instance.User.Character.Position, sender);                      
                        //Aetheryte ae = new Aetheryte(1280007, 20925, new Position(128, 582.47f, 54.52f, -1.2f, 0f, 0));
                        //_log.Info("sent test");
                        break;

                    case @"\text":

                        data = new byte[] { 0x41, 0x29, 0x9B, 0x02, 0x01, 0x00, 0xF8, 0x5F, 0x89, 0x77, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x02, 0x00, 0x00, 0x6B, 0x1E, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                        if (parameters.Count > 0)
                            Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToUInt32(parameters[0])), 0, data, 0x08, 4);

                        Buffer.BlockCopy(BitConverter.GetBytes(pc.Id), 0, data, 0, 4);

                        GamePacket g = new GamePacket
                        {
                            Opcode = (ushort)ServerOpcode.TextSheetMessage70b,
                            Data = data
                        };

                        SubPacket sb = new SubPacket(g)
                        {
                            SourceId = 0x5ff80001
                        };

                        Packet pk = new Packet(sb);

                        sender.Send(pk.ToBytes());
                        break;

                    case @"\addloot":
                        Inventory.AddLoot(sender);
                        break;

                    case @"\additem":
                        pc.Inventory.AddItem(ref pc.Inventory.Bag,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToUInt32(parameters[1]) : 1),
                            sender);
                        break;

                    case @"\addkeyitem":
                        pc.Inventory.AddItem(ref pc.Inventory.KeyItems,
                            parameters[0].Replace("_", " ").Replace("'", "''"),
                            (parameters.Count > 2 ? Convert.ToUInt32(parameters[1]) : 1), //TODO: should key items be always 1?
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

                    case @"\mapui":
                        uint code = 0;

                        if (hasParameters)
                            code = Convert.ToUInt32(parameters[0]);

                        //User.Instance.Character.ToggleControl(sender, code);
                        break;

                    default:
                        SendMessage(sender, MessageType.System, "Unknown command.");
                        break;
                }
            }
            else
                SendMessage(sender, MessageType.System, "Sorry, no one is listening. What a lonely life...");
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

            Log.Instance.Chat("[" + World.Instance.ServerName + "] " + message); //show sent message on launcher output window (TODO: make it optional?)

            Packet packet = new Packet(new SubPacket(messagePacket) { SourceId = User.Instance.SessionId, TargetId = User.Instance.SessionId });
            sender.Send(packet.ToBytes());
        }
    }
}
