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
using System.Text;

namespace PrimalLauncher
{
    class PopulaceShopSalesman : Populace
    {
        public int WelcomeTalk { get; set; }
        public ShopType ShopType { get; set; }
        public int ItemSet { get; set; }
        public int MenuId { get; set; }
        private string CurrentMenu { get; set; }
        private int CurrentItemSet { get; set; }

        private int[] WeaponPacks { get; } = { 5001, 5002, 5007, 5008 };
        private int[] ArmorPacks { get; } = { 5004, 5005, 5006, 5003 };

        public PopulaceShopSalesman()
        {
            ClassName = "PopulaceShopSalesman";
            ClassCode = 0x33800000;
            Icon = 1;
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(0x08);
            SetEventConditions();
            SetSpeeds();
            SetPosition(spawnType, isZoning);
            SetAppearance();
            SetName();
            SetMainState();
            SetSubState();
            SetAllStatus();
            SetIcon();
            SetIsZoning(false);
            SetLuaScript();
            Init();
            SetEventStatus();
            Spawned = true;
        }

        public override void Prepare()
        {
            LuaParameters = new LuaParameters
            {
                ActorName = GenerateName(),
                ClassName = ClassName,
                ClassCode = ClassCode,
                Parameters = new object[] { ClassPath + "Shop/" + ClassName, false, false, false, false, false, (int)ClassId, false, false, 0, 1 }
            };
        }

        public override void Init()
        {
            WorkProperties property = new WorkProperties(Id, @"/_init");
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
            property.Add("charaWork.property[4]", true);
            property.FinishWritingAndSend(Id);
        }

