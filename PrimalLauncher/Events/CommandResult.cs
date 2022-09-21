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

namespace PrimalLauncher
{
    public class CommandResult
    {
        public uint TargetId { get; set; }
        public short Amount { get; set; }
        public ushort TextId { get; set; }
        public uint EffectId { get; set; }
        public byte Direction { get; set; }
        public byte HitSequence { get; set; }

        public CommandResult() { }

        public CommandResult(uint targetId, short amount, ushort textSheetId, uint effectId, byte direction, byte hitSequence)
        {
            TargetId = targetId;
            Amount = amount;
            TextId = textSheetId;
            EffectId = effectId;
            Direction = direction;
            HitSequence = hitSequence;
        }

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
                bw.Write(Direction);
                bw.Write(HitSequence);
            }

            return result;
        }

        public static byte[] ToBytes(List<CommandResult> results)
        {
            byte[] resultBytes = new byte[0xB0];

            int targetActorIndex = 0;
            int amountIndex = 0x28;
            int textSheetIndex = 0x3C;
            int effectIdIndex = 0x50;
            int paramIndex = 0x78;
            int hitNumIndex = 0x82;

            foreach (var result in results)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(result.TargetId), 0, resultBytes, targetActorIndex, sizeof(uint));
                Buffer.BlockCopy(BitConverter.GetBytes(result.Amount), 0, resultBytes, amountIndex, sizeof(ushort));
                Buffer.BlockCopy(BitConverter.GetBytes(result.TextId), 0, resultBytes, textSheetIndex, sizeof(ushort));
                Buffer.BlockCopy(BitConverter.GetBytes(result.EffectId), 0, resultBytes, effectIdIndex, sizeof(uint));
                Buffer.BlockCopy(BitConverter.GetBytes(result.Direction), 0, resultBytes, paramIndex, sizeof(byte));
                Buffer.BlockCopy(BitConverter.GetBytes(result.HitSequence), 0, resultBytes, hitNumIndex, sizeof(byte));

                targetActorIndex += sizeof(uint);
                amountIndex += sizeof(ushort);
                textSheetIndex += sizeof(ushort);
                effectIdIndex += sizeof(uint);
                paramIndex += sizeof(byte);
                hitNumIndex += sizeof(byte);
            }

            return resultBytes;
        }
    }
}
