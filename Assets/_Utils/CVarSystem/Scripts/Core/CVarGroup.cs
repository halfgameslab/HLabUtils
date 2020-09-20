using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

public enum CvarGroupLoadType
{ 
    LOAD_ON_DEMAND,
    PRELOAD,
    LOAD_MANUAL
}


public class CVarGroup
{
    public string Name { get; set; }
    //public string Base { get; set; }

    public string SceneName { get; set; }

    public CvarGroupLoadType LoadType { get; set; }

    [XmlIgnore]
    public bool IsLoaded { get; set; }

    [XmlIgnore]
    public bool HasChanged { get; set; }

    [XmlIgnore]
    public List<CVarObject> Vars { get; set; } = new List<CVarObject>();
    
    private List<CVarObject> _persistentsVars = new List<CVarObject>();

    public void Add(CVarObject var)
    {
        Vars.Add(var);
        if (var.IsPersistent)
            _persistentsVars.Add(var); 
    }

    public void Remove(CVarObject var)
    {
        Vars.Remove(var);
        if (var.IsPersistent)
            _persistentsVars.Remove(var);
    }

    public void Load()
    {
        if (!IsLoaded)
        {
            // load all default and after load all persistent
            //CVarSystem.LoadGroup(Base);

            LoadDefault();
            IsLoaded = true;
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
        }

#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            //if(!CVarSystem.IsEditModeActived)
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
            CVarSystem.AddPersistentData(data.Objects, this);
        }
    }

    /// <summary>
    /// Unload data from memory
    /// </summary>
    public void Unload()
    {
        if (HasChanged)
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                Flush();
            else
                FlushPersistent();
#else
            FlushPersistent();
#endif
        }
        
        for (int i = Vars.Count-1; i >= 0; i--)
        {   
            CVarSystem.RemoveVarByFullName(Vars[i].FullName);
        }

        IsLoaded = false;
        HasChanged = false;
    }

    public void Save()
    {
        if (!HasChanged)
        {
#if UNITY_EDITOR
            if (CVarSystem.EditorAutoSave)
            {
                HasChanged = true;
                DelayToSaveOnEditor();
            }
            else if (CVarSystem.InGameAutoSave && Application.isPlaying)
            {
                HasChanged = true;
                DelayToSaveRuntime();
            }
#else
            if(CVarSystem.InGameAutoSave)
            {
                HasChanged = true;
                DelayToSaveRuntime();
            }
#endif

            //CVarSystem.UpdateDerivedGroupsFrom(this);// send changes for all childs
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

        List<CVarDataObject> objects = new List<CVarDataObject>();
        //para cada entrada do dicionario faça
        foreach (CVarObject objectData in Vars)
        {
            //transforme o valor para string
            //armazene em um vetor temporário
            objects.Add(CVarDataObject.ParseToCVarDataObject(objectData));
        }

        //salve o vetor temporário de strings em um arquivo
        M_XMLFileManager.Save(GetFilePath(), new CVarData() { Objects = objects.ToArray() });

        HasChanged = false;
    }

    /// <summary>
    /// Save persistent objects to file
    /// </summary>
    public void FlushPersistent()
    {
        if (!HasChanged)
            return;

        List<CVarDataObject> objects = new List<CVarDataObject>();
        //para cada entrada do dicionario faça
        foreach (CVarObject objectData in _persistentsVars)
        {
            //transforme o valor para string
            //armazene em um vetor temporário
            objects.Add(CVarDataObject.ParseToCVarDataObject(objectData));
        }

        //salve o vetor temporário de strings em um arquivo
        M_XMLFileManager.Save(GetPersistentFilePath(), new CVarData() { Objects = objects.ToArray() });

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

        // rename
        Name = newName;

#if UNITY_EDITOR
        // move files
        M_XMLFileManager.RenameOrMove(oldPath, GetFilePath());
#endif
        M_XMLFileManager.RenameOrMove(oldPersistentPath, GetPersistentFilePath());

        // rename all vars
        foreach(CVarObject c in Vars)
        {
            CVarSystem.MoveVarToGroup(c, this);
        }

        Save();
    }

    /// <summary>
    /// Delete the persistent file and reload the default file
    /// </summary>
    public void ResetToDefault()
    {
        // delete persistent file
        M_XMLFileManager.Delete(GetPersistentFilePath());

        // unload current data to avoid garbage to be saved again
        Unload();

        // load default values
        LoadDefault();
    }// end ResetToDefault

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
        return GetPersistentFilePath(Name);
    }

    /// <summary>
    /// Return the file path to default data
    /// </summary>
    /// <returns></returns>
    private string GetFilePath(string name)
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "Data", string.Concat(name, ".xml"));
    }

    /// <summary>
    /// Reeturn the file path to persistent data
    /// </summary>
    /// <returns></returns>
    private string GetPersistentFilePath(string name)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Data", string.Concat(name, ".xml"));
    }

}// end CVarGroup
