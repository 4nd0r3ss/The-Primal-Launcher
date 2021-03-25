using System.IO;

namespace Launcher
{
    public class CommandResult
    {
        public uint TargetId { get; set; }
        public ushort Amount { get; set; }
        public ushort TextId { get; set; }
        public uint EffectId { get; set; }
        public byte Param { get; set; }
        public byte Sequence { get; set; }

        public byte[] ToBytes()
        {
            byte[] result = new byte[0x0e];

            using (MemoryStream ms = new MemoryStream(result))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(TargetId);
                bw.Write(Amount);
                bw.Write(TextId);
                bw.Write(EffectId);
                bw.Write(Param);
                bw.Write(Sequence);
            }

            return result;
        }
    }
}
