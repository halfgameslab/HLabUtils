﻿using HLab.H_DataSystem;
using H_Misc;
using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Mup.Multilanguage.Plugins;

namespace HLab.H_QuestSystem
{
    public class H_Quest: H_Cloneable<H_Quest>, H_Processable<H_Quest>, H_Groupable<H_Quest, H_PersistentQuestData>
    {
        private string _uname = string.Empty;

        [XmlAttribute("uid")]
        public string UID { get; set; }

        [XmlElement("un")]
        public string UName 
        {
            get
            {
                return _uname;
            }
            set
            {
                if (value != _uname)
                {
                    if (Group != null)
                        _uname = ObjectNamesManager.GetUniqueName(Group.Data.Where(e=>e != this).Select(e => e.UName).ToArray(), value, "_");
                    else
                        _uname = value;

                    //if(!string.IsNullOrEmpty(_uname))
                    //{
                    //    this.SetInstanceName(_uname);
                    //}

                    StartCondition.ParentGlobalUName = GlobalUName;
                    TaskCondition.ParentGlobalUName = GlobalUName;
                    FailCondition.ParentGlobalUName = GlobalUName;

                    this.DispatchEvent(ES_Event.ON_UPDATE);
                }
            }

        }

        [XmlIgnore]
        public string GlobalUName
        {
            get
            {
                return string.Format("quest.{0}.{1}", Group != null ? Group.Name : string.Empty, UName); 
            }
        }

        //[XmlArray("il")]
        //[XmlArrayItem("i")]
        //public List<H_QuestInfo> Info { get; set; } = new List<H_QuestInfo>();

        [XmlArray("il")]
        [XmlArrayItem("i")]
        public List<string> Info { get; set; } = new List<string>();

        [XmlElement("sc")]
        public H_Condition StartCondition { get; set; } = new H_Condition() { Type = H_EConditionType.CONDITION, UName = "start" };
        [XmlElement("tc")]
        public H_Condition TaskCondition { get; set; } = new H_Condition() { Type = H_EConditionType.CONDITION, UName = "task" };
        [XmlElement("fc")]
        public H_Condition FailCondition { get; set; } = new H_Condition() { Type = H_EConditionType.CONDITION, UName = "fail" };

        [XmlAttribute("v")]
        public bool IsVisible { get; set; }

        private H_DataGroup<H_Quest, H_PersistentQuestData> _group;
        [XmlIgnore]
        public H_DataGroup<H_Quest, H_PersistentQuestData> Group 
        {
            get
            {
                return _group;
            }
            set
            {
                if(_group != value)
                {
                    _group = value;

                    StartCondition.ParentGlobalUName = GlobalUName;
                    TaskCondition.ParentGlobalUName = GlobalUName;
                    FailCondition.ParentGlobalUName = GlobalUName;
                }
            }
        }

        //[XmlIgnore]
        //public bool HasStarted { get; set; }

        //[XmlIgnore]
        //public bool WasCompleted { get; set; }

        //[XmlIgnore]
        //public bool IsActivated { get; private set; }

        [XmlIgnore]
        public List<H_Reward> Rewards { get; set; }

        public void Open()
        {
            StartCondition.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
            TaskCondition.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
            FailCondition.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);

            StartCondition.ListenConditions();
            TaskCondition.ListenConditions();
            FailCondition.ListenConditions();
        }

