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

namespace PrimalLauncher
{
    //When the client is unable to start a requested event, is will send a value
    //in byte 0x10 that is different from the event codes (see EventType enum)
    //0x12D - Event Start Request (client)
    //event codes :
    //50 - actor not found/spawned
    //37 - requested event disabled for actor
    public class EventRequest
    {
        protected LuaParameters _requestParameters;

        public LuaParameters RequestParameters
        {
            get { return _requestParameters; }
            set { _requestParameters = value; }
        }

        public uint CallerId { get; set; }
        public uint OwnerId { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint QuestId { get; set; }
        public byte Code { get; set; }
        public byte[] RequestPacket { get; set; }
        public string Callback { get; set; }
        public string Name { get; set; }
        public bool IsQuestion { get; set; }
        public uint?[] Selection { get; set; }
        public QuestPhaseStep QuestStep { get; set; }
        public bool ReturnToOwner { get; set; }
        public byte[] Data { get; set; }
        public List<KeyValuePair<string, string>> OnCutsceneStart { get; set; }
        public List<KeyValuePair<string, string>> OnCutsceneEnd { get; set; }

        public EventRequest(byte[] requestPacket)
        {
            RequestParameters = new LuaParameters();

            CallerId = (uint)(requestPacket[0x13] << 24 | requestPacket[0x12] << 16 | requestPacket[0x11] << 8 | requestPacket[0x10]);
            OwnerId = (uint)(requestPacket[0x17] << 24 | requestPacket[0x16] << 16 | requestPacket[0x15] << 8 | requestPacket[0x14]);
            Unknown1 = (uint)(requestPacket[0x1b] << 24 | requestPacket[0x1a] << 16 | requestPacket[0x19] << 8 | requestPacket[0x18]);
            Unknown2 = (uint)(requestPacket[0x1f] << 24 | requestPacket[0x1e] << 16 | requestPacket[0x1d] << 8 | requestPacket[0x1c]);
            RequestPacket = requestPacket;
        }

        private void ExecuteStepFunction(Actor eventOwner, uint questId)
        {
            Callback = QuestStep.Value ?? "";
            QuestId = QuestStep.QuestId > 0 ? QuestStep.QuestId : questId;

            if (!string.IsNullOrEmpty(QuestStep.Value))
            {
                if (QuestStep.Value == "default")
                    InvokeActorEvent(eventOwner);
                else
                    SwitchOrDelegateEvent(eventOwner);
            }
            else
            {
                Finish();
            }
        }

        private void SwitchOrDelegateEvent(Actor eventOwner)
        {
            if (QuestStep.SwitchEvent && eventOwner.TalkFunctions.Count > 0)
            {
                SwitchEvent(new uint[] { Actor.GetTalkCode(), QuestId });
            }
            else
            {
                DelegateEvent(QuestId, QuestStep.Value, QuestStep.Parameters);
                QuestStep.ExecuteOnDelegate();
            }
        }

        public virtual void Execute()
        {            
            //Log.Instance.Warning("EventRequest.Execute() ");

            Actor eventOwner = GetActor();
           
            if (eventOwner != null)
            {                
                foreach (Quest quest in User.Instance.Character.Journal.GetAllQuests())
                {                   
                    QuestStep = quest.GetActorStep(GetType().Name, eventOwner);
                    
                    if (QuestStep != null)
                    {
                        ExecuteStepFunction(eventOwner, quest.Id);
                        Log.Instance.Warning("Event: " + GetType().Name + ", Actor: 0x" + OwnerId.ToString("X") + ", Function: " + QuestStep.Value);                        
                        return;
                    }
                }

                //if reached here, there wasn't a step for the actor in any active quest, so we give control back to the actor.
                InvokeActorEvent(eventOwner);
                return;
            }
            else
                Log.Instance.Error("Actor 0x" + OwnerId.ToString("X") + " not found.");            
        }       

        public void InvokeActorEvent(Actor eventOwner)
        {
            //Log.Instance.Warning("EventRequest.InvokeActorEvent()");

            if(eventOwner != null)
            {
                var type = GetType();
                eventOwner.InvokeMethod(type.Name, new object[] { });
                ReturnToOwner = true;
            }            
        }

        /// <summary>
        /// Prepare the lua parameters packet for the current event.
        /// </summary>
        public void InitLuaParameters()
        {
            RequestParameters = new LuaParameters();
            //EventType eventType = (EventType)Enum.Parse(typeof(EventType), GetType().Name);
            RequestParameters.Add((sbyte)0x05);
            RequestParameters.Add(Encoding.ASCII.GetBytes(GetType().Name));
        }

        /// <summary>
        /// Send a packet to the client acknowledging the start of an event processing.
        /// </summary>      
        public virtual void Response(object[] parameters = null)
        {
            //Log.Instance.Warning("EventRequest.Response()");

            if(parameters != null)            
                RequestParameters = new LuaParameters(parameters);

            byte[] data = new byte[0x298];
            Buffer.BlockCopy(BitConverter.GetBytes(CallerId), 0, data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(OwnerId), 0, data, 0x04, 4);
            LuaParameters.WriteParameters(ref data, RequestParameters, 0x08);
            Packet.Send(ServerOpcode.EventRequestResponse, data);
        }

        public virtual void Finish()
        {
            //Log.Instance.Warning("EventRequest.Finish()");

            //process question
            if (QuestStep != null && QuestStep.IsQuestion)
            {
                GetQuestionSelection();

                bool finishEvent = QuestStep.ExecuteSelectedOption();

                if (!finishEvent)
                    return;                
            }

            //finished current event
            byte[] data = new byte[0x30];
            Buffer.BlockCopy(BitConverter.GetBytes(User.Instance.Character.Id), 0, data, 0, sizeof(uint));
            data[0x08] = 1;
            string name = GetType().Name;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(name), 0, data, 0x09, name.Length);
            Packet.Send(ServerOpcode.EventRequestFinish, data);

            //execute tasks after event is finished
            if(QuestStep != null)
            {
                QuestStep.ExecuteOnFinish();
                Thread.Sleep(500); //just to be sure all tasks packets will arrive before finishing phase

                //checked the quest as it was throwing a null pointer for some reason.
                Quest quest = User.Instance.Character.Journal.GetQuestById(QuestStep.QuestId);
                
                if(quest != null) quest.CheckPhase();
            }    
            
            User.Instance.Character.ToggleHeadDirection();
        }

