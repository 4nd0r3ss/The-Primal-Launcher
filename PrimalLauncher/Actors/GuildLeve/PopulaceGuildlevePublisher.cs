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
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace PrimalLauncher
{
    public class PopulaceGuildlevePublisher : PopulaceStandard
    {       
        private int MenuNav { get; set; }
        private int SelectedLeveIndex { get; set; }
        private int SelectedPackId { get; set; }
        public GuildLevePackSet GuildLevePackSet { get; set; }       

        public PopulaceGuildlevePublisher()
        {
            ClassName = GetType().Name;        
        }

        public override void Spawn(ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            MenuNav = 0;            
            base.Spawn(spawnType, isZoning, changingZone);
        }

        /// <summary>
        /// Default talk function.        ///
        /// </summary>
        public override void talkDefault()
        {
            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                StartTalk();                
            }
            else
            {
                uint? selection = (uint?)EventManager.Instance.CurrentEvent.Selection[0];
                                
                if(MenuNav == 0) //we're on main menu
                {
                    if (selection == 1) //we're on battlecraft location selection screen
                    {
                        GetLevePacks();
                    }
                    else if (selection == null || selection == 0 || (selection > 1 && selection < 9))
                    {
                        EndTalk();//selected 'nothing' on main menu
                    }
                    else if (selection >= 21 && selection <= 24) //We're on fieldcraft job selection
                    {
                        GetFieldcraftLeve(selection);
                    }
                   
                    else //faction leves selected
                    {
                        //selecting one of the factions is crashing the game.
                    }

                    //eventHistoryleveCannot - add evaluation menu options here   
                }
                else if(MenuNav == 1) //we're on select leve plate screen
                {
                    if (selection == null) //clicked cancel                    
                        StartTalk();                    
                    else                    
                        GetBattlecraftLeves(selection);                                                              
                }
                else if(MenuNav == 2)
                {
                    if (selection == 0xffffffff) //clicked cancel
                    {
                        GetLevePacks(); //go back to leve location selection                        
                    }
                    else
                    {
                        GetLeveDetails(selection); //show leve details
                    }                        
                }
                else if(MenuNav == 3)
                {
                    if(selection == null) //clicked cancel
                    {
                        GetBattlecraftLeves(selection);
                    }
                    else //accepted leve
                    {
                        //TODO: still working on this, need more data mining.
                        //eventHistoryleveExist - player already have the leve in journal, abort
                        int selectedLeve = (int)GuildLevePackSet.GetGuildLevesFromPack(SelectedPackId)[SelectedLeveIndex];
                        int result = User.Instance.Character.Journal.AddLocalGuidleve((uint)selectedLeve);

                        if (result == 1) //guildleve cap reached
                        {

                        }
                        else if(result == 2) //already have selected guildleve
                        {

                        }                           
                        else
                        {
                            World.SendTextSheet(0xC3E8, new object[] { selectedLeve });
                            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkAfterOffer", new List<object> { null }, true);
                        }

                        User.Instance.Character.Journal.AddLocalLeveUpdate((uint)selectedLeve);
                    }
                }

                Log.Instance.Info("PopulaceGuildlevePublisher: " + selection + ", nav: " + MenuNav);                
            }            
        }

        private void GetLevePacks()
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkPack", GuildLevePackSet.GetStartEndPacks(), true);
            MenuNav = 1;
        }

        private void GetBattlecraftLeves(uint? selection)
        {
            if(selection != null)
                SelectedPackId = (int)selection;

            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkCard", GuildLevePackSet.GetGuildLevesFromPack(SelectedPackId), true);
            MenuNav = 2;
        }

        private void GetLeveDetails(uint? selection)
        {
            if(selection!=null && selection > 0 && selection != 0xffffffff)
                SelectedLeveIndex = (int)selection - 1;

            int selectedLeve = (int)GuildLevePackSet.GetGuildLevesFromPack(SelectedPackId)[SelectedLeveIndex];


            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkDetail", new List<object> 
            {
                selectedLeve, //leveId
                1, //mark - values 1 to 11. this is a table in lua script with reward values.
                1000001,//rewardItem - an item id. seems gil is default (1000001)
                100,//rewardNum - reward pack?
                0,//subItem - another item id?
                0,//subNum - another reward pack?
                0,//boost - gil bonus? [byte]
                true,//completeFlag - player completed leve before
                0//bonusType - values 1 to 11, same as mark

            }, true);

            MenuNav = 3;
        }

        private void GetFieldcraftLeve(uint? selection)
        {
            //if(selection == 0x24)
                EventManager.Instance.CurrentEvent.Finish();
        }

        private void AcceptLeve()
        {

        }

        private void StartTalk()
        {
            //Play around with the parameter list value to see what happens.
            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkType", new List<object> {
                    User.Instance.Character.CharaWork.CurrentClass.Level,
                    null, //talkFlag - not used in the lua function
                    true, //isFirstCall - if set to false will finish talk. need to figure out nextState first.
                    false, //p3
                    false, //p4
                    false, //p5
                    false, //restrictFlags
                    1, //omenParam - if set to zero, npc won't say it's random omen speech.
                    null, //nextState - null or int, changing to int numbers removes items from talk menu
                    null, //rewardParam - not used in the lua function
                    0, //extraParam1 - can be 0, 1 or 2.
                    0, //extraParam2 - it looks like it can be 1 or 2. it seems 0 does nothing.
                    0 //extraParam3
                   
                }, true);

            MenuNav = 0;
        }

        private void EndTalk()
        {
            EventManager.Instance.CurrentEvent.Finish();
            MenuNav = 0;
        }

    }
}
