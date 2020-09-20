using Mup.EventSystem.Events;
using Mup.Misc.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class M_ValueDisplay : MonoBehaviour
{
    [SerializeField]
    private M_Value _m_value;

    [SerializeField]
    private UnityEvent _onValueUpdate;

    private Text _label;
    
    private void OnEnable()
    {
        UpdateDisplay(_m_value);

        _m_value.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);
    }

    private void OnDisable()
    {
        _m_value.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);
    }

    private void OnValueChangeHandler(ES_Event ev)
    {
        UpdateDisplay((M_Value)ev.Data);

        _onValueUpdate.Invoke();
    }

    private void UpdateDisplay(M_Value m)
    {
        if(!_label)
            _label = this.GetComponent<Text>();
        
        _label.text = m.ToString();
    }
}
