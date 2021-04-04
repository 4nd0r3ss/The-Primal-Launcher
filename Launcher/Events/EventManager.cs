using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Launcher
{
    public class EventManager
    {
        private static EventManager _instance = null;
        private static readonly object _padlock = new object();
        public ClientEventRequest CurrentEvent { get; set; } = null;

        public static EventManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new EventManager();

                    return _instance;
                }
            }
        }

        private EventManager() { }

        public void Process(Socket sender, byte[] data)
        {
            ClientEventRequest eventRequest = new ClientEventRequest(data);
            eventRequest.Process(sender);
            CurrentEvent = eventRequest;
        }


        public void ProcessIncoming(Socket sender, byte[] data)
        {
            CurrentEvent = new ClientEventRequest(data);

            Log.Instance.Warning("Event: " + CurrentEvent.Name + ", Actor: 0x" + CurrentEvent.OwnerId.ToString("X"));

            switch (CurrentEvent.Name)
            {
                case "talkDefault":
                    CurrentEvent.TalkDefault(sender);
                    break;
                case "noticeEvent":
                    CurrentEvent.Process(sender);
                    break;
                case "pushDefault":
                    CurrentEvent.Process(sender);
                    break;
            }




            //PlayerCharacter playerCharacter = User.Instance.Character;
            //string eventType = Encoding.ASCII.GetString(data, 0x21, 14);
            //ushort command = (ushort)(data[0x15] << 8 | data[0x14]);

            ////File.WriteAllBytes(command + ".txt", data);

            ////Log.Instance.Warning("Received event request: 0x12d");
            //Log.Instance.Warning("event: " + eventType + ", command: " + command.ToString("X2"));
            ////File.WriteAllBytes("eventrequest_" + DateTime.Now.Ticks.ToString() + ".txt", data);

            //if (eventType.IndexOf("commandForced") >= 0)
            //{
            //    switch (command)
            //    {
            //        case (ushort)Command.BattleStance:
            //            playerCharacter.State.Main = MainState.Active;
            //            playerCharacter.SetMainState(sender);
            //            playerCharacter.SendActionResult(sender, Command.BattleStance);
            //            playerCharacter.EndClientOrderEvent(sender, eventType);
            //            break;
            //        case (ushort)Command.NormalStance:
            //            playerCharacter.State.Main = MainState.Passive;
            //            playerCharacter.SetMainState(sender);
            //            playerCharacter.SendActionResult(sender, Command.NormalStance);
            //            playerCharacter.EndClientOrderEvent(sender, eventType);
            //            break;
            //        case (ushort)Command.MountChocobo:
            //            playerCharacter.ToggleMount(sender, Command.MountChocobo);
            //            Log.Instance.Success("Player is now mounted.");
            //            break;

            //    }
            //}
            //else if (eventType.IndexOf("commandRequest") >= 0)
            //{
            //    switch (command)
            //    {
            //        case (ushort)Command.UmountChocobo:
            //            playerCharacter.ToggleMount(sender, Command.UmountChocobo);
            //            Log.Instance.Success("Player is now dismounted.");
            //            break;

            //        case (ushort)Command.DoEmote:
            //            Log.Instance.Warning("emote id:" + data[0x45].ToString("X2"));
            //            playerCharacter.DoEmote(sender);
            //            break;

            //        case (ushort)Command.ChangeEquipment:
            //            playerCharacter.ChangeGear(sender, data);
            //            break;
            //        case (ushort)Command.EquipSoulStone:
            //            playerCharacter.EquipSoulStone(sender, data);
            //            break;
            //    }
            //}
            //else if (eventType.IndexOf("commandContent") >= 0)
            //{
            //    switch (command)
            //    {
            //        case (ushort)Command.Teleport:
            //            EventManager.Instance.StartRequest(sender, data);
            //            break;

            //    }
            //}
            //else if (eventType.IndexOf("noticeEvent") >= 0 || eventType.IndexOf("pushDefault") >= 0)
            //{
            //    EventManager.Instance.Process(sender, data);
            //}
            //else if (eventType.IndexOf("talkDefault") >= 0)
            //{
            //    File.WriteAllBytes("talkDefault_eventrequest_" + DateTime.Now.Ticks.ToString() + ".txt", data);
            //}


        }

        public void StartRequest(Socket sender, byte[] data)
        {
            //if(CurrentEvent == null)
            //{
            //    int rawCommand = (data[0x15] << 8 | data[0x14]);

            //    if(Enum.IsDefined(typeof(Command), rawCommand))
            //    {
            //        Command command = (Command)rawCommand;

            //        switch (command)
            //        {
            //            case Command.Teleport: //command teleport
            //                CurrentEvent = new EventMenuTeleport();
            //                break;
            //            default:
            //                CurrentEvent = new Event();
                            
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        Log.Instance.Error("Unknown command 0x" + rawCommand.ToString("X2"));
            //    }               
            //}

            //CurrentEvent.StartRequest(sender, data);
        }

        public void EventResult(Socket sender, byte[] data)
        {
            //CurrentEvent.EventResult(sender, data);
        }
    }

    public class EventMenuTeleport : Event
    {
        byte Step { get; set; } = 1;
        uint RegionSelected { get; set; }
        uint AetheryteSelected { get; set; }

        public EventMenuTeleport() { }

        public override void StartRequest(Socket sender, byte[] data)
        {            
            base.RequestResponsePacket(sender, Command.Teleport, new List<object> { "eventRegion", User.Instance.Character.TotalAnima });
        }

        public override void EventResult(Socket sender, byte[] data)
        {
            int rawCommand = (data[0x15] << 8 | data[0x14]);            
            byte type = data[0x21];            

            if(type == 0) //if zero, it's an int
            {
                List<object> additionalParameters = new List<object>();
                uint selection = (uint)(data[0x22] << 24 | data[0x23] << 16 | data[0x24] << 8 | data[0x25]);

                switch (Step)
                {
                    case 1: //region selected, send aetheryte list
                        Step++;                        
                        RegionSelected = selection;
                        additionalParameters.Add("eventAetheryte");
                        additionalParameters.Add(selection);

                        var regionAetherytes = from a in ActorRepository.Instance.Aetherytes
                                               where a.TeleportMenuPageId == selection
                                               orderby a.TeleportMenuId ascending
                                               select a;

                        foreach (Aetheryte a in regionAetherytes)                                
                            additionalParameters.Add(a.AnimaCost);

                        RequestResponsePacket(sender, Command.Teleport, additionalParameters);
                        break;
                    case 2: //artheryte selected, send confirm dialog
                        Step++;
                        AetheryteSelected = selection;
                        additionalParameters.Add("eventConfirm");
                        additionalParameters.Add(false);
                        additionalParameters.Add(false);
                        additionalParameters.Add(0x02);
                        additionalParameters.Add(0x13883f);
                        additionalParameters.Add(false);
                        RequestResponsePacket(sender, Command.Teleport, additionalParameters);
                        break;
                    case 3: //clicked confirm dialog           
                        if(selection == 1) //selected yes
                        {
                            //teleport sequence
                            User.Instance.Character.PlayAnimationEffect(sender, AnimationEffect.Teleport);
                            //User.Instance.Character.ToggleUIControl(sender, UIControl.Off);
                            SendPacket(sender, ServerOpcode.SetUIControl, new byte[] { 0x14, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 });
                            World.Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, new byte[] { 0x01, 0x00, 0xF8, 0x5F, 0x39, 0x85, 0x20, 0x00 });

                            byte[] endrequest = {
                                0x41, 0x29, 0x9B, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64,
                                0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF2, 0xD4, 0x09, 0x30, 0xA9, 0x09, 0x0A
                            };
                            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, endrequest, 0, 4);
                            SendPacket(sender, ServerOpcode.EndClientOrderEvent,endrequest);                            

                            Aetheryte aeth =  ActorRepository.Instance.Aetherytes.FirstOrDefault(x => x.TeleportMenuPageId == RegionSelected && x.TeleportMenuId == AetheryteSelected);
                            World.Instance.TeleportPlayer(sender, aeth.Position);
                            //User.Instance.Character.ToggleUIControl(sender, UIControl.On);
                        }
                        else //selected no
                        {
                            User.Instance.Character.MapUIChange(sender, 0x16);
                        }

                        break;

                }



                if(Step == 1)
                {
                    
                }
                
            }
            else if(type == 0x05) //it's a null, close aetheryte list window and send region again.
            {
                if(Step == 2)                
                    RequestResponsePacket(sender, (Command)rawCommand, new List<object> { "eventRegion", User.Instance.Character.TotalAnima });                

                Step--;
            }
            
        }
       
    }

    public class Event
    {
        public uint ClientId { get; set; }
        public long ServerId { get; set; }

        public Event(){}

        public virtual void StartRequest(Socket sender, byte[] data)
        {
            throw new NotImplementedException();
        }

        public virtual void Response(Socket sender, byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Finish()
        {
            byte[] data = new byte[0x28];

            //write finish event packet

            return data;
        }
             
        public virtual void RequestResponsePacket(Socket sender, Command command, List<object> additionalparameters)
        {
            byte[] data = new byte[0x90];
            uint parsedCommand = (0xA0F00000 | (ushort)command);
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(parsedCommand), 0, data, 0x04, 4);

            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("commandContent"));
            parameters.Add(Encoding.ASCII.GetBytes("delegateCommand"));
            parameters.Add((Command)parsedCommand);            

            foreach(object i in additionalparameters)           
                parameters.Add(i);            
            
            LuaParameters.WriteParameters(ref data, parameters, 0x08);
            Buffer.BlockCopy(BitConverter.GetBytes(0x0000004053b47a00), 0, data, 0x88, 8);

            SendPacket(sender, ServerOpcode.StartEventRequest, data);
        }

        public virtual void EndClientOrderEvent(Socket sender, string eventType)
        {
            byte[] data = new byte[0x30];

            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(eventType), 0, data, 0x08, eventType.Length);

            Buffer.BlockCopy(BitConverter.GetBytes(0x09d4f200), 0, data, 0x28, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x0a09a930), 0, data, 0x2c, sizeof(uint));

            SendPacket(sender, ServerOpcode.EndClientOrderEvent, data);
        }

        public void SendPacket(Socket sender, ServerOpcode opcode, byte[] data, uint sourceId = 0, uint targetId = 0)
        {            
            sender.Send(new Packet(new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            }).ToBytes());
        }

        public virtual void EventResult(Socket sender, byte[] data)
        {
            throw new NotImplementedException();
        }
    }

    public class ClientEventRequest
    {
        public uint CallerId { get; set; }
        public uint OwnerId { get; set; }

        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

        public byte Code { get; set; }
        public string Name { get; set; }

        public byte[] Data { get; set; }

        public ClientEventRequest(byte[] data)
        {
            CallerId = (uint)(data[0x13] << 24 | data[0x12] << 16 | data[0x11] << 8 | data[0x10]);
            OwnerId  = (uint)(data[0x17] << 24 | data[0x16] << 16 | data[0x15] << 8 | data[0x14]);
            Unknown1 = (uint)(data[0x1b] << 24 | data[0x1a] << 16 | data[0x19] << 8 | data[0x18]);
            Unknown2 = (uint)(data[0x1f] << 24 | data[0x1e] << 16 | data[0x1d] << 8 | data[0x1c]);

            Code = data[0x20];         

            //Event name handling
            byte nameSize;            

            for(nameSize = 1; nameSize < 0x20; nameSize++)           
                if(data[0x20 + nameSize] == 0) break;     

            byte[] nameBytes = new byte[nameSize - 1];
            Array.Copy(data, 0x21, nameBytes, 0, nameSize -1);
            Name = Encoding.ASCII.GetString(nameBytes);

            //Remaining event data
            int dataStart = 0x21 + nameSize;
            Data = new byte[data.Length - dataStart];
            Array.Copy(data, dataStart, Data, 0, Data.Length);
        }

        public void Process(Socket sender)
        {
            var eventOwner = World.Instance.Actors.FirstOrDefault(x => x.Id == OwnerId);

            if (eventOwner != null)
                eventOwner.ProcessEventRequest(sender, this);
        }

        public void TalkDefault(Socket sender)
        {
            List<object> parameters = new List<object>();
            parameters.Add(Encoding.ASCII.GetBytes("talkDefault"));
            parameters.Add(Encoding.ASCII.GetBytes("delegateEvent"));
            parameters.Add(CallerId);
            parameters.Add((uint)0xA0F1ADB1); //talk event command id??
            parameters.Add("processEvent000_14");
            parameters.Add(null);
            parameters.Add(null);
            parameters.Add(null);      
            RequestResponse(sender, 0x05, parameters);
        }

        public virtual void RequestResponse(Socket sender, byte eventType, List<object> additionalparameters)
        {
            byte[] data = new byte[0xb0];            
            Buffer.BlockCopy(BitConverter.GetBytes(CallerId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(OwnerId), 0, data, 0x04, 4);
            data[0x08] = eventType;
            LuaParameters parameters = new LuaParameters();
                       

            foreach (object i in additionalparameters)
                parameters.Add(i);

            LuaParameters.WriteParameters(ref data, parameters, 0x09);     
            SendPacket(sender, ServerOpcode.StartEventRequest, data);
        }

        public void EndClientOrder(Socket sender, string eventType)
        {
            byte[] data = new byte[0x30];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            data[0x08] = 1;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(Name), 0, data, 0x09, Name.Length);
            SendPacket(sender, ServerOpcode.EndClientOrderEvent, data);
            EventManager.Instance.CurrentEvent = null;
        }

        public void SendPacket(Socket sender, ServerOpcode opcode, byte[] data, uint sourceId = 0, uint targetId = 0)
        {
            sender.Send(new Packet(new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            }).ToBytes());
        }
    }

   
}
