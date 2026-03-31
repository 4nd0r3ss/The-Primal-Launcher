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
using static System.Collections.Specialized.BitVector32;

namespace PrimalLauncher
{
    public class ObjectItemStorage : Object
    {
        private int MenuNav { get; set; }
        private int SelectedMenu { get; set; }
       
        public ObjectItemStorage()
        {
            ClassName = GetType().Name;
        }

        public override void talkDefault()
        {
            int selection = 0;           

            if (EventManager.Instance.CurrentEvent.IsQuestion)            
                selection = Convert.ToInt32(EventManager.Instance.CurrentEvent.Selection[0]);
            
            Menu(selection);
        }

        private void Menu(int selection)
        {           
            switch (MenuNav)
            {
                case 0:
                    SendTalk("storageMenu", new List<object> { null, 0 });
                    MenuNav = 1;
                    break;
                case 1:
                    if(selection == 4)
                    {
                        EndTalk();
                    }
                    else
                    {
                        SendTalk("selectCategory", new List<object> { });
                        MenuNav = 2;
                        SelectedMenu = selection;
                    }                    
                    break;
                case 2:
                    if (selection > 0)
                    {
                        if (selection == 5)
                        {
                            MenuNav = 0;
                            Menu(selection);
                        }
                        else
                        {
                            string function = "selectStoreItem";

                            if (SelectedMenu == 2)                    
                                function = "selectReceiveItem";

                            SendTalk(function, new List<object> { null, selection });
                        }                            
                    }
                    else if (selection == 0)
                    {
                        MenuNav = 1;
                        Menu(selection);
                    }
                    else
                    {
                        EndTalk();
                    }
                    break;
                default:
                    EndTalk();
                    break;
            }
        }

        private void EndTalk()
        {           
            EventManager.Instance.CurrentEvent.Finish();
            MenuNav = 0;
            SelectedMenu = 0;
        }

        private void SendTalk(string function, List<object> parameters)
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse(function, parameters, true);
        }
    }
}
