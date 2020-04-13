using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Launcher
{
    [Serializable]
    public class Inventory
    {
        //The key in these dictionaries are the slot numbers. 
        public Dictionary<ushort, object> Bag = new Dictionary<ushort, object>();
        public Dictionary<ushort, object> Currency = new Dictionary<ushort, object>();
        public Dictionary<ushort, object> KeyItems = new Dictionary<ushort, object>();
        public Dictionary<ushort, object> Loot = new Dictionary<ushort, object>();
        public Dictionary<ushort, object> MeldRequest = new Dictionary<ushort, object>();
        public Dictionary<ushort, object> Bazaar = new Dictionary<ushort, object>();

        public Dictionary<ushort, ushort> GearSlots = new Dictionary<ushort, ushort>();

        public Inventory()
        {
            AddEmptySlots(ref Bag, InventoryMaxSlots.Bag);
            AddEmptySlots(ref Currency, InventoryMaxSlots.Currency);
            AddEmptySlots(ref KeyItems, InventoryMaxSlots.KeyItems);
            AddEmptySlots(ref Loot, InventoryMaxSlots.Loot);
            AddEmptySlots(ref MeldRequest, InventoryMaxSlots.MeldRequest);
            AddEmptySlots(ref Bazaar, InventoryMaxSlots.Bazaar);
        }

        #region Packet Handling
        private void ChunkStart(Socket sender, InventoryMaxSlots maxSize, InventoryType type)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(UserFactory.Instance.User.Character.Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)maxSize), 0, data, 0x04, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)type), 0, data, 0x06, sizeof(ushort));
            SendPacket(sender, ServerOpcode.ChunkStart, data);
        }
        public void ChunkEnd(Socket sender) => SendPacket(sender, ServerOpcode.ChunkEnd, new byte[0x08]);
        public void InventoryStart(Socket sender) => SendPacket(sender, ServerOpcode.InventoryStart, new byte[0x08]);
        public void InventoryEnd(Socket sender) => SendPacket(sender, ServerOpcode.InventoryEnd, new byte[0x08]);
        private void SendChunk(Socket sender, ServerOpcode opcode, byte[] data) => SendPacket(sender, opcode, data);

        /// <summary>
        /// Send an inventory packet.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="opcode"></param>
        /// <param name="data"></param>
        private void SendPacket(Socket handler, ServerOpcode opcode, byte[] data)
        {
            GamePacket gamePacket = new GamePacket
            {
                Opcode = (ushort)opcode,
                Data = data
            };

            Packet packet = new Packet(new SubPacket(gamePacket) { SourceId = UserFactory.Instance.User.Character.Id, TargetId = UserFactory.Instance.User.Character.Id });
            handler.Send(packet.ToBytes());
        }
        #endregion

        /// <summary>
        /// Initialize an inventory dict with all slots empty.
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="numSlots"></param>
        private void AddEmptySlots(ref Dictionary<ushort, object> inventory, InventoryMaxSlots numSlots)
        {
            for (int i = 0; i < (int)numSlots; i++)
                inventory.Add((ushort)i, null);
        }

        /// <summary>
        /// Returns the first empty slot in an inventory this is necessary to keep the continuity
        /// </summary>
        /// <param name="inventory"></param>
        /// <returns></returns>
        private ushort GetFirstEmptySlot(Dictionary<ushort, object> inventory)
        {
            foreach (var slot in inventory)
                if (slot.Value == null) return slot.Key;

            return 0; //inventory is full
        }

        /// <summary>
        /// Get item data from game data table, create item object and add to character bag. Used from chat window command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="itemName"></param>
        /// <param name="quantity"></param>
        public void AddItem(ref Dictionary<ushort, object> inventory, string itemName, uint quantity, Socket sender = null)
        {
            DataTable itemNames = GameDataFile.Instance.GetGameData("xtx/itemName");
            DataRow[] selected = itemNames.Select("strc0 = '" + itemName + "'");

            if (selected.Length > 0)
            {
                uint itemId = (uint)selected[0][0];
                AddItemById(ref inventory, itemId, quantity, sender);
            }
        }

        /// <summary>
        /// Add a new item to specified inventory by item id.
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="itemId"></param>
        /// <param name="quantity"></param>
        /// <param name="sender"></param>
        public void AddItemById(ref Dictionary<ushort, object> inventory, uint itemId, uint quantity, Socket sender = null)
        {
            DataTable itemsData = GameDataFile.Instance.GetGameData("itemData");
            DataTable itemsStack = GameDataFile.Instance.GetGameData("_item");

            DataRow itemData = itemsData.Select("id = '" + itemId + "'")[0];
            DataRow itemStack = itemsStack.Select("id = '" + itemId + "'")[0];

            int itemMaxStack = (int)itemStack.ItemArray[2];
            int itemKind = (int)itemData.ItemArray[4];

            //if max stack is > 1, we change the quantity bytes to the quantity the user requested. 
            if (itemMaxStack > 1)
            {
                //if the requested quantity is greater than the max stack, we limit it to the max stack.
                uint itemQuantity = quantity > itemMaxStack ? (uint)itemMaxStack : quantity;
                ushort slotToAddTo = GetFirstEmptySlot(inventory);

                Item item = new Item
                {
                    Id = itemId,
                    ItemKind = (uint)itemKind,
                    InventorySlot = slotToAddTo,
                    Quantity = itemQuantity,
                    MaxQuantity = (uint)itemMaxStack
                };

                inventory[slotToAddTo] = item;

                if (sender != null)
                    SendBagItem(sender, item.ToBytes());
            }
            else
            {
                //calculate the amount of item units to add to the slot. Max is the number of available slots.
                int numAvailableSlots = inventory.Select(x => x.Value == null).Count();
                int itemTotalUnits = quantity > numAvailableSlots ? numAvailableSlots : (int)quantity;

                //if item max stack is 1 (i.e. equipent), send the requested amount of the same item
                for (int i = 0; i < itemTotalUnits; i++)
                {
                    ushort slotToAddTo = GetFirstEmptySlot(inventory);
                    int durability = (int)itemData.ItemArray[1];

                    Item item = new Item
                    {
                        Id = itemId,
                        ItemKind = (uint)itemKind,
                        InventorySlot = slotToAddTo,
                        Durability = (uint)durability,
                        MaxDurability = (uint)durability
                    };

                    inventory[slotToAddTo] = item;

                    if (sender != null)
                        SendBagItem(sender, item.ToBytes());
                }
            }
        }

        /// <summary>
        /// Send packets from a single item added to inventory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public void SendBagItem(Socket sender, byte[] data)
        {
            InventoryStart(sender);
            ChunkStart(sender, InventoryMaxSlots.Bag, InventoryType.Bag);
            SendChunk(sender, ServerOpcode.x01InventoryChunk, data);
            ChunkEnd(sender);
            InventoryEnd(sender);
        }

        /// <summary>
        /// Put all default items into character's bag. Used only during character creation.
        /// </summary>
        /// <param name="graphId">An arrray with selected class default equipment graphic codes from game data table boot_skillequip</param>
        public void AddDefaultItems(uint[] graphId, uint underShirtId, uint underGarmentId)
        {
            //equipment
            AddEquipmentPiece(ItemGraphics.Weapon, 0, graphicId: graphId[1]);
            //AddDefaultItem(ItemGraphics.Weapon, graphId[2]); //leaving this on crashes the game. Seems that no class starts with a secondary weapon/tool. The value for archer/bard is for quiver graphics only.
            AddEquipmentPiece(ItemGraphics.Head, 8, graphicId: graphId[8]);
            AddEquipmentPiece(ItemGraphics.Body, 9, itemId: underShirtId);
            AddEquipmentPiece(ItemGraphics.Body, 10, graphicId: graphId[9]);
            AddEquipmentPiece(ItemGraphics.Legs, 11, itemId: underGarmentId);
            AddEquipmentPiece(ItemGraphics.Legs, 12, graphicId: graphId[10]);
            AddEquipmentPiece(ItemGraphics.Hands, 13, graphicId: graphId[11]);
            AddEquipmentPiece(ItemGraphics.Feet, 14, graphicId: graphId[12]);
            AddEquipmentPiece(ItemGraphics.Waist, 15, graphicId: graphId[13]);

            // giveaway on me =)
            AddItem(ref Bag, "Potion", 10);
            AddItem(ref Currency, "Gil", 200);
        }

        /// <summary>
        /// Sends all inventories packets. Used when spawning player.
        /// </summary>
        /// <param name="sender"></param>
        public void SendInventories(Socket sender)
        {
            InventoryStart(sender);
            SendInventory(sender, Bag, InventoryMaxSlots.Bag, InventoryType.Bag);
            SendInventory(sender, Loot, InventoryMaxSlots.Loot, InventoryType.Loot);
            SendInventory(sender, MeldRequest, InventoryMaxSlots.MeldRequest, InventoryType.MeldRequest);
            SendInventory(sender, Bazaar, InventoryMaxSlots.Bazaar, InventoryType.Bazaar);
            SendInventory(sender, Currency, InventoryMaxSlots.Currency, InventoryType.Currency);
            SendInventory(sender, KeyItems, InventoryMaxSlots.KeyItems, InventoryType.KeyItems);
            InventoryEnd(sender);
        }

        /// <summary>
        /// Send packets to update character's inventory data.
        /// </summary>
        /// <param name="sender"></param>
        public void Update(Socket sender)
        {
            InventoryStart(sender);
            SendInventory(sender, Bag, InventoryMaxSlots.Bag, InventoryType.Bag);
            SendGearSlots(sender);
            InventoryEnd(sender);
        }

        /// <summary>
        ///  Used to send a test item to the loot list. Part of chat window commands for test purpose.
        /// </summary>
        /// <param name="handler"></param>
        public static void AddLoot(Socket handler)
        {

            //Inventory start
            handler.Send(new Packet(new GamePacket
            {
                Opcode = 0x16d,
                Data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            }).ToBytes());

            //Chunk start
            byte[] b = { 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x04, 0x00 };
            Buffer.BlockCopy(BitConverter.GetBytes(UserFactory.Instance.User.Character.Id), 0, b, 0, 4);
            handler.Send(new Packet(new GamePacket
            {
                Opcode = 0x146,
                Data = b
            }).ToBytes());

            //Chunk
            handler.Send(new Packet(new GamePacket
            {
                Opcode = 0x148,
                Data = new byte[] {
                    0x77, 0x52, 0x40, 0x08, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x7B, 0xBA, 0x98, 0x00,
                    0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                }
            }).ToBytes());

            //Chunk end
            handler.Send(new Packet(new GamePacket
            {
                Opcode = 0x147,
                Data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            }).ToBytes());

            //Invenroty end
            handler.Send(new Packet(new GamePacket
            {
                Opcode = 0x16e,
                Data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            }).ToBytes());
        }

        /// <summary>
        /// Send packets containing all items in the specified inventory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="inventory"></param>
        private void SendInventory(Socket sender, Dictionary<ushort, object> inventory, InventoryMaxSlots maxSlots, InventoryType invType)
        {
            ChunkStart(sender, maxSlots, invType);

            //get the number if items in the inventory
            List<Item> items = new List<Item>();
            foreach (var slot in inventory)
                if (slot.Value != null)
                    items.Add((Item)slot.Value);

            int itemCount = items.Count;

            if (itemCount > 0)
            {
                int index = 0;

                while (itemCount > 0)
                {
                    if (itemCount >= 64)
                        itemCount = SendItemsChunk(sender, ServerOpcode.x64InventoryChunk, 64, itemCount, ref index, ref items);
                    else if (itemCount >= 32)
                        itemCount = SendItemsChunk(sender, ServerOpcode.x32InventoryChunk, 32, itemCount, ref index, ref items);
                    else if (itemCount >= 16)
                        itemCount = SendItemsChunk(sender, ServerOpcode.x16InventoryChunk, 16, itemCount, ref index, ref items);
                    else if (itemCount >= 08 || itemCount < 08)
                        itemCount = SendItemsChunk(sender, ServerOpcode.x08InventoryChunk, 08, itemCount, ref index, ref items);
                }
            }

            ChunkEnd(sender);
        }

        /// <summary>
        /// Calculate how many chunk packets of the given size need to be sent and send them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="opcode"></param>
        /// <param name="chunkSize"></param>
        /// <param name="numItems"></param>
        /// <param name="index"></param>
        /// <param name="itemList"></param>
        /// <returns></returns>
        private int SendItemsChunk(Socket sender, ServerOpcode opcode, int chunkSize, int numItems, ref int index, ref List<Item> itemList)
        {
            int numChunks = numItems < chunkSize ? 1 : numItems / chunkSize;
            int itemSlotSize = 0x70;

            //writes numChunks number of packets of chunkSize size.
            for (int i = 0; i < numChunks; i++)
            {
                int numItemsInChunk = 0;

                using (MemoryStream data = new MemoryStream())
                {
                    //write items
                    for (int j = 0; j < chunkSize; j++)
                    {
                        if ((index + j) > (itemList.Count - 1))
                        {
                            int zeroFillerSize = itemSlotSize * (chunkSize - numItems); //how many empty slots we have to write
                            data.Write(new byte[zeroFillerSize], 0, zeroFillerSize);
                            break;
                        }
                        else
                        {
                            data.Write(itemList[index + j].ToBytes(), 0, itemSlotSize);
                            numItemsInChunk++;
                        }
                    }

                    if (chunkSize == 8)
                        data.Write(BitConverter.GetBytes((long)numItemsInChunk), 0, sizeof(long));

                    //send chunk                   
                    SendChunk(sender, opcode, data.ToArray());
                }
                index += chunkSize;
            }

            return numItems < chunkSize ? 0 : numItems - (chunkSize * numChunks);
        }

        #region Equipment Methods
        /// <summary>
        /// Adds one single default item into character's bag.
        /// </summary>
        /// <param name="equipList"></param>
        /// <param name="graphicId"></param>
        /// <param name="gearSlot"></param>
        private void AddEquipmentPiece(Dictionary<uint, uint> equipList, ushort gearSlot, uint graphicId = 0, uint itemId = 0)
        {
            uint equipId = 0;

            if (graphicId > 0)
                equipId = equipList.FirstOrDefault(x => x.Value == graphicId).Key;
            else if (itemId > 0)
                equipId = itemId;

            if (equipId > 0)
            {
                ushort inventorySlot = AddEquipmentToBag(equipId);
                AddEquipmentToGearSlot(gearSlot, inventorySlot);
            }
        }

        /// <summary>
        /// Equips an equipment piece to designated gear slot.
        /// </summary>
        /// <param name="gearSlot"></param>
        /// <param name="inventorySlot"></param>
        private void AddEquipmentToGearSlot(ushort gearSlot, ushort inventorySlot)
        {
            if (inventorySlot != 0xffff)
                GearSlots[gearSlot] = inventorySlot;
        }

        /// <summary>
        /// Adds equipment piece to character's bag.
        /// </summary>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public ushort AddEquipmentToBag(uint equipId)
        {
            ushort slot = 0xffff;
            DataTable itemsData = GameDataFile.Instance.GetGameData("itemData");
            DataRow itemData = itemsData.Select("id = '" + equipId + "'")[0];

            int itemKind = (int)itemData.ItemArray[4];

            if (Bag.Select(x => x.Value == null).Count() > 0)
            {
                ushort slotToAddTo = GetFirstEmptySlot(Bag);
                int durability = (int)itemData.ItemArray[1];

                Item item = new Item
                {
                    Id = equipId,
                    ItemKind = (uint)itemKind,
                    InventorySlot = slotToAddTo,
                    Durability = (uint)durability,
                    MaxDurability = (uint)durability
                };

                Bag[slotToAddTo] = item;
                slot = slotToAddTo;
            }

            return slot;
        }

        /// <summary>
        /// Loads equipped gear pieces into their respective gear slots. Used at character spawn and inventory update.
        /// </summary>
        /// <param name="sender"></param>
        public void SendGearSlots(Socket sender)
        {
            ChunkStart(sender, InventoryMaxSlots.Equipment, InventoryType.Equipment);

            int slotCount = GearSlots.Count;
            int index = 0;

            while (slotCount > 0)
            {
                if (slotCount >= 16)
                    slotCount = SendGearSlotChunk(sender, ServerOpcode.x16SetEquipment, 16, slotCount, ref index);
                else if (slotCount >= 8 || slotCount < 8)
                    slotCount = SendGearSlotChunk(sender, ServerOpcode.x08SetEquipment, 8, slotCount, ref index);
            }

            ChunkEnd(sender);
        }

        /// <summary>
        /// Send one chunk of equipment slots. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="opcode"></param>
        /// <param name="chunkSize"></param>
        /// <param name="numSlots"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int SendGearSlotChunk(Socket sender, ServerOpcode opcode, int chunkSize, int numSlots, ref int index)
        {
            int numChunks = numSlots < chunkSize ? 1 : numSlots / chunkSize;
            int gearSlotSize = 0x06;

            //writes numChunks number of packets of chunkSize size.
            for (int i = 0; i < numChunks; i++)
            {
                using (MemoryStream data = new MemoryStream())
                {
                    int numSlotsInChunk = 0;

                    //write slots
                    for (int j = 0; j < chunkSize; j++)
                    {
                        if ((index + j) > (GearSlots.Count - 1))
                        {
                            int zeroFillerSize = gearSlotSize * (chunkSize - numSlots); //how many empty slots we have to write
                            data.Write(new byte[zeroFillerSize], 0, zeroFillerSize);
                            break;
                        }
                        else
                        {
                            data.Write(BitConverter.GetBytes(GearSlots.ElementAt(index + j).Key), 0, sizeof(ushort));
                            data.Write(BitConverter.GetBytes(GearSlots.ElementAt(index + j).Value), 0, sizeof(ushort));
                            data.Write(BitConverter.GetBytes((ushort)0), 0, sizeof(ushort));
                            numSlotsInChunk++;
                        }

                    }

                    if (chunkSize == 8)
                        data.Write(BitConverter.GetBytes((long)numSlotsInChunk), 0, sizeof(long));

                    //send chunk                    
                    SendChunk(sender, opcode, data.ToArray());
                }
                index += chunkSize;
            }

            return numSlots < chunkSize ? 0 : numSlots - (chunkSize * numChunks);
        }

        /// <summary>
        /// Called from gear menu to swap gear in a gear slot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        public void SwitchGear(Socket sender, byte[] request)
        {
            //we read the bytes in the index below to be able to differentiate equip/unequip packets. It's the fastest way I can think of.
            uint pattern = (uint)(request[0x53] << 24 | request[0x52] << 16 | request[0x51] << 8 | request[0x50]);

            byte gearSlot = 0;
            byte invSlot = 0;
            uint itemUniqueId = 0;
            ServerOpcode opcode;

            if (pattern == 0x05050505)
            {
                gearSlot = (byte)(request[0x58] - 1);
                itemUniqueId = (uint)(request[0x5e] << 24 | request[0x5f] << 16 | request[0x60] << 8 | request[0x61]);
                opcode = ServerOpcode.x01SetEquipment;

                foreach (var slot in Bag)
                {
                    Item item = (Item)slot.Value;

                    if (item.UniqueId == itemUniqueId)
                    {
                        invSlot = (byte)item.InventorySlot;
                        UserFactory.Instance.User.Character.GearGraphics.Set(gearSlot, item.Id);

                        if (GearSlots.Any(x => x.Key == gearSlot))
                            GearSlots[gearSlot] = invSlot; //if there is anything in the slot, replace
                        else
                            GearSlots.Add(gearSlot, invSlot);

                        break;
                    }
                }
                SendSwitchGearResult(sender, opcode, gearSlot, invSlot);
            }
            else
            {
                gearSlot = (byte)(request[0x51] - 1);

                if (gearSlot != 0x09 && gearSlot != 0x0b && gearSlot != 0) //can't unequip underwear and main weapon, just switch to another piece.
                {
                    GearSlots.Remove(gearSlot);
                    UserFactory.Instance.User.Character.GearGraphics.Set(gearSlot, 0);
                    opcode = ServerOpcode.x01RemoveEquipment;
                    SendSwitchGearResult(sender, opcode, gearSlot, invSlot);
                }
                else
                {
                    //TODO: send message warning that underwear cannot be unequipped.
                }
            }
        }

        /// <summary>
        /// Send switch gear result packets and updates character appearance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="opcode"></param>
        /// <param name="gearSlot"></param>
        /// <param name="invSlot"></param>
        private void SendSwitchGearResult(Socket sender, ServerOpcode opcode, byte gearSlot, byte invSlot)
        {
            InventoryStart(sender);
            ChunkStart(sender, InventoryMaxSlots.Equipment, InventoryType.Equipment);

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(gearSlot), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(invSlot), 0, data, 2, 1);

            SendPacket(sender, opcode, data);
            ChunkEnd(sender);
            InventoryEnd(sender);

            Update(sender);
            UserFactory.Instance.User.Character.SetAppearance(sender);
        }

        /// <summary>
        /// Used to unequip a piece of gear by passing the gear slot number. Part of chat window commands for test purpose.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gearslot"></param>
        public void UnequipGear(Socket sender, byte gearslot)
        {
            InventoryStart(sender);
            ChunkStart(sender, InventoryMaxSlots.Equipment, InventoryType.Equipment);

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(gearslot), 0, data, 0, 1);

            SendPacket(sender, ServerOpcode.x01RemoveEquipment, data);
            ChunkEnd(sender);
            InventoryEnd(sender);

            Update(sender);
        }

        /// <summary>
        /// Used to equip a piece of gear by passing the gear slot number and ite inventory item slot number. Part of chat window commands for test purpose.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gearslot"></param>
        /// <param name="invSlot"></param>
        public void EquipGear(Socket sender, byte gearslot, byte invSlot)
        {
            InventoryStart(sender);
            ChunkStart(sender, InventoryMaxSlots.Equipment, InventoryType.Equipment);

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(gearslot), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(invSlot), 0, data, 2, 1);

            SendPacket(sender, ServerOpcode.x01SetEquipment, data);
            ChunkEnd(sender);
            InventoryEnd(sender);

            Update(sender);
        }
        #endregion
    }

    [Serializable]
    public class Item
    {
        public uint UniqueId { get; set; }
        public uint ItemKind { get; set; }
        public uint Id { get; set; }
        public ushort InventorySlot { get; set; }

        public uint Quantity { get; set; } = 1;
        public uint Durability { get; set; }

        public uint MaxQuantity { get; set; }
        public uint MaxDurability { get; set; }




        public uint Unknown1 { get; set; } //always 0

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

        public Item()
        {
            UniqueId = NewId();
        }

        public uint NewId()
        {
            Random rnd = new Random();
            byte[] id = new byte[0x4];
            rnd.NextBytes(id);
            return BitConverter.ToUInt32(id, 0);
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[0x70];
            Buffer.BlockCopy(BitConverter.GetBytes(UniqueId), 0, buffer, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Quantity), 0, buffer, 0x08, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(Id), 0, buffer, 0x0c, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(InventorySlot), 0, buffer, 0x10, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes(Durability), 0, buffer, 0x2a, sizeof(uint));
            buffer[0x28] = 0x01;
            buffer[0x29] = 0x01;
            buffer[0x38] = 0x01;
            buffer[0x39] = 0x01;
            buffer[0x3a] = 0x01;
            buffer[0x3b] = 0x01;
            return buffer;
        }
    }
}
