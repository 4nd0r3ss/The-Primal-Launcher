using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

            WelcomeTalk = 34;
            ShopType = ShopType.Item; //should become an enum
            ItemSet = 1016;
            MenuId = 36;
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender, spawnType, isZoning);
            SetAppearance(sender);
            SetName(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIcon(sender);
            SetIsZoning(sender, false);
            SetLuaScript(sender);
            Init(sender);
            SetEventStatus(sender);
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

        public override void Init(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");
            property.Add("charaWork.property[0]", true);
            property.Add("charaWork.property[1]", true);
            property.Add("charaWork.property[4]", true);
            property.FinishWriting(Id);
        }

        public override void talkDefault(Socket sender)
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                SendTalkResponse(sender, "welcomeTalk", new List<object> { WelcomeTalk, User.Instance.Character.Id }, true);
            }
            else
            {
                uint selection = EventManager.Instance.CurrentEvent.Selection;

                if (string.IsNullOrEmpty(CurrentMenu))
                {
                    SelectShopType(sender);
                }
                else if (CurrentMenu == "shopMenu")
                {
                    switch (selection)
                    {
                        case 0xFF:
                            EventManager.Instance.CurrentEvent.Finish(sender);
                            CurrentMenu = "";
                            break;
                        case 0x01:
                            Buy(sender);
                            break;
                        case 0x02:
                            Sell(sender);
                            break;
                        case 0x03:
                            UseFacility(sender);
                            break;
                        case 0x04:
                            ShopTutorial(sender);
                            break;
                        default:
                            EventManager.Instance.CurrentEvent.Finish(sender);
                            break;
                    }
                }
                else
                {
                    switch (CurrentMenu)
                    {
                        case "shopBuy":
                            Buy(sender);
                            break;                       
                        case "shopSell":
                        case "shopSellList":
                            Sell(sender);
                            break;
                    }
                }
            }
        }

        private void SelectShopType(Socket sender)
        {
            CurrentMenu = "shopMenu";

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

            SendTalkResponse(sender, functionName, parameters, true);
        }

        private void Buy(Socket sender)
        {
            int selection = (int)EventManager.Instance.CurrentEvent.Selection;
            List<object> parameters = new List<object> { User.Instance.Character.Id };
            string functionName = "";

            //buy was just selected in shop menu.
            if(CurrentMenu == "shopMenu")
            {
                int currentSet;                

                if (ShopType == ShopType.Weapon)
                    currentSet = WeaponPacks[selection];
                else if (ShopType == ShopType.Armor)
                    currentSet = ArmorPacks[selection];
                else
                    currentSet = (ItemSet - 1) + selection;

                CurrentMenu = "shopBuy";
                CurrentItemSet = currentSet;
                parameters.Add(currentSet);
                Log.Instance.Info("Loading shop items...");
                functionName = "openShopBuy";
            }
            else
            {
                if (selection == 0xFF)
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
                    uint itemIndex = EventManager.Instance.CurrentEvent.Selection - 1;       
                    
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
                    playerInventory.AddGil(sender, (totalToPay * -1));

                    //send confirmation text mesage.
                    World.Instance.SendTextSheetMessage(sender, 0x002061E5, new LuaParameters
                    {
                        Parameters = new object[]
                        {
                            (int)itemId,
                            1, /* looks like quality of item, like: "spring water +2" */
                            itemQuantity,                            
                            totalToPay
                        }                        
                    });

                    functionName = "selectShopBuy";
                }
            }            

            SendTalkResponse(sender, functionName, parameters, true);
        }

        private void Sell(Socket sender)
        {
            int selection = (int)EventManager.Instance.CurrentEvent.Selection;
            List<object> parameters = new List<object> { User.Instance.Character.Id };
            string functionName = "";

            if(CurrentMenu == "shopMenu")
            {
                CurrentMenu = "shopSell";
                functionName = "openShopSell";
            }
            else
            {
                if (selection == 0xFF)
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
                    functionName = "informSellPrice";
                    //User.Instance.Character.Inventory.Send(sender);

                    parameters = new List<object> { 1, 7, 56 };
                }
            }            

            SendTalkResponse(sender, functionName, parameters, true);
        }

        private void UseFacility(Socket sender)
        {

        }

        private void ShopTutorial(Socket sender)
        {
            SendTalkResponse(sender, "startTutorial", new List<object> { null, MenuId }, true);
        }

        private void SendTalkResponse(Socket sender, string functionName, List<object> parameters, bool isQuestion = false)
        {
            List<object> toSend = new List<object>
            {
                (sbyte)1,
                Encoding.ASCII.GetBytes("talkDefault"),
                Encoding.ASCII.GetBytes(functionName)
            };

            toSend.AddRange(parameters);

            EventManager.Instance.CurrentEvent.RequestParameters = new LuaParameters() { Parameters = toSend.ToArray() };
            EventManager.Instance.CurrentEvent.Response(sender);
            EventManager.Instance.CurrentEvent.FunctionName = "talkDefault";
            EventManager.Instance.CurrentEvent.IsQuestion = isQuestion;
        }
    }
}
