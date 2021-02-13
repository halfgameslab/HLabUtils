//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Mup.EventSystem.Events;
//using Mup.EventSystem.Events.Internal;
//using System;

//namespace H_QuestSystem
//{
//    public class H_QuestElementGroup
//    { 
//        H_QuestVar[] Vars { get; set; }
//        H_QuestEvent[] Events { get; set; }

//        H_Quest[] Quests { get; set; }

//        public bool WasCompleted
//        { 
//            get
//            {
//                foreach(H_QuestVar v in Vars)
//                {
//                    if(!v.WasCompleted)
//                    {
//                        return false;
//                    }
//                }
//                foreach (H_QuestEvent ev in Events)
//                {
//                    if (!ev.WasCompleted)
//                    {
//                        return false;
//                    }
//                }

//                foreach (H_Quest q in Quests)
//                {
//                    if (!q.WasCompleted)
//                    {
//                        return false;
//                    }
//                }

//                return true;
//            } 
//        }

//        public void Start()
//        {
//            foreach (H_QuestVar v in Vars)
//            {
//                if (!v.WasCompleted)
//                {
//                    v.AddEventListener(ES_Event.ON_COMPLETE, OnElementCompleteHandler);
//                    v.Start();
//                }
//            }
//            foreach (H_QuestEvent ev in Events)
//            {
//                if (!ev.WasCompleted)
//                {
//                    ev.AddEventListener(ES_Event.ON_COMPLETE, OnElementCompleteHandler);
//                    ev.Start();
//                }
//            }

//            foreach (H_Quest q in Quests)
//            {
//                if (!q.WasCompleted)
//                {
                    
//                }
//            }
//        }

//        private void OnElementCompleteHandler(ES_Event ev)
//        {
//            this.DispatchEvent(ES_Event.ON_COMPLETE);
//        }

//        public void RemoveListeners()
//        {
//            foreach (H_QuestVar v in Vars)
//            {
//                v.RemoveEventListener(ES_Event.ON_COMPLETE, OnElementCompleteHandler);       
//            }
//            foreach (H_QuestEvent ev in Events)
//            {
//                ev.RemoveEventListener(ES_Event.ON_COMPLETE, OnElementCompleteHandler);
//            }

//            foreach (H_Quest q in Quests)
//            {
//                if (!q.WasCompleted)
//                {

//                }
//            }
//        }

//        public void Invoke()
//        {
//            foreach (H_QuestVar v in Vars)
//            {
//                // verificar a probabilidade caso necessário sorteio
//                v.Invoke();
//            }
//            foreach (H_QuestEvent ev in Events)
//            {
//                ev.Invoke();
//            }
//        }
//    }

//    public class H_QuestElement
//    {
//        public string ID { get; set; }
//        public bool Optional { get; set; }

//        public bool WasCompleted 
//        {
//            get { return CVarSystem.GetValue(string.Format("q_o_c_{0}", ID), false); }
//            set { CVarSystem.SetValue(string.Format("q_o_c_{0}", ID), value); } 
//        }

//        //public void Start(string targetIdentifier, string ev, ES_MupAction onEventExecutedHandler)
//        //{
//        //    ES_EventManager.AddEventListener(targetIdentifier, ev, onEventExecutedHandler);
//        //}
//    }

//    public class H_QuestVar : H_QuestElement
//    {
//        public string Fullname { get; set; }
//        public int Address { get; set; }
//        public object StartValue { get; set; }
//        public object Value { get; set; }

//        public string Command { get; set; }

//        public bool ReachedTheEnd
//        {
//            get
//            {
//                return Check(Command, CVarSystem.GetValueByFullName(Fullname, StartValue), Value);
//            }
//        }

//        public void Start()
//        {
//            if (!ReachedTheEnd)
//                ES_EventManager.AddEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
//            else
//                Debug.Log("Complete");
//        }

//        public void StopListeners()
//        {
//            ES_EventManager.RemoveEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
//        }

//        public void Invoke()
//        {
//            if(CVarCommand.TryExecuteAction(Command, out object result, CVarSystem.GetValueByFullName(Fullname, StartValue), Value))
//            {
//                CVarSystem.SetValueByFullName(Fullname, result);
//            }
//        }

//        private void OnValueChangerHandler(ES_Event ev)
//        {
//#if UNITY_EDITOR
//            if(!Application.isPlaying)
//            {
//                ES_EventManager.RemoveEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
//                return;
//            }
//#endif
//            Debug.Log(Check(Command, ev.Data, Value));
//            if (Check(Command, ev.Data, Value))
//            {
//                ES_EventManager.RemoveEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);

//                WasCompleted = true;
//                this.DispatchEvent(ES_Event.ON_COMPLETE);
//            }
//        }

//        private static bool Check(string command, object a, object b)
//        {
//            return CVarCommand.ExecuteAction(command, a, b);
//        }
//    }

//    public class H_QuestEvent : H_QuestElement
//    { 
//        public string Target { get; set; }

//        public string Event { get; set; }

//        public int CountExecutions { get; set; }

//        public bool ReachedTheEnd
//        {
//            get
//            {
//                return CountExecutions == 0;
//            }
//        }

//        public void Start()
//        {
//            ES_EventManager.AddEventListener(Target, Event, OnExecuteHandler);
//        }

//        public void StopListeners()
//        {
//            ES_EventManager.RemoveEventListener(Target, Event, OnExecuteHandler);
//        }

//        public void Invoke()
//        {
//            ES_EventManager.DispatchEvent(Target, Event);
//        }

//        private void OnExecuteHandler(ES_Event ev)
//        {
//            CountExecutions--;

//            if(CountExecutions == 0)
//            {
//                ES_EventManager.RemoveEventListener(Target, Event, OnExecuteHandler);
                
//                WasCompleted = true;
//                this.DispatchEvent(ES_Event.ON_COMPLETE);
//            }
//        }
//    }

//}