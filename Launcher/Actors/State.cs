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
