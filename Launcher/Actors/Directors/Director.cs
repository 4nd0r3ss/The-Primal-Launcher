using System;
using System.Text;
using System.Net.Sockets;

namespace PrimalLauncher
{
    public class Director : Actor
    {
        public Director()
        {           
            Id = (6 << 28 | User.Instance.Character.GetCurrentZone().Id << 19 | (uint)World.Instance.Directors.Count + 1) ;
            ClassName = GetType().Name;
            ClassCode = 0x30400000;
            Appearance.BaseModel = 0;
            Appearance.Size = 0;
            Appearance.SkinColor = 0;
            Appearance.HairColor = 0;
            Appearance.EyeColor = 0;
            NameId = -1;
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender, spawnType, isZoning);
            SetName(sender);
            SetMainState(sender);
            SetIsZoning(sender);
            SetLuaScript(sender);
            Getwork(sender);
        }

        public void Getwork(Socket sender)
        {
            byte[] data = new byte[0x88];
            string init = "/_init";
            Buffer.BlockCopy(BitConverter.GetBytes(0x8807), 0, data, 0, sizeof(ushort)); //TODO: 88 wrapper byte, 07 total bytes. change this to work property class.
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(init), 0, data, 0x02, init.Length);
            Packet.Send(sender, ServerOpcode.ActorInit, data, Id);
        }

        public void StartEvent(Socket sender, string functionName = null)
        {
            byte[] data = new byte[0x70];
            uint characterId = User.Instance.Character.Id;
            Buffer.BlockCopy(BitConverter.GetBytes(characterId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, data, 0x04, 4);

            Buffer.BlockCopy(BitConverter.GetBytes(0x75dc1705), 0, data, 0x08, 4); //the last byte is the event type (05). the other bytes are unknown.
            Buffer.BlockCopy(BitConverter.GetBytes(ClassCode), 0, data, 0x0c, 4);
            LuaParameters parameters = new LuaParameters();
            parameters.Add(Encoding.ASCII.GetBytes("noticeEvent"));

            if (functionName == null)
                parameters.Add(true);
            else
                parameters.Add(functionName);

            LuaParameters.WriteParameters(ref data, parameters, 0x10);
            Packet.Send(sender, ServerOpcode.StartEvent, data, sourceId: characterId);
        }
    }
}
