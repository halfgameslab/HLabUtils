using Mup.EventSystem.Events;

namespace Mup.Misc.Generic
{
    public class M_PlayerLevel : M_ValueInfo
    {
        private int _level = 1;

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                this.Max = this.GetMaxXPByLevel(this.Level);
            }
        }

        private void Start()
        {
            Amount = 0;
        }

        private void OnEnable()
        {
            this.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);
        }

        private void OnDisable()
        {
            this.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);
        }

        private void OnValueChangeHandler(ES_Event ev)
        {
            if(((float)ev.Data) == 1)
            {
                IncreaseLevel();
            }
        }

        private void IncreaseLevel()
        {
            this.Level++;
            this.Max = this.GetMaxXPByLevel(this.Level);
            this.Amount = 0;

            this.DispatchEvent(ES_Event.ON_UPDATE, this.Level);
        }

        private int GetMaxXPByLevel(int level)
        {
            return level * 20;
        }
    }
}