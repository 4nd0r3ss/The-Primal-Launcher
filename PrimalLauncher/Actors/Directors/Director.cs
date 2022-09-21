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
using System.Text;

namespace PrimalLauncher
{
    public class Director : Actor
    {
        public Director()
        {
            Zone zone = User.Instance.Character.GetCurrentZone();
            Id = (6 << 28 | zone.Id << 19 | (uint)zone.Directors.Count + 1) ;
            ClassName = GetType().Name;
            ClassCode = 0x30400000;
            Appearance.BaseModel = 0;
            Appearance.Size = 0;
            Appearance.SkinColor = 0;
            Appearance.HairColor = 0;
            Appearance.EyeColor = 0;
            NameId = -1;
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor();
            SetEventConditions();
            SetSpeeds();
            SetPosition(spawnType, isZoning);
            SetName();
            SetMainState();
            SetIsZoning();
            SetLuaScript();
            Getwork();
            Spawned = true;
        }

        public void Getwork()
        {
            byte[] data = new byte[0x88];
            string init = "/_init";
            Buffer.BlockCopy(BitConverter.GetBytes(0x8807), 0, data, 0, sizeof(ushort)); //TODO: 88 wrapper byte, 07 total bytes. change this to work property class.
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(init), 0, data, 0x02, init.Length);
            Packet.Send(ServerOpcode.ActorInit, data, Id);
        }

        //public void StartEvent(string functionName = null)
        //{
        //    Log.Instance.Warning("Director.StartEvent()");

        //    byte[] data = new byte[0x70];
        //    uint characterId = User.Instance.Character.Id;
        //    Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
        //    Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);

        //    Buffer.BlockCopy(BitConverter.GetBytes(0x75dc1705), 0, data, 0x08, 4); //the last byte is the event type (05). the other bytes are unknown.
        //    Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);
        //    LuaParameters parameters = new LuaParameters();
        //    parameters.Add(Encoding.ASCII.GetBytes("noticeEvent"));

        //    if (functionName == null)
        //        parameters.Add(false);
        //    else
        //        parameters.Add(functionName);

        //    LuaParameters.WriteParameters(ref data, parameters, 0x10);
        //    Packet.Send(ServerOpcode.StartEvent, data, sourceId: characterId);
        //}
    }
}
