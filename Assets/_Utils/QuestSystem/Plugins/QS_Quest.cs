/// <summary>
/// Quest
/// V 1.0
/// Developed by: Eder Moreira
/// Copyrights: MUP Studios 2017
/// Used to support the quest system
/// </summary>

using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using Mup.Misc.Generic;
using UnityEngine;

namespace Mup.QuestSystem
{
    /// <summary>
    /// A single quest is storage here.
    /// </summary>
    public class QS_Quest
    {

        public QS_Quest Clone()
        {
            QS_Quest q = new QS_Quest()
            {
                Amount = this.Amount,
                ID = this.ID,
                Event = this.Event,
                Goal = this.Goal,
                Rewards = this.Rewards,
                Name = this.Name,
                EventSender = this.EventSender,
                Description = this.Description,
                Type = this.Type,
                StartTime = this.StartTime
            };

            return q;
        }
        /// <summary>
        /// Store the active state of quest
        /// </summary>
        private bool _active = false;

        /// <summary>
        /// Manage the system
        /// </summary>
        M_Alarm _alarm;

        /// <summary>
        /// Quest identifier
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// quest name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// quest description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// dispatcher
        /// </summary>
        public string EventSender { get; set; }

        /// <summary>
        /// event to
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// type of event
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// target
        /// </summary>
        public int Goal { get; set; }

        /// <summary>
        /// collected/executed value
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// The time when the quest was started
        /// </summary>
        public int StartTime { get; set; }

        public QS_Reward[] Rewards { get; set; }
        
        /// <summary>
        /// True if the quest is active
        /// </summary>
        public bool IsActive
        {
            get { return _active; }
        }

        /// <summary>
        /// return if the quest is finished
        /// </summary>
        public bool WasCompleted
        {
            get { return Goal <= Amount; }
        }

        /// <summary>
        /// return if the quest was collected 
        /// </summary>
        public bool WasCollected
        {
            get
            {
                //if not completed
                if (!WasCompleted)
                    return false;

                if (Rewards == null)
                    return true;

                //for each reward on the list
                foreach (QS_Reward r in Rewards)
                {
                    //if some reward wasnt collected, the quest wasnt collected completly
                    if (!r.WasCollected)
                        return false;
                }

                //all rewards were collected
                return true;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public double ElapsedTime
        {
            get
            {
                return (_alarm != null) ? _alarm.ElapsedTime : -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double TimeLeft
        {
            get
            {
                return (_alarm != null) ? _alarm.TimeLeft : -1;
            }
        }

        public void Start()
        {
            SetActive(true);
        }

        /// <summary>
        /// Define the state of the quest
        /// </summary>
        /// <param name="state">active or deactive</param>
        public void SetActive(bool state)
        {
            //if deactived and state == true
            if (state && !_active)
            {
                if(!WasCompleted)
                //add listener
                ES_EventManager.AddEventListener(EventSender, Event, OnQuestEventExecutedHandler);

                //tell the quest is enable
                this.DispatchEvent(ES_Event.ON_ENABLE, this);
            }
            else if (_active) // else if actived
            {
                //remove listener
                ES_EventManager.RemoveEventListener(EventSender, Event, OnQuestEventExecutedHandler);
                //tell the quest is disable
                this.DispatchEvent(ES_Event.ON_DISABLE, this);
            }

            //update quest state
            _active = state;
        }

        /// <summary>
        /// Update the quest ingame stats to the saved one
        /// </summary>
        /// <param name="questData">saved quest data</param>
        public void Update(QS_QuestData questData)
        {
            ID = questData.ID;
            Amount = questData.Amount;
            Goal = questData.Goal;
            if (questData.Collected)
            {
                Rewards = null;
            }
        }

        /// <summary>
        /// Mark the current quest as collected
        /// </summary>
        /// <param name="reward">Quest index</param>
        public void Collect(int reward)
        {
            if (!_active)
                return;

            if(Rewards.Length > reward && !Rewards[reward].WasCollected)
            {
                Debug.Log(Rewards[reward].ID);
                Rewards[reward].Collect();
                //avisa qual foi o premio coletado
                this.DispatchEvent(ES_Event.ON_COLLECT, reward);
            }
        }

        /// <summary>
        /// Mark all quest as collected
        /// </summary>
        public void CollectAll()
        {
            for(int i = 0; i < Rewards.Length; i++)
            {               
                Collect(i);
            }
            UserData.Instance.SaveData(1, ID + "Collect");
            this.DispatchEvent(ES_Event.ON_COLLECT_ALL, this);
        }

        /// <summary>
        /// Wait x seconds to activate the quest again
        /// </summary>
        /// <param name="time">in seconds</param>
        /// <param name="elapsedTime">in seconds</param>
        public void Suspend(double time, double elapsedTime = 0)
        {
            _alarm = new M_Alarm();
            _alarm.Start(time, OnAlarmCompleteHandler, elapsedTime);
            
            SetActive(false);
        }

        private void OnAlarmCompleteHandler(ES_Event ev)
        {
            SetActive(true);
            _alarm.Stop();
        }

        /// <summary>
        /// Reset the quest to the start
        /// </summary>
        public void Reset()
        {
            Amount = 0;
            if (Rewards != null)
            {
                foreach (QS_Reward r in Rewards)
                {
                    r.Reset();
                }
            }
        }

        /// <summary>
        /// Executed when object send some message
        /// </summary>
        /// <param name="ev"></param>
        private void OnCollectEventHandler(ES_Event ev)
        {
            int amount = (int)ev.Data;
            //increment +dataAmount to amount            
            if (Amount < Goal)
            {
                Amount += amount;
                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, this);
            }
            if(Amount >= Goal)
            {
                Amount = Goal;
                this.DispatchEvent(ES_Event.ON_COMPLETE, this);
                ES_EventManager.RemoveEventListener(EventSender, Event, OnQuestEventExecutedHandler);
            }
        }

        /// <summary>
        /// If the quest is about execute single action
        /// </summary>
        /// <param name="ev"></param>
        private void OnExecuteEventHandler(ES_Event ev)
        {
            //add +1 to quest amount
            Amount++;

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, this);
            if (Amount >= Goal)
            {
                Amount = Goal;
                this.DispatchEvent(ES_Event.ON_COMPLETE, this);
                ES_EventManager.RemoveEventListener(EventSender, Event, OnQuestEventExecutedHandler);
            }
        }

        /// <summary>
        /// If the quest is about collect
        /// </summary>
        /// <param name="ev"></param>
        private void OnQuestEventExecutedHandler(ES_Event ev)
        {
            //if the quest wasnt complete
            if (!WasCompleted)
            {
                //if not data is int
                if (!(ev.Data is int))
                {
                    //quest event is about exectute single action
                    OnExecuteEventHandler(ev);
                }
                else
                {
                    //quest event is about collect
                    OnCollectEventHandler(ev);
                }
            }
        }        
    }
}