using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    public class Object : Actor
    {
        public Object()
        {
            ClassPath = "/chara/npc/object/";
            ClassCode = 0x30400000;            
        }

        public override void Spawn(Socket sender, ushort spawnType = 0, ushort isZoning = 0, int changingZone = 0)
        {
            Prepare();
            CreateActor(sender, 0x08);
            SetEventConditions(sender);
            SetSpeeds(sender);
            SetPosition(sender);
            SetAppearance(sender);
            SetName(sender);
            SetMainState(sender);
            SetSubState(sender);
            SetAllStatus(sender);
            SetIsZoning(sender);
            SetLuaScript(sender);
            Init(sender);
            SetEventStatus(sender);           
            Spawned = true;
        }

        public override void Init(Socket sender)
        {
            WorkProperties property = new WorkProperties(sender, Id, @"/_init");

            if (ClassName == "RetainerFurniture")
            {                
                property.Add("charaWork.property[0]", true);
                property.Add("charaWork.property[1]", true);
                property.Add("npcWork.pushCommand", (short)0x2718);
                property.Add("npcWork.pushCommandPriority", (byte)0x08);
                property.FinishWriting(Id);
                return;
            }
            
            property.Add("charaWork.battleSave.potencial", 0x3F800000);
            property.Add("charaWork.property[0]", true);  
            
            if(ClassName == "ObjectBed" || ClassName == "ObjectItemStorage")
            {
                property.Add("charaWork.property[1]", true);
                property.Add("charaWork.property[4]", true);
            }

            property.Add("charaWork.parameterSave.hp[0]", (short)0x01F4);
            property.Add("charaWork.parameterSave.hpMax[0]", (short)0x01F4);
            property.Add("charaWork.parameterSave.mp", (short)0);
            property.Add("charaWork.parameterSave.mpMax", (short)0);
            property.Add("charaWork.parameterTemp.tp", (short)0);
            property.Add("charaWork.parameterSave.state_mainSkill[0]", (byte)0x03);
            property.Add("charaWork.parameterSave.state_mainSkill[2]", (byte)0x03);
            property.Add("charaWork.parameterSave.state_mainSkillLevel", (short)0x02);
            property.Add("npcWork.hateType", (byte)0x01);
            property.FinishWriting(Id);
        }

        #region Opening stopper methods
        public void caution(Socket sender)
        {
            Event caution = Events.Find(x => x.Name == "caution");

            if (caution != null && !string.IsNullOrEmpty(caution.Action))
            {
                string action = caution.Action;
                List<object> parameters = new List<object>();
                parameters.Add(sender);

                //if the action has parameters
                if (action.IndexOf(":") > 0)
                {
                    string[] split = action.Split(new char[] { ':' }); //separate function name from parameter string
                    action = split[0]; //function name 
                    parameters.Add(split[1]); //parameter string
                }

                MethodInfo method = typeof(World).GetMethod(action);
                PropertyInfo property = typeof(World).GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                object instance = property.GetValue(null);
                method.Invoke(instance, parameters.ToArray());

            }
        } 
        
        public void exit(Socket sender)
        {
            Event exit = Events.Find(x => x.Name == "exit");

            if (exit != null && !string.IsNullOrEmpty(exit.Action))
            {
                string action = exit.Action;

                if (action.IndexOf(":") > 0)
                {
                    string[] split = action.Split(new char[] { ':' });

                    switch (split[0])
                    {
                        case "TurnBack":                            
                            User.Instance.Character.TurnBack(sender, Convert.ToSingle(split[1]));
                            break;
                    }
                }
            }
        }
        #endregion

        public void AskLogout(Socket sender)
        {
            if (EventManager.Instance.CurrentEvent.IsQuestion)
            {
                switch (EventManager.Instance.CurrentEvent.Selection)
                {
                    case 2:
                        PlayerCharacter.ExitGame(sender);
                        break;
                    case 3:
                        PlayerCharacter.Logout(sender);
                        break;
                    case 4:
                        Log.Instance.Success("Check bed selected.");
                        break;
                }

                EventManager.Instance.CurrentEvent.Finish(sender);
            }
            else
            {
                EventManager.Instance.CurrentEvent.IsQuestion = true;
                EventManager.Instance.CurrentEvent.FunctionName = "AskLogout";
                EventManager.Instance.CurrentEvent.RequestParameters.Add(Encoding.ASCII.GetBytes("askLogout"));
                EventManager.Instance.CurrentEvent.RequestParameters.Add(User.Instance.Character.Id);
                EventManager.Instance.CurrentEvent.Response(sender);
            }
        }      
    }
}
