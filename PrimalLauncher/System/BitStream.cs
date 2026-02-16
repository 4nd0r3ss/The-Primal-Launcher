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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public sealed class BitStream
    {
        private readonly byte[] _data;
        private int _bitPos;

        public BitStream(byte[] data)
        {
            _data = data;
        }

        public int Position => _bitPos;

        public bool ReadBit()
        {
            int byteIndex = _bitPos >> 3;
            int bitIndex = _bitPos & 7;

            bool bit = ((_data[byteIndex] >> bitIndex) & 1) != 0;
            _bitPos++;

            return bit;
        }

        public void WriteBit(bool value)
        {
            int byteIndex = _bitPos >> 3;
            int bitIndex = _bitPos & 7;

            if (value)
                _data[byteIndex] |= (byte)(1 << bitIndex);
            else
                _data[byteIndex] &= (byte)~(1 << bitIndex);

            _bitPos++;
        }

        public void Seek(int bitPosition)
        {
            _bitPos = bitPosition;
        }
    }
}
