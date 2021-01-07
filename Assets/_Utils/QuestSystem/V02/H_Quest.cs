using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;

namespace H_QuestSystem
{
    public class H_Quest
    {
        public string ID { get; set; }
        public string Type { get; set; }

        public string Name { get; set; }
        public string Desc { get; set; }

        public H_QuestElementGroup StartConditions { get; set; }

        public H_QuestElementGroup Goals { get; set; }

        public H_QuestElementGroup FailConditions { get; set; }

        public H_Reward[] Rewards { get; set; }

        public bool HasStarted { get; set; }

        public bool WasCompleted { get; set; }

        public bool IsActivated { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        public void SetActive(bool status)
        {
            // check if we have a diferente instance name setted for this quest
            /* if (this.GetInstanceName() != ID)
             {
                 // rename the object
                 ES_EventManager.SwapInstanceEvents(this.GetInstanceName(), ID);
                 this.SetInstanceName(ID);
             }*/

            this.SetInstanceName(ID);


            if (status && !IsActivated)
            {
                if (StartConditions.WasCompleted)
                {
                    Start();
                }
                else
                {
                    StartConditions.AddEventListener(ES_Event.ON_COMPLETE, OnStartConditionsHandler);
                    StartConditions.Start();
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
            FailConditions.AddEventListener(ES_Event.ON_COMPLETE, OnFailConditionsCompleteHandler);
            Goals.AddEventListener(ES_Event.ON_COMPLETE, OnGoalsCompleteHandler);

            FailConditions.Start();
            Goals.Start();

            HasStarted = true;
        }

        public void Stop()
        {
            if (HasStarted)
            {
                FailConditions.Cancel();
                Goals.Cancel();
                FailConditions.RemoveEventListener(ES_Event.ON_COMPLETE, OnFailConditionsCompleteHandler);
                Goals.RemoveEventListener(ES_Event.ON_COMPLETE, OnGoalsCompleteHandler);
            }
            else
            {
                StartConditions.RemoveEventListener(ES_Event.ON_COMPLETE, OnStartConditionsHandler);
            }
        }

        private void OnStartConditionsHandler(ES_Event ev)
        {
            StartConditions.RemoveEventListener(ES_Event.ON_COMPLETE, OnStartConditionsHandler);
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
        }
    }
}