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
using System.IO;
using System.Text;
using System.Threading;

namespace PrimalLauncher
{
    [Serializable]
    public class World : Actor
    {
        #region Properties 
        private static World _instance = null;       
        private List<Zone> Zones { get; set;}
        public List<Aetheryte> Aetherytes { get; set;}
        public ZoneInstance ZoneInstance { get; set;}
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
            Aetherytes = ActorRepository.GetAetherytes();
        }   

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor();
            SetSpeeds();
            SetPosition(1, isZoning);
            SetName();
            SetMainState();
            SetIsZoning();
            SetLuaScript();
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

        public void Initialize()
        {
            User.Instance.Character.GetGroups();
            User.Instance.Character.InitializeOpening();
            Zoning(0x01);
            User.Instance.Character.ToggleZoneActors();
        }

        public void ChangeZone(Position position = null, ushort spawnType = 0x10, ushort mapUiChange = 0x02)
        {
            MassDeleteActors();
            MapUIChange(mapUiChange);            
            User.Instance.Character.Position = position;
            Zoning(spawnType, 1);
            User.Instance.SavePlayerCharacter();
        }

        private void Zoning(ushort spawnType = 0x10, ushort isZoning = 0)
        {
            Zone zone = User.Instance.Character.GetCurrentZone();            
            SetMapEnvironment(zone);
            Debug.Spawn();
            Spawn(0x01);
            zone.Spawn();
            User.Instance.Character.Spawn(spawnType, isZoning);
            
            zone.Actors.Add(User.Instance.Character);
        }

        #region World Environment Methods
        private void SetMapEnvironment(Zone zone)
        {
            SetIsZoning();
            SetDalamudPhase();
            SetMusic(zone.GetCurrentBGM());
            SetWeather(Weather.Clear);
            SetMap(zone);
        }

        public void SetDalamudPhase()
        {
            byte[] data = new byte[0x08];
            data[0] = 0xff; //test other values and make enum later
            Packet.Send(ServerOpcode.SetDalamud, data, sourceId: User.Instance.Character.Id);
        }       

        public void SetMusic(uint musicId, MusicMode mode = MusicMode.Play)
        {
            byte[] data = new byte[0x08];            
            data[0] = (byte)musicId; //these numbers will not vary, so no need for blockcopy
            data[0x02] = (byte)mode;
            Packet.Send(ServerOpcode.SetMusic, data);
        }

