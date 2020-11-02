using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System.Collections.Generic;
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
    public string Name { get; set; }

    public string SceneName { get; set; }

    public CVarGroupPersistentType PersistentType { get; set; } = CVarGroupPersistentType.SHARED;

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
        M_XMLFileManager.NewLoad<CVarData>(GetFilePath(), OnLoadDefaultCompleteHandler);
    }


    private void OnLoadDefaultCompleteHandler(CVarData obj)
    {
        if (obj != null)
        {
            //IsLoaded = true;
            CVarSystem.AddData(obj, this);
            ES_EventManager.DispatchEvent(Name, ES_Event.ON_LOAD);
        }

        //if(!CVarSystem.IsEditModeActived)
        if(CVarSystem.CanLoadRuntimePersistent)
            LoadPersistent();
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
        await Task.Delay(3000);
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
    public void Flush()
    {
        if (!HasChanged)
            return;

        /*List<CVarDataObject> objects = new List<CVarDataObject>();
        //para cada entrada do dicionario faça
        foreach (CVarObject objectData in Vars)
        {
            //transforme o valor para string
            //armazene em um vetor temporário
            objects.Add(CVarDataObject.ParseToCVarDataObject(objectData));
        }*/

        //salve o vetor temporário de strings em um arquivo
        M_XMLFileManager.Save(GetFilePath(), new CVarData() { Objects = Vars.ToArray() });

        HasChanged = false;
    }

    /// <summary>
    /// Save persistent objects to file
    /// </summary>
    public void FlushPersistent()
    {
        if (!HasChanged)
            return;

        /*List<CVarDataObject> objects = new List<CVarDataObject>();
        //para cada entrada do dicionario faça
        foreach (CVarObject objectData in _persistentsVars)
        {
            //transforme o valor para string
            //armazene em um vetor temporário
            objects.Add(CVarDataObject.ParseToCVarDataObject(objectData));
        }*/

        //salve o vetor temporário de strings em um arquivo
        M_XMLFileManager.Save(GetPersistentFilePath(), new CVarData() { Objects = _persistentsVars.ToArray() });

        HasChanged = false;
    }

    /// <summary>
    /// Rename the group
    /// </summary>
    /// <param name="newName"></param>
    public void Rename(string newName)
    {
        /*
         * Verificar este método porque em tempo real não conseguirá alterar o nome do grupo
         */

        // store the old file path
        string oldPath = GetFilePath();
        string oldPersistentPath = GetPersistentFilePath();

        Unload();

        // rename
        Name = newName;

        // move files
        M_XMLFileManager.RenameOrMove(oldPath, GetFilePath());
        M_XMLFileManager.RenameOrMove(oldPersistentPath, GetPersistentFilePath());

        Load();
        /*// rename all vars
        foreach(CVarObject c in Vars)
        {
            CVarSystem.MoveVarToGroup(c, this);
        }

        if (CVarSystem.CanLoadRuntimePersistent)
            FlushPersistent();
        else
            Flush();

        Unload();

        bool canLoadRuntimeDefault = CVarSystem.CanLoadRuntimeDefault;
        bool canLoadRuntimePersistent = CVarSystem.CanLoadRuntimePersistent;

        if (!CVarSystem.CanLoadRuntimePersistent)
        {

        }
            

        

        CVarSystem.CanLoadRuntimeDefault = true;
        CVarSystem.CanLoadRuntimePersistent = false;
        HasChanged = true;
        Flush();*/
        

/*#if UNITY_EDITOR
        // move files
        M_XMLFileManager.RenameOrMove(oldPath, GetFilePath());
#endif
        M_XMLFileManager.RenameOrMove(oldPersistentPath, GetPersistentFilePath());*/

        //Save();
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

        // delete persistent
        M_XMLFileManager.Delete(GetPersistentFilePath());        
    }// end Clear

    /// <summary>
    /// Return the file path to default data
    /// </summary>
    /// <returns></returns>
    private string GetFilePath()
    {
        return GetFilePath(Name);
    }

    /// <summary>
    /// Reeturn the file path to persistent data
    /// </summary>
    /// <returns></returns>
    private string GetPersistentFilePath()
    {
        return GetPersistentFilePath(Name, PersistentPrefix);
    }

    /// <summary>
    /// Return the file path to default data
    /// </summary>
    /// <returns></returns>
    public static string GetFilePath(string name)
    {
        /*#if UNITY_EDITOR
                if(!Application.isPlaying)
                    return System.IO.Path.Combine(Application.streamingAssetsPath, "Data", string.Concat(name, ".xml"));
        #endif

                return System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default", string.Concat(name, ".xml"));*/
        return CVarSystem.ParseDataPathWith(string.Concat(name, ".xml"));
    }
    /// <summary>
    /// Reeturn the file path to persistent data
    /// </summary>
    /// <returns></returns>
    public static string GetPersistentFilePath(string name, string persistentPrefix)
    {
        return CVarSystem.ParsePersistentDataPathWith(string.Format("{0}{1}.xml", persistentPrefix, name));
        //return System.IO.Path.Combine(Application.persistentDataPath, "Data","Persistent", string.Format("{0}{1}.xml", persistentPrefix, name));
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
