using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CVarGroupPersistentType
{ 
    SHARED,// the data will be share for every scene
    PER_SCENE,// the data will be save for every scene
    CUSTOM// the data will be save by a custom identifier
}

public class CVarGroup
{
    public const string DEFAULT_EXTENSION = ".xml";
    public string UID { get; set; }
    public string Name { get; set; }

    public CVarGroupPersistentType PersistentType { get; set; } = CVarGroupPersistentType.SHARED;

    [XmlAttribute("las")]
    public bool CanLoadAtStart { get; set; } = true;

    [XmlIgnore]
    public string PersistentPrefix { get; set; } = string.Empty;

    [XmlIgnore]
    public bool IsLoaded { get; set; }

    [XmlIgnore]
    public bool HasChanged { get; set; }

    [XmlIgnore]
    public List<CVarObject> Vars { get; set; } = new List<CVarObject>();
    
    private List<CVarObject> _persistentsVars = new List<CVarObject>();

    public void SetPersistentTypeAndSave(CVarGroupPersistentType persistentType)
    {
        if (PersistentType != persistentType)
        {
            PersistentType = persistentType;
            CVarSystem.SaveGroupListToFile();
        }
    }

    public void SetCanLoadAtStartAndSave(bool canLoadAtStart)
    {
        if (CanLoadAtStart != canLoadAtStart)
        {
            CanLoadAtStart = canLoadAtStart;
            CVarSystem.SaveGroupListToFile();
        }
    }
    /// <summary>
    /// Change the group save prefix given the group a new persistent file
    /// the PersistentType was automatically changed for CUSTOM
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="prefix"></param>
    public void SetGroupPersistentPrefix(string prefix, CVarGroupPersistentType persistentType = CVarGroupPersistentType.CUSTOM)
    {
        PersistentPrefix = ParsePrefixName(prefix);
        SetPersistentTypeAndSave(persistentType);
    }

    public void SetPersistentVar(CVarObject var, bool state)
    {
        if (var.IsPersistent != state)
        {
            var.IsPersistent = state;
            if(state)
            {
                if (!_persistentsVars.Contains(var))
                    _persistentsVars.Add(var);
            }
            else
            {
                if (_persistentsVars.Contains(var))
                    _persistentsVars.Remove(var);
            }

        }

    }

    public void Add(CVarObject var, bool canSave = false)
    {
        Vars.Add(var);
        var.Group = this;
        if (var.IsPersistent)
            _persistentsVars.Add(var);

        if(canSave)
            Save();
    }

    public void Remove(CVarObject var, bool canSave = false)
    {
        Vars.Remove(var);
        var.Group = null;
        if (var.IsPersistent)
            _persistentsVars.Remove(var);

        if(canSave)
            Save();
    }

    public void Load(bool canChangePrefix = true)
    {
        if (!IsLoaded)
        {
            if (canChangePrefix)
            {
                if (PersistentType == CVarGroupPersistentType.SHARED)
                {
                    PersistentPrefix = string.Empty;
                }
                else if (PersistentType == CVarGroupPersistentType.PER_SCENE)
                {
                    PersistentPrefix = ParsePrefixName(SceneManager.GetActiveScene().name);
                }
            }
            LoadDefault();
            IsLoaded = true;
        }
        else
        {
            ES_EventManager.DispatchEvent(Name, ES_Event.ON_LOAD, this);
        }
    }

    /// <summary>
    /// Load default var value from disk
    /// </summary>
    private void LoadDefault()
    {
        string fileName = CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(UID, DEFAULT_EXTENSION));

