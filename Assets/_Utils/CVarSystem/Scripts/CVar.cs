using Mup.EventSystem.Events;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Hold values to edit in runtime from code or from console and system
/// througth string name also we can listen the value change of CVar
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class CVar<T>: ISerializationCallbackReceiver
{
    [SerializeField] private string _name;
    [SerializeField] private int _address = CVarSystem.VOID;
    //[SerializeField] private T _value;

    public T Value
    {
        get
        {
            return CVarSystem.ContainsVarAt(_address)?CVarSystem.GetValueAt<T>(_address):CVarSystem.GetValue<T>(_name);
        }
        set
        {
            if (CVarSystem.ContainsVarAt(_address))
                CVarSystem.SetValueAt<T>(_address, value);
            else if(_name != null && _name.Length > 0) CVarSystem.SetValue<T>(_name, value);
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
                        CVarSystem.RemoveOnValueChangeListener<T>(_name, OnValueChangeHandler); // remove last value

                    // name receive new value
                    _name = value;

                    // try get some adress from this var
                    _address = CVarSystem.GetAddress<T>(_name);

                    if(!CVarSystem.HasOnValueChangeListener<T>(_name, OnValueChangeHandler))
                        // add listener to the new var
                        CVarSystem.AddOnValueChangeListener<T>(_name, OnValueChangeHandler);
                }// end if value
            }// end if _name != value
        }// end set
    }// end property Name

    /// <summary>
    /// When you convert the class to the type base you receive direct the value
    /// </summary>
    /// <param name="d"></param>
    public static implicit operator T(CVar<T> d) => d.Value;

    //public CVar(string name, T value, bool overrideValueIfExist = false):this(name)
    //{
    //    CVarSystem.SetValue<T>(name, value, overrideValueIfExist);
    //}

    //public CVar(string name)
    //{
    //    Name = name;
    //    // if has sceneLoaded listener - remove
    //    SceneManager.sceneLoaded -= OnSceneLoadHandler;
    //    SceneManager.sceneLoaded += OnSceneLoadHandler;//wait the scene be loaded to add callback
    //}

    public CVar(string name):this()
    {
        _name = name;
        
    }

    public CVar()
    {
        SceneManager.sceneLoaded -= OnSceneLoadHandler;
        SceneManager.sceneLoaded += OnSceneLoadHandler;
    }

    public void OnVarChangeHandler(CVarObject obj)
    {
        if (obj.Address == _address)
        {
            _name = CVarSystem.RemoveType(obj.FullName);
        }
        else if (obj.FullName == CVarSystem.GetFullName<T>(_name))
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
            _name = CVarSystem.RemoveType(obj.FullName);
        }
        else if(oldName == CVarSystem.GetFullName<T>(_name))
        {
            _address = obj.Address;
            _name = CVarSystem.RemoveType(obj.FullName);
        }
    }

    ~CVar()
    {
        CVarSystem.OnVarChanged -= OnVarChangeHandler;
        CVarSystem.OnVarRenamed -= OnVarRenamedHandler;
        CVarSystem.OnVarDeleted -= OnVarDeletedHandler;

        SceneManager.sceneLoaded -= OnSceneLoadHandler;
        CVarSystem.RemoveOnValueChangeListener<T>(_name, OnValueChangeHandler);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    private void OnSceneLoadHandler(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!CVarSystem.HasOnValueChangeListener<T>(_name, OnValueChangeHandler))
            CVarSystem.AddOnValueChangeListener<T>(_name, OnValueChangeHandler);
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
                _address = CVarSystem.GetAddress<T>(_name);
        }
        else
        {
            _address = CVarSystem.GetAddress<T>(_name);
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

/// <summary>
/// Used on unity serialization
/// </summary>
[System.Serializable]
public class CVarString : CVar<string>
{
    public CVarString(string name) : base(name)
    {

    }

    //public CVarString(string name, string value, bool overrideValueIfExist = false) : base(name, value, overrideValueIfExist)
    //{

    //}
}

/// <summary>
/// Used on unity serialization
/// </summary>
[System.Serializable]
public class CVarInt : CVar<int>
{
    public CVarInt(string name) : base(name)
    {
        
    }

    //public CVarInt(string name, int value, bool overrideValueIfExist = false) : base(name, value, overrideValueIfExist)
    //{
        
    //}
}

/// <summary>
/// Used on unity serialization
/// </summary>
[System.Serializable]
public class CVarFloat : CVar<float>
{
    public CVarFloat(string name) : base(name)
    {

    }

    //public CVarFloat(string name, float value, bool overrideValueIfExist = false) : base(name, value, overrideValueIfExist)
    //{

    //}
}

/// <summary>
/// Used on unity serialization
/// </summary>
[System.Serializable]
public class CVarBool : CVar<bool>
{
    public CVarBool(string name) : base(name)
    {

    }

    //public CVarBool(string name, bool value, bool overrideValueIfExist = false) : base(name, value, overrideValueIfExist)
    //{

    //}
}