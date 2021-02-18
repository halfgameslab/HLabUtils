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

    public class H_Condition
    {
        private string _type = "CheckVar";

        [XmlAttribute("uid")]
        public string UID { get; set; }


        [XmlAttribute("t")]
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    if (_type == "Condition")
                    {
                        Conditions = new List<H_Condition>();
                    }
                    else
                    {
                        Conditions = null;
                    }
                    
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE);
                }
            }
        }

        public H_EConditionOperation _operation = H_EConditionOperation.AND;
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

        public H_ECombineOperation _combineOperation = H_ECombineOperation.JOIN;
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

        [XmlArray("pl")]
        [XmlArrayItem("pi")]
        public object[] Params { get; set; }

        [XmlArray("cl")]
        [XmlArrayItem("ci")]
        public List<H_Condition> Conditions { get; set; }

        public bool IsLeaf { get { return !Type.Equals("Condition"); } }

        public void CreateParamsByType(string type)
        {
            if(type == "Condition")
            {
            }
            else if(type == "CheckVar" || type == "OnChangeVar")
            {
                AddParams("global", "String", "<undefined>", CVarCommands.EQUAL, "");

            }
            else if (type == "OnEventDispatch")
            {

            }
            else if (type == "ListenQuest")
            {

            }
            else if (type == "Timer")
            {

            }
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

        public void ListenConditions()
        {
            if(Conditions != null)
                foreach(H_Condition condition in Conditions)
                {
                    condition?.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                    condition?.ListenConditions();
                }
        }

        public void StopListenConditions()
        {
            foreach (H_Condition condition in Conditions)
            {
                condition?.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
                condition?.StopListenConditions();
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
            clone.UID = string.Concat(UID, sufix);

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
