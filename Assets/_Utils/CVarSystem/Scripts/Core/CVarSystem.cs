﻿using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class to manage the CVar table
/// Idea from:
/// https://gamedev.stackexchange.com/questions/80210/best-way-to-store-game-wide-variables/80215#80215?newreg=8bf90503b1cf413488584a2ce4e443f7
/// https://github.com/id-Software/DOOM-3/blob/master/neo/framework/CVarSystem.h
/// </summary>
public static class CVarSystem
{
    /// <summary>
    /// When playing the game edit mode will be automatically desactivated
    /// if you want active the edit mode again use ActiveEditMode(true);
    /// When the edit mode is activated all changes will affect the persistent file
    /// </summary>
    public static bool IsEditModeActived { get; private set; } = true;
    public static void ActiveEditMode(bool status)
    {
        if (IsEditModeActived != status)
        {
            // reload groups
            UnloadGroups();

            IsEditModeActived = status;
            CanLoadRuntimeDefault = !IsEditModeActived || Application.isPlaying;
            CanLoadRuntimePersistent = !IsEditModeActived;

            Init();
        }
    }

    /// <summary>
    /// If activated all changes will affect the file on Application.persistentDataPath
    /// </summary>
    public static bool CanLoadRuntimeDefault{ get; set; }
    /// <summary>
    /// If activated all changes will affect the persistent file on Application.persistentDataPath
    /// If true changes on default file will be ignored
    /// </summary>
    public static bool CanLoadRuntimePersistent { get; set; }

    private static int _currentAddress = -1;
    //private static int CurrentAddress { 
    //    get { return GetValue<int>("CurrentAddress", 0, "global"); }
    //    set { SetValue<int>("CurrentAddress", value); } 
    //}

