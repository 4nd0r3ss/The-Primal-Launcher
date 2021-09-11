using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;

namespace PrimalLauncher
{
    [Serializable]
    public class World : Actor
    {
        #region Properties 
        private static World _instance = null;       
        public List<Zone> Zones { get; set;}
        public List<Director> Directors { get; set; } = new List<Director>();
        public Debug Debug { get; set; } = new Debug();       

        public static World Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new World();

                return _instance;
            }
        }
        #endregion
        
        private World()
        {
            Id = 0x5ff80001;
            Zones = ZoneRepository.GetZones();           
            Name = Encoding.ASCII.GetBytes("worldMaster");     
        }   

        public override void Spawn(Socket handler, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(handler);
            SetSpeeds(handler);
            SetPosition(handler, 1, isZoning);
            SetName(handler);
            SetMainState(handler);
            SetIsZoning(handler);
            SetLuaScript(handler);
        }

        public override void Prepare()
        {
            LuaParameters = new LuaParameters { ActorName = "worldMaster", ClassName = "WorldMaster", ClassCode = 0x30400000 };
            LuaParameters.Add("/World/WorldMaster_event");
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(false);
            LuaParameters.Add(null);            
        }
        
        public void Initialize(Socket sender)
        {
            PlayerCharacter playerCharacter = User.Instance.Character;
            Zone zone = playerCharacter.GetCurrentZone();
            
            playerCharacter.GetGroups(sender);           
            SetMapEnvironment(sender, zone);
            playerCharacter.InitializeOpening(sender);
            zone.LoadActors();

            playerCharacter.Spawn(sender, spawnType: 0x01, isZoning: 0);
            zone.Spawn(sender);
            Debug.Spawn(sender);
            Spawn(sender, 0x01);

            if(zone.Id == 0xF4) //if it's inn
            {
                User.Instance.Character.SetUnendingJourney(sender);
                User.Instance.Character.SetEntrustedItems(sender);
            }
            
            User.Instance.Character.Journal.InitializeQuests(sender);
            User.Instance.Character.ToggleZoneActors(sender);
        }

        #region World Environment Methods
        private void SetMapEnvironment(Socket sender, Zone zone)
        {
            SetIsZoning(sender);
            SetDalamudPhase(sender);
            SetMusic(sender, zone.GetCurrentBGM());
            SetWeather(sender, Weather.Clear);
            SetMap(sender, zone);
        }

        public void SetDalamudPhase(Socket sender)
        {
            byte[] data = new byte[0x08];
            data[0] = 0xff; //test other values and make enum later
            Packet.Send(sender, ServerOpcode.SetDalamud, data, sourceId: User.Instance.Character.Id);
        }       

        public void SetMusic(Socket sender, uint musicId, MusicMode mode = MusicMode.Play)
        {
            byte[] data = new byte[0x08];            
            data[0] = (byte)musicId; //these numbers will not vary, so no need for blockcopy
            data[0x02] = (byte)mode;
            Packet.Send(sender, ServerOpcode.SetMusic, data);
        }

        public void SetWeather(Socket sender, Weather weather)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)weather), 0, data, 0, sizeof(ushort));
            data[0x02] = 0x02;
            Packet.Send(sender, ServerOpcode.SetWeather, data);           
        }    

        public void SetMap(Socket sender, Zone zone)
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(zone.RegionId), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(zone.Id), 0, data, 0x04, sizeof(uint));            
            data[0x08] = 0x28;
            Packet.Send(sender, ServerOpcode.SetMap, data, sourceId: User.Instance.Character.Id);           
        }
        #endregion

        public void TeleportPlayer(Socket sender, uint zoneId)
        {
            Position entryPoint = EntryPoints.List.Find(x => x.ZoneId == zoneId);

            if (entryPoint == null)
                entryPoint = new Position() { ZoneId = zoneId };

            ChangeZone(sender, position: entryPoint, spawnType: 2);
        }

        public void TeleportPlayer(Socket sender, Position position)
        {
            MassDeleteActors(sender);
            User.Instance.Character.Position = position;
            ChangeZone(sender, position: position, spawnType: 2);
        }

        public void RepositionPlayer(Socket sender, Position position)
        {
            MassDeleteActors(sender);
            User.Instance.Character.Position = position;
            ChangeZone(sender, position: position, spawnType: 0x11);
        }

        public void TeleportPlayerToAetheryte(Socket sender, Aetheryte destinationAetheryte)
        {
            if (destinationAetheryte != null)
            {
                Position aethPosition = destinationAetheryte.Position;
                Position newPosition = new Position
                {
                    ZoneId = aethPosition.ZoneId,
                    X = aethPosition.X + (float)(7 * Math.Sin(aethPosition.R)),
                    Z = aethPosition.Z + (float)(7 * Math.Cos(aethPosition.R)),
                    Y = aethPosition.Y,
                    R = aethPosition.R + 0.8f
                };

                TeleportPlayer(sender, newPosition);
            }
            else
                Log.Instance.Error("Something went wrong, aetheryte or gate not found.");
        }

        public Zone GetZone(uint id) => Zones.Find(x => x.Id == id);

        public void ChangeZone(Socket sender, Position position = null, ushort spawnType = 0x10, ushort mapUiChange = 0x02)
        {                 
            MassDeleteActors(sender);
            MapUIChange(sender, mapUiChange);
            PlayerCharacter pc = User.Instance.Character;

            pc.Position = position;
            Zone toZone = Zones.Find(x => x.Id == position.ZoneId);   
               
            SetMapEnvironment(sender, toZone);
            pc.Spawn(sender, spawnType, 1);
            toZone.Spawn(sender);
            Debug.Spawn(sender);
            Spawn(sender, 0x01);
            toZone.LoadActors();

            if (toZone.Id == 0xF4) //if it's inn
            {
                pc.SetUnendingJourney(sender);
                pc.SetEntrustedItems(sender);                
            }

            pc.Journal.InitializeQuests(sender);
            pc.ToggleZoneActors(sender);
            User.Instance.AccountList[0].CharacterList[pc.Slot] = pc;
            User.Instance.Save();
        }

        public Director GetDirector(string directorName) => Directors.Find(x => x.GetType().Name == directorName + "Director");        

        public void MassDeleteActors(Socket sender)
        {            
            Zone zone = User.Instance.Character.GetCurrentZone();

            if(zone.Actors != null && zone.Actors.Count > 0) //anti-crash for debugging
                zone.Actors.ForEach(x => x.Spawned = false); // 'despawn' zone actors.

            Packet.Send(sender, ServerOpcode.MassDeleteEnd, new byte[0x08], User.Instance.Character.Id, User.Instance.Character.Id);
        }

        public void MapUIChange(Socket sender, uint code)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 0, sizeof(uint)); // (0x02 & 0xff);           
            //Buffer.BlockCopy(BitConverter.GetBytes(unknown), 0, data, 0x04, sizeof(uint)); // (0x02 & 0xff);           
            Packet.Send(sender, ServerOpcode.MapUiChange, data);
        }

        #region World Text Sheet Methods
        public void SendTextSheetMessage(Socket sender, uint sheetNumber, LuaParameters parameters)
        {
            byte[] data = new byte[0x30];
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(sheetNumber), 0, data, 0x04, sizeof(uint)); //sheet#
            LuaParameters.WriteParameters(ref data, parameters, 0x08);
            Packet.Send(sender, ServerOpcode.TextSheetMessageNoSource48b, data);
        }

        public void SendTextSheetMessage(Socket sender, ServerOpcode opcode, byte[] data) => Packet.Send(sender, opcode, data, Id);

        public void SendTextSheetMessage(Socket sender, uint sheetNumber, uint questId)
        {
            byte[] data = new byte[0x18];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(sheetNumber), 0, data, 0x04, sizeof(uint)); //sheet#
            Buffer.BlockCopy(BitConverter.GetBytes(LuaParameters.SwapEndian(questId)), 0, data, 0x09, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x0800000F), 0, data, 0x0D, sizeof(uint)); //unknown

            Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource38b, data);
        }

        public void SendTextQuestUpdated(Socket sender, uint questId)
        {
            byte[] data = new byte[0x18];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x20621C), 0, data, 0x04, sizeof(uint)); //sheet#
            Buffer.BlockCopy(BitConverter.GetBytes(LuaParameters.SwapEndian(questId)), 0, data, 0x09, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x0800000F), 0, data, 0x0D, sizeof(uint)); //unknown

            Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource38b, data);
        }

        public void SendTextEnteredDuty(Socket sender)
        {
            byte[] data = new byte[0x08];

            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x20853C), 0, data, 0x04, sizeof(uint)); //sheet#

            Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessageNoSource28b, data);
        }

        public void SendTextLeaveInstance(Socket sender)
        {
            byte[] data = new byte[0x10];

            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(0x20853D), 0, data, 0x08, sizeof(uint)); //sheet#

            Instance.SendTextSheetMessage(sender, ServerOpcode.TextSheetMessage30b, data);
        }


        #endregion

        public void GMActiveRequest(Socket sender)
        {
            byte[] data = new byte[0x08];
            Packet.Send(sender, ServerOpcode.GMTicketActiveRequest, data);
        }    
    }    
}
