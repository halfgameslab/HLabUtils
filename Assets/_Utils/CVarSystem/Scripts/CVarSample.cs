using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using Mup.Multilanguage.Plugins;
using Mup.Multilanguage.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CVarSample : MonoBehaviour
{
    /*[SerializeField] private CVarInt _varCoins = new CVarInt("coins");

    [SerializeField] private CVarString _coins4 = new CVarString("teste");

    [SerializeField] private CVarString _teste1 = new CVarString("teste1");

    [SerializeField] private CVarInt _teste13 = new CVarInt("teste", "group_01");

    [SerializeField] private CVarVector3 _v3 = new CVarVector3("new_vector4");*/

    [SerializeField] private CVar<int> _varInt;
    [SerializeField] private CVar<float> _varFloat;

    //[SerializeField] private CVar<Transform> _varT;

    [SerializeField] private CVar<int> _varCoins = new CVar<int>("coins");

    [SerializeField] private CVar<string> _coins4 = new CVar<string>("teste");

    [SerializeField] private CVar<string> _teste1 = new CVar<string>("teste1");

    [SerializeField] private CVar<int> _teste13 = new CVar<int>("teste", "group_01");

    [SerializeField] private CVar<Vector3> _v3 = new CVar<Vector3>("new_vector4");

    public int Coins { get => _varCoins.Value; set => _varCoins.Value = value; }

    //CVarCommand _command = new CVarCommand();

    private void OnEnable()
    {
        ES_EventManager.AddEventListener("CVarSystem", "OnLoadComplete", OnCVarSystemLoadCompleteHandler);
    }

    private void OnCVarSystemLoadCompleteHandler(ES_Event ev)
    {
        Debug.Log("LoadedCompleteHandler");
        Init();
    }

    void Init()
    {
        //if(!_varCoins.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler))
        //    _varCoins.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);

        //Debug.Log(Environment.CurrentDirectory);

        //CVarSystem.AddOnValueChangeListener<string>("teste4", OnValueChangeStringHandler);

        //if (!_coins4.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeStringHandler))
        //   _coins4.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeStringHandler);

        Debug.Log("Start");

        _varCoins.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChangeHandler);

        Debug.Log(_varCoins.Value);
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
        ES_EventManager.RemoveEventListener("CVarSystem", "OnLoadComplete", OnCVarSystemLoadCompleteHandler);
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    private void Update()
    {
        if (!CVarSystem.IsReady)
            return;

        if(Input.GetMouseButtonDown(0))
        {
            _varCoins.Value += 1;
            _coins4.Value = _varCoins.Value.ToString();
            Debug.Log(_varCoins.Value);
        }
    }

    private void OnValueChangeHandler(ES_Event ev)
    {
        //Debug.Log((int)ev.Data);

        //Debug.Log(_command.ExecuteAction("EQUAL", (IComparable)ev.Data, 10));
    }

    private void OnValueChangeStringHandler(ES_Event ev)
    {
        Debug.Log("value change "+(string)ev.Data);
    }
}