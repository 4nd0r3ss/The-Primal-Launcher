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
using System.Data;
using System.IO;
using System.Linq;

namespace PrimalLauncher
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
        private void ChunkStart(InventoryMaxSlots maxSize, InventoryType type)
        {
            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)maxSize), 0, data, 0x04, sizeof(ushort));
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)type), 0, data, 0x06, sizeof(ushort));
            Packet.Send(ServerOpcode.ChunkStart, data);
        }
        public void ChunkEnd() => Packet.Send(ServerOpcode.ChunkEnd, new byte[0x08]);
        public void InventoryStart() => Packet.Send(ServerOpcode.InventoryStart, new byte[0x08]);
        public void InventoryEnd() => Packet.Send(ServerOpcode.InventoryEnd, new byte[0x08]);
        private void SendChunk(ServerOpcode opcode, byte[] data) => Packet.Send(opcode, data);
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
        public void AddItem(InventoryType type, string itemName, int quantity = 1)
        {
            DataTable itemNames = GameData.Instance.GetGameData("xtx/itemName");
            DataRow[] selected = itemNames.Select("strc0 = '" + itemName + "'");

            if (selected.Length > 0)
            {
                uint itemId = (uint)selected[0][0];
                AddItemById(type, itemId, quantity);
            }
        }

        public void AddKeyItem(string itemName, int quantity = 1)
        {
            AddItem(InventoryType.KeyItems, itemName, quantity);
        }

        /// <summary>
        /// Add a new item to specified inventory by item id.
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="itemId"></param>
        /// <param name="quantity"></param>
        /// <param name="sender"></param>
        public void AddItemById(InventoryType type, uint itemId, int quantity)
        {
            DataTable itemsData = GameData.Instance.GetGameData("itemData");
            DataTable itemsStack = GameData.Instance.GetGameData("_item");

            DataRow itemData = itemsData.Select("id = '" + itemId + "'")[0];
            DataRow itemStack = itemsStack.Select("id = '" + itemId + "'")[0];

            int itemMaxStack = (int)itemStack.ItemArray[2];
            int itemKind = (int)itemData.ItemArray[4];
            Dictionary<ushort, object> inventory;
            InventoryMaxSlots maxSlots;

            switch (type)
            {                
                case InventoryType.Bazaar:
                    inventory = Bazaar;
                    maxSlots = InventoryMaxSlots.Bazaar;
                    break;
                case InventoryType.Currency:
                    inventory = Currency;
                    maxSlots = InventoryMaxSlots.Currency;
                    break;                
                case InventoryType.KeyItems:
                    inventory = KeyItems;
                    maxSlots = InventoryMaxSlots.KeyItems;
                    break;
                case InventoryType.Loot:
                    inventory = Loot;
                    maxSlots = InventoryMaxSlots.Loot;
                    break;
                case InventoryType.MeldRequest:
                    inventory = MeldRequest;
                    maxSlots = InventoryMaxSlots.MeldRequest;
                    break;
                default:
                    inventory = Bag;
                    maxSlots = InventoryMaxSlots.Bag;
                    break;
            }

            //if max stack is > 1, we change the quantity bytes to the quantity the user requested. 
            if (itemMaxStack > 1)
            {
                //if the requested quantity is greater than the max stack, we limit it to the max stack.
                int itemQuantity = quantity > itemMaxStack ? itemMaxStack : quantity;
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
                SendItem(type, maxSlots, item.ToBytes());
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
                    SendItem(type, maxSlots, item.ToBytes());
                }
            }
        }

        /// <summary>
        /// Send packets from a single item added to inventory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public void SendItem(InventoryType type, InventoryMaxSlots maxSlots, byte[] data)
        {
            InventoryStart();
            ChunkStart(maxSlots, type);
            SendChunk(ServerOpcode.x01InventoryChunk, data);
            ChunkEnd();
            InventoryEnd();
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
            //AddItem(InventoryType.Bag, "Potion", 10);             
            //AddItem(InventoryType.Currency, "Gil", 200);           
        }

        public void AddGil(int quantity)
        {
            if(quantity > 0)
            {
                foreach (var slot in Currency)
                {
                    if (slot.Value != null)
                    {
                        Item item = (Item)slot.Value;

                        if (item.Id == 1000001)
                        {
                            //we do it like this because quantity can be negative, meaning we are taking Gil.
                            item.Quantity += quantity;
                            SendItem(InventoryType.Currency, InventoryMaxSlots.Currency, item.ToBytes());
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                World.SendTextSheet(0x61C7, new object[] { quantity });
            }            
        }
              
        /// <summary>
        /// Sends all inventories packets. Used when spawning player.
        /// </summary>
        /// <param name="sender"></param>
        public void Send()
        {           
            InventoryStart();                    
            SendInventory(Bag, InventoryMaxSlots.Bag, InventoryType.Bag);
            SendInventory(Currency, InventoryMaxSlots.Currency, InventoryType.Currency);
            SendInventory(KeyItems, InventoryMaxSlots.KeyItems, InventoryType.KeyItems);
            SendInventory(Bazaar, InventoryMaxSlots.Bazaar, InventoryType.Bazaar);
            SendInventory(MeldRequest, InventoryMaxSlots.MeldRequest, InventoryType.MeldRequest);
            SendInventory(Loot, InventoryMaxSlots.Loot, InventoryType.Loot);
            SendGearSlots();
            InventoryEnd();
        }

        /// <summary>
        /// Send packets to update character's inventory data.
        /// </summary>
        /// <param name="sender"></param>
        public void Update()
        {
            InventoryStart(); 
            SendInventory(Bag, InventoryMaxSlots.Bag, InventoryType.Bag);
            SendGearSlots();
            InventoryEnd();
        }
        
        /// <summary>
        ///  Used to send a test item to the loot list. Part of chat window commands for test purpose.
        /// </summary>
        /// <param name="handler"></param>
        public static void AddLoot()
        {

            ////Inventory start
            //handler.Send(new Packet(new GamePacket
            //{
            //    Opcode = 0x16d,
            //    Data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            //}).ToBytes());

            ////Chunk start
            //byte[] b = { 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x04, 0x00 };
            //Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, b, 0, 4);
            //handler.Send(new Packet(new GamePacket
            //{
            //    Opcode = 0x146,
            //    Data = b
            //}).ToBytes());

            ////Chunk
            //handler.Send(new Packet(new GamePacket
            //{
            //    Opcode = 0x148,
            //    Data = new byte[] {
            //        0x77, 0x52, 0x40, 0x08, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x7B, 0xBA, 0x98, 0x00,
            //        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    }
            //}).ToBytes());

            ////Chunk end
            //handler.Send(new Packet(new GamePacket
            //{
            //    Opcode = 0x147,
            //    Data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            //}).ToBytes());

            ////Invenroty end
            //handler.Send(new Packet(new GamePacket
            //{
            //    Opcode = 0x16e,
            //    Data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            //}).ToBytes());
        }
          
        /// <summary>
        /// Send packets containing all items in the specified inventory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="inventory"></param>
        private void SendInventory(Dictionary<ushort, object> inventory, InventoryMaxSlots maxSlots, InventoryType invType)
        {
            ChunkStart(maxSlots, invType);

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
                        itemCount = SendItemsChunk(ServerOpcode.x64InventoryChunk, 64, itemCount, ref index, ref items);
                    else if (itemCount >= 32)
                        itemCount = SendItemsChunk(ServerOpcode.x32InventoryChunk, 32, itemCount, ref index, ref items);
                    else if (itemCount >= 16)
                        itemCount = SendItemsChunk(ServerOpcode.x16InventoryChunk, 16, itemCount, ref index, ref items);
                    else if (itemCount >= 08 || itemCount < 08)
                        itemCount = SendItemsChunk(ServerOpcode.x08InventoryChunk, 08, itemCount, ref index, ref items);
                }
            }  

            ChunkEnd();
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
        private int SendItemsChunk(ServerOpcode opcode, int chunkSize, int numItems, ref int index, ref List<Item> itemList)
        {
            int numChunks = numItems < chunkSize ? 1 : numItems / chunkSize;
            int itemSlotSize = 0x70;

            //writes numChunks number of packets of chunkSize size.
            for (int i = 0; i < numChunks; i++)
            {
                int numItemsInChunk = 0;

                using(MemoryStream data = new MemoryStream())
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
                    
                    if(chunkSize == 8)
                        data.Write(BitConverter.GetBytes((long)numItemsInChunk), 0, sizeof(long));

                    //send chunk                   
                    SendChunk(opcode, data.ToArray());
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
            DataTable itemsData = GameData.Instance.GetGameData("itemData");
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
        public void SendGearSlots()
        {
            ChunkStart(InventoryMaxSlots.Equipment, InventoryType.Equipment);

            int slotCount = GearSlots.Count;
            int index = 0;

            while (slotCount > 0)
            {
                if (slotCount >= 16)
                    slotCount = SendGearSlotChunk(ServerOpcode.x16SetEquipment, 16, slotCount, ref index);
                else if (slotCount >= 8 || slotCount < 8)
                    slotCount = SendGearSlotChunk(ServerOpcode.x08SetEquipment, 8, slotCount, ref index);
            }

            ChunkEnd();
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
        private int SendGearSlotChunk(ServerOpcode opcode, int chunkSize, int numSlots, ref int index)
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
                    SendChunk(opcode, data.ToArray());
                }
                index += chunkSize;
            }

            return numSlots < chunkSize ? 0 : numSlots - (chunkSize * numChunks);
        }

        /// <summary>
        /// Get an item from the character's bag by its unique id.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public Item GetBagItemByUniqueId(uint uniqueId)
        {
            Item result = null;

            foreach (var slot in Bag)
            {
                Item item = (Item)slot.Value;

                //if item is found
                if (item != null && item.UniqueId == uniqueId)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        public Item GetBagItemByGearSlot(byte gearSlot)
        {
            ushort bagSlot = GearSlots[gearSlot];
            return (Item)Bag.FirstOrDefault(x => x.Key == bagSlot).Value;
        }

        public Item GetKeyItemByName(string itemName)
        {
            Item result = null;
            DataTable itemNames = GameData.Instance.GetGameData("xtx/itemName");            

            foreach (var slot in KeyItems)
            {
                Item item = (Item)slot.Value;

                if(item != null)
                {
                    DataRow[] selected = itemNames.Select("ID = '" + item.Id + "'");
                    string currItemName = (string)selected[0][1];
                    currItemName = currItemName.Substring(0, currItemName.Length - 1).ToLower();
                
                    if (currItemName == itemName)
                    {
                        result = item;
                        break;
                    }
                }                
            }

            return result;
        }

        public bool HasKeyItem(string itemName)
        {
            Item item = GetKeyItemByName(itemName);
            return item != null;
        }

        /// <summary>
        /// Called from gear menu to change gear in a gear slot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        public void ChangeGear(byte gearSlot, uint itemUniqueId)
        {           
            byte invSlot = 0;           
            ServerOpcode opcode;

            //equip item
            if (itemUniqueId > 0)
            {               
                opcode = ServerOpcode.x01SetEquipment;

                //search the bag for the item to be equipped.
                foreach (var slot in Bag)
                {
                    Item item = (Item)slot.Value;
                    //if item is found
                    if (item.UniqueId == itemUniqueId)
                    {
                        //get the item bag slot
                        invSlot = (byte)item.InventorySlot;

                        //chage the graphics in appearance slot to the piece being equipped
                        User.Instance.Character.Appearance.Set(gearSlot, item.Id);

                        if (GearSlots.Any(x => x.Key == gearSlot))
                            GearSlots[gearSlot] = invSlot; //if there is anything in the slot, replace
                        else
                            GearSlots.Add(gearSlot, invSlot);

                        break;
                    }
                }

                SendChangeGearResult(opcode, gearSlot, invSlot);
            }
            else
            {              
                if (gearSlot != 0x09 && gearSlot != 0x0b && gearSlot != 0) //can't unequip underwear and main weapon, just switch to another piece.
                {
                    GearSlots.Remove(gearSlot);
                    User.Instance.Character.Appearance.Set(gearSlot, 0);
                    opcode = ServerOpcode.x01RemoveEquipment;
                    SendChangeGearResult(opcode, gearSlot, invSlot);
                }
                else
                {
                    //TODO: send message warning that underwear cannot be unequipped.
                }
            }
        }

        /// <summary>
        /// Send change gear result packets and updates character appearance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="opcode"></param>
        /// <param name="gearSlot"></param>
        /// <param name="invSlot"></param>
        private void SendChangeGearResult(ServerOpcode opcode, byte gearSlot, byte invSlot)
        {
            InventoryStart();
            ChunkStart(InventoryMaxSlots.Equipment, InventoryType.Equipment);

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(gearSlot), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(invSlot), 0, data, 2, 1);

            Packet.Send(opcode, data);
            ChunkEnd();
            InventoryEnd();

            Update();
            User.Instance.Character.SetAppearance();
        }

        /// <summary>
        /// Used to unequip a piece of gear by passing the gear slot number. Part of chat window commands for test purpose.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gearslot"></param>
        public void UnequipGear(byte gearslot)
        {
            InventoryStart();
            ChunkStart(InventoryMaxSlots.Equipment, InventoryType.Equipment);

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(gearslot), 0, data, 0, 1);

            Packet.Send(ServerOpcode.x01RemoveEquipment, data);
            ChunkEnd();
            InventoryEnd();

            Update();
        }

        /// <summary>
        /// Used to equip a piece of gear by passing the gear slot number and ite inventory item slot number. Part of chat window commands for test purpose.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gearslot"></param>
        /// <param name="invSlot"></param>
        public void EquipGear(byte gearslot, byte invSlot)
        {
            InventoryStart();
            ChunkStart(InventoryMaxSlots.Equipment, InventoryType.Equipment);

            byte[] data = new byte[0x08];
            Buffer.BlockCopy(BitConverter.GetBytes(gearslot), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(invSlot), 0, data, 2, 1);

            Packet.Send(ServerOpcode.x01SetEquipment, data);
            ChunkEnd();
            InventoryEnd();

            Update();
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

        public int Quantity { get; set; } = 1;
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
