using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mup.EventSystem.Events;

namespace Mup.Misc.Generic
{
    public class M_ValueInfo : MonoBehaviour
    {
        [SerializeField]
        private float _max = 10;

        [SerializeField]
        private float _amount = 0;

        [SerializeField]
        private UnityEvent _onEmpty;

        [SerializeField]
        private UnityEvent _onValueUpdate;

        [SerializeField]
        private UnityEvent _onFull;

        public UnityEvent OnFull
        {
            get
            {
                return _onFull;
            }

            set
            {
                _onFull = value;
            }
        }

        public void Awake()
        {
            //_amount = Max;
        }

        public float Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = Mathf.Clamp(value, 0, Max);

                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, NormalizedAmount);
                    _onValueUpdate.Invoke();

                    if (_amount == 0)
                        _onEmpty.Invoke();
                    else if (_amount == Max) { 
                        this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, NormalizedAmount);
                    OnFull.Invoke();
                        }
                }
            }
        }

        public float Max
        {
            get
            {
                return _max;
            }

            set
            {
                _max = value;
            }
        }

        public float NormalizedAmount { get { return _amount / Max; } }


        private void OnDestroy()
        {
            this.DispatchEvent(ES_Event.ON_DESTROY, NormalizedAmount);
        }

        public void Add(float amount)
        {
            Amount += amount;
        }

        public void FullFill()
        {
            Amount = Max;
        }

        public string ToFormatedString()
        {
            return string.Concat(Amount, "/", Max);
        }
    }

}
