using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace H_QuestSystemV2
{
    public enum ConditionOperation
    {
        AND,
        OR
    }

    public class Condition
    {
        private string _type = "CheckVar";

        public string ID { get; set; }
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    if (_type == "Condition")
                    {
                        Conditions = new List<Condition>();
                    }
                    else
                    {
                        Conditions = null;
                    }
                }
            }
        }

        public ConditionOperation Operation { get; set; } = ConditionOperation.AND;

        public object[] _params;

        public List<Condition> Conditions { get; set; }

        public bool IsLeaf { get { return !Type.Equals("Condition"); } }

        public Condition Clone(string sufix = " (clone)")
        {
            Condition clone = new Condition();
            clone.Type = _type;
            clone._params = _params;
            clone.Operation = Operation;
            clone.ID = string.Concat(ID, sufix);

            if (Conditions != null)
            {
                foreach (Condition c in Conditions)
                {
                    clone.Conditions.Add(c.Clone());
                }
            }

            return clone;
        }
    }

    public class QuestInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public QuestInfo Clone()
        {
            QuestInfo clone = new QuestInfo();
            clone.Name = Name;
            clone.Description = Description;

            return clone;
        }
    }

    public class H_Quest
    {
        public string ID { get; set; }
        public List<QuestInfo> Info { get; set; } = new List<QuestInfo>();

        public Condition StartCondition { get; set; } = new Condition() { Type = "Condition", ID = "s1" };
        public Condition TaskCondition { get; set; } = new Condition() { Type = "Condition", ID = "g1" };
        public Condition FailCondition { get; set; } = new Condition() { Type = "Condition", ID = "f1" };

        public H_Quest Clone(string sufix = " (clone)")
        {
            H_Quest q = new H_Quest();

            q.ID = string.Concat(ID, sufix);
            q.StartCondition = this.StartCondition.Clone(string.Empty);
            q.TaskCondition = this.TaskCondition.Clone(string.Empty);
            q.FailCondition = this.FailCondition.Clone(string.Empty);

            foreach (QuestInfo info in Info)
                q.Info.Add(info.Clone());

            return q;
        }
    }

    public class H_DataGroupList<T>
    {
        public Dictionary<string, T> Groups { get; set; }
        public static bool CanLoadRuntimeDefault { get; set; }

        public void SaveGroupListToFile()
        {
            M_XMLFileManager.Save<T[]>(ParseDataPathWith("groups_data.xml"), GetGroups());
        }

        public T[] GetGroups()
        {
            return Groups.Values.ToArray();
        }

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
            if (!CanLoadRuntimeDefault)
                return ParseStreamingDefaultDataPathWith(filename);

            return ParsePersistentDefaultDataPathWith(filename);
        }
    }

    public class H_DataGroup<T>
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
                if (state)
                {
                    //if (!_persistentsVars.Contains(var))
                    //    _persistentsVars.Add(var);
                }
                else
                {
                    //if (_persistentsVars.Contains(var))
                    //    _persistentsVars.Remove(var);
                }

            }

        }

        public void Add(CVarObject var, bool canSave = false)
        {
            /*Vars.Add(var);
            var.Group = this;
            if (var.IsPersistent)
                _persistentsVars.Add(var);*/

            if (canSave)
                Save();
        }

        public void Remove(CVarObject var, bool canSave = false)
        {
            //Vars.Remove(var);
            //var.Group = null;
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
                //CVarSystem.AddData(obj, this);
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
                //CVarSystem.AddData(data, this);
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
        public void Flush(bool force = false)
        {
            if (force || (HasChanged && !CVarSystem.CanLoadRuntimePersistent))
            {
                //M_XMLFileManager.Save(GetFilePath(), new CVarData() { Objects = Vars.ToArray() });

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
                //M_XMLFileManager.Save(GetPersistentFilePath(), new CVarData() { Objects = _persistentsVars.ToArray() });

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
    }


    public class H_QuestGroup : H_DataGroup<H_Quest>
    {
        public List<H_Quest> Quests { get; set; } = new List<H_Quest>();


        
    }
}