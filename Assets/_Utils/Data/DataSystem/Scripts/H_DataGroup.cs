using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLab.H_DataSystem
{
    public interface H_Cloneable<T>
    {
        T Clone(string cloneUID);
    }
    public interface H_Processable<T>
    {
        //bool IsPersistent { get; set; }

        void Process();
    }

    public interface H_Groupable<T, K> where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
    { 
        H_DataGroup<T, K> Group { get; set; }
    }


    public class H_DataGroup<T, K> where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
    {
        public const string DEFAULT_EXTENSION = ".xml";
        
        [XmlAttribute("uid")]
        public string UID { get; set; }

        [XmlElement("n")]
        public string Name { get; set; }

        [XmlAttribute("as")]
        public bool RuntimeAutoSave { get; set; } = true;

        [XmlAttribute("pt")]
        public CVarGroupPersistentType PersistentType { get; set; } = CVarGroupPersistentType.SHARED;

        [XmlAttribute("las")]
        public bool CanLoadAtStart { get; set; } = true;

        [XmlIgnore]
        public List<T> Data { get; set; } = new List<T>();

        [XmlIgnore]
        public List<K> PersistentData { get; set; } = new List<K>();

        [XmlIgnore]
        public string PersistentPrefix { get; set; } = string.Empty;

        [XmlIgnore]
        public bool IsLoaded { get; set; }

        [XmlIgnore]
        public bool HasChanged { get; set; }

        public void SetPersistentTypeAndSave(CVarGroupPersistentType persistentType)
        {
            if (PersistentType != persistentType)
            {
                PersistentType = persistentType;
                this.DispatchEvent(ES_Event.ON_UPDATE, "PersistentType");
                //CVarSystem.SaveGroupListToFile();
            }
        }

        public void SetCanLoadAtStartAndSave(bool canLoadAtStart)
        {
            if (CanLoadAtStart != canLoadAtStart)
            {
                CanLoadAtStart = canLoadAtStart;
                this.DispatchEvent(ES_Event.ON_UPDATE, "CanLoadAtStart");
                //CVarSystem.SaveGroupListToFile();
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

        public void SetPersistentData(K data, bool state)
        {
            //if (data.IsPersistent != state)
            //{
                //data.IsPersistent = state;
            if (state)
            {
                if(!PersistentData.Contains(data))
                    PersistentData.Add(data);
            }
            else
            {
                if (PersistentData.Contains(data))
                    PersistentData.Remove(data);
            }

            //}

        }

        public void Add(T item, bool canSave = true)
        {
            Data.Add(item);
            item.Group = this;
            item.AddEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander);
            item.Process();
            /*var.Group = this;
            if (var.IsPersistent)
                _persistentsVars.Add(var);*/

            if (canSave)
                Save();
        }

        public void Insert(int index, T item, bool canSave = true)
        {
            Data.Insert(index, item);
            item.Group = this;
            item.AddEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander);
            item.Process();

            if (canSave)
                Save();
        }

        public void Remove(T item, bool canSave = true)
        {
            Data.Remove(item);
            item.Group = null;
            item.RemoveEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander);
            //if (var.IsPersistent)
            //    _persistentsVars.Remove(var);

            if (canSave)
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
                M_XMLFileManager.NewLoad<T[]>(CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(UID, DEFAULT_EXTENSION)), OnLoadDefaultCompleteHandler);
            else
                M_XMLFileManager.NewLoad<T[]>(fileName, OnLoadDefaultCompleteHandler);
        }

        private void OnLoadDefaultCompleteHandler(T[] data)
        {
            if (data != null)
            {
                //            string fileName = CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(UID, DEFAULT_EXTENSION));
                //#if UNITY_EDITOR
                //            if (!System.IO.File.Exists(fileName) || CVarSystem.ClearDefaultOnPlay)// if we need copy default to runtime
                //#else
                //            if (!System.IO.File.Exists(fileName))
                //#endif
                //                M_XMLFileManager.Save(fileName, obj);// save to runtime

                //IsLoaded = true;
                //CVarSystem.AddData(obj, this);
                foreach (T d in data)
                {
                    d.Group = this;
                    
                    if(!d.HasEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander))
                        d.AddEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander);

                    d.Process();
                }

                Data.InsertRange(0, data);
            }

            //if(!CVarSystem.IsEditModeActived)
            if (CVarSystem.CanLoadRuntimePersistent)
                LoadPersistent();
            else
            {
                ES_EventManager.DispatchEvent(Name, ES_Event.ON_LOAD, this);
            }
        }


        /// <summary>
        /// Load persistent var value from disk
        /// </summary>
        private void LoadPersistent()
        {
            K[] data = M_XMLFileManager.Load<K[]>(GetPersistentFilePath());
            if (data != null)
            {
                IsLoaded = true;
                //IsLoaded = true;
                //CVarSystem.AddData(data, this);
                foreach(K d in data)
                {
                    d.Group = this;
                    
                    if (!d.HasEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander))
                        d.AddEventListener(ES_Event.ON_UPDATE, OnDataUpdateHander);

                    d.Process();
                }

                PersistentData.InsertRange(0, data);
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


                //for (int i = Vars.Count - 1; i >= 0; i--)
                //{
                //    CVarSystem.RemoveVarByFullName(Vars[i].FullName);
                //}
                Data.Clear();

                IsLoaded = false;
                HasChanged = false;

                //this.DispatchEvent(ES_Event.ON_UPDATE, "unload");
            }
        }

        public void Save()
        {
            Debug.Log("save");
            if (!HasChanged)
            {
                Debug.Log("save HasChanged");
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
        public void Flush(bool force = false)
        {
            Debug.Log("flush "+ GetFilePath());
            if (force || (HasChanged && !CVarSystem.CanLoadRuntimePersistent))
            {
                Debug.Log("flushed");
                M_XMLFileManager.Save(GetFilePath(), Data.ToArray());

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
                M_XMLFileManager.Save(GetPersistentFilePath(), PersistentData.ToArray());

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
                Unload();

                // rename
                Name = ObjectNamesManager.RemoveForbiddenCharacters(newName);

                Load();

                this.DispatchEvent(ES_Event.ON_UPDATE, "Rename");
                //CVarSystem.SaveGroupListToFile();// update group list table
                return true;
            }

            return false;
        }

        public H_DataGroup<T, K> Clone(string cloneUID, string name)
        {
            H_DataGroup<T, K> clone = new H_DataGroup<T, K>();

            clone.PersistentType = PersistentType;
            clone.PersistentPrefix = PersistentPrefix;
            clone.Name = name;
            clone.UID = cloneUID;
            clone.CanLoadAtStart = CanLoadAtStart;

            foreach (T d in Data)
            {
                d.Group = null;// clear the group to avoid the clone to change the name
                clone.Add(d.Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString()));
                d.Group = this;// set group again to the group before clone
            }

            foreach (K pd in PersistentData)
            {
                pd.Group = null;// clear the group to avoid the clone to change the name
                clone.SetPersistentData(pd.Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString()), true);
                pd.Group = this;// set group again to the group before clone
            }

            

            return clone;
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
        /// Save the group when some quest has change
        /// </summary>
        /// <param name="ev"></param>
        private void OnDataUpdateHander(ES_Event ev)
        {
            if(!Application.isPlaying || RuntimeAutoSave)
                Save();
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

    }

}