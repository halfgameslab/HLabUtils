using Mup.EventSystem.Events;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Hold values to edit in runtime from code or from console and system
/// througth string name also we can listen the value change of CVar
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class CVar<T>: ISerializationCallbackReceiver
{
    [SerializeField] private string _name;
    //[SerializeField] private string _groupName = "global";
    [SerializeField] private string _groupUID = "1";
    [SerializeField] private int _address = CVarSystem.VOID;

    public string GroupName 
    {
        get
        {
            if(CVarSystem.TryGetGroupByUID(_groupUID, out CVarGroup group))
                return group.Name;

            return string.Empty;
        }

        set
        {
            CVarGroup group = CVarSystem.GetGroupByName(value);

            if(group != null)
                _groupUID = group.UID;
        }
    }

    public T Value
    {
        get
        {
            return CVarSystem.ContainsVarAt(_address)?CVarSystem.GetValueAt<T>(_address):CVarSystem.GetValue<T>(_name, default, GroupName);
        }
        set
        {
            if (CVarSystem.ContainsVarAt(_address))
                CVarSystem.SetValueAt<T>(_address, value);
            else if(_name != null && _name.Length > 0) CVarSystem.SetValue<T>(_name, value, GroupName);
        }
    }

    public string Name
    {
        get
        {
            return _address != CVarSystem.VOID && CVarSystem.ContainsVarAt(_address)?CVarSystem.GetNameAt<T>(_address): _name;
        }

        set
        {
            // if value realy change
            if (_name != value)
            {
                if (value != null && value.Length > 0)
                {
                    // if there is some value before
                    if (_name != null && _name.Length > 0)
                        CVarSystem.RemoveOnValueChangeListener<T>(_name, GroupName, OnValueChangeHandler); // remove last value

                    // name receive new value
                    _name = value;

                    // try get some adress from this var
                    _address = CVarSystem.GetAddress<T>(_name);

                    if(!CVarSystem.HasOnValueChangeListener<T>(_name, GroupName, OnValueChangeHandler))
                        // add listener to the new var
                        CVarSystem.AddOnValueChangeListener<T>(_name, GroupName, OnValueChangeHandler);
                }// end if value
            }// end if _name != value
        }// end set
    }// end property Name

    

    /// <summary>
    /// When you convert the class to the type base you receive direct the value
    /// </summary>
    /// <param name="d"></param>
    public static implicit operator T(CVar<T> d) => d.Value;

    public CVar(string name, string groupName = "global"):this()
    {
        _name = name;
        GroupName = groupName;
        
    }

    public CVar()
    {
        GroupName = "global";
        SceneManager.sceneLoaded -= OnSceneLoadHandler;
        SceneManager.sceneLoaded += OnSceneLoadHandler;
    }

    public void OnVarChangeHandler(CVarObject obj)
    {
        if (obj.Address == _address)
        {
            _name = CVarSystem.RemoveTypeAndGroup(obj.FullName);
        }
        else if (obj.FullName == CVarSystem.GetFullName<T>(_name, GroupName))
        {
            _address = obj.Address;
        }
    }

    public void OnVarDeletedHandler(CVarObject obj)
    {
        if (obj.Address == _address)
        {
            _address = CVarSystem.VOID;
        }
    }

    public void OnVarRenamedHandler(CVarObject obj, string oldName)
    {
        if (obj.Address == _address)
        {
            _name = CVarSystem.RemoveTypeAndGroup(obj.FullName);
        }
        else if(oldName == CVarSystem.GetFullName<T>(_name, GroupName))
        {
            _address = obj.Address;
            _name = CVarSystem.RemoveTypeAndGroup(obj.FullName);
        }
    }

    ~CVar()
    {
        CVarSystem.OnVarChanged -= OnVarChangeHandler;
        CVarSystem.OnVarRenamed -= OnVarRenamedHandler;
        CVarSystem.OnVarDeleted -= OnVarDeletedHandler;

        SceneManager.sceneLoaded -= OnSceneLoadHandler;
        CVarSystem.RemoveOnValueChangeListener<T>(_name, GroupName, OnValueChangeHandler);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    private void OnSceneLoadHandler(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!CVarSystem.HasOnValueChangeListener<T>(_name, GroupName, OnValueChangeHandler))
            CVarSystem.AddOnValueChangeListener<T>(_name, GroupName, OnValueChangeHandler);
    }

    private void OnValueChangeHandler(ES_Event ev)
    {
        this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ev.Data);
    }

    public void OnBeforeSerialize()
    {
        if (_address != CVarSystem.VOID)
        {
            if (CVarSystem.ContainsVarAt(_address))
            {
                _name = CVarSystem.GetNameAt<T>(_address);
            }
            else if (CVarSystem.ContainsVar<T>(_name))
                _address = CVarSystem.GetAddress<T>(_name, GroupName);
        }
        else
        {
            _address = CVarSystem.GetAddress<T>(_name, GroupName);
        }
    }

    public void OnAfterDeserialize()
    {
        CVarSystem.OnVarChanged -= OnVarChangeHandler;
        CVarSystem.OnVarRenamed -= OnVarRenamedHandler;
        CVarSystem.OnVarDeleted -= OnVarDeletedHandler;

        CVarSystem.OnVarChanged += OnVarChangeHandler;
        CVarSystem.OnVarRenamed += OnVarRenamedHandler;
        CVarSystem.OnVarDeleted += OnVarDeletedHandler;
    }
}