    /// <summary>
    /// Used after build
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string ParsePersistentDefaultDataPathWith(string filename)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default", filename);     
    }

    /// <summary>
    /// Used after build
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string ParsePersistentDataPathWith(string filename)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Data", "Persistent", filename);
    }

    /// <summary>
    /// Used on editor
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string ParseStreamingDefaultDataPathWith(string filename)
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "Data", filename);
    }

    public static string ParseDataPathWith(string filename)
    {
#if UNITY_EDITOR
        if (!CanLoadRuntimeDefault)
            return ParseStreamingDefaultDataPathWith(filename);
#endif
        return ParsePersistentDefaultDataPathWith(filename);
    }

    private static int CurrentAddress
    {
        get 
        {
            /*if (_currentAddress == -1)
            {
                string text = System.IO.File.ReadAllText(@"C:\Users\Public\TestFolder\WriteText.txt");
                _currentAddress = int.Parse(text);//load
            }*/

            return _currentAddress;
        }
        set 
        {
            if (_currentAddress != value)
            {
                _currentAddress = value;

                //save
                H_FileManager.Save(ParseStreamingDefaultDataPathWith("current_address.xml"), _currentAddress.ToString());
            }
        }
    }

    private const char SLASH = '|';

    // that will get the _adress of some var
    internal static Dictionary<int, CVarObject> Address { get; set; } = new Dictionary<int, CVarObject>();
    internal static Dictionary<string, CVarObject> CVars { get; set; } = new Dictionary<string, CVarObject>();
    //internal static List<CVarObject> Persistent = new List<CVarObject>();// stock realtime persistent objects

    private static Dictionary<string, CVarGroup> Groups { get; set; } = new Dictionary<string, CVarGroup>();

    public static int VOID { get => 0; }

    /// <summary>
    /// if true will update all persistent variables automatically on editor
    /// </summary>
    public static bool EditorAutoSave { get; set; } = true;

    /// <summary>
    /// if true will update all persistent variables automatically
    /// </summary>
    public static bool InGameAutoSave { get; set; } = true;

    /// <summary>
    /// Tell if all data is ready to use
    /// </summary>
    public static bool IsReady { get; set; } = false;

    public static Action<CVarObject> OnVarDeleted { get; set; }
    public static Action<CVarObject, string> OnVarRenamed { get; set; }
    public static Action<CVarObject> OnVarChanged { get; set; }


    [RuntimeInitializeOnLoadMethod]
    static void InitializeOnLoad()
    {
#if UNITY_EDITOR
        UnloadGroups();
        Groups.Clear();
#endif

        if (Application.isPlaying)
        {
            CopyDefaultFilesToPersistentFolder();
        }

        RunOnStart();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void RunOnStart()
    {
        Debug.Log("RunOnStart");        
        IsEditModeActived = false;
        CanLoadRuntimeDefault = true;
        CanLoadRuntimePersistent = true;
        Init();
        Application.quitting += OnApplicationQuitHandler;        
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnloadNotShared();
        RunOnStart();
    }

    private static void UnloadNotShared()
    {
        foreach(CVarGroup group in Groups.Values)
        {
            if (group.PersistentType != CVarGroupPersistentType.SHARED)
                group.Unload();
        }
    }

    public static void ResetToDefault()
    {
        UnloadGroups();

        PlayerPrefs.SetInt("FilesCopied", 0);
        
        System.IO.Directory.Delete(System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default"));
        System.IO.Directory.Delete(System.IO.Path.Combine(Application.persistentDataPath, "Data", "Persistent"));

        CopyDefaultFilesToPersistentFolder();

        LoadGroups();
    }

    private static void OnApplicationQuitHandler()
    {
        FlushPersistent();

#if UNITY_EDITOR
        Application.quitting -= OnApplicationQuitHandler;
        //IsEditModeActived = true;   
#endif
    }

    public static void Init()
    {
        // load groups file
        // on groups file loaded load global group
        if (Groups != null)
            if (Groups.Count == 0)
            {
                M_XMLFileManager.NewLoad<CVarGroup[]>(ParseDataPathWith("groups_data.xml"), OnLoadGroupDataHandler);
                //if(!Application.isPlaying)
                //    M_XMLFileManager.NewLoad<CVarGroup[]>(ParseStreamingDefaultDataPathWith("groups_data.xml"), OnLoadGroupDataHandler);
                //else
                //    M_XMLFileManager.NewLoad<CVarGroup[]>(ParsePersistentDefaultDataPathWith("groups_data.xml"), OnLoadGroupDataHandler);
            }
            else
                LoadGroups();
    }

    public static void UnloadGroups()
    {
        foreach (CVarGroup group in Groups.Values)
            group.Unload();

        IsReady = false;
    }
    private static bool _copied = false;
    private static void CopyDefaultFilesToPersistentFolder()
    {
#if UNITY_EDITOR
        if (!_copied)
#else
        if (PlayerPrefs.GetInt("FilesCopied", 0) != 1)
#endif
        {
            string[] files = System.IO.Directory.EnumerateFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Data"), "*.*", System.IO.SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).ToArray();

            //foreach (CVarGroup group in data)
            foreach (string file in files)
            {
                //Debug.Log("File copied " + file);
            
                M_XMLFileManager.Copy
                (
                    //System.IO.Path.Combine(Application.streamingAssetsPath, "Data", string.Concat(group.Name, ".xml")),
                    file,
                    //System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default", string.Concat(group.Name, ".xml"))
                    ParsePersistentDefaultDataPathWith(System.IO.Path.GetFileName(file)),
                    true
                );
            }

            M_XMLFileManager.Copy
            (
                ParseStreamingDefaultDataPathWith("groups_data.xml"),
                ParsePersistentDefaultDataPathWith("groups_data.xml"),
                true
            );

            PlayerPrefs.SetInt("FilesCopied", 1);
            _copied = true;
        }
    }

    private static void OnLoadGroupDataHandler(CVarGroup[] data)
    {
        if (data != null)
        {
            foreach (CVarGroup group in data)
            {
                Groups.Add(group.Name, group);
            }

#if UNITY_EDITOR
            H_FileManager.Load(ParseStreamingDefaultDataPathWith("current_address.xml"), (d) => { _currentAddress = int.Parse(d); LoadGroups(); });
#else
            H_FileManager.Load(ParsePersistentDefaultDataPathWith("current_address.xml"), (d) => { _currentAddress = int.Parse(d); LoadGroups(); });
#endif
            //GetGroup("global")?.Load();
            //SetPersistent<int>("CurrentAddress", true);
        }
        else
        {
            // create global group
            CreateGroup("global");
            IsReady = true;
            CurrentAddress = 0;
            //SetPersistent<int>("CurrentAddress", true);
        }

        
    }

    public static void LoadGroups(bool canChangePrefix = true)
    {
        _loadedGroups = Groups.Values.Count;
        foreach (CVarGroup group in Groups.Values)
        {            
            group.Load(canChangePrefix);
            if(!ES_EventManager.HasEventListener(group.Name, ES_Event.ON_LOAD, OnGroupLoadedHandler))
                ES_EventManager.AddEventListener(group.Name, ES_Event.ON_LOAD, OnGroupLoadedHandler);
        }
    }
    private static int _loadedGroups = 0;
    public static void OnGroupLoadedHandler(ES_Event ev)
    {
        ES_EventManager.RemoveEventListener( ev.TargetIdentifier , ES_Event.ON_LOAD, OnGroupLoadedHandler);

        _loadedGroups--;        
        
        if (_loadedGroups == 0)
        {
            IsReady = true;
            ES_EventManager.DispatchEvent("CVarSystem", "OnLoadComplete");
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="baseGroup"></param>
    /// <param name="overrideIfExist"></param>
    /// <returns>if not override and already exists will return a reference from current</returns>
    public static CVarGroup CreateGroup(string name, bool overrideIfExist = false)
    {
        // check if group exists
        if (!Groups.TryGetValue(name, out CVarGroup oldGroup) || overrideIfExist)//  se o grupo não existir
        {
            // Create group with the desired name
            CVarGroup group = new CVarGroup() { Name = name };

            if (oldGroup == null)
                // update group list
                Groups.Add(name, group);
            else
            {
                // Delete old
                Groups[name].Clear();

                // insert new group
                Groups[name] = group;
            }

            // update group file
            SaveGroupListToFile();

            return group;
        }

        return oldGroup;
        //     internamente ao settar uma base o programa deve ler todas as variáveis da base e salvar o arquivo no seu devido lugar

        // se existir 
        //  ignore
    }

    public static void RenameGroup(string name, string newName)
    {
        // check if exist group with the desired name
        if (Groups.TryGetValue(name, out CVarGroup g))
        {
            // if exist
            // check if there isnt another group with the same name
            if (!Groups.ContainsKey(newName))
            {
                Groups.Remove(name);
                Groups.Add(newName, g);

                // rename group
                g.Rename(newName);
                SaveGroupListToFile();
            }
        }
    }

    public static void RemoveGroup(string name)
    {
        if(Groups.TryGetValue(name, out CVarGroup g))
        {
            g.Clear();
            Groups.Remove(name);

            SaveGroupListToFile();
        }
    }

    /// <summary>
    /// Remove var from old group and add on the new group
    /// </summary>
    /// <param name="var"></param>
    /// <param name="group"></param>
    /// <param name="changeAlteredInToCurrentGroup"></param>
    public static void MoveVarToGroup(CVarObject var, CVarGroup group, bool overrideIfExist = true)
    {
        // get the new identifier for var
        string newFullName = ChangeVarGroupName(var.FullName, group.Name);
        
        // if group was diferente
        if (var.FullName != newFullName)
        {
            // check if there is some var with the new name
            if (!CVars.ContainsKey(newFullName) || overrideIfExist)
            {
                // if current var is on the table
                if (CVars.ContainsKey(var.FullName))
                {
                    // remove current var from the table
                    CVars.Remove(var.FullName);// remove
                }

                // if not add new var from table
                CVars.Add(newFullName, var);// add new

                // change the identifier
                var.FullName = newFullName;

                // set group to current
                var.Group = group;

            }
        }
    }

    /// <summary>
    /// Clone var to group
    /// </summary>
    /// <param name="var"></param>
    /// <param name="group"></param>
    /// <param name="overrideIfExist"></param>
    /// <param name="changeAlteredInToCurrentGroup"></param>
    public static void CloneVarToGroup(CVarObject var, CVarGroup group, bool overrideIfExist = true)
    {
        // get the new identifier for var
        string newFullName = ChangeVarGroupName(var.FullName, group.Name);

        // if group was diferente
        if (var.FullName != newFullName)
        {
            // check if there is some var with the new name
            if (!CVars.TryGetValue(newFullName, out CVarObject clonedVar) || overrideIfExist)
            {
                // if there isnt var or the changes are made in base not in the current group we could apply base changes
                if (clonedVar == null || clonedVar != null)
                {
                    SetValueByFullName(newFullName, var.Value);
                    SetPersistentByFullName(newFullName, var.IsPersistent);
                    SetLockedByFullName(newFullName, var.IsLocked);
                }
            }
        }
    }

    public static CVarGroup[] GetGroups()
    {
        CVarGroup[] array = new CVarGroup[Groups.Values.Count];
        Groups.Values.CopyTo(array, 0);
        return array;
    }

    public static void SaveGroupListToFile()
    {
        M_XMLFileManager.Save<CVarGroup[]>(ParseDataPathWith("groups_data.xml"), GetGroups());
    }

    /// <summary>
    /// Get var value from table using the var name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="defautValue"></param>
    /// <returns></returns>
    public static T GetValue<T>(string name, T defautValue = default, string group = "global")
    {
        if (CVars.TryGetValue(GetFullName<T>(name, group), out CVarObject value))
            return (T)value.Value;
        
        return defautValue;
    }

    /// <summary>
    /// Get value using the variable int address
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T GetValueAt<T>(int address, T defaultValue = default)
    {
        if (Address.TryGetValue(address, out CVarObject value))
            return (T)value.Value;
        
        return defaultValue;
    }

    public static int GetAddress<T>(string name, string group="global")
    {
        if (CVars.TryGetValue(GetFullName<T>(name, group), out CVarObject value))
        {
            return value.Address;
        }

        return 0;
    }

    public static string GetNameAt<T>(int address, string defaultName = "")
    {
        return Address.TryGetValue(address, out CVarObject value) ? RemoveTypeAndGroup(value.FullName) : defaultName;
    }

    public static string GetFullNameAt(int address, string defaultName = "")
    {
        return Address.TryGetValue(address, out CVarObject value) ? value.FullName : defaultName;
    }

    /// <summary>
    /// Set value to var using the var name and type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="overrideValueIfExist"></param>
    public static void SetValue<T>(string name, T value, string group = "global", bool overrideValueIfExist = true)
    {
        SetValueByFullName(GetFullName<T>(name, group), value, overrideValueIfExist);
    }


    /// <summary>
    /// Set value to var using the var address and type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <param name="overrideValueIfExist"></param>
    public static void SetValueAt<T>(int address, T value, bool overrideValueIfExist = true)
    {
        if (Address.TryGetValue(address, out CVarObject obj))
        {
            SetValueByFullName(obj.FullName, value, overrideValueIfExist);
        }
    }

    public static void SetValueByType(string name, object value, string type, string group="global", bool overrideValueIfExist = true)
    {
        SetValueByFullName(GetFullName(name, type, group), value, overrideValueIfExist);
    }

    /// <summary>
    /// Set value to var using full name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fullName"></param>
    /// <param name="value"></param>
    /// <param name="overrideValueIfExist"></param>
    public static void SetValueByFullName(string fullName, object value, bool overrideValueIfExist = true)
    {
        // if string has more than one character
        if (fullName?.Length > 0)
        {
            // if the table contains
            if (CVars.TryGetValue(fullName, out CVarObject var))
            {
                // and the value change
                if (overrideValueIfExist && !var.Value.Equals(value))
                {
                    var.Value = value;// set the new value
                    
                    OnVarChanged?.Invoke(var);
                    var.Group.Save();// save the var

                    // dispatch value change
                    ES_EventManager.DispatchEvent(fullName, ES_Event.ON_VALUE_CHANGE, null, value);
                }
            }
            else// if the table doesnt contains the var
            {
                CVarObject cVarObject = CreateVar(fullName, value);

                if (cVarObject != null)
                {
                    OnVarChanged?.Invoke(cVarObject);

                    // dispatch value change
                    ES_EventManager.DispatchEvent(fullName, ES_Event.ON_VALUE_CHANGE, null, value);
                }
            }
        }
    }

    private static CVarObject CreateVar(string fullName, object value)
    {
        // get new address
        CVarGroup group = GetGroup(GetGroupNameByFullName(fullName));

        // if group exist create the var on new group
        if (group != null)
        {
            int address = CurrentAddress + 1;

            CVarObject obj = new CVarObject() { FullName = fullName, Value = value, Address = address, Group = group };

            // add var
            CVars.Add(fullName, obj);
            Address.Add(address, obj);
            obj.Group.Add(obj);

            // save table
            obj.Group.Save();

            CurrentAddress = address;

            return obj;
        }

        Debug.LogWarning(string.Format("Trying create Var {0}. Group not found {1}. Are You missing something?", RemoveTypeAndGroup(fullName),GetGroupNameByFullName(fullName)));

        return null;
    }

    public static CVarGroup GetGroup(string name)
    {
        return Groups.TryGetValue(name, out CVarGroup g)?g:null;
    }

    public static bool TryGetGroup(string name, out CVarGroup g)
    {
        return Groups.TryGetValue(name, out g);
    }

    public static bool ContainsGroup(string name)
    {
        return Groups.ContainsKey(name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool ContainsVar<T>(string name, string group="global")
    {
        return CVars.ContainsKey(GetFullName<T>(name, group));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool ContainsVarAt(int address)
    {
        return Address.ContainsKey(address);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool TryGetVarValue<T>(string name, out T value, string group = "global")
    {
        if (CVars.TryGetValue(GetFullName<T>(name, group), out CVarObject obj))
        {
            value = (T)obj.Value;
            return true;
        }

        value = default;

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool TryGetVarValueAt<T>(int address, out T value)
    {
        if (Address.TryGetValue(address, out CVarObject var))
        {
            value = (T)var.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool TryGetCVarObject<T>(string name, out CVarObject value, string group = "global")
    {
        return CVars.TryGetValue(GetFullName<T>(name, group), out value);
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool TryGetCVarObjectAt<T>(int address, out CVarObject value)
    {
        return Address.TryGetValue(address, out value);
    }

    public static void RemoveVarByFullName(string fullName)
    {
        // check if exists
        if (CVars.TryGetValue(fullName, out CVarObject obj))
        {
            // clear the address
            Address.Remove(obj.Address);

            // remove from cvars table
            CVars.Remove(fullName);

            obj.Group.Remove(obj);

            OnVarDeleted?.Invoke(obj);

            // save
            obj.Group.Save();

            //ES_EventManager.RemoveEventListeners(fullName, ES_Event.ON_VALUE_CHANGE);
        }
    }

    /// <summary>
    /// Remove the var from table
    /// Editor components that use this var will show <missing> after remove
    /// If some script create the var at runtime it will be add again
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    public static void RemoveVar<T>(string name, string group="global")
    {
        // get the real var name
        RemoveVarByFullName(GetFullName<T>(name, group));
    }

    /// <summary>
    /// Give a new name from some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="currentName"></param>
    /// <param name="newName"></param>
    public static void RenameVar<T>(string currentName, string newName, string group="global")
    {
        // only rename if the name was different
        if (currentName != newName)
        {
            currentName = GetFullName<T>(currentName, group);
            // there is some var with this name
            if (CVars.TryGetValue(currentName, out CVarObject varToRename))
            {
                // get full name
                newName = GetFullName<T>(newName, group);

                // there isnt some var with the new name
                if (!CVars.ContainsKey(newName))
                {
                    /////////////varToRename.Name = RemoveTypeAndGroup(newName);
                    varToRename.FullName = newName;

                    // get the current value at currentName and add to the newName key
                    CVars.Add(newName, varToRename);
                    // add the new key to the address
                    //Address[currentVar.Address] = newName;
                    // remove the old key
                    CVars.Remove(currentName);

                    OnVarRenamed(varToRename, currentName);

                    varToRename.Group.Save();

                    ES_EventManager.SwapInstanceEvents(currentName, newName);
                }
            }
        }
    }

    public static string[] GetVarNamesByType<T>(string group="global")
    {
        List<string> names = new List<string>();

        CVarGroup g = GetGroup(group);

        if (g != null)
        {
            foreach (CVarObject obj in g.Vars)
            {
                if (GetObjectTypeByVarName(obj.FullName) == GetTypeName<T>())
                    names.Add(RemoveTypeAndGroup(obj.FullName));
                //names.Add(obj.Name);
            }
        }

        return names.ToArray();
    }


    /// <summary>
    /// Set persistent for some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public static void SetPersistent<T>(string name, bool state, string group="global")
    {
        SetPersistentByFullName(GetFullName<T>(name, group), state);
    }

    /// <summary>
    /// Set persistent for some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public static void SetPersistentByFullName(string fullName, bool state)
    {
        if (CVars.TryGetValue(fullName, out CVarObject obj))
        {
            if (obj.IsPersistent != state)
            {
                //obj.IsPersistent = state;

                obj.Group.SetPersistentVar(obj, state);

                obj.Group.Save();
            }
        }
    }

    /// <summary>
    /// Return if some var is persistent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetPersistent<T>(string name, string group="global")
    {
        return GetPersistentByFullName(GetFullName<T>(name, group));
    }

    /// <summary>
    /// Return if some var is persistent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetPersistentByFullName(string fullName)
    {
        if (CVars.TryGetValue(fullName, out CVarObject obj))
        {
            return obj.IsPersistent;
        }

        return false;
    }

    /// <summary>
    /// Return if some var is persistent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetPersistentAt<T>(int address)
    {
        if (TryGetCVarObjectAt<T>(address, out CVarObject obj))
        //if (CVars.TryGetValue(GetFullName<T>(name), out CVarObject obj))
        {
            return obj.IsPersistent;
        }

        return false;
    }

    /// <summary>
    /// Set persistent for some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public static void SetPersistentAt<T>(int address, bool state)
    {
        //if (CVars.TryGetValue(GetFullName<T>(name), out CVarObject obj))
        if (TryGetCVarObjectAt<T>(address, out CVarObject obj))
        {
            if (obj.IsPersistent != state)
            {
                //obj.IsPersistent = state;

                obj.Group.SetPersistentVar(obj, state);

                obj.Group.Save();
            }
        }
    }

    /// <summary>
    /// Set persistent for some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public static void SetLocked<T>(string name, bool state, string group="global")
    {
        SetLockedByFullName(GetFullName<T>(name, group), state);
    }

    /// <summary>
    /// Set persistent for some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public static void SetLockedByFullName(string fulName, bool state)
    {
        if (CVars.TryGetValue(fulName, out CVarObject obj))
        {
            if (obj.IsLocked != state)
            {
                obj.IsLocked = state;

                obj.Group.Save();
            }
        }
    }

    /// <summary>
    /// Return if some var was locked
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetLocked<T>(string name, string group="global")
    {
        return GetLockedByFullName(GetFullName<T>(name, group));
    }

    /// <summary>
    /// Return if some var was locked
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetLockedByFullName(string fullName)
    {
        if (CVars.TryGetValue(fullName, out CVarObject obj))
        {
            return obj.IsLocked;
        }

        return false;
    }

    /// <summary>
    /// Return if some var is persistent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool GetLockedAt<T>(int address)
    {
        if (TryGetCVarObjectAt<T>(address, out CVarObject obj))
        {
            return obj.IsLocked;
        }

        return false;
    }

    /// <summary>
    /// Set persistent for some var
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="state"></param>
    public static void SetLockedAt<T>(int address, bool state)
    {
        if (TryGetCVarObjectAt<T>(address, out CVarObject obj))
        {
            if (obj.IsLocked != state)
            {
                obj.IsLocked = state;

                obj.Group.Save();
            }
        }
    }

    private static int GetValidAddress()
    {
        return CurrentAddress = CurrentAddress + 1;
    }

    /// <summary>
    /// Return the type of some var
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string GetObjectTypeByVarName(string name)
    {
        return name.Substring(0, name.IndexOf(SLASH));
    }

    public static Dictionary<string, CVarObject>.KeyCollection VarNames => CVars.Keys;


    /*public static void AddData(CVarData data, CVarGroup group)
    {
        //para cada entrada ou elemento no arquivo faça
        foreach (CVarDataObject obj in data.Objects)
        {
            //converta o valor para o tipo desejado
            //adicione ao dicionário
            CVarObject newObj = obj.ToCVarObject();

            CVars.Add(obj.VarName, newObj);
            Address.Add(obj.VarAddress, newObj);
            group.Add(newObj);

            if (CurrentAddress < obj.VarAddress)
                CurrentAddress = obj.VarAddress;
        }

    }*/

    public static void AddData(CVarData data, CVarGroup group)
    {
        CVarObject current;
        //para cada entrada ou elemento no arquivo faça
        foreach (CVarDataObject obj in data.Objects)
        {
            if(CVars.TryGetValue(obj.VarName, out current))// if the var exists name
            {
                // just update data
                current.Value = obj.ParseValue();
                //current.IsPersistent = obj.VarPersistent;
                current.IsLocked = obj.VarLocked;
                current.Group.SetPersistentVar(current, obj.VarPersistent);

            }
            else if(Address.TryGetValue(obj.VarAddress, out current))// the var name doesnt exists but the address yes (maybe was renamed at runtime)
            {
                // just update data
                current.Value = obj.ParseValue();
                //current.IsPersistent = obj.VarPersistent;

                current.IsLocked = obj.VarLocked;
                current.Group.SetPersistentVar(current, obj.VarPersistent);

                current.FullName = obj.VarName;
            }
            else // if var not exist (maybe was created in runtime)
            {
                current = obj.ToCVarObject();
                CVars.Add(obj.VarName, current);// add var
                Address.Add(obj.VarAddress, current);// add var address
                group.Add(current);
                //Persistent.Add(current);// add to persistent list

                if (CurrentAddress < obj.VarAddress)// try update cvar address
                    CurrentAddress = obj.VarAddress;
            }
        }
    }

    public static void FlushPersistent()
    {
        foreach (CVarGroup g in Groups.Values)
            g.FlushPersistent();
    }

    /// <summary>
    /// Reset group to default values
    /// </summary>
    public static void ResetToDefault(string group="global")
    {
        if(Groups.TryGetValue(group, out CVarGroup g)) 
            g.ResetToDefault();
    }

    /// <summary>
    /// Add a callback to listen when some var change its value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public static void AddOnValueChangeListener<T>(string name, string groupName, ES_MupAction callback)
    {
        ES_EventManager.AddEventListener(GetFullName<T>(name, groupName), ES_Event.ON_VALUE_CHANGE, callback);
    }
    
    /// <summary>
    /// Add a callback to listen when some var change its value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public static bool HasOnValueChangeListener<T>(string name, string groupName, ES_MupAction callback)
    {
        return ES_EventManager.HasEventListener(GetFullName<T>(name, groupName), ES_Event.ON_VALUE_CHANGE, callback);
    }

    /// <summary>
    /// Remove the event add on value change
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public static void RemoveOnValueChangeListener<T>(string name, string groupName, ES_MupAction callback)
    {
        ES_EventManager.RemoveEventListener(GetFullName<T>(name, groupName), ES_Event.ON_VALUE_CHANGE, callback);
    }

    /// <summary>
    /// Clear the var table
    /// </summary>
    public static void Clear(string group = "global")
    {
        if (Groups.TryGetValue(group, out CVarGroup g))
            g.Clear();

    }

    public static string GetVarGroupName(string fullName)
    {
        return fullName.Split(SLASH)[1];
    }

    public static string ChangeVarGroupName(string fullName, string group)
    {
        string[] n = fullName.Split(SLASH);
        return string.Concat(n[0], SLASH, group, SLASH, n[2]);
    }

    public static string GetFullName(string name, Type type, string group = "global")
    {
        return string.Concat(type.Name, SLASH, group, SLASH, name);
    }

    public static string GetFullName(string name, string type, string group = "global")
    {
        return string.Concat(type, SLASH, group, SLASH, name);
    }

    /// <summary>
    /// Get the full name of the var based on the type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetFullName<T>(string name, string group="global")
    {
        return string.Concat(GetTypeName<T>(), SLASH, group, SLASH, name);
    }

    public static string GetGroupNameByFullName(string fullName)
    {
        return fullName.Split(SLASH)[1];
    }
    
    

    public static string RemoveType(string n)
    {
        return string.Concat(n.Split(SLASH)[1], n.Split(SLASH)[2]);
    }


    public static string RemoveTypeAndGroup(string n)
    {
        return n.Split(SLASH)[2];
    }

    public static string GetTypeName<T>()
    {
        return typeof(T).Name;
    }
}