        public void Close()
        {
            StartCondition.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
            TaskCondition.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);
            FailCondition.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnConditionChangeValueHandler);

            StartCondition.StopListenConditions();
            TaskCondition.StopListenConditions();
            FailCondition.StopListenConditions();
        }

        public void AddInfo(string info)
        {
            Info.Add(info);
            this.DispatchEvent(ES_Event.ON_UPDATE);
        }

        public void InsertInfo(int index, string info)
        {
            Info.Insert(index, info);
            this.DispatchEvent(ES_Event.ON_UPDATE);
        }

        public void UpdateInfo(int index, string info)
        {
            if(index >= 0 && index < Info?.Count)
            {
                Info[index] = info;

                this.DispatchEvent(ES_Event.ON_UPDATE);
            }
        }

        public void RemoveInfoAt(int index)
        {
            if (index >= 0 && index < Info?.Count)
            {
                Info.RemoveAt(index);

                this.DispatchEvent(ES_Event.ON_UPDATE);
            }
        }

        public string GetQuestInfoAt(int index)
        {
            if (index >= 0 && index < Info?.Count)
                return ML_Reader.GetText(Info[index]);

            return string.Empty;
        }

        public string FormatQuestInfoAt(int index, params object[] args)
        {
            if (index >= 0 && index < Info?.Count)
                return string.Format(ML_Reader.GetText(Info[index]), args);

            return string.Empty;
        }

        private void OnConditionChangeValueHandler(ES_Event ev)
        {
            this.DispatchEvent(ES_Event.ON_UPDATE);
        }

        /*/// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        public void SetActive(bool status)
        {
            this.SetInstanceName(UName);

            if (status && !IsActivated)
            {
                if (StartCondition.WasCompleted)
                {
                    Start();
                }
                else
                {
                    StartCondition.AddEventListener(ES_Event.ON_COMPLETE, OnStartConditionsHandler);
                    StartCondition.Start();
                }
            }
            else if (!status && IsActivated)
            {
                Stop();
            }

            IsActivated = status;
        }

        public void Start()
        {
            FailCondition.AddEventListener(ES_Event.ON_COMPLETE, OnFailConditionsCompleteHandler);
            TaskCondition.AddEventListener(ES_Event.ON_COMPLETE, OnGoalsCompleteHandler);

            FailCondition.Start();
            TaskCondition.Start();

            HasStarted = true;
        }

        public void Stop()
        {
            if (HasStarted)
            {
                FailCondition.RemoveListeners();
                TaskCondition.RemoveListeners();
                FailCondition.RemoveEventListener(ES_Event.ON_COMPLETE, OnFailConditionsCompleteHandler);
                TaskCondition.RemoveEventListener(ES_Event.ON_COMPLETE, OnGoalsCompleteHandler);
            }
            else
            {
                StartCondition.RemoveEventListener(ES_Event.ON_COMPLETE, OnStartConditionsHandler);
            }
        }*/

        public void CheckAndUpdateUName()
        {
            if (Group != null)
            {
                _uname = ObjectNamesManager.GetUniqueName(Group.Data.Where(e => e != this).Select(e => e.UName).ToArray(), _uname, "_");
            }
        }

        public H_Quest Clone(string cloneUID)
        {
            H_Quest q = new H_Quest();

            q.Group = Group;
            q.UName = UName;
            q.UID = cloneUID;
            q.StartCondition = this.StartCondition.Clone(string.Empty);
            q.TaskCondition = this.TaskCondition.Clone(string.Empty);
            q.FailCondition = this.FailCondition.Clone(string.Empty);

            foreach (string info in Info)
                q.Info.Add(info);

            return q;
        }

        public void Process()
        {
            Open();

            StartCondition.ParentGlobalUName = GlobalUName;
            TaskCondition.ParentGlobalUName = GlobalUName;
            FailCondition.ParentGlobalUName = GlobalUName;
            //StartCondition.SetInstanceName();
        }

        /*private void OnStartConditionsHandler(ES_Event ev)
        {
            StartCondition.RemoveEventListener(ES_Event.ON_COMPLETE, OnStartConditionsHandler);
            Start();
        }

        private void OnGoalsCompleteHandler(ES_Event ev)
        {
            Stop();

            this.DispatchEvent(ES_Event.ON_COMPLETE);

            foreach (H_Reward reward in Rewards)
            {
                if (reward.AutoCollect)
                {
                    reward.Collect();
                }
            }
        }

        private void OnFailConditionsCompleteHandler(ES_Event ev)
        {
            Stop();

            this.DispatchEvent(ES_Event.ON_FAIL);
        }*/
    }
}