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

namespace PrimalLauncher
{
    public class PopulaceLinkshellManager : PopulaceStandard
    {
        private bool MenuIsOpen { get; set; }

        public PopulaceLinkshellManager()
        {
            ClassName = GetType().Name;            
        }       

        public override void talkDefault()
        {
            if (MenuIsOpen)
            {
                uint? selection = (uint?)EventManager.Instance.CurrentEvent.Selection[0];

                switch (selection)
                {
                    
                    case 0x03:                        
                        CreateLinkshell();
                        break;
                    case 0x0A:
                    default:
                        EventManager.Instance.CurrentEvent.Finish();
                        MenuIsOpen = false;
                        break;
                }
            }
            else
            {
                if (!EventManager.Instance.CurrentEvent.IsQuestion)
                {
                    bool isFirstTime = User.Instance.Character.Groups.Any(x => x.GetType().Name == "GroupLinkshell");

                    EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkStep1", new List<object> { isFirstTime, User.Instance.Character.Id }, true);
                }
                else
                {
                    EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkStep2", new List<object>(), true);
                    MenuIsOpen = true;
                }
            }
        }
        
        /// <summary>
        /// Creates a new linkshell.
        /// TODO: fix crest as it's not showing up.
        /// </summary>
        private void CreateLinkshell()
        {           
            var newLinkshell = new LinkshellGroup(EventManager.Instance.CurrentEvent.Data);

            newLinkshell.SendPackets();
            newLinkshell.InitWork();
            newLinkshell.SetActive();

            EventManager.Instance.CurrentEvent.SendTalkResponse("eventTalkStepMakeupDone", new List<object> { true, User.Instance.Character.Id }, true);
            MenuIsOpen = false;
        }

        /// <summary>
        /// TODO: implement crest change.
        /// </summary>
        private void ChangeCrest()
        {
            EventManager.Instance.CurrentEvent.Finish();
        }

        /// <summary>
        /// TODO: implement name change.
        /// </summary>
        private void ChangeName()
        {
            EventManager.Instance.CurrentEvent.Finish();
        }

        /// <summary>
        /// TODO: implement disband
        /// </summary>
        private void Disband()
        {
            EventManager.Instance.CurrentEvent.Finish();
        }
    }
}
