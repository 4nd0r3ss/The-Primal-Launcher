using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Characters
{
    [Serializable]
    public struct Item
    {
        public uint ItemId { get; set; }
        public uint Unknown1 { get; set; } //always 0
        public uint Quantity { get; set; }
        public uint Category { get; set; }

        public ulong Unknown2 { get; set; } //only first byte has value
        public ulong Unknown3 { get; set; } //always 0

        public ulong Unknown4 { get; set; } //always 0
        public byte HQ { get; set; } //can be 1 or 2
        public byte Unknown5 { get; set; }
        public ushort Unknown6 { get; set; }
        public ushort Unknown7 { get; set; }
        public ushort Unknown8 { get; set; }

        public ulong Unknown9 { get; set; } //always 0
        public byte Unknown10 { get; set; } //can be 1 or 2
        public byte Unknown11 { get; set; } //can be 1 or 2
        public byte Unknown12 { get; set; } //can be 1 or 2
        public byte Unknown13 { get; set; } //can be 1 or 2
        public uint Unknown14 { get; set; } //always 0

        public byte[] ToBytes()
        {
            byte[] toBytes = new byte[0x70];


            return toBytes;
        }

        public Item Prepare(byte[] data)
        {
            Item item = new Item();

            return item;
        }
    }

    [Serializable]
    public class Inventory
    {
        #region Constants
        private const byte MAX_ITEMS = 200;

        private const ushort x01_CHUNK = 0x0148;
        private const ushort x08_CHUNK = 0x0149;
        private const ushort x16_CHUNK = 0x014a;
        private const ushort x32_CHUNK = 0x014b;
        private const ushort x64_CHUNK = 0x014c;

        private const ushort CHUNK_START = 0x0146;
        private const ushort CHUNK_END = 0x0147;
        #endregion

        public List<Item> Items = new List<Item>();

        public byte[] ToBytes()
        {
            byte[] toBytes = new byte[Items.Count];


            return toBytes;
        }

        public void Add(Item item) => Items.Add(item);
    }
}
