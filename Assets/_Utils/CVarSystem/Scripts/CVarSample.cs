using Mup.EventSystem.Events;
using Mup.Multilanguage.Plugins;
using Mup.Multilanguage.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CVarSample : MonoBehaviour
{
    [SerializeField] private CVarInt _varCoins = new CVarInt("coins");

    [SerializeField] private CVarString _coins4 = new CVarString("teste");

    [SerializeField] private CVarString _teste1 = new CVarString("teste1");

    [SerializeField] private CVarInt _teste13 = new CVarInt("teste", "group_01");

    public int Coins { get => _varCoins.Value; set => _varCoins.Value = value; }

    CVarCommand _command = new CVarCommand();

    void OnEnable()
    {
        //if(!_varCoins.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler))
        //    _varCoins.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);

        //Debug.Log(Environment.CurrentDirectory);

        //CVarSystem.AddOnValueChangeListener<string>("teste4", OnValueChangeStringHandler);

        //if (!_coins4.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeStringHandler))
        //   _coins4.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeStringHandler);

        Debug.Log("on enable");

        _varCoins.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);

        
        //Debug.Log(command.ExecuteAction((CVarCommands)Enum.Parse(typeof(CVarCommands), "EQUAL"), 0, 0));
        //Debug.Log(command.ExecuteAction((CVarCommands)Enum.Parse(typeof(CVarCommands), "GREATER"), 0, 0));
        //Debug.Log(command.ExecuteAction((CVarCommands)Enum.Parse(typeof(CVarCommands), "LESS"), 0, 0));
        //Debug.Log(command.ExecuteAction((CVarCommands)Enum.Parse(typeof(CVarCommands), "GREATER_EQUAL"), 0, 0));
        //Debug.Log(command.ExecuteAction((CVarCommands)Enum.Parse(typeof(CVarCommands), "LESS_EQUAL"), 0, 0));

        /*Debug.Log(_command.ExecuteAction("EQUAL", 0, 0));
        Debug.Log(_command.ExecuteAction("GREATER", 0, 0));
        Debug.Log(_command.ExecuteAction("LESS", 0, 0));
        Debug.Log(_command.ExecuteAction("GREATER_EQUAL", 0, 0));
        Debug.Log(_command.ExecuteAction("LESS_EQUAL", 0, 0));*/


        //_coins4.Name = "444";
    }

    void OnDisable()
    {
        //if (_varCoins.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler))
        //    _varCoins.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _varCoins.Value += 1;
            _coins4.Value = _varCoins.Value.ToString();
        }
    }

    private void OnValueChangeHandler(ES_Event ev)
    {
        Debug.Log((int)ev.Data);

        Debug.Log(_command.ExecuteAction("EQUAL", (IComparable)ev.Data, 10));
    }

    private void OnValueChangeStringHandler(ES_Event ev)
    {
        Debug.Log("value change "+(string)ev.Data);
    }
}