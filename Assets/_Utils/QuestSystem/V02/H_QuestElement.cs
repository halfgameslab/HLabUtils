using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;

namespace H_QuestSystem
{
    public class H_QuestElementGroup
    { 
        H_QuestVar[] Vars { get; set; }
        H_QuestEvent[] Events { get; set; }

        H_Quest[] Quests { get; set; }

        public bool WasCompleted 
        { 
            get
            {
                foreach(H_QuestVar v in Vars)
                {
                    if(!v.WasCompleted)
                    {
                        return false;
                    }
                }
                foreach (H_QuestEvent ev in Events)
                {
                    if (!ev.WasCompleted)
                    {
                        return false;
                    }
                }

                foreach (H_Quest q in Quests)
                {
                    if (!q.WasCompleted)
                    {
                        return false;
                    }
                }

                return true;
            } 
        }

        public void Start()
        {
            foreach (H_QuestVar v in Vars)
            {
                if (!v.WasCompleted)
                {
                    v.Start(OnCompleteHandler);
                }
            }
            foreach (H_QuestEvent ev in Events)
            {
                if (!ev.WasCompleted)
                {
                    ev.Start();
                }
            }

            foreach (H_Quest q in Quests)
            {
                if (!q.WasCompleted)
                {
                    
                }
            }
        }

        private void OnCompleteHandler(ES_Event ev)
        {
            
        }

        public void Cancel()
        {

        }

        public void Invoke()
        {

        }
    }

    public class H_QuestElement
    {
        public string ID { get; set; }
        public bool Optional { get; set; }

        public bool WasCompleted { get; set; }

        //public void Start(string targetIdentifier, string ev, ES_MupAction onEventExecutedHandler)
        //{
        //    ES_EventManager.AddEventListener(targetIdentifier, ev, onEventExecutedHandler);
        //}
    }

    public class H_QuestVar : H_QuestElement
    {
        public string Fullname { get; set; }
        public int Address { get; set; }
        public object StartValue { get; set; }
        public object Value { get; set; }

        public string Command { get; set; }

        public bool ReachedTheEnd
        {
            get
            {
                return Check(Command, CVarSystem.GetValueByFullName(Fullname, StartValue), Value);
            }
        }

        public void Start(ES_MupAction onCompleteHandler)
        {
            if (!ReachedTheEnd)
                ES_EventManager.AddEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
            else
                Debug.Log("Complete");
        }

        public void Cancel()
        {
            ES_EventManager.RemoveEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
        }

        public void Invoke()
        {
            if(CVarCommand.TryExecuteAction(Command, out object result, CVarSystem.GetValueByFullName(Fullname, StartValue), Value))
            {
                CVarSystem.SetValueByFullName(Fullname, result);
            }
        }

        private void OnValueChangerHandler(ES_Event ev)
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                ES_EventManager.RemoveEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
                return;
            }
#endif
            Debug.Log(Check(Command, ev.Data, Value));
            if (Check(Command, ev.Data, Value))
            {
                ES_EventManager.RemoveEventListener(Fullname, ES_Event.ON_VALUE_CHANGE, OnValueChangerHandler);
                this.DispatchEvent(ES_Event.ON_COMPLETE);
            }
        }

        private static bool Check(string command, object a, object b)
        {
            return CVarCommand.ExecuteAction(command, a, b);
        }
    }

    public class H_QuestEvent : H_QuestElement
    { 
        public string Target { get; set; }

        public string Event { get; set; }

        public int CountExecutions { get; set; }

        public void Start()
        {
            ES_EventManager.AddEventListener(Target, Event, OnExecuteHandler);
        }

        public void Cancel()
        {
            ES_EventManager.RemoveEventListener(Target, Event, OnExecuteHandler);
        }

        public bool Check()
        {
            return true;
        }

        public void Invoke()
        {
            ES_EventManager.DispatchEvent(Target, Event);
        }

        private void OnExecuteHandler(ES_Event ev)
        {
            CountExecutions--;

            if(CountExecutions == 0)
            {
                this.DispatchEvent(ES_Event.ON_COMPLETE);
                ES_EventManager.RemoveEventListener(Target, Event, OnExecuteHandler);
            }
        }
    }

}