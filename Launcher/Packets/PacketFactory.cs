using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class PacketFactory
    {
        public static uint PlayerId { get; set; }
        public static uint SourceId { get; set; }        

        public static Packet GetPacket(ushort opcode, byte[] data)
        {
            uint targetId = PlayerId;
            uint sourceId = SourceId != PlayerId ? SourceId : PlayerId;

            GamePacket gamePacket = new GamePacket
            {
                Opcode = opcode,
                Data = data
            };

            return new Packet(new SubPacket(gamePacket) { SourceId = sourceId, TargetId = targetId });
        }
    }
}
