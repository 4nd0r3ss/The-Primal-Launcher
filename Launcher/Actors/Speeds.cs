using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    [Serializable]
    public class Speeds
    {
        public ActorSpeed Stopped { get; set; }
        public ActorSpeed Walking { get; set; }
        public ActorSpeed Running { get; set; }
        public ActorSpeed Active { get; set; }

        public byte[] ToBytes()
        {
            byte[] data = new byte[0x88];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((uint)Stopped);
                    bw.Write((uint)0);
                    bw.Write((uint)Walking);
                    bw.Write((uint)1);
                    bw.Write((uint)Running);
                    bw.Write((uint)2);
                    bw.Write((uint)Active);
                    bw.Write((uint)3);
                    bw.Seek(0x80, SeekOrigin.Begin);
                    bw.Write((uint)4);
                }
            }

            return data;
        }

        public void SetMounted()
        {
            Walking = ActorSpeed.WalkingMount;
            Running = ActorSpeed.RunningMount;
        }

        public void SetUnmounted()
        {
            Walking = ActorSpeed.Walking;
            Running = ActorSpeed.Running;
        }
    }
}
