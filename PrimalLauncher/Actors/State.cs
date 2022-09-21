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

namespace PrimalLauncher
{
    [Serializable]
    public class State
    {
        public MainState Main { get; set; } = MainState.Passive;
        public MainStateType Type { get; set; } = MainStateType.Default;

        public byte[] ToBytes()
        {
            byte[] data = new byte[0x08];
            data[0] = (byte)Main;
            data[1] = (byte)Type;

            return data;
        }
    }
}