        public void SetWeather(Weather weather)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)weather), 0, data, 0, sizeof(ushort));
            data[0x02] = 0x02;
            Packet.Send(ServerOpcode.SetWeather, data);           
        }    

        public void SetMap(Zone zone)
        {
            byte[] data = new byte[0x10];
            Buffer.BlockCopy(BitConverter.GetBytes(zone.RegionId), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(zone.Id), 0, data, 0x04, sizeof(uint));            
            data[0x08] = 0x28;
            Packet.Send(ServerOpcode.SetMap, data, sourceId: User.Instance.Character.Id);           
        }
        #endregion

        #region Teleport Player and Zoning Methods
        public void TeleportPlayer(uint zoneId)
        {
            Position entryPoint = EntryPoints.List.Find(x => x.ZoneId == zoneId);

            if (entryPoint == null)
                entryPoint = new Position() { ZoneId = zoneId };

            ChangeZone(position: entryPoint, spawnType: 2);
        }

        public void TeleportPlayer(Position position)
        {
            MassDeleteActors();
            User.Instance.Character.RemoveFromZone();
            User.Instance.Character.Position = position;
            ChangeZone(position: position, spawnType: 2);
        }

        public void RepositionPlayer(Position position)
        {
            MassDeleteActors();
            User.Instance.Character.Position = position;
            ChangeZone(position: position, spawnType: 0x11);
        }

        public void TeleportPlayerToAetheryte(Aetheryte destinationAetheryte)
        {
            if (destinationAetheryte != null)
            {
                Position aethPosition = destinationAetheryte.Position;
                int distanceFromAetheryte = 3;

                if (destinationAetheryte.Type == AetheryteType.Gate)
                    distanceFromAetheryte = 2;
                else if (destinationAetheryte.Type == AetheryteType.Crystal && (aethPosition.ZoneId == 230 || aethPosition.ZoneId == 206 || aethPosition.ZoneId == 175))
                    distanceFromAetheryte = 7;

                Position newPosition = new Position
                {
                    ZoneId = aethPosition.ZoneId,
                    X = aethPosition.X + (float)(distanceFromAetheryte * Math.Sin(aethPosition.R)),
                    Z = aethPosition.Z + (float)(distanceFromAetheryte * Math.Cos(aethPosition.R)),
                    Y = aethPosition.Y,
                    R = aethPosition.R + 0.8f
                };

                TeleportPlayer(newPosition);
            }
            else
                Log.Instance.Error("Something went wrong, aetheryte or gate not found.");
        }

        public Zone GetZone(uint id)
        {
            Zone zone = Zones.Find(x => x.Id == id);

            if (zone == null)
                zone = ZoneInstance;

            return zone;
        }       

        public void MassDeleteActors()
        {            
            Zone zone = User.Instance.Character.GetCurrentZone();

            if(zone.Actors != null && zone.Actors.Count > 0) //anti-crash for debugging
                zone.Actors.ForEach(x => x.Spawned = false); // 'despawn' zone actors.

            Packet.Send(ServerOpcode.MassDeleteEnd, new byte[0x08], User.Instance.Character.Id, User.Instance.Character.Id);
        }

        public void MapUIChange(uint code)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 0, sizeof(uint)); // (0x02 & 0xff);           
            //Buffer.BlockCopy(BitConverter.GetBytes(unknown), 0, data, 0x04, sizeof(uint)); // (0x02 & 0xff);           
            Packet.Send(ServerOpcode.MapUiChange, data);
        }

        public void ToInstance(uint id, ushort spawnType = 0x0F)
        {
            PlayerCharacter pc = User.Instance.Character;
            CreateInstance(id);
            Zones.Add(ZoneInstance);
            pc.Position = ZoneInstance.StartPoint;

            MassDeleteActors();
            MapUIChange(0x02);

            SetMapEnvironment(ZoneInstance);            
            pc.Spawn(spawnType, 1);
            ZoneInstance.Spawn();
            Debug.Spawn();
            Spawn(0x01);
            User.Instance.Character.ToggleZoneActors();
            User.Instance.SavePlayerCharacter(pc);
        }
        #endregion

        #region World Text Sheet Methods
        public static void SendTextSheet(string strParams)
        {
            var paramList = strParams.ParseStringParameters();
            int sheetId = (int)paramList[0];
            paramList.RemoveAt(0);
            SendTextSheet(sheetId, paramList.ToArray());
        }
        public static void SendTextSheet(int sheetNumber, object[] parameters = null, uint source = 0, uint customOwnerId = 0)
        {
            byte writeIndex = 0;
            bool hasSource = source > 0;            
            int totalSize = 8; //world id + textSheet sizes (4 bytes each).
            LuaParameters luaParameters = new LuaParameters { Parameters = parameters };

            if (hasSource)
                totalSize += 4;

            if (parameters != null)  
                totalSize += luaParameters.GetTotalByteSize();
                           
            int mod = totalSize % 8;            
            totalSize += (mod > 0 ? 8 - mod : 0);

            int dataSize = GetTextSheetDataSize(totalSize + 32, hasSource);
            byte[] data = new byte[dataSize];

            using (MemoryStream ms = new MemoryStream(data))
            using(BinaryWriter br = new BinaryWriter(ms))
            {
                if (hasSource)
                    br.Write(User.Instance.Character.Id);

                if(customOwnerId > 0)
                    br.Write(customOwnerId);
                else
                    br.Write(Instance.Id);

                br.Write(0x200000 | sheetNumber);
                writeIndex = (byte)ms.Position;
            }

            if(parameters != null)
                LuaParameters.WriteParameters(ref data, luaParameters, writeIndex);

            Packet.Send(GetTextSheetOpcode(dataSize, hasSource), data, customOwnerId > 0 ? customOwnerId : Instance.Id);
        }

        private static int GetTextSheetDataSize(int totalSize, bool hasSource)
        {
            int dataSize = 0;

            if (hasSource)
            {
                if (totalSize <= 0x30)
                    dataSize = 0x30;
                else if (totalSize > 0x30 && totalSize <= 0x38)
                    dataSize = 0x38;
                else if (totalSize > 0x38 && totalSize <= 0x40)
                    dataSize = 0x40;
                else if (totalSize > 0x40 && totalSize < 0x50)
                    dataSize = 0x50;
                else if (totalSize >= 0x50 && totalSize <= 0x70)
                    dataSize = 0x70;
                else
                    return 0;
            }
            else
            {
                if (totalSize <= 0x28)
                    dataSize = 0x28;
                else if (totalSize > 28 && totalSize <= 0x38)
                    dataSize = 0x38;
                else if (totalSize > 0x38 && totalSize <= 0x48)
                    dataSize = 0x48;
                else if (totalSize > 0x48 && totalSize <= 0x68)
                    dataSize = 0x68;
                else
                    return 0;
            }

            return dataSize;
        }

        private static ServerOpcode GetTextSheetOpcode(int dataSize, bool hasSource)
        {
            string opcodeName = "TextSheet";

            if (!hasSource)
                opcodeName += "NoSource";            

            opcodeName += dataSize.ToString("X") + "b";

            return (ServerOpcode)Enum.Parse(typeof(ServerOpcode), opcodeName);
        }
        #endregion

        #region Text dialogs        
        public void OpenTutorialWidget(object[] parameters) => SendDialogData(new List<object> { 0x04, null, null }, parameters);     

        public void ShowSuccessDialog(object[] parameters) => SendDialogData(new List<object> { 0x02, null, null }, parameters);

        public void CloseTutorialWidget() => SendData(new object[] { 0x05 });

        
        #endregion


        public void GMActiveRequest()
        {
            byte[] data = new byte[0x08];
            Packet.Send(ServerOpcode.GMTicketActiveRequest, data);
        }    

        public ZoneInstance CreateInstance(uint zoneId)
        {
            if (ZoneInstance != null && ZoneInstance.Id == zoneId)
                return ZoneInstance;

            ZoneInstance = ZoneRepository.GetInstance(zoneId);    
            return ZoneInstance;
        }

        public void StartEvent(byte[] packet, string functionName = null)
        {
            byte[] data = new byte[0x70];
            uint characterId = User.Instance.Character.Id;
            Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(0xA0F1ADB6), 0, data, 0x04, 4);

            Buffer.BlockCopy(BitConverter.GetBytes(0x05), 0, data, 0x08, 4); //the last byte is the event type (05). the other bytes are unknown.
            Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);
            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("noticeEvent"));


            //parameters.Add("");
            List<object> parameters2 = LuaParameters.ReadParameters(packet, 0x41);

            parameters.AddRange(parameters2.ToArray());

            LuaParameters.WriteParameters(ref data, parameters, 0x10);
            Packet.Send(ServerOpcode.StartEvent, data, sourceId: characterId);
        }
    }    
}