        /// <summary>
        /// Send a packet to the client acknowledging the end of an event processing.
        /// </summary>       
        public virtual void ProcessEventResult(byte[] data)
        {
            Log.Instance.Warning("EventRequest.ProcessEventResult()");

            Data = data;
            Finish();
        }

        /// <summary>
        /// Get the Actor obj by its id#.
        /// </summary>       
        public Actor GetActor()
        {
            Actor eventOwner = User.Instance.Character.GetCurrentZone().Directors.FirstOrDefault(x => x.Id == OwnerId);

            if (eventOwner == null)
                eventOwner = User.Instance.Character.GetCurrentZone().Actors.FirstOrDefault(x => x.Id == OwnerId);

            return eventOwner;
        }

        /// <summary>
        /// Use reflection to call methods by string name.
        /// </summary>      
        public void InvokeMethod(string methodName, object[] methodParams = null)
        {
            //Log.Instance.Warning("EventRequest.InvokeMethod(): " + methodName);

            Type type = GetType();
            var method = type.GetMethod(methodName);

            if (method != null)
                method.Invoke(this, methodParams);
            //else //should be enabled/disabled by debug option?
            //Log.Instance.Warning("EventManager.InvokeMethod: Type " + GetType().Name + " has no method " + methodName + ".");
        }       

        /// <summary>
        /// Delegates a specified function to the caller event. 
        /// </summary>       
        public virtual void DelegateEvent(uint questId, string functionName, object[] parameters = null)
        {
            Log.Instance.Warning("EventRequest.DelegateEvent, function: " + functionName);

            //we may want to execute the function from another quest that is not the current one,
            //like when transitioning from the first to the second start quests for example.
            if(functionName.IndexOf(",") > 0)
            {
                string[] split = functionName.Split(new char[] { ',' });
                functionName = split[0];
                questId = uint.Parse(split[1]);                
            }

            RequestParameters.Add(Encoding.ASCII.GetBytes("delegateEvent"));
            RequestParameters.Add(CallerId);
            RequestParameters.Add(0xA0F00000 | questId);
            RequestParameters.Add(functionName);

            if (parameters == null)
                parameters = new object[] { null, null, null };

            foreach (object obj in parameters)
                RequestParameters.Add(obj);

            Response();
        }

        /// <summary>
        /// Get player selection from a question dialog.
        /// </summary>
        public void GetQuestionSelection()
        {            
            var parameters = LuaParameters.ReadParameters(Data, 0x21);
            List<uint?> result = new List<uint?>();            

            for (int i = 0; i < parameters.Count; i++)
            {
                //this is to fix an exception where negative integers cannot be cast as uint.
                if(parameters[i] is Int32 j && j < 0)                                   
                    parameters[i] = j.IntToUint32();

                if (parameters[i] == null)
                    result.Add(null);
                else
                    result.Add(Convert.ToUInt32(parameters[i]));
            }
            
            Selection = result.ToArray();
        }

        public void SwitchEvent(uint[] talkOptions)
        {
            List<object> list = new List<object>
            {
                Encoding.ASCII.GetBytes("switchEvent")
            };

            foreach (uint i in talkOptions)            
                list.Add((uint)0xA0F00000 | i);            

            list.AddRange(new object[] { null, null, 1, 1, 0x3F1 });

            RequestParameters.Parameters = list.ToArray();
            IsQuestion = true;
            Callback = "talkDefault";
            Response();
        }

        public void AddCutsceneTask(string onStatus, string taskName, string taskParameters)
        {
            if (onStatus == "started")
            {
                if (OnCutsceneStart == null)
                    OnCutsceneStart = new List<KeyValuePair<string, string>>();

                OnCutsceneStart.Add(new KeyValuePair<string, string>(taskName, taskParameters));
            }
            else
            {
                if (OnCutsceneEnd == null)
                    OnCutsceneEnd = new List<KeyValuePair<string, string>>();

                OnCutsceneEnd.Add(new KeyValuePair<string, string>(taskName, taskParameters));
            }
        }

        public void OnCutscene(string status)
        {
            List<KeyValuePair<string, string>> toExecute;

            if (status == "started")
                toExecute = OnCutsceneStart;
            else 
                toExecute = OnCutsceneEnd;

            if (toExecute != null && toExecute.Count > 0)
                QuestPhaseStep.ExecuteStaticTasks(toExecute);
        }

     
    }
}
