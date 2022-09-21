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
    class BitField
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class BitfieldLengthAttribute : Attribute
        {
            public uint Length { get; }
            public BitfieldLengthAttribute(uint length) => Length = length;           
        }

        public static class PrimitiveConversion
        {
            public static uint ToUInt32<T>(T t) where T : struct
            {
                uint r = 0;
                int offset = 0;

                // For every field suitably attributed with a BitfieldLength
                foreach (System.Reflection.FieldInfo f in t.GetType().GetFields())
                {
                    object[] attrs = f.GetCustomAttributes(typeof(BitfieldLengthAttribute), false);
                    if (attrs.Length == 1)
                    {
                        uint fieldLength = ((BitfieldLengthAttribute)attrs[0]).Length;

                        // Calculate a bitmask of the desired length
                        uint mask = 0;
                        for (int i = 0; i < fieldLength; i++)
                            mask |= (uint)1 << i;

                        r |= ((uint)f.GetValue(t) & mask) << offset;

                        offset += (int)fieldLength;
                    }
                }

                return r;
            }

            public static long ToLong<T>(T t) where T : struct
            {
                long r = 0;
                int offset = 0;

                // For every field suitably attributed with a BitfieldLength
                foreach (System.Reflection.FieldInfo f in t.GetType().GetFields())
                {
                    object[] attrs = f.GetCustomAttributes(typeof(BitfieldLengthAttribute), false);
                    if (attrs.Length == 1)
                    {
                        uint fieldLength = ((BitfieldLengthAttribute)attrs[0]).Length;

                        // Calculate a bitmask of the desired length
                        long mask = 0;
                        for (int i = 0; i < fieldLength; i++)
                            mask |= (uint)1 << i;

                        r |= ((uint)f.GetValue(t) & mask) << offset;

                        offset += (int)fieldLength;
                    }
                }

                return r;
            }
        }
    }
}
