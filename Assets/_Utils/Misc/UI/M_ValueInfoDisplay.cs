using Mup.EventSystem.Events;
using Mup.Misc.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mup.Misc.UI
{
    [RequireComponent(typeof(Text))]
    public class M_ValueInfoDisplay : MonoBehaviour
    {
        [SerializeField] private M_ValueInfo _valueInfo;
        [SerializeField] private UnityEvent _onValueChange;

        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (_valueInfo)
                _valueInfo.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);

            UpdateDisplay(_valueInfo.ToFormatedString());
        }

        private void OnDisable()
        {
            if (_valueInfo)
                _valueInfo.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);
        }

        public void UpdateDisplay(string value)
        {
            _text.text = value;
        }

        private void OnValueChangeHandler(ES_Event ev)
        {
            //UpdateDisplay(((M_ValueInfo)ev.Target).ToFormatedString());
            UpdateDisplay(_valueInfo.ToFormatedString());
            _onValueChange.Invoke();
        }
    }

}