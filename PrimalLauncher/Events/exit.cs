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

namespace PrimalLauncher
{
    public class exit : EventRequest
    {
        public exit(byte[] data) : base(data) { }

        public override void Execute()
        {
            //base.Execute();
            foreach (Quest quest in User.Instance.Character.Journal.GetAllQuests())
            {
                QuestStep = quest.GetActorStep(GetType().Name);

                if (QuestStep != null)
                    break;
            }

            if(QuestStep == null)            
                InvokeActorEvent(GetActor());

            //Finish();
        }
    }
}
