using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ScriptOrder(-32000)]
[ExecuteInEditMode]
public class IS_InstanceSceneManager : MonoBehaviour, ISerializationCallbackReceiver
{
    public static List<IS_InstanceSceneManager> Instances { get; private set; } = new List<IS_InstanceSceneManager>();
    [HideInInspector]
    [SerializeField]
    private List<UnityEngine.Object> _keys = new List<UnityEngine.Object>();

    [HideInInspector]
    [SerializeField]
    private List<string> _values = new List<string>();

    public List<UnityEngine.Object> Keys
    {
        get
        {
            return _keys;
        }

        set
        {
            _keys = value;
        }
    }

    public List<string> Values
    {
        get
        {
            return _values;
        }

        set
        {
            _values = value;
        }
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            //IS_InstanceManager.ClearNull();//clear null objects on scene, remain objects that dont be destroyed. Object prefix mantain the original scene where the object was started

            //for (int i = 0; i < _keys.Count; i++)
            //{
            //    IS_InstanceManager.Add(_keys[i], Values[i]);
            //}

            if (IS_InstanceManager.OnBeforeInstanceNameChanged != null)
            {
                foreach (Delegate del in IS_InstanceManager.OnBeforeInstanceNameChanged.GetInvocationList())
                {
                    if (del.Method.Name == "OnBeforeNameChangeHandler")
                    {
                        return;
                    }
                }
            }

            IS_InstanceManager.OnBeforeInstanceNameChanged += OnBeforeNameChangeHandler;
        }
    }

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        IS_InstanceManager.ClearNull();//clear null objects on scene, remain objects that dont be destroyed. Object prefix mantain the original scene where the object was started

        for (int i = 0; i < _keys.Count; i++)
        {
            IS_InstanceManager.Add(_keys[i], Values[i]);
        }
    }

    private void OnBeforeNameChangeHandler(System.Object obj, string oldName, string newName)
    {
        ES_EventManager.SwapInstanceEvents(IS_InstanceManager.RemoveOrigenPrefix(oldName), IS_InstanceManager.RemoveOrigenPrefix(newName));
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        IS_InstanceManager.OnBeforeInstanceNameChanged = null;
        IS_InstanceManager.Clear();
    }
#endif

    public string this[UnityEngine.Object key]
    {
        get
        {
            int i = _keys.IndexOf(key);

            if (i >= 0)
                return _values[i];

            return null;
        }

        set
        {
            Add(key, value);
        }
    }

    public void Add(UnityEngine.Object key, string value)
    {
        if (key != null)
        {
            int i = Keys.IndexOf(key);
            if (i < 0)
            {
                Keys.Add(key);
                Values.Add(value);
            }
            else
            {
                Values[i] = value;
            }
        }
    }

    public void Remove(UnityEngine.Object key)
    {
        int i = Keys.IndexOf(key);
        
        if (i >= 0)
        {
            Keys.RemoveAt(i);
            Values.RemoveAt(i);
        }
    }
	
    public bool ContainsKey(UnityEngine.Object key)
    {
        return Keys.Contains(key);
    }

    public bool ContainsValue(string value)
    {
        return Values.Contains(value);
    }

    public System.Object GetKeyByValue(string value)
    {
        int i = Values.IndexOf(value);
        return Keys[i];
    }

    public void RemoveByValue(string value)
    {
        int i = Values.IndexOf(value);

        if (i >= 0)
        {
            Keys.RemoveAt(i);
            Values.RemoveAt(i);
        }
    }

    public void RemoveAt(int index)
    {
        Keys.RemoveAt(index);
        Values.RemoveAt(index);
    }
    
    public void Clear()
    {
        Keys.Clear();
        Values.Clear();
    }

    
    public void OnEnable()
    {
        Instances.Add(this);
    }

    public void OnDisable()
    {
        Instances.Remove(this);
    }

}