using Mup.EventSystem.Events.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mup.EventSystem.Events
{
    [System.Serializable]
    public class ES_UnityEvent : UnityEvent<ES_Event>
    {

    }

    [ScriptOrder(-31000)]
    public class ES_MupEventListener : MonoBehaviour
    {
        //targetidenfier || object - nome do evento || ES_MupEvent
        [SerializeField] private string _targetIdentifier = string.Empty;
        [SerializeField] private string _eventName = string.Empty;
        [SerializeField] private ES_UnityEvent _eventListener;

        public ES_UnityEvent EventListener
        {
            get { return _eventListener; }
            set { _eventListener = value; }
        }

        public void OnEnable()
        {
            if (Application.isPlaying)
                ES_EventManager.AddEventListener(_targetIdentifier, _eventName, OnEventDispatchHandler);
        }

        private void OnDisable()
        {
            if(Application.isPlaying)
                ES_EventManager.RemoveEventListener(_targetIdentifier, _eventName, OnEventDispatchHandler);
        }

        private void OnEventDispatchHandler(ES_Event ev)
        {
            EventListener.Invoke(ev);
        }
    }
}