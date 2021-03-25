using System;

namespace Launcher
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
                            mask |= 1 << i;

                        r |= ((uint)f.GetValue(t) & mask) << offset;

                        offset += (int)fieldLength;
                    }
                }

                return r;
            }
        }
    }
}
