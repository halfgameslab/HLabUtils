using HLab.H_DataSystem;
using Mup.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HLab.H_QuestSystem.H_Editor
{
    public class H_QuestGroupListEditor<T, K> where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
    {
        public int CurrentState { get; set; }

        private string _auxString = string.Empty;

        private H_DataGroup<T, K> _lastGroup;
        public void Draw(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            CVarSystem.ClearDefaultOnPlay = GUILayout.Toggle(CVarSystem.ClearDefaultOnPlay, "Clear default on Play");

            EditorGUI.BeginDisabledGroup(CurrentState != 0);
            DrawFileManager(currentGroup);
            EditorGUI.EndDisabledGroup();

            if (CurrentState == 0)
                DrawGroupPopup(currentGroup, groups);
            else if (CurrentState == 1)
                DrawCreateGroup(currentGroup, groups);
            else if (CurrentState == 2)
                DrawEditGroup(currentGroup, groups);

            EditorGUILayout.Space();
        }

        protected void DrawFileManager(H_DataGroup<T, K> currentGroup)
        {
            Color backgroundColor = GUI.backgroundColor;
            backgroundColor.a = 0;
            GUI.backgroundColor = backgroundColor;

            EditorGUILayout.BeginHorizontal();
            if (DefaultTextureButton("d_Collab.FileAdded", "Create New Group"))
            {
                _auxString = "new_group_name";
                CurrentState = 1;
                _lastGroup = currentGroup;
                this.DispatchEvent(ES_Event.ON_CHANGE, null);
            }
            DefaultTextureButton("d_Refresh@2x", "Reload");
            DefaultTextureButton(CVarSystem.IsEditModeActived ? "d_PlayButton@2x" : "d_PauseButton@2x", CVarSystem.IsEditModeActived ? "Disable Edit Mode" : "Enable Edit Mode");

            if (DefaultTextureButton("FolderOpened Icon", "Open persistent folder"))
            {
                Application.OpenURL(Application.persistentDataPath);
            }

            if (DefaultTextureButton("Collab.FolderDeleted", CVarSystem.IsEditModeActived ? "Delete all data files in default runtime folder" : "Revert to default all files in default runtime folder"))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Delete Runtime Default"), false, () => { if (EditorUtility.DisplayDialog("Reset runtime default data", "Are You sure you want reset runtime default data?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.DeleteRuntimeDefault(); });
                menu.AddItem(new GUIContent("Delete Runtime Persistent"), false, () => { if (EditorUtility.DisplayDialog("Delete persistent data", "Are You sure you want delete persistent data?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.DeletePersistent(); });
                menu.AddItem(new GUIContent("Delete All Runtime"), false, () => { if (EditorUtility.DisplayDialog("Delete persistent data", "Are You sure you want delete all runtime files?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.ResetToDefault(); });
                menu.AddItem(new GUIContent("Delete All"), false, () => { if (EditorUtility.DisplayDialog("Delete all files", "Are You sure you want delete all created files?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.DeleteAll(); });

                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            backgroundColor.a = 1f;
            GUI.backgroundColor = backgroundColor;

            EditorGUILayout.Space();
        }

        protected void DrawGroupPopup(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            string[] names = groups.Select(e => e.Name).ToArray();
            int index = Array.IndexOf(groups, currentGroup);
            EditorGUILayout.BeginHorizontal();
            int newIndex = EditorGUILayout.Popup(string.Format("Group [#{0}]", currentGroup.UID), index, names);

            if (newIndex != index)
            {
                this.DispatchEvent(ES_Event.ON_CHANGE, groups[newIndex]);
            }
            EditorGUI.BeginDisabledGroup(currentGroup.Name == "global");
            if (GUILayout.Button("E", GUILayout.Width(18)))
            {
                _auxString = currentGroup.Name;
                CurrentState = 2;
            }
            if (GUILayout.Button("-", GUILayout.Width(18)) && EditorUtility.DisplayDialog("Remove Group", string.Format("Want delete group {0}?\n\nYou can't revert this operation.", currentGroup.Name), "Delete", "Cancel"))
            {
                this.DispatchEvent(ES_Event.ON_DESTROY, currentGroup);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DrawPersistentTypeInfo(currentGroup);
        }

        protected void DrawCreateGroup(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            EditorGUILayout.BeginHorizontal();
            _auxString = EditorGUILayout.TextField("Name", _auxString);


            if (GUILayout.Button("V", GUILayout.Width(19)))
            {
                if (ObjectNamesManager.ValidateIfNameHasntForbiddenCharacters(_auxString) && groups.FirstOrDefault(g => g.Name == _auxString) == null)
                {
                    CurrentState = 0;

                    this.DispatchEvent(ES_Event.ON_CONFIRM, _auxString);

                    _auxString = string.Empty;

                    EditorGUI.FocusTextInControl("");
                }
                else
                {
                    Debug.LogWarning("There is some incorrect character or another group with same name");
                }
            }
            if (GUILayout.Button("X", GUILayout.Width(19)))
            {
                CurrentState = 0;
                _auxString = string.Empty;
                this.DispatchEvent(ES_Event.ON_CHANGE, _lastGroup);
            }
            EditorGUILayout.EndHorizontal();

            DrawCopyFromOption(currentGroup, groups);
        }

        protected void DrawEditGroup(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            EditorGUILayout.BeginHorizontal();
            _auxString = EditorGUILayout.TextField("New Name", _auxString);

            if (GUILayout.Button("V", GUILayout.Width(19)))
            {
                if (_auxString != currentGroup.Name)
                {
                    if (ObjectNamesManager.ValidateIfNameHasntForbiddenCharacters(_auxString) && groups.FirstOrDefault(g => g.Name == _auxString) == null)
                    {
                        CurrentState = 0;

                        currentGroup.Rename(_auxString);
                        //this.DispatchEvent(ES_Event.ON_RESTART, _auxString);

                        _auxString = string.Empty;
                    }
                    else
                    {
                        Debug.LogWarning("There is some incorrect character or another group with same name");
                    }
                }
                else
                {
                    CurrentState = 0;
                    _auxString = string.Empty;
                }
            }
            if (GUILayout.Button("X", GUILayout.Width(19)))
            {
                CurrentState = 0;
                _auxString = string.Empty;
            }
            EditorGUILayout.EndHorizontal();

            DrawPersistentTypeInfo(currentGroup);
        }

        protected void DrawPersistentTypeInfo(H_DataGroup<T, K> currentGroup)
        {
            EditorGUI.BeginDisabledGroup(CurrentState != 0 || currentGroup.Name == "global");
            EditorGUILayout.BeginHorizontal();
            currentGroup.SetPersistentTypeAndSave((CVarGroupPersistentType)EditorGUILayout.EnumPopup(new GUIContent("Persistent Type"), currentGroup.PersistentType));
            currentGroup.SetCanLoadAtStartAndSave(EditorGUILayout.ToggleLeft("Load group at Start", currentGroup.CanLoadAtStart, GUILayout.Width(EditorGUIUtility.currentViewWidth / 3f)));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        protected void DrawCopyFromOption(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            List<string> groupList = new List<string>();

            groupList.Add("<none>");

            groupList.AddRange(groups.Select((e) => e.Name));

            int currentIndex = currentGroup != null ? Array.IndexOf(groupList.ToArray(), currentGroup.Name) : 0;
            int newIndex = EditorGUILayout.Popup("Copy From", currentIndex, groupList.ToArray());

            if (newIndex != currentIndex)
            {
                if (newIndex != 0)
                    this.DispatchEvent(ES_Event.ON_CHANGE, groups[newIndex - 1]);
                else
                    this.DispatchEvent(ES_Event.ON_CHANGE, null);
                //CurrentGroup = CVarSystem.GetGroupByName(groups[newIndex]);
            }
        }

        private bool DefaultTextureButton(string textureName, string hint, float w = 50, float h = 40)
        {
            return GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture(textureName), hint), GUILayout.Width(w), GUILayout.Height(h));
        }
    }
}