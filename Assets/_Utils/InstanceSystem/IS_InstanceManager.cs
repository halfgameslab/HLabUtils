/// <summary>
/// IS_InstanceManager
/// V 4.0
/// Developed by: Eder Moreira
/// Copyrights: Eder Moreira 2020
/// Used to store all instances and gave an unique and affordable name
/// </summary>
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IS_InstanceManager
{
    public const char DOT = '.';
    //public const string PREFAB_PREFIX = "Prefab";
    public const string SPACE = " ";
    public const string DEFAULT_INSTANCE_NAME = "__instance__";

    private static Dictionary<string, System.Object> _inverseInstances = new Dictionary<string, System.Object>();

    public static Dictionary<System.Object, string> Instances { get; set; } = new Dictionary<System.Object, string>();
    public static Action<System.Object, string> OnBeforeInstanceNameCreated { get; set; }
    public static Action<System.Object, string, string> OnBeforeInstanceNameChanged { get; set; }

    public static void Clear()
    {
        Instances.Clear();
        _inverseInstances.Clear();
    }

    public static void ClearNull()
    {
        List<KeyValuePair<string, System.Object>> goodPair = _inverseInstances.Where(pair => pair.Value != null)
                        .Select(pair => pair)
                        .ToList();

        Instances.Clear();
        _inverseInstances.Clear();

        foreach (KeyValuePair<string, System.Object> pair in goodPair)
        {
            Add(pair.Value, pair.Key);
        }
    }

    public static void Add(System.Object key, string value)
    {
        //Debug.Log(key+ " - "+value);
        if (key != null)
        {
            if (!(Instances.ContainsKey(key)))
            {
                Instances.Add(key, value);
            }
            else
            {
                Instances[key] = value;
            }

            if (!(_inverseInstances.ContainsKey(value)))
            {
                _inverseInstances.Add(value, key);
            }
            else
            {
                _inverseInstances[value] = key;
            }
        }
    }

    public static void Remove(System.Object key)
    {
        string value;

        if (Instances.TryGetValue(key, out value))
        {
            _inverseInstances.Remove(value);
            Instances.Remove(key);
        }
    }

    public static void Remove(string value)
    {
        if (Instances.TryGetValue(value, out value))
        {
            _inverseInstances.Remove(value);
            Instances.Remove(value);
        }
    }

    /// <summary>
    /// Get the instance using the full path of the instance
    /// the id must contains the scene prefix
    /// This is faster than GetInstanceById
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T GetInstanceByFullPath<T>(string id)
    {
        if (_inverseInstances.TryGetValue(id, out System.Object obj))
            return (T)obj;

        return default;
    }

    /// <summary>
    /// Get instance using the sceneName and cast
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public static T GetInstanceByID<T>(string id, string sceneName)
    {
        return GetInstanceByFullPath<T>(string.Concat(sceneName, DOT, id));
    }

    /// <summary>
    /// Get instance using the index of scene on sceneBuildIndex and cast
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sceneBuildIndex"></param>
    /// <returns></returns>
    public static T GetInstanceByID<T>(string id, int sceneBuildIndex)
    {
        return GetInstanceByID<T>(id, SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name);
    }

    /// <summary>
    /// Look for the instance in the SceneManager.GetActiveScene(). and cast
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T GetInstanceByID<T>(string id)
    {
        return GetInstanceByID<T>(id, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Look for the instance in the scene passed as argument and cast
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T GetInstanceByID<T>(this Scene scene, string id)
    {
        return GetInstanceByID<T>(id, scene.name);
    }

    /// <summary>
    /// Get the instance using the full path of the instance
    /// the id must contains the scene prefix
    /// This is faster than GetInstanceById
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static System.Object GetInstanceByFullPath(string id)
    {
        if (_inverseInstances.TryGetValue(id, out System.Object obj))
        {
            return obj;
        }

        return null;    
    }

    /// <summary>
    /// Get instance using the sceneName
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public static System.Object GetInstanceByID(string id, string sceneName)
    {
        return GetInstanceByFullPath(string.Concat(sceneName, DOT, id));
    }

    /// <summary>
    /// Get instance using the index of scene on sceneBuildIndex
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sceneBuildIndex"></param>
    /// <returns></returns>
    public static System.Object GetInstanceByID(string id, int sceneBuildIndex)
    {
        return GetInstanceByID(id, SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name);
    }

    /// <summary>
    /// Look for the instance in the SceneManager.GetActiveScene().
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static System.Object GetInstanceByID(string id)
    {
        return GetInstanceByID(id, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Look for the instance in the scene passed as argument
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static System.Object GetInstanceByID(this Scene scene, string id)
    {
        return GetInstanceByID(id, scene.name);
    }

    /// <summary>
    /// Return all instances with the id ignoring the scene path
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static System.Object[] GetAllInstancesInOpenScenesByID(string id)
    {
        System.Object obj;
        List<System.Object> objects = new List<System.Object>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if((obj = GetInstanceByID(id, SceneManager.GetSceneAt(i).name)) != null)
                objects.Add(obj);
        }
        
        return objects.ToArray();
    }

    /// <summary>
    /// Set the instance name with the path of scene passed
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="instanceName"></param>
    /// <returns>Full name with path and remove duplicates</returns>
    public static string SetInstanceName(this System.Object obj, string instanceName)
    {
        return obj?.SetInstanceName(instanceName, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Set the instance name with the path of scene passed
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="instanceName"></param>
    /// <param name="scene"></param>
    /// <returns>Full name with path and remove duplicates</returns>
    public static string SetInstanceName(this System.Object obj, string instanceName, Scene scene)
    {
        return obj?.SetInstanceName(instanceName, scene.name);
    }

    /// <summary>
    /// Set the instance name with the path of scene passed
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sceneBuildIndex"></param>
    /// <returns>Full name with path and remove duplicates</returns>
    public static string SetInstanceName(this System.Object obj, string instanceName, int sceneBuildIndex)
    {
        return obj?.SetInstanceName(instanceName, SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name);
    }

    /// <summary>
    /// Set the instance name with the path of scene passed
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="instanceName"></param>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public static string SetInstanceName(this System.Object obj, string instanceName, string sceneName)
    {
        if (obj != null)
        {
            instanceName = string.Concat(sceneName, DOT, instanceName);

            if (instanceName.Replace(SPACE, string.Empty).Length != 0)
            {
                if (!Instances.ContainsKey(obj))
                {
                    OnBeforeInstanceNameCreated?.Invoke(obj, instanceName);
                    Add(obj, ObjectNamesManager.GetUniqueNameFromDictionary(_inverseInstances, instanceName));
                    
                    return Instances[obj];
                }
                else if (Instances[obj] != instanceName)
                {
                    OnBeforeInstanceNameChanged?.Invoke(obj, Instances[obj], instanceName);
                    return (Instances[obj] = ObjectNamesManager.GetUniqueNameFromDictionary(_inverseInstances, instanceName));
                }
            }
        }

        return instanceName;
    }
    
    /// <summary>
    /// Return the instance name with or without path prefix
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="ignoreOrigenExtension"></param>
    /// <returns></returns>
    public static string GetInstanceName(this System.Object obj, bool ignoreOrigenExtension = true)
    {
        if (Instances.ContainsKey(obj))
        {
            return ignoreOrigenExtension ? RemoveOrigenPrefix (Instances[obj]) : Instances[obj];
        }

        return ignoreOrigenExtension ? RemoveOrigenPrefix(obj.SetInstanceName(DEFAULT_INSTANCE_NAME)) : obj.SetInstanceName(DEFAULT_INSTANCE_NAME);
    }

    public static string GetPrefixByScene(Scene scene)
    {
        //return scene.IsValid() ? scene.name : PREFAB_PREFIX;
        return scene.name;
    }

    public static string RemoveOrigenPrefix(string name)
    {
        return name.Substring(name.IndexOf(DOT) + 1); ;
    }

    public static string GetOrigenPrefix(string name)
    {
        return name.Substring(0, name.IndexOf(DOT));
    }

    public static void DestroySelf(this UnityEngine.Object self, float f = 0f)
    {
        RemoveInstanceIDRecursivily(self);

        UnityEngine.Object.Destroy(self, f);
    }

    private static void RemoveInstanceIDRecursivily(UnityEngine.Object self)
    {
        if (Instances.ContainsKey(self))
            Remove(self);

        if (self is GameObject)
        {
            GameObject selfObject = self as GameObject;
            RemoveInstanceIDFromComponent(selfObject);

            for (int i = 0; i < selfObject.transform.childCount; i++)
            {
                GameObject g = selfObject.transform.GetChild(i).gameObject;

                RemoveInstanceIDRecursivily(g);
            }
        }
    }

    private static void RemoveInstanceIDFromComponent(GameObject gameObject)
    {
        Component[] components = gameObject.GetComponents<Component>();
        foreach (Component c in components)
        {
            if (Instances.ContainsKey(c))
                Remove(c);
        }
    }
}