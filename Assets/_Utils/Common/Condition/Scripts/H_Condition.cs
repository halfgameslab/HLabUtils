using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace H_Misc
{
    public enum H_EConditionOperation
    {
        AND,
        OR
    }

    public enum H_ECombineOperation
    {
        JOIN,
        APPEND
    }

    public enum H_EValueType
    { 
        CVAR,
        VALUE,
        METHOD
    }

    public enum H_EValueMode
    {
        SINGLE_VALUE,
        RANDOM_VALUE,
        RANDOM_INTERVAL
    }

    public enum H_EConditionType
    {
        CHECK_VAR,
        ON_CHANGE_VAR,
        CONDITION,
        ON_EVENT_DISPATCH,
        LISTEN_QUEST,
        TIMER,
        METHOD,
        NONE
    }

    public enum H_ETimeMode
    {
        ASC,
        DESC
    }

    public class H_Val
    {
        public H_EValueType ValueType { get; set; } = H_EValueType.VALUE;
        public object Value { get; set; }
    }

    public class H_Condition
    {
        private H_EConditionType _type = H_EConditionType.NONE;

        private string _uname = string.Empty;
        [XmlElement("un")]
        public string UName 
        {
            get
            {
                return _uname;
            }
            set
            {
                if(_uname != value)
                {
                    _uname = value;
                    ParentGlobalUName = ParentGlobalUName;// update the globalUID
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
                }
            }
        }

        private string _globalUName = string.Empty;
        [XmlIgnore]
        public string GlobalUName
        {
            get
            {
                return _globalUName;
            }
            set
            {
                if (_globalUName != value)
                {
                    _globalUName = value;

                    if (Conditions != null)
                    {
                        foreach (H_Condition c in Conditions)
                        {
                            c.ParentGlobalUName = GlobalUName;
                        }
                    }
                }
            }
        }

        [XmlIgnore]
        public string ParentGlobalUName
        {
            get
            {
                string[] s = GlobalUName.Split('.');

                if(s?.Length > 0)
                    return string.Join(".", s, 0, s.Length-1);

                return GlobalUName;
            }
            set
            {
                GlobalUName = string.Format("{0}.{1}", value, UName);
            }
        }


        [XmlAttribute("t")]
        public H_EConditionType Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    if (_type == H_EConditionType.CONDITION)
                    {
                        Conditions = new List<H_Condition>();
                        Params = null;
                    }
                    else
                    {
                        Conditions = null;
                        CreateParamsByType(value);
                    }

                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
                }
            }
        }

        private H_EConditionOperation _operation = H_EConditionOperation.AND;
        [XmlAttribute("op")]
        public H_EConditionOperation Operation 
        {
            get
            {
                return _operation;
            }
            set
            {
                if (_operation != value)
                {
                    _operation = value;
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
                }
            }
        }

        private H_ECombineOperation _combineOperation = H_ECombineOperation.JOIN;
        [XmlAttribute("cop")]
        public H_ECombineOperation CombineOperation
        {
            get
            {
                return _combineOperation;
            }
            set
            {
                if (_combineOperation != value)
                {
                    _combineOperation = value;
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
                }
            }
        }

        private int _repeatCount = 0;
        [XmlAttribute("r")]
        public int RepeatCount 
        { 
            get 
            { 
                return _repeatCount; 
            } 
            set
            {
                if (_repeatCount != value)
                {
                    _repeatCount = value;
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
                }
            }
        }

        [XmlArray("pl")]
        [XmlArrayItem("pi")]
        public object[] Params { get; set; }

        [XmlArray("cl")]
        [XmlArrayItem("ci")]
        public List<H_Condition> Conditions { get; set; }

        public bool IsLeaf { get { return !Type.Equals("Condition"); } }

        public void CreateParamsByType(H_EConditionType type)
        {
            if(type == H_EConditionType.CHECK_VAR || type == H_EConditionType.ON_CHANGE_VAR)
            {
                AddParams("Int32.global.undefined", CVarCommands.EQUAL, H_EValueMode.SINGLE_VALUE, H_EValueType.VALUE, 0);
            }
            else if (type == H_EConditionType.ON_EVENT_DISPATCH)
            {
                AddParams("", "");
            }
            else if (type == H_EConditionType.LISTEN_QUEST)
            {
                AddParams("global.undefined", "", true);
            }
            else if (type == H_EConditionType.TIMER)
            {
                AddParams("timer_id", H_ETimeMode.ASC, 1.0f, false);
            }
            else if (type == H_EConditionType.METHOD)
            {
                AddParams("", "undefined", (object[])null);
            }
        }

        public void AddCondition(H_Condition condition)
        {
            if (Conditions == null)
                Conditions = new List<H_Condition>();

            if (condition != null && !Conditions.Contains(condition))
            {
                Conditions.Add(condition);
                condition.ParentGlobalUName = GlobalUName;
                condition.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                condition.ListenConditions();

                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
            }
        }

        public void InsertCondition(int index, H_Condition condition)
        {
            if (Conditions == null)
                Conditions = new List<H_Condition>();

            if (condition != null && !Conditions.Contains(condition))
            {
                Conditions.Insert(index, condition);
                condition.ParentGlobalUName = GlobalUName;
                condition.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                condition.ListenConditions();

                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
            }
        }

        public void SwapConditions(int a, int b)
        {
            if (a != b)
            {
                H_Condition aux = Conditions[a];
                Conditions[a] = Conditions[b];
                Conditions[b] = aux;

                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
            }
        }

        public void RemoveCondition(int index)
        {
            if (Conditions != null && index < Conditions.Count)
            {
                Conditions[index].RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                Conditions[index].StopListenConditions();
                Conditions.RemoveAt(index);
            }

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
        }

        public void RemoveCondition(H_Condition condition)
        {
            if (Conditions != null)
            {
                condition?.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                condition?.StopListenConditions();

                Conditions.Remove(condition);
            }

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
        }

        public void AddParams(params object[] values)
        {
            Params = values;

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
        }

        public void UpdateParam(int index, object value)
        {
            if (Params == null)
                CreateParamsByType(_type);

            if (Params.Length > index)
                Params[index] = value;

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
        }

        public void UpdateParams(params object[] values)
        {
            Params = values;
            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
        }

        public void ListenConditions()
        {
            if(Conditions != null)
                foreach(H_Condition condition in Conditions)
                {
                    if ( condition != null )
                    {
                        if(!condition.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler))
                            condition.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);

                        condition.ListenConditions();
                    }
                }
        }

        public void StopListenConditions()
        {
            if (Conditions != null)
            {
                foreach (H_Condition condition in Conditions)
                {
                    condition?.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                    condition?.StopListenConditions();
                }
            }
        }

        private void OnConditionChangeValueHandler(ES_Event ev)
        {
            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
        }

        public H_Condition Clone(string sufix = " (clone)")
        {
            H_Condition clone = new H_Condition();
            clone.Type = _type;

            // clone params one by one
            if (Params != null)
            {
                clone.Params = new object[Params.Length];
                for (int i = 0; i < Params.Length; i++)
                {
                    clone.Params[i] = Params[i];
                }
            }
            clone.Operation = Operation;
            clone.UName = string.Concat(UName, sufix);

            if (Conditions != null)
            {
                foreach (H_Condition c in Conditions)
                {
                    clone.Conditions.Add(c.Clone(string.Empty));
                }
            }

            return clone;
        }
    }


}
