using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace H_DataSystem
{
    public class H_DataGroupList<T, K> where T : H_Clonnable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Clonnable<K>, H_Processable<K>, H_Groupable<T, K>
    {
        public Dictionary<string, H_DataGroup<T, K>> Groups { get; set; } = new Dictionary<string, H_DataGroup<T, K>>();

        public string Filename { get; set; } = "group_list.xml";

        private int _loadedGroups = 0;

        public void LoadGroups(bool canChangePrefix = true)
        {
            _loadedGroups = Groups.Values.Count;
            foreach (H_DataGroup<T, K> group in Groups.Values)
            {
                if (group.CanLoadAtStart)
                {
                    if (!ES_EventManager.HasEventListener(group.Name, ES_Event.ON_LOAD, OnGroupLoadedHandler))
                        ES_EventManager.AddEventListener(group.Name, ES_Event.ON_LOAD, OnGroupLoadedHandler);

                    group.Load(canChangePrefix);
                }
                else
                {
                    _loadedGroups--;

                    if (_loadedGroups == 0)
                    {
                        this.DispatchEvent(ES_Event.ON_LOAD);
                    }
                }
            }
        }

        public void UnloadGroups(bool clearListAfterUnload = false)
        {
            foreach (H_DataGroup<T, K> group in Groups.Values)
            {
                group.RemoveEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler);
                group.Unload();
            }

            if (clearListAfterUnload)
                Groups.Clear();
        }

        public void RemoveGroupByName(string name)
        {
            RemoveGroup(GetGroupByName(name));
        }

        public void RemoveGroupByUID(string uid)
        {
            if (Groups.TryGetValue(uid, out H_DataGroup<T, K> g))
            {
                RemoveGroup(g);
            }
        }

        public void RemoveGroup(H_DataGroup<T, K> group)
        {
            if (group != null)
            {
                group.Clear();
                group.RemoveEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler);
                Groups.Remove(group.UID);
                Save();
            }
        }

        public void Load()
        {
            if (Groups != null)
            {
                if (Groups.Count == 0)
                {
                    H_DataManager.Instance.AddEventListener(ES_Event.ON_LOAD, OnLoadGroupListHandler);
                    H_DataManager.Instance.LoadGroupList<T, K>(Filename);
                }
                else
                    LoadGroups();
            }
        }
        public void OnLoadGroupListHandler(ES_Event ev)
        {

            if (ev.Data != null)
            {
                H_DataGroup<T, K>[] groups = (H_DataGroup<T, K>[])ev.Data;

                foreach (H_DataGroup<T, K> group in groups)
                {
                    if (!group.HasEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler))
                        group.AddEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler);

                    Groups.Add(group.UID, group);
                }

                LoadGroups();
            }
            else
            {
                CreateGroup("global");
                this.DispatchEvent(ES_Event.ON_LOAD);
            }
        }

        public void Save()
        {
            H_DataManager.SaveGroupListToFile(Filename, Groups.Values.ToArray());
        }

        public H_DataGroup<T, K> CreateGroup(string name, H_DataGroup<T, K> baseGroup = null, bool overrideIfExist = false)
        {
            if (CVarSystem.ValidateName(name))
            {
                H_DataGroup<T, K> oldGroup = GetGroupByName(name);

                // check if group exists
                if (oldGroup == null || overrideIfExist)//  se o grupo não existir
                {
                    int address = H_DataManager.Instance.Address.GetNextAvaliableAddress();

                    H_DataGroup<T, K> group;
                    // Create group with the desired name
                    if (baseGroup == null)
                        group = new H_DataGroup<T, K>() { Name = name, UID = address.ToString(), IsLoaded = true };
                    else
                    {
                        group = baseGroup.Clone(address.ToString(), name);

                        // if there is some data then save to create file
                        if (group.Data.Count > 0)
                            group.Save();
                    }

                    if (!group.HasEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler))
                        group.AddEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler);

                    if (oldGroup == null)
                        // update group list
                        Groups.Add(group.UID, group);
                    else
                    {
                        // Remove Update listener
                        oldGroup.RemoveEventListener(ES_Event.ON_UPDATE, OnGroupUpdateHandler);

                        // Delete old
                        Groups[oldGroup.UID].Clear();

                        // insert new group
                        Groups[oldGroup.UID] = group;
                    }

                    // update group file
                    Save();

                    return group;
                }

                return oldGroup;

                // se existir 
                //  ignore
            }

            Debug.LogWarning("Invalid name");
            return null;
        }

        public H_DataGroup<T, K>[] GetGroups()
        {
            return Groups.Values.ToArray();
        }

        public H_DataGroup<T, K> GetGroupByName(string name)
        {
            return Groups.Values.FirstOrDefault(group => group.Name == name);
        }

        public H_DataGroup<T, K> GetGroupByUID(string UID)
        {
            return Groups.TryGetValue(UID, out H_DataGroup<T, K> g) ? g : null;
        }

        private void OnGroupUpdateHandler(ES_Event ev)
        {
            Save();
        }

        private void OnGroupLoadedHandler(ES_Event ev)
        {
            ES_EventManager.RemoveEventListener(ev.TargetIdentifier, ES_Event.ON_LOAD, OnGroupLoadedHandler);
            _loadedGroups--;
            if (_loadedGroups == 0)
            {
                this.DispatchEvent(ES_Event.ON_LOAD);
            }
        }
    }
}