        if (!CVarSystem.CanLoadRuntimeDefault || !System.IO.File.Exists(fileName)
#if UNITY_EDITOR
            || CVarSystem.ClearDefaultOnPlay)
#else
)
#endif
            M_XMLFileManager.NewLoad<CVarData>(CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(UID, DEFAULT_EXTENSION)), OnLoadDefaultCompleteHandler);
        else
            M_XMLFileManager.NewLoad<CVarData>(fileName, OnLoadDefaultCompleteHandler);
    }

    private void OnLoadDefaultCompleteHandler(CVarData obj)
    {
        if (obj != null)
        {
//            string fileName = CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(UID, DEFAULT_EXTENSION));
//#if UNITY_EDITOR
//            if (!System.IO.File.Exists(fileName) || CVarSystem.ClearDefaultOnPlay)// if we need copy default to runtime
//#else
//            if (!System.IO.File.Exists(fileName))
//#endif
//                M_XMLFileManager.Save(fileName, obj);// save to runtime

            //IsLoaded = true;
            CVarSystem.AddData(obj, this);
        }
        
        //if(!CVarSystem.IsEditModeActived)
        if (CVarSystem.CanLoadRuntimePersistent)
            LoadPersistent();
        else
            ES_EventManager.DispatchEvent(Name, ES_Event.ON_LOAD, this);
    }

    /// <summary>
    /// Load persistent var value from disk
    /// </summary>
    private void LoadPersistent()
    {
        CVarData data = M_XMLFileManager.Load<CVarData>(GetPersistentFilePath());
        if (data != null)
        {
            //IsLoaded = true;
            CVarSystem.AddData(data, this);
        }
        ES_EventManager.DispatchEvent(Name, ES_Event.ON_LOAD, this);
    }

    /// <summary>
    /// Unload data from memory
    /// </summary>
    public void Unload()
    {
        if (IsLoaded)
        {
            if (HasChanged)
            {
                if (CVarSystem.CanLoadRuntimePersistent)
                    FlushPersistent();
                else
                    Flush();
            }

            for (int i = Vars.Count - 1; i >= 0; i--)
            {
                CVarSystem.RemoveVarByFullName(Vars[i].FullName);
            }

            IsLoaded = false;
            HasChanged = false;
        }
    }

    public void Save()
    {
        if (!HasChanged)
        {
            if (CVarSystem.EditorAutoSave && !CVarSystem.CanLoadRuntimePersistent)
            {
                HasChanged = true;
                DelayToSaveOnEditor();
            }
            else if (CVarSystem.InGameAutoSave && CVarSystem.CanLoadRuntimePersistent)
            {
                HasChanged = true;
                DelayToSaveRuntime();
            }

        }
    }

    /// <summary>
    /// Async operation that refresh the var file after 3 seconds
    /// </summary>
    async void DelayToSaveOnEditor()
    {
        await Task.Delay(3000) ;
        Flush();
    }

    /// <summary>
    /// Async operation that refresh the var file after 3 seconds
    /// </summary>
    async void DelayToSaveRuntime()
    {
        await Task.Delay(3000);
        FlushPersistent();
    }

    /// <summary>
    /// Save data to file
    /// System call flush automatically be careful to call flush for your own
    /// </summary>
    public void Flush(bool force = false)
    {
        if (force || (HasChanged && !CVarSystem.CanLoadRuntimePersistent))
        {
            M_XMLFileManager.Save(GetFilePath(), new CVarData() { Objects = Vars.ToArray() });

            HasChanged = false;
        }
    }

    /// <summary>
    /// Save persistent objects to file
    /// </summary>
    public void FlushPersistent(bool force = false)
    {
        if (force || (HasChanged && CVarSystem.CanLoadRuntimePersistent))
        {
            M_XMLFileManager.Save(GetPersistentFilePath(), new CVarData() { Objects = _persistentsVars.ToArray() });

            HasChanged = false;
        }
    }

    /// <summary>
    /// Rename the group
    /// </summary>
    /// <param name="newName"></param>
    public bool Rename(string newName)
    {
        if (Name != newName)
        {
            if (CVarSystem.ValidateName(newName))
            {
                Unload();

                // rename
                Name = newName;

                Load();

                CVarSystem.SaveGroupListToFile();// update group list table
                return true;
            }
            else
            {
                Debug.LogWarning("Invalid name");
            }
        }

        return false;
    }

    /// <summary>
    /// Delete the persistent file and reload the default file
    /// </summary>
    public void ResetToDefault()
    {
        DeletePersistentFile();

        // unload current data to avoid garbage to be saved again
        Unload();

        // load default values
        LoadDefault();
    }// end ResetToDefault

    public void DeletePersistentFile()
    {
        // delete persistent file
        M_XMLFileManager.Delete(GetPersistentFilePath());
    }

    /// <summary>
    /// Only can be used on editor
    /// </summary>
    public void Clear()
    {
        // unload vars from table
        Unload();

        // delete default
        M_XMLFileManager.Delete(GetFilePath());
        M_XMLFileManager.Delete(GetFilePath(UID, string.Concat(DEFAULT_EXTENSION, ".meta")));

        // delete persistent
        M_XMLFileManager.Delete(GetPersistentFilePath());        
    }// end Clear

    public string[] ListAllPersistentFileNames()
    {
        // positive lookbehind ?<= ignore the \[ and get all between \]_ in a literal way
        Regex pattern = new Regex(@"(?<=\[)(.*?)(?=\]_)");
        Regex otherPattern = new Regex(@"^[^_]*_(.*)[\d.]");

        string[] files;
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Data", "Persistent");

        if (System.IO.Directory.Exists(directory))
        {
            files = System.IO.Directory.EnumerateFiles(directory).Where(name => otherPattern.Match(name).Groups.Count > 1 && otherPattern.Match(name).Groups[1].Value == UID).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = pattern.Match(System.IO.Path.GetFileNameWithoutExtension(files[i])).Value;
            }
        }
        else
        {
            files = new string[] { };
        }

        return files;
    }

    /// <summary>
    /// Return the file path to default data
    /// </summary>
    /// <returns></returns>
    private string GetFilePath()
    {
        return GetFilePath(UID);
    }

    /// <summary>
    /// Reeturn the file path to persistent data
    /// </summary>
    /// <returns></returns>
    private string GetPersistentFilePath()
    {
        return GetPersistentFilePath(UID, PersistentPrefix);
    }

    /// <summary>
    /// Return the file path to default data
    /// </summary>
    /// <returns></returns>
    public static string GetFilePath(string name, string extension = DEFAULT_EXTENSION)
    {
        return CVarSystem.ParseDataPathWith(string.Concat(name, extension));
    }
    /// <summary>
    /// Reeturn the file path to persistent data
    /// </summary>
    /// <returns></returns>
    public static string GetPersistentFilePath(string name, string persistentPrefix)
    {
        return CVarSystem.ParsePersistentDataPathWith(string.Format("{0}{1}{2}", persistentPrefix, name, DEFAULT_EXTENSION));
    }

    /// <summary>
    /// Format the prefix to add some espe                                                                                                                                                                                                                                                                                                                        cial characteres to the name
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static string ParsePrefixName(string prefix)
    {
        return string.Format("[{0}]_", prefix);
    }

}// end CVarGroup
