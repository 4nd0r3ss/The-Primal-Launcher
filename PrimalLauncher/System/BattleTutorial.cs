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

using System.Collections.Generic;
using System.Threading;

namespace PrimalLauncher
{
    public class BattleTutorial
    {
        private static BattleTutorial _instance = null;
        private static readonly object _padlock = new object();   

        private bool AttackingTutorialDone { get; set; }
        private bool WeaponskillTutorialDone { get; set; }
        private bool WeaponskillSuccessTutorialDone { get; set; }

        private bool MagicTutorialDone { get; set; }
        private bool MagicSuccessTutorialDone { get; set; }

        public static BattleTutorial Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new BattleTutorial();

                    return _instance;
                }
            }
        }

        /// <summary>
        /// This function will start the battle tutorial at the beginning of the game.
        /// </summary>
        /// <param name="sender"></param>
        public void Start()
        {                   
            uint openingStoperClassId = 0;           
            List<uint> dutyActorsClassId = null;
            uint instanceId = 0;

            switch (User.Instance.Character.InitialTown)
            {
                case 1:
                    instanceId = 2;
                    dutyActorsClassId = new List<uint> { 2290001, 2290002, 2205403 };
                    break;
                case 2:       
                    instanceId = 6;
                    openingStoperClassId = 1090384;
                    dutyActorsClassId = new List<uint> { 2290005, 2290006, 2201407 };
                    break;
                case 3:
                    instanceId = 10;
                    openingStoperClassId = 1090373;
                    dutyActorsClassId = new List<uint> { 2290003, 2290004, 2203301 };
                    break;
            }

            //if there is an opening stoper, we want to disable its events before antering battle.
            //this is to fix the player being positioned in the wrong place in the battle tutorial.
            if (openingStoperClassId > 0)
            {
                Actor openingStoper = User.Instance.Character.GetCurrentZone().GetActorByClassId(openingStoperClassId);

                if (openingStoper != null)
                {
                    openingStoper.ToggleEvents(false);
                    Thread.Sleep(1500);
                }
            }

            World.Instance.CreateInstance(instanceId);
            World.Instance.ZoneInstance.Directors.Add(new OpeningDirector());           
            World.Instance.ToInstance(instanceId, 0x10);
            BattleManager.Instance.AddDutyGroup(dutyActorsClassId, true);  

            //Set allies to battle position before first attack
            List<Actor> fighters = User.Instance.Character.GetCurrentZone().GetActorsByFamily("fighter");

            foreach (Actor fighter in fighters)
            {
                fighter.State.Main = MainState.Active;
                fighter.SetMainState();
            }            
        }

        public void NextTutorial(string tutorialName)
        {
            JobClassCategory category = User.Instance.Character.CharaWork.CurrentJob.GetCategory();

            if(category == JobClassCategory.DoW)
            {
                if (tutorialName == "tp" && !AttackingTutorialDone) TpTutorial();
                else if (tutorialName == "weaponskill" && !WeaponskillTutorialDone) WeaponskillTutorial();
                else if (tutorialName == "weaponskillsuccess" && !WeaponskillSuccessTutorialDone) WeaponskillSuccessTutorial();
            }
            else if(category == JobClassCategory.DoM)
            {              
                if (tutorialName == "magic" && !MagicSuccessTutorialDone) MagicSuccessTutorial();
            }            
        }

        private void TpTutorial()
        {
            World.Instance.CloseTutorialWidget();
            World.Instance.ShowSuccessDialog(new object[] { 0x235f });      //succesfully attacked
            Thread.Sleep(2000);
            World.Instance.OpenTutorialWidget(new object[] { User.Instance.Character.ControlScheme, 0x0C }); //TP
            AttackingTutorialDone = true;            
        }

        private void WeaponskillTutorial()
        {
            World.Instance.CloseTutorialWidget();
            World.Instance.OpenTutorialWidget(new object[] { User.Instance.Character.ControlScheme, 0x0D }); //weaponskill
            WeaponskillTutorialDone = true;
        }

        private void WeaponskillSuccessTutorial()
        {
            World.Instance.CloseTutorialWidget();            
            World.Instance.ShowSuccessDialog(new object[] { 0x2369 });  //weaponskill success
            WeaponskillSuccessTutorialDone = true;
            //Finish(); //this will be removed once I can detect group mobs dead
        }

        private void MagicSuccessTutorial()
        {
            MagicSuccessTutorialDone = true;
            //Finish(); //this will be removed once I can detect group mobs dead
        }

        public void Finish()
        {
            JobClassCategory jobClassCategory = User.Instance.Character.CharaWork.CurrentJob.GetCategory();

            if (jobClassCategory == JobClassCategory.DoW || jobClassCategory == JobClassCategory.DoM)
            {
                World.Instance.CloseTutorialWidget();
                World.Instance.ShowAttentionDialog(new object[] { 0xC781, (int)User.Instance.Character.InitialTown });
            }
            
            Thread.Sleep(3000);
            World.Instance.SetMusic(0x07, MusicMode.Crossfade); //0x07=silence                         
            ((QuestDirector)User.Instance.Character.GetCurrentZone().GetDirector("Quest")).StartEvent("noticeEvent");            
            User.Instance.Character.IsTutorialComplete = true;
        }
    }
}
