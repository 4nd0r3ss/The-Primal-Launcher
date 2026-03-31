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
using System.Threading;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class commandJudgeMode : EventRequest
    {
        public Command CommandId { get; set; }
        public byte[] Data { get; set; }

        public int MenuNav { get; set; }

        public commandJudgeMode(byte[] data) : base(data)
        {
            Data = data;
            CommandId = (Command)(data[0x15] << 8 | data[0x14]);
            OwnerId = 0;
        }

        public override void Execute()
        {
            Log.Instance.Info("Event: " + GetType().Name + ", Command: 0x" + CommandId.ToString("X"));

            switch (CommandId)
            {
                case Command.AttributePoints:
                    AttributePoints();
                    break;
                case Command.Craft:
                    CraftStart();
                    break;
            }
        }



        #region Craft
        private void CraftEventResult(List<object> parameters)
        {
            if (parameters[0] is bool) //confirm dialogs
            {
                bool option = Convert.ToBoolean(parameters[0]);

            }
            else if (parameters[0] is int) //window navigation
            {
                int selection = Convert.ToInt32(parameters[0]);

                if(parameters.Count == 1)
                {
                    switch (selection)
                    {
                        case 1:                        
                            CraftConfirmRecipe();
                            break;
                        case 2:
                            //if reached here, we should give material list.
                            CraftStart();
                            break;
                        case 3:
                            CraftSelectLeve();
                            break;
                    }
                }
                else if(parameters.Count > 1)
                {
                    switch (selection)
                    {
                        case 0:
                            CraftCloseInterface();
                            break;
                        case 3:
                            CraftSelectLeve();
                            break;
                        case 7:
                            CraftSelectRecipe();
                            break;
                    }
                }
            }
            else if (parameters[0] is uint) //leve selected
            {
                uint questId = Convert.ToUInt32(parameters[0]);


            }            
        }

        private void CraftStart(List<int> materials = null)
        {
            PlayerCharacter pc = User.Instance.Character;
            var luaParameters = new LuaParameters();
            uint commandActor = 0xA0F00000 | (int)Command.Craft;

            if (pc.State.Main != MainState.CraftStance)
            {
                pc.State.Main = MainState.CraftStance;
                pc.SetMainState();
                
                luaParameters.Add(0xA0F4E203); //not sure what this actor is.
                luaParameters.Add("loadTextData");
                luaParameters.Add(commandActor); //total points earned so far  

                SendDelegateCommand(luaParameters);

                //change player substate
                pc.SubState.Waste = 0x0C;
                pc.SetSubState();

                //kneel down animation
                pc.SendCommandResult(0, animationId: 0x7C000062, senderId: pc.Id);
            }            

            //open synth interface
            luaParameters = new LuaParameters();
            luaParameters.Add(0xA0F4E203);
            luaParameters.Add("start");
            luaParameters.Add(pc.Id);
            luaParameters.Add(0);
            luaParameters.Add(materials != null && materials.Count > 0);

            if (materials != null)
            {
                foreach (var item in materials)
                    luaParameters.Add(item);
            }
            else
            {
                for(int i = 0; i < 8; i++)
                    luaParameters.Add(0);
            }

            SendDelegateCommand(luaParameters);
        }
             
        private void CraftSelectRecipe()
        {
            var luaParameters = new LuaParameters();
            luaParameters.Add(0xA0F4E203);
            luaParameters.Add("selectRcp");
            luaParameters.Add(0xA0F00000 | (uint)CommandId);
            luaParameters.Add(0x7A88D4); //recipes are item ids. can add multiple, doesnt look like it has a limit.
            SendDelegateCommand(luaParameters);
        }

        private void CraftConfirmRecipe()
        {
            var luaParameters = new LuaParameters();
            luaParameters.Add(0xA0F4E203);
            luaParameters.Add("confirmRcp");
            luaParameters.Add(0xA0F00000 | (uint)CommandId);

            //fake value just for testing
            luaParameters.Add(0x7A88D4);
            luaParameters.Add(1);
            luaParameters.Add(0x0F4247);
            luaParameters.Add(1);
            luaParameters.Add(0x0F4245);
            luaParameters.Add(1);
            luaParameters.Add(0);
            luaParameters.Add(0);

            SendDelegateCommand(luaParameters);
        }

        private void CraftSelectLeve()
        {
            var luaParameters = new LuaParameters();
            luaParameters.Add(0xA0F4E203);
            luaParameters.Add("selectCraftQuest");
            luaParameters.Add(0xA0F00000 | (uint)CommandId);            

            Thread.Sleep(2000); //need a small delay here or the game will ignore the command for some reason.
            SendDelegateCommand(luaParameters);
        }

        private void CraftCloseInterface()
        {
            var luaParameters = new LuaParameters();
            luaParameters.Add(0xA0F4E203);
            luaParameters.Add("closeCraftStartWidget");
            luaParameters.Add(0xA0F00000 | (uint)CommandId);
            SendDelegateCommand(luaParameters);

            PlayerCharacter pc = User.Instance.Character;
            pc.SubState.Waste = 0;
            pc.SetSubState();
            pc.State.Main = MainState.Passive;
            pc.SetMainState();
            pc.SendCommandResult(0, animationId: 0x7C000062, senderId: pc.Id);

            Finish();
        }
        #endregion

        private void AttributePoints()
        {
            byte[] data = new byte[0x90];
            uint commandActor = 0xA0F00000 | (int)Command.AttributePoints;

            data.Write(0, User.Instance.Character.Id);
            data.Write(0x04, commandActor);
            data.Write(0x09, "commandJudgeMode");
            data.Write(0x29, "delegateCommand");

            var luaParameters = new LuaParameters();
            luaParameters.Add(commandActor);
            luaParameters.Add("operateUI");
            luaParameters.Add(0x20); //total points earned so far
            luaParameters.Add(0x0A); //seems to affect total points and available points.
            luaParameters.Add(0x0C); //strength
            luaParameters.Add(0); //vitality
            luaParameters.Add(0x0D); //dexterity
            luaParameters.Add(0); //intelligence
            luaParameters.Add(0); //mind
            luaParameters.Add(0); //piety

            LuaParameters.WriteParameters(ref data, luaParameters, 0x49);
            Packet.Send(ServerOpcode.EventRequestResponse, data);
        }

        private void SendDelegateCommand(LuaParameters parameters)
        {
            byte[] data = new byte[0x90];

            data.Write(0, User.Instance.Character.Id);
            data.Write(0x04, 0xA0F00000 | (uint)CommandId);
            data.Write(0x09, "commandJudgeMode");
            data.Write(0x29, "delegateCommand");

            LuaParameters.WriteParameters(ref data, parameters, 0x49);
            Packet.Send(ServerOpcode.EventRequestResponse, data);
        }

        public override void ProcessEventResult(byte[] data)
        {
            List<object> luaParameters = LuaParameters.ReadParameters(data, 0x21);

            switch (CommandId)
            {                
                case Command.Craft:
                    CraftEventResult(luaParameters);
                    break;
            }
        }
    }
}
