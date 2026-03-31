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
using System.Deployment.Application;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace PrimalLauncher
{
    /// <summary>
    /// TODO: fix menu navigation; check leve quests for class/level;implement allowances check and leve management functions.
    /// </summary>
    public class PopulacePassiveGLPublisher : PopulaceStandard
    {
        private sbyte MenuNav { get; set; }
        private sbyte LevePackSelected { get; set; }
        private sbyte LeveRankSelected { get; set; }
        private sbyte LeveSelected { get; set; }
        private sbyte MenuPrevious { get; set; } //had to create this to flag when to close the menu.
        private sbyte PlayerSelection { get; set; }

        private List<PassiveGL> PassiveGLs { get; set; }

        public PopulacePassiveGLPublisher()
        {
            ClassName = GetType().Name;            
        }

        public override void talkDefault()
        {
            int localLeveAvailableSlots = User.Instance.Character.Journal.QuestGuildleve.Values.Count(x => x is null);

            if (!EventManager.Instance.CurrentEvent.IsQuestion)
            {
                MenuNav = 1;

                //if(User.Instance.Character.LeveAllowances == 0)
                //{                    
                //    SendTalk("talkOfferMaxOver", new List<object> { User.Instance.Character.Id }); //not sure if this is correct
                //    EndTalk();
                //    World.SendTextSheet(50142);
                //    return;
                //}
                //else if(localLeveAvailableSlots == 0)
                //{
                //    SendTalk("askDiscardGuildleve", new List<object> { User.Instance.Character.Id });
                //}
                //else
                //{
                SendTalk("talkOfferWelcome", new List<object> { User.Instance.Character.Id, User.Instance.Character.LeveAllowances });
                //}
            }
            else
            {
                uint selection = 0;                

                if (EventManager.Instance.CurrentEvent.Selection.Length > 0)
                    selection = Convert.ToUInt32(EventManager.Instance.CurrentEvent.Selection[0]);

                PlayerSelection = selection == 0xffffffff ? (sbyte)0 : (sbyte)selection;

                //if (localLeveAvailableSlots == 0)
                //{
                //    MenuDiscardLeve();
                //}
                //else
                //{
                Menu();
                //}
            }
        }

        private void Menu()
        {            
            int navCode = GetNavCode();

            //Log.Instance.Info("MenuCode: " + navCode);

            switch (navCode)
            {
                case 100:
                case 102:
                    AskOfferPack();
                    MenuNav = 2;
                    MenuPrevious = 1;
                    break;
                case 211:
                case 203:
                case 403:
                    AskOfferRank();
                    MenuNav = 3;
                    MenuPrevious = 2;
                    break;
                case 302:
                    MenuNav = 1;
                    MenuPrevious = 2;
                    Menu();
                    break;
                case 201:
                    EndTalk();
                    break;
                case 304:
                case 312:
              
                    AskOfferQuest();
                    MenuNav = 4;
                    MenuPrevious = 3;                    
                    return;                
                case 413:
                    TalkOfferDecide();
                    MenuNav = 3;
                    MenuPrevious = 2;
                    break;
            }
        }

        private void AskOfferPack()
        {          
            SendTalk("askOfferPack", new List<object> { User.Instance.Character.Id });
        }

        private void AskOfferRank()
        {           
            LevePackSelected = PlayerSelection > 0 ? PlayerSelection : LevePackSelected;
            SendTalk("askOfferRank", new List<object> { User.Instance.Character.Id });
        }

        private void AskOfferQuest()
        {
            List<object> parameters = new List<object> { User.Instance.Character.Id, 2 };
            LeveRankSelected = PlayerSelection > 0 ? PlayerSelection : LeveRankSelected;      
            
            foreach(var id in GetLeveListIds()) //had to do it like this because uint will crash the function.
                parameters.Add(Convert.ToInt32(id));

            SendTalk("askOfferQuest", parameters);
        }

        private void TalkOfferDecide()
        {          
            LeveSelected = PlayerSelection;
            User.Instance.Character.Journal.AddLocalleve(GetLeveById());
            Thread.Sleep(600);
            SendTalk("talkOfferDecide", new List<object> { User.Instance.Character.Id });
            User.Instance.Character.LeveAllowances--;
            User.Instance.Character.Journal.SendLeveAllowancesRamaining();
        }

        private int GetNavCode()
        {
            int hasSelection = PlayerSelection > 0 ? 10 : 0;
            return MenuNav * 100 + hasSelection + MenuPrevious;
        }

        private List<PassiveGL> GetLeveList()
        {
            int minLevel = 1;
            int maxLevel = 19;

            if(LeveRankSelected == 2)
            {
                minLevel = 20;
                maxLevel = 39;
            }
            else if(LeveRankSelected == 3)
            {
                minLevel = 40;
                maxLevel = 59;
            }

            List<PassiveGL> result = PassiveGLs
                .Where(x =>
                    x.Class == LevePackSelected &&
                    x.Level >= minLevel &&
                    x.Level <= maxLevel)                
                .ToList();

            return result;
        }

        private List<object> GetLeveListIds()
        {
            var result = GetLeveList().Select(x => (object)x.Id).ToList();

            foreach(PassiveGL leve in User.Instance.Character.Journal.QuestGuildleve.Values)
            {
                if (leve != null && result.Contains(leve.Id))
                    result.Remove(leve.Id);
            }           

            if (!result.Any()) //if there are no leves for the selection, we want to show an empty screen.
                result.Add(0);

            return result;
        }

        private PassiveGL GetLeveById()
        {
            var list = GetLeveList();
            return list[LeveSelected - 1];
        }

        private void MenuDiscardLeve()
        {
            switch (MenuNav)
            {
                case 1:
                    SendTalk("askDiscardGuildleve", new List<object> { User.Instance.Character.Id });
                    MenuNav = 2;
                    break;
                case 2:
                    SendTalk("selectDiscardGuildleve", new List<object> { User.Instance.Character.Id });
                    MenuNav = 3;
                    break;
                case 3:
                    SendTalk("confirmDiscardGuildleve", new List<object> { User.Instance.Character.Id });
                    MenuNav = 3;
                    break;
            }

            //discard
            //1. askDiscardGuildleve you cant accept more, want to return one?
            //2. selectDiscardGuildleve opens guildleve list dialog so user can select one to discard. send player id as param
            //3. confirmDiscardGuildleve parameters will come from 2? param 1 = null,      
        }

        private void CheckAllowances()
        {
            //confirmOffer yes/no dialog. 
            //confirmMaxOffer if you accept you will have maximum. yes/no
            //confirmJournal ??
        }

        private void SendTalk(string function, List<object> parameters)
        {
            EventManager.Instance.CurrentEvent.SendTalkResponse(function, parameters, true);
        }

        private void EndTalk()
        {
            EventManager.Instance.CurrentEvent.Finish();  
            MenuNav = 0;
            LevePackSelected = 0;
            LeveRankSelected = 0;
            MenuPrevious = 0;
        }

        public void LoadPassiveGls()
        {
            PassiveGLs = GuildLeveXmlLoader.GetPassiveGLs(Position.ZoneId);
        }
    }
}
