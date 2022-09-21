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
using System.IO;

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
            //Walking = ActorSpeed.Walking;
            //Running = ActorSpeed.Running;
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