        public override void talkDefault()
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                SendTalkResponse("welcomeTalk", new List<object> { WelcomeTalk, User.Instance.Character.Id }, true);
            }
            else
            {
                uint? selection = EventManager.Instance.CurrentEvent.Selection[0];

                if (string.IsNullOrEmpty(CurrentMenu))
                {
                    SelectShopType();
                }
                else if (CurrentMenu == "shopMenu")
                {
                    if(selection == null || selection == 0xFFFFFFFD)
                    {
                        EventManager.Instance.CurrentEvent.Finish();
                        CurrentMenu = "";
                        return;
                    }
                    else
                    {
                        SelectShopOption();
                    }                    
                }
                else
                {
                    switch (CurrentMenu)
                    {
                        case "shopBuy":
                            Buy();
                            break;                       
                        case "shopSell":
                        case "shopSellList":
                            Sell();
                            break;
                    }
                }
            }
        }

        //Item = 0,
        //Class = 1,
        //Weapon = 2,
        //Armor = 3,
        //Hamlet = 4

        private void SelectShopOption()
        {
            uint? selection = EventManager.Instance.CurrentEvent.Selection[0];

            switch (ShopType)
            {
                case ShopType.Item:
                case ShopType.Hamlet:
                    if (selection == 1)
                        Buy();
                    else if (selection == 2)
                        Sell();
                    else if (selection == 3)
                        UseFacility();
                    else if (selection == 4)
                        ShopTutorial();
                    break;
                case ShopType.Class:
                    if(selection >= 1 && selection <= 6)
                        Buy();
                    else
                        Sell();
                    break;
                case ShopType.Weapon:
                case ShopType.Armor:
                    if (selection >= 1 && selection <= 4)
                        Buy();
                    else
                        Sell();
                    break;                   
            }
        }        

        private void SelectShopType()
        {
            List<object> parameters = new List<object>();
            string functionName = "";

            switch (ShopType)
            {
                case ShopType.Item:
                case ShopType.Hamlet:
                    functionName = "selectMode";
                    break;
                case ShopType.Class:
                    functionName = "selectModeOfClassVendor";
                    break;
                case ShopType.Weapon:
                    functionName = "selectModeOfMultiWeaponVendor";
                    break;
                case ShopType.Armor:
                    functionName = "selectModeOfMultiArmorVendor";
                    break;
            }

            if (ShopType != ShopType.Class)
                parameters.Add(MenuId);

            CurrentMenu = "shopMenu";
            SendTalkResponse(functionName, parameters, true);
        }

        private void Buy()
        {
            uint? selection = EventManager.Instance.CurrentEvent.Selection[0];
            List<object> parameters = new List<object> { User.Instance.Character.Id };
            string functionName = "";

            //buy was just selected in shop menu.
            if(CurrentMenu == "shopMenu")
            {
                int currentSet;                

                if (ShopType == ShopType.Weapon)
                    currentSet = WeaponPacks[(int)selection - 1];
                else if (ShopType == ShopType.Armor)
                    currentSet = ArmorPacks[(int)selection - 1];
                else
                    currentSet = (ItemSet - 1) + (int)selection;

                CurrentMenu = "shopBuy";
                CurrentItemSet = currentSet;
                parameters.Add(currentSet);
                Log.Instance.Info("Loading shop items...");
                functionName = "openShopBuy";
            }
            else
            {
                if (selection == null)
                {                   
                    Log.Instance.Info("Shop items loaded.");
                    functionName = "selectShopBuy";
                }
                else if (selection == 0)
                {
                    CurrentMenu = null;
                    functionName = "closeShopBuy";
                }
                else
                {
                    //get requested item quantity and index
                    byte[] data = EventManager.Instance.CurrentEvent.Data;
                    int itemQuantity = data[0x27] << 24 | data[0x28] << 16 | data[0x29] << 8 | data[0x2A];
                    uint itemIndex = (uint)(EventManager.Instance.CurrentEvent.Selection[0] - 1);       
                    
                    //get item from shop item set
                    DataRow shopBaseRow = GameData.Instance.GetGameData("shopBase").Select("id = '" + CurrentItemSet + "'")[0];
                    uint baseId = Convert.ToUInt32(shopBaseRow.ItemArray[1]) + itemIndex;

                    //get item id and price
                    DataRow shopItemRow = GameData.Instance.GetGameData("shopItem").Select("id = '" + baseId + "'")[0];
                    uint itemId = Convert.ToUInt32(shopItemRow.ItemArray[1]);
                    int itemPrice = Convert.ToInt32(shopItemRow.ItemArray[3]);

                    //add item to character inventory.
                    Inventory playerInventory = User.Instance.Character.Inventory;
                    playerInventory.AddItemById(InventoryType.Bag, itemId, itemQuantity);

                    //take the gil from character 
                    int totalToPay = itemPrice * itemQuantity;
                    playerInventory.AddGil((totalToPay * -1));

                    //send confirmation text mesage.
                    World.SendTextSheet(0x002061E5, new object[] {
                        (int)itemId,
                        1, /* item quality */
                        itemQuantity,                            
                        totalToPay
                    });

                    functionName = "selectShopBuy";
                }
            }            

            SendTalkResponse(functionName, parameters, true);
        }

        private void Sell()
        {
            uint? selection = EventManager.Instance.CurrentEvent.Selection[0];
            List<object> parameters = new List<object> { User.Instance.Character.Id };
            string functionName = "";

            if(CurrentMenu == "shopMenu")
            {
                CurrentMenu = "shopSell";
                functionName = "openShopSell";
            }
            else
            {
                if (selection == null)
                {
                    if(CurrentMenu == "shopSell") //display item list
                    {
                        CurrentMenu = "shopSellList";
                        functionName = "selectShopSell";
                    }
                    else if(CurrentMenu == "shopSellList") //back to shop menu
                    {
                        CurrentMenu = null;
                        functionName = "closeShopSell";
                    }
                }
                else
                {
                    functionName = "finishTalkTurn";
                    //User.Instance.Character.Inventory.Send();

                    parameters = new List<object> { null }; // 1, 7, 56 };
                }
            }            

            SendTalkResponse(functionName, parameters, true);
        }

        private void UseFacility()
        {

        }

        private void ShopTutorial()
        {
            SendTalkResponse("startTutorial", new List<object> { null, MenuId }, true);
        }

        private void SendTalkResponse(string functionName, List<object> parameters, bool isQuestion = false)
        {
            List<object> toSend = new List<object>
            {
                (sbyte)1,
                Encoding.ASCII.GetBytes("talkDefault"),
                Encoding.ASCII.GetBytes(functionName)
            };

            toSend.AddRange(parameters);

            EventManager.Instance.CurrentEvent.RequestParameters = new LuaParameters() { Parameters = toSend.ToArray() };
            EventManager.Instance.CurrentEvent.Response();
            EventManager.Instance.CurrentEvent.Callback = "talkDefault";
            EventManager.Instance.CurrentEvent.IsQuestion = isQuestion;
        }
    }
}
