using H_DataSystem;
using H_Misc;
using Mup.EventSystem.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Util.InstanceSystem.Editor;

namespace H_QuestSystem.H_QuestEditor
{
    public class H_QuestWindow : EditorWindow
    {
        QuestGroupListEditor<H_Quest, H_PersistentQuestData> _questListEditor = new QuestGroupListEditor<H_Quest, H_PersistentQuestData>();
        QuestEditor _questEditor = new QuestEditor();
        QuestGroupEditor _questGroupEditor = new QuestGroupEditor();
        RuntimeFilesEditor<H_Quest, H_PersistentQuestData> _runtimeFilesEditor = new RuntimeFilesEditor<H_Quest, H_PersistentQuestData>();
        string _currentGroupUID = string.Empty;
        string _currentQuestUID = string.Empty;

        [MenuItem("HLab/Quest Editor")]
        public static void ShowWindow()
        {
            H_QuestWindow ow = null;

            if (HasOpenInstances<H_QuestWindow>())
                ow = GetWindow<H_QuestWindow>(false, "Quest Editor", true);

            H_QuestWindow editorWindow = CreateWindow<H_QuestWindow>("Quest Editor");

            if (ow != null)
                editorWindow.position = new Rect(ow.position.x + 20f, ow.position.y + 20f, ow.position.width, ow.position.height);

            ConfigureWindow(editorWindow);
        }

        public void OnEnable()
        {
            _questListEditor.AddEventListener(ES_Event.ON_CHANGE, OnChangeGroupsListHandler);
            _questListEditor.AddEventListener(ES_Event.ON_CONFIRM, OnCreateGroupHandler);
            _questListEditor.AddEventListener(ES_Event.ON_DESTROY, OnRemoveGroupHandler);

            _questGroupEditor.AddEventListener(ES_Event.ON_CLICK, OnSelectQuestHandler);
            _questGroupEditor.AddEventListener(ES_Event.ON_DESTROY, OnQuestDestroyHandler);
        }
        
        public void OnDisable()
        {
            if(_questListEditor != null)
            {
                _questListEditor.RemoveEventListener(ES_Event.ON_CHANGE, OnChangeGroupsListHandler);
                _questListEditor.RemoveEventListener(ES_Event.ON_CONFIRM, OnCreateGroupHandler);
                _questListEditor.RemoveEventListener(ES_Event.ON_DESTROY, OnRemoveGroupHandler);
            }
            if(_questGroupEditor != null)
            {
                _questGroupEditor.RemoveEventListener(ES_Event.ON_CLICK, OnSelectQuestHandler);
                _questGroupEditor.RemoveEventListener(ES_Event.ON_DESTROY, OnSelectQuestHandler);
            }
        }    

        public static void ConfigureWindow(H_QuestWindow window)
        {
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(450, window.minSize.y);
            window.Show();
        }

        public void OnGUI()
        {
            CheckToAvoidNullQuestList();

            float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

            _questListEditor.Draw(_questGroupEditor.CurrentQuestGroup, H_QuestManager.Instance.QuestGroups.GetGroups());

            if (_questGroupEditor.CurrentQuestGroup != null)
            {
                EditorGUI.BeginDisabledGroup(_questListEditor.CurrentState != 0);

                _runtimeFilesEditor.Draw(_questGroupEditor.CurrentQuestGroup, H_QuestManager.Instance.QuestGroups.GetGroups());

                EditorGUILayout.BeginHorizontal();

                _questGroupEditor.Draw();

                EditorGUILayout.BeginVertical(GUILayout.MinWidth((firstCollumWidth * 2) - 50), GUILayout.MaxWidth((firstCollumWidth * 2) - 50));

                _questEditor.Draw();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
        }

        public void SelectGroup(H_DataGroup<H_Quest, H_PersistentQuestData> group)
        {
            if (group != null)
            {
                _currentGroupUID = group.UID;
            }
            else
            {
                _currentGroupUID = string.Empty;
            }

            _questGroupEditor.Start(group);
            _questEditor.Clear();
        }

        public void SelectQuest(int index)
        {
            if(_questGroupEditor.CurrentQuestGroup.Data.Count > index)
                SelectQuest(_questGroupEditor.CurrentQuestGroup.Data[index]);
        }

        public void SelectQuest(H_Quest quest)
        {
            _questEditor.Start(quest);
            //_questGroupEditor.SelectedQuest = quest;
            _currentQuestUID = quest!=null?quest.UID:string.Empty;
        }

        private void CheckToAvoidNullQuestList()
        {
            if (_questGroupEditor.CurrentQuestGroup == null && _questListEditor.CurrentState != 1)
            {
                H_DataGroup<H_Quest, H_PersistentQuestData> currentQuestGroup = H_QuestManager.Instance.QuestGroups.GetGroupByUID(_currentGroupUID);
                if (currentQuestGroup != null)
                {
                    _questGroupEditor.Start(currentQuestGroup, currentQuestGroup.Data.FirstOrDefault(e => e.UID == _currentQuestUID));
                    SelectQuest(_questGroupEditor.SelectedQuest);
                }
                else
                {
                    _questGroupEditor.Start(H_QuestManager.Instance.QuestGroups.GetGroupByName("global"));
                    _currentGroupUID = _questGroupEditor.CurrentQuestGroup.UID;
                    _currentQuestUID = string.Empty;

                }
            }
        }

        private void OnRemoveGroupHandler(ES_Event ev)
        {
            H_QuestManager.Instance.QuestGroups.RemoveGroup((H_DataGroup<H_Quest, H_PersistentQuestData>)ev.Data);
            SelectGroup(H_QuestManager.Instance.QuestGroups.GetGroupByName("global"));
        }

        private void OnChangeGroupsListHandler(ES_Event ev)
        {
            SelectGroup((H_DataGroup<H_Quest, H_PersistentQuestData>)ev.Data);
        }

        private void OnCreateGroupHandler(ES_Event ev)
        {
            string name = (string)ev.Data;

            SelectGroup(H_QuestManager.Instance.QuestGroups.CreateGroup(name, _questGroupEditor.CurrentQuestGroup));
        }

        private void OnSelectQuestHandler(ES_Event ev)
        {
            SelectQuest((H_Quest)ev.Data);
            //_questEditor.Start((H_Quest)ev.Data);
        }

        private void OnQuestDestroyHandler(ES_Event ev)
        {
            if ((H_Quest)ev.Data == _questEditor.CurrentQuest)
                _questEditor.Clear();
        }
    }

    public class QuestGroupListEditor<T, K> where T:H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
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
            if(DefaultTextureButton("d_Collab.FileAdded", "Create New Group"))
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
            int newIndex = EditorGUILayout.Popup(string.Format("Group ({0})", currentGroup.UID), index, names);

            if(newIndex != index)
            {
                this.DispatchEvent(ES_Event.ON_CHANGE, groups[newIndex]);
            }
            EditorGUI.BeginDisabledGroup(currentGroup.Name == "global");
            if(GUILayout.Button("E", GUILayout.Width(18)))
            {
                _auxString = currentGroup.Name;
                CurrentState = 2;
            }
            if(GUILayout.Button("-", GUILayout.Width(18)) && EditorUtility.DisplayDialog("Remove Group", string.Format("Want delete group {0}?\n\nYou can't revert this operation.", currentGroup.Name), "Delete", "Cancel"))
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
                if (CVarSystem.ValidateName(_auxString) && groups.FirstOrDefault(g=>g.Name == _auxString) == null)
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
                    if (CVarSystem.ValidateName(_auxString) && groups.FirstOrDefault(g => g.Name == _auxString) == null)
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
                if(newIndex != 0)
                    this.DispatchEvent(ES_Event.ON_CHANGE, groups[newIndex-1]);
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

    public class RuntimeFilesEditor<T, K> where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
    {
        private CVarWindowAction _currentAction;
        private string _editableAuxName;

        public void Draw(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            if (currentGroup == null)
                return;

            DrawPersistentFileManager(currentGroup);
            EditorGUILayout.Space();
        }

        protected void DrawPersistentFileManager(H_DataGroup<T, K> currentGroup)
        {
            //EditorGUI.BeginDisabledGroup(PlayerPrefs.GetInt("FilesCopied", 0) == 0);
            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived);

            EditorGUILayout.BeginHorizontal();

            bool boolAux = CVarSystem.CanLoadRuntimeDefault;

            GUI.backgroundColor = Color.cyan;
            boolAux = EditorGUILayout.ToggleLeft("Show Runtime Default", CVarSystem.CanLoadRuntimeDefault);
            GUI.backgroundColor = Color.white;
            if (boolAux != CVarSystem.CanLoadRuntimeDefault)
            {
                if (boolAux && CVarSystem.ClearDefaultOnPlay)
                {
                    CVarSystem.DeleteRuntimeDefault(false);
                }

                CVarSystem.UnloadGroups(true);

                CVarSystem.ClearVars();

                CVarSystem.CanLoadRuntimeDefault = boolAux;
                CVarSystem.Init();

                //CVarSystem.Reload();
                //CVarSystem.LoadGroups(false);
            }
            EditorGUI.EndDisabledGroup();
            //EditorGUI.EndDisabledGroup();
            if (CVarSystem.CanLoadRuntimeDefault)
            {
                DrawDefaultFileManagerOptions
                    (
                        currentGroup,
                        CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParseStreamingDefaultDataPathWith("groups_data.xml")
                    );

            }
            else
            {
                DrawDefaultFileManagerOptions
                    (
                        currentGroup,
                        CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParsePersistentDefaultDataPathWith("groups_data.xml")
                    );
            }


            EditorGUILayout.EndHorizontal();
            /////////////////

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived);
            boolAux = CVarSystem.CanLoadRuntimePersistent;
            GUI.backgroundColor = Color.red;
            boolAux = EditorGUILayout.ToggleLeft("Show Runtime Persistent", CVarSystem.CanLoadRuntimePersistent);
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            if (boolAux != CVarSystem.CanLoadRuntimePersistent)
            {
                CVarSystem.UnloadGroups();
                // change state
                CVarSystem.CanLoadRuntimePersistent = boolAux;
                CVarSystem.LoadGroups(false);
                GUI.FocusControl("");
            }

            if (currentGroup.PersistentType != CVarGroupPersistentType.SHARED)
            {
                if (_currentAction != CVarWindowAction.CREATE_PERSISTENT_FILE && _currentAction != CVarWindowAction.RENAME_PERSISTENT_FILE)
                {
                    // positive lookbehind ?<= ignore the \[ and get all between \]_ in a literal way
                    Regex pattern = new Regex(@"(?<=\[)(.*?)(?=\]_)");

                    string[] files = currentGroup.ListAllPersistentFileNames();

                    EditorGUI.BeginDisabledGroup(!CVarSystem.CanLoadRuntimePersistent);

                    if (files.Length > 0)
                    {
                        int id = Array.IndexOf(files, pattern.Match(currentGroup.PersistentPrefix).Value);//string.Format(g.PersistentPrefix.Replace("[", string.Empty).Replace("]", string.Empty).Replace("_", string.Empty)));
                        int currentId;

                        if (id >= 0 && id < files.Length)
                        {
                            currentId = EditorGUILayout.Popup(id, files);

                            if (id != currentId)
                            {
                                currentGroup?.Unload();
                                currentGroup.PersistentPrefix = string.Format("[{0}]_", files[currentId]);
                                currentGroup?.Load(false);
                            }
                        }
                        else
                        {

                            //g.PersistentPrefix = files[0];
                            currentGroup?.Unload();
                            currentGroup.PersistentPrefix = string.Format("[{0}]_", files[EditorGUILayout.Popup(0, files)]);
                            currentGroup?.Load(false);
                        }


                    }
                    else
                    {
                        EditorGUILayout.Popup(0, new string[] { "<none>" });
                    }

                    if (GUILayout.Button("+"))
                    {
                        //_currentAction = CVarWindowAction.CREATE_PERSISTENT_FILE;
                        //_editableAuxName = SceneManager.GetActiveScene().name;
                    }
                    EditorGUI.BeginDisabledGroup(files.Length == 0);
                    if (GUILayout.Button("-") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                    {
                        currentGroup.ResetToDefault();
                    }

                    if (GUILayout.Button("E"))
                    {
                        //rename
                        _currentAction = CVarWindowAction.RENAME_PERSISTENT_FILE;
                        _editableAuxName = pattern.Match(currentGroup.PersistentPrefix).Value;
                        /*g.ResetToDefault();
                        CVarSystem.GetGroup(_currentGroup).Save();
                        CVarSystem.GetGroup(_currentGroup).FlushPersistent();*/
                    }
                    if (GUILayout.Button("R") && EditorUtility.DisplayDialog("Reset persistent", "Want reset persistent file to default?", "Reset", "Cancel"))
                    {
                        currentGroup.ResetToDefault();
                        currentGroup.Save();
                        currentGroup.FlushPersistent();
                    }
                    EditorGUI.EndDisabledGroup();// disable files.Length == 0

                    //EditorGUI.EndDisabledGroup();
                }
                else
                {
                    _editableAuxName = EditorGUILayout.TextField(_editableAuxName);

                    if (GUILayout.Button("v"))
                    {
                        // check name consistence
                        if (CVarSystem.ValidateName(_editableAuxName))
                        {
                            if (!System.IO.File.Exists(CVarGroup.GetPersistentFilePath(currentGroup.Name, CVarGroup.ParsePrefixName(_editableAuxName))))
                            {
                                if (_currentAction == CVarWindowAction.CREATE_PERSISTENT_FILE && currentGroup.PersistentPrefix.Length > 0)
                                {
                                    // save the current file
                                    currentGroup?.Save();
                                    currentGroup?.FlushPersistent();
                                }
                                else if (_currentAction == CVarWindowAction.RENAME_PERSISTENT_FILE)
                                {
                                    // delete the current file
                                    currentGroup?.DeletePersistentFile();
                                }

                                // set the new persistent prefix
                                currentGroup?.SetGroupPersistentPrefix(_editableAuxName, currentGroup.PersistentType);
                                // save the new persistent file
                                currentGroup?.Save();
                                // force create file
                                currentGroup?.FlushPersistent();

                                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                                _editableAuxName = string.Empty;
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Invalid name", "There is another file with this name!", "OK");
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Invalid name", "Invalid name try another one!\nThe name can't have only spaces!", "OK");
                        }
                    }

                    if (GUILayout.Button("x"))
                    {
                        _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                        _editableAuxName = string.Empty;
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!CVarSystem.CanLoadRuntimePersistent);
                if (!System.IO.File.Exists(CVarGroup.GetPersistentFilePath(currentGroup.UID, string.Empty)))
                {
                    if (GUILayout.Button("Create File"))
                    {
                        currentGroup?.Save();
                        currentGroup?.FlushPersistent();
                        EditorUtility.DisplayDialog("Success", "File Created", "Continue");
                    }
                }
                else
                {

                    if (GUILayout.Button("Revert") && EditorUtility.DisplayDialog("Revert persistent", "Want revert persistent file to default?", "Revert", "Cancel"))
                    {
                        currentGroup?.ResetToDefault();
                        currentGroup?.Save();
                        currentGroup?.FlushPersistent();
                    }

                    if (DefaultButton("-", "Delete Persistent File") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                    {
                        currentGroup?.ResetToDefault();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDefaultFileManagerOptions(H_DataGroup<T, K> currentGroup, string loadedPath, string unloadedPath, string readGroupsFrom)
        {
            //if (CVarSystem.FilesHasBeenCopied)
            //{
            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived || _currentAction != CVarWindowAction.EDIT_VAR_VALUES);

            if (CVarSystem.CanLoadRuntimeDefault && System.IO.File.Exists(loadedPath))
            {
                // If group there isn't the group in the default (group create at runtime) revert button won't be showed
                if (GUILayout.Button(new GUIContent("Revert", "Revert data to default")) && EditorUtility.DisplayDialog("Revert to default", "Want revert runtime default file with editor default data?", "Revert", "Cancel"))
                {
                    RevertGroupToDefault(
                        currentGroup,
                        unloadedPath,
                        loadedPath
                        );
                }
            }
            if (System.IO.File.Exists(loadedPath))
            {
                if (GUILayout.Button(new GUIContent(CVarSystem.CanLoadRuntimeDefault ? "Overwrite Editor" : "Overwrite Runtime", CVarSystem.CanLoadRuntimeDefault ? "Copy and overwrite default data on editor" : "Copy and overwrite default data on persistent folder"))
                    && EditorUtility.DisplayDialog("Overwrite default", "Want overwrite runtime default file?", "Ovewrite", "Cancel"))
                {
                    OverwriteData(
                        currentGroup,
                        loadedPath,
                        unloadedPath,
                        readGroupsFrom
                        );
                }
                if (DefaultButton("-", "Delete") && EditorUtility.DisplayDialog("Delete default", "Want delete default file?\nObs.: This process just delete the file and erase the data. If you want delete the whole group try the (-) symbol next to Group name.", "Delete", "Cancel"))
                {
                    currentGroup?.Unload();
                    M_XMLFileManager.Delete
                    (
                        loadedPath
                    );

                    M_XMLFileManager.Delete
                    (
                        string.Concat(loadedPath, ".meta")
                    );
                    currentGroup?.Load(false);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Create File", "Create File")))
                {
                    currentGroup.Save();
                    currentGroup.Flush(true);
                }
            }

            EditorGUI.EndDisabledGroup();

        }

        private void RevertGroupToDefault(H_DataGroup<T, K> currentGroup, string origenPath, string destinationPath)
        {
            // read all groups from path
            H_DataGroup<T, K>[] groups = M_XMLFileManager.Load<H_DataGroup<T, K>[]>(CVarSystem.ParseStreamingDefaultDataPathWith("groups_data.xml"));
            // check if group exists
            H_DataGroup<T, K> group = groups.FirstOrDefault((g) => g.UID == currentGroup.UID);
            if (group != null)
            {
                //change group data
                currentGroup.Name = group.Name;
                currentGroup.PersistentType = group.PersistentType;

                currentGroup?.Unload();
                M_XMLFileManager.Copy
                (
                    origenPath,
                    destinationPath,
                    true
                );
                currentGroup?.Load(false);

                // save new group list to runtime
                ///////////////////CVarSystem.SaveGroupListToFile();
            }
            else
            {
                // add
                //groups.Add(currentGroup);
                Debug.LogWarning(string.Format("Can't revert data to default! There isn't Group with Name ({0}) and UID ({1}) on group list!", currentGroup.Name, currentGroup.UID));
            }
        }

        private void OverwriteData(H_DataGroup<T, K> currentGroup, string origenPath, string destinationPath, string readGroupsFrom)
        {
            // read all groups from path
            List<H_DataGroup<T, K>> groups = new List<H_DataGroup<T, K>>();
            groups.AddRange(M_XMLFileManager.Load<H_DataGroup<T, K>[]>(readGroupsFrom));

            // check if group exists
            H_DataGroup<T, K> group = groups.FirstOrDefault((g) => g.UID == currentGroup.UID);
            if (group != null)
            {
                //change group data
                group.Name = currentGroup.Name;
                group.PersistentType = currentGroup.PersistentType;
            }
            else
            {
                // add
                groups.Add(currentGroup);
                //Debug.LogWarning(string.Format("Can't revert data to default! There isn't Group with Name ({0}) and UID ({1}) on group list!", currentGroup.Name, currentGroup.UID));
            }

            M_XMLFileManager.Copy
                (
                    origenPath,
                    destinationPath,
                    true
                );
            M_XMLFileManager.Save(readGroupsFrom, groups.ToArray());
        }

        private bool DefaultButton(string label, string tooltip)
        {
            return GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(19));
        }
    }


    public class QuestGroupEditor
    {
        public H_DataGroup<H_Quest, H_PersistentQuestData> CurrentQuestGroup { get; set; }
        public H_Quest SelectedQuest { get; set; }

        private ReorderableList _questList;
        private Vector2 _scrollPosition;

        private H_Quest _copiedQuest;

        public void Start(string currentGroup)
        {
            Start(H_QuestManager.Instance.QuestGroups.GetGroupByName(currentGroup));
        }

        public void Start(H_DataGroup<H_Quest, H_PersistentQuestData> currentGroup, H_Quest selected = null)
        {
            if (CurrentQuestGroup != currentGroup)
            {
                CurrentQuestGroup = currentGroup;
                
                if (currentGroup != null)
                    _questList = new ReorderableList(currentGroup.Data, typeof(H_Quest), true, true, true, true)
                    {
                        drawHeaderCallback = OnDrawHeaderHandler,
                        drawElementCallback = OnDrawElementHandler,
                        onAddCallback = OnAddElementHandler,
                        onRemoveCallback = OnRemoveElementHandler,
                        onSelectCallback = OnSelectElementHandler
                    };
                else
                    _questList = null;

                _scrollPosition = Vector2.zero;
                SelectedQuest = selected;

                if (selected != null)
                    _questList.index = currentGroup.Data.IndexOf(selected);
            }
        }

        public void Draw()
        {
            if (CurrentQuestGroup == null)
                return;

            float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(firstCollumWidth));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUILayout.MinWidth(firstCollumWidth), GUILayout.MaxWidth(firstCollumWidth));

            _questList?.DoLayoutList();

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            EditorGUI.LabelField(rect, string.Format("Quests {0}", CurrentQuestGroup.Name));
            rect.x += rect.width - 40;
            rect.width = 20;
            EditorGUI.BeginDisabledGroup(SelectedQuest == null);
            if(GUI.Button(rect, new GUIContent("C", "Copy Quest")))
            {
                _copiedQuest = SelectedQuest;
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;
            EditorGUI.BeginDisabledGroup(_copiedQuest == null);

            if(GUI.Button(rect, new GUIContent("P", "Paste Quest")))
            {
                H_Quest clone = _copiedQuest.Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString());

                clone.Group = null; // remove group to avoid name error
                clone.UName = _copiedQuest.UName;// save the UName without changes

                if (_questList.index >= 0 && _questList.index < CurrentQuestGroup.Data.Count)
                {
                    CurrentQuestGroup.Insert(_questList.index + 1, clone);
                    clone.CheckAndUpdateUName();// process a new UName for the clone avoiding duplicated names in the group
                    _questList.index++;
                }
                else
                {
                    CurrentQuestGroup.Add(clone);
                    clone.CheckAndUpdateUName();// process a new UName for the clone avoiding duplicated names in the group
                    _questList.index = _questList.list.Count - 1;
                }

                SelectedQuest = clone;
                this.DispatchEvent(ES_Event.ON_CLICK, clone);
            }

            EditorGUI.EndDisabledGroup();
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 2;
            EditorGUI.DrawRect(rect, new Color(0.1f,0.1f,0.1f,0.3f));
            
            rect.width = rect.width - 20;
            GUI.Label(rect, string.Format("[#{0}][#{1}] {2}", (index+1).ToString("#00"), CurrentQuestGroup.Data[index].UID, CurrentQuestGroup.Data[index].UName));

            rect.x += rect.width;
            rect.width = 19;
            if (GUI.Button(rect, new GUIContent("D", "Duplicate Quest")))
            {
                //duplicate quest
                CurrentQuestGroup.Insert(index + 1, CurrentQuestGroup.Data[index].Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString()));
                SelectQuest(index+1);
                /*_questList.index = index + 1;
                this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[index+1]);*/
            }
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            H_Quest q = CurrentQuestGroup.Data[list.index];
            if(EditorUtility.DisplayDialog("Delete Quest", String.Format("Want delete quest {0}?\n\nYou can't revert this operation.", q.UName), "Delete", "Cancel"))
            {
                CurrentQuestGroup.Remove(q);
                this.DispatchEvent(ES_Event.ON_DESTROY, q);
                if (list.index > 0)
                {
                    SelectQuest(list.index-1);
                    /*list.index = list.index - 1;
                    SelectedQuest = CurrentQuestGroup.Data[list.index];
                    this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);*/
                }
                else if(list.count > 0)
                {
                    SelectQuest(list.index);
                    //SelectedQuest = CurrentQuestGroup.Data[list.index];
                    //this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);
                }
                else
                {
                    SelectedQuest = null;
                }
            }
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            if (list.index > 0)
            {
                CurrentQuestGroup.Insert(list.index + 1, CurrentQuestGroup.Data[list.index].Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString()));
                list.index++;
            }
            else
            {
                CurrentQuestGroup.Add(
                    new H_Quest()
                    {
                        UID = H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString(),
                        Group = CurrentQuestGroup,
                        UName = "quest_(0)"
                    });

                list.index = list.count - 1;
            }

            SelectQuest(list.index);
            //this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);
        }

        private void OnSelectElementHandler(ReorderableList list)
        {
            //list.index
            SelectQuest(list.index);
            /*SelectedQuest = CurrentQuestGroup.Data[list.index];
            this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);*/
        }

        private void SelectQuest(int index)
        {
            _questList.index = index;
            SelectedQuest = CurrentQuestGroup.Data[index];
            this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[index]);
        }
    }

    public class QuestEditor
    {
        public H_Quest CurrentQuest { get; set; }

        private ReorderableList _infoList;

        private int _currentOption = 0;

        ConditionEditor _start = new ConditionEditor();
        ConditionEditor _goals = new ConditionEditor();
        ConditionEditor _fails = new ConditionEditor();

        RewardListEditor _rewardList = new RewardListEditor();

        Vector2 _scroolPosition;

        public void Start(H_Quest quest)
        {
            if (quest != null)
            {
                CurrentQuest = quest;

                _infoList = new ReorderableList(quest.Info, typeof(QuestInfo))
                {
                    drawHeaderCallback = OnDrawHeaderHandler,
                    drawElementCallback = OnDrawElementHandler,
                    elementHeightCallback = OnElementHeightHandler
                };

                _start.Start(quest.StartCondition, "Start Conditions");
                _goals.Start(quest.TaskCondition, "Goals/Tasks");
                _fails.Start(quest.FailCondition, "Fails Conditions");
                _rewardList.Start();
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            _scroolPosition = Vector2.zero;
            _currentOption = 0;
            CurrentQuest = null;
            _infoList = null;
            _start.Clear();
            _goals.Clear();
            _fails.Clear();
        }

        public void Draw()
        {
            float size = ((EditorGUIUtility.currentViewWidth / 3f) * 2f) - 20f;

            if (CurrentQuest != null)
            {
                float w = size / 4f - 3f;

                _scroolPosition = EditorGUILayout.BeginScrollView(_scroolPosition, GUILayout.MinWidth(((EditorGUIUtility.currentViewWidth / 3f) * 2f)), GUILayout.MaxWidth(((EditorGUIUtility.currentViewWidth / 3f) * 2f)));
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                string uname = EditorGUILayout.TextField(new GUIContent(string.Format("Unique Name ({0})", CurrentQuest.UID), "Used for internal control"), CurrentQuest.UName);
                if(uname != CurrentQuest.UName)
                {
                    if (ObjectNamesManager.ValidateName(uname, '.', '[', ']', ' ', '\0'))
                        CurrentQuest.UName = uname;
                    else
                        Debug.LogWarning("Avoid use . [ ] to name your strings. Names with only spaces are not alloweds to.");
                }

                if (GUILayout.Button("E", GUILayout.MinWidth(19), GUILayout.MaxWidth(19)))
                {

                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                _infoList.DoLayoutList();

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));

                DrawTabButton("Start", 0, size, w);
                DrawTabButton("Goals/Tasks", 1, size, w);
                DrawTabButton("Fails", 2, size, w);
                DrawTabButton("Rewards", 3, size, w);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));
                
                if (_currentOption == 0)
                    _start.Draw();
                else if (_currentOption == 1)
                    _goals.Draw();
                else if (_currentOption == 2)
                    _fails.Draw();
                else if (_currentOption == 3)
                    _rewardList.Draw();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));

                EditorGUILayout.HelpBox("Select some quest to edit!", MessageType.Warning);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTabButton(string label, int option, float size, float width)
        {
            EditorGUI.BeginDisabledGroup(_currentOption == option);
            if (GUILayout.Button(label, GUILayout.MinWidth(width), GUILayout.MaxWidth(width)))
                _currentOption = option;
            EditorGUI.EndDisabledGroup();
        }

        private float OnElementHeightHandler(int index)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 5f;
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            rect.width -= 19;
            EditorGUI.LabelField(rect, "Quest Info");
            rect.x += rect.width;
            rect.width = 19;
            EditorGUI.Toggle(rect, true);
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            CurrentQuest.Info[index].Name = EditorGUI.TextField(rect, "Name:", CurrentQuest.Info[index].Name);
            rect.y += rect.height;
            CurrentQuest.Info[index].Description = EditorGUI.TextField(rect, "Description:", CurrentQuest.Info[index].Description);            
        }

    }

    public class ConditionEditor
    {
        private string _title = string.Empty;

        private H_Condition _condition;

        private ReorderableList _reorderableList;

        private List<ConditionEditor> _conditions = new List<ConditionEditor>();

        private bool _useStringFullname = false;

        public void Start(H_Condition condition, string title = "Conditions")
        {
            //save title
            _title = title;
            //save condition
            _condition = condition;
            _conditions.Clear();
            if (_condition.Type == H_EConditionType.CONDITION)
            {
                foreach(H_Condition c in condition.Conditions)
                {
                    ConditionEditor ce = new ConditionEditor();
                    _conditions.Add(ce);
                    ce.Start(c);
                }

                //create the reorderableList
                _reorderableList = new ReorderableList(_conditions, typeof(ConditionEditor))
                {
                    drawHeaderCallback = OnDrawHeaderHandler,
                    drawElementCallback = OnDrawElementHandler,
                    elementHeightCallback = OnElementHeightHandler,
                    onAddCallback = OnAddElementHandler,
                    onRemoveCallback = OnRemoveElementHandler
                };
            }
            else
            {
                _reorderableList = null;
            }
        }

        public void Clear()
        {
            _title = string.Empty;   
            _condition = null;
            _reorderableList = null;
            _conditions.Clear();
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            _condition.RemoveCondition(list.index);
            _conditions.RemoveAt(list.index);

            // select the up next element to facilitate delete operation
            if (list.index != 0)
                list.index = list.index - 1;
        }

        public void Draw()
        {
            _reorderableList?.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Button(string.Format("Create On {0} Complete Scene Handler", _title));
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            ConditionEditor c = new ConditionEditor();
            H_Condition condition = new H_Condition() { Type = H_EConditionType.CHECK_VAR };
            if (list.index < 0 || list.count == 0)
            {
                condition.UID = string.Concat(_condition.UID, "_c", 0);
                c._condition = condition;
                _conditions.Add(c);
                _condition.AddCondition(condition);
                list.index = 0;
            }
            else
            {
                condition.UID = string.Concat(_condition.UID, "_c", list.index);
                c._condition = condition;
                _conditions.Insert(list.index + 1, c);
                _condition.InsertCondition(list.index + 1, condition);
                list.index++;
            }
            c.Start(condition);
        }

        private float OnElementHeightHandler(int index)
        {
            if (_conditions[index]._condition.Type == H_EConditionType.CONDITION)
                return _conditions[index]._reorderableList.GetHeight() + EditorGUIUtility.singleLineHeight * 1.5f;//(_conditions[index]._conditions.Count+6) * EditorGUIUtility.singleLineHeight;
            else if (_conditions[index]._condition.Type == H_EConditionType.ON_EVENT_DISPATCH)
                return EditorGUIUtility.singleLineHeight * 3 + 10;

            return EditorGUIUtility.singleLineHeight * 2 + 10;
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            float w = rect.width/2;

            rect.width = w;
            EditorGUI.LabelField(rect, _title);
            
            rect.x += rect.width;
            rect.width = GUI.skin.label.CalcSize(new GUIContent("repeat")).x+9;
            EditorGUI.LabelField(rect, "Repeat:");

            rect.x += rect.width;
            rect.width = (w / 2)-rect.width;
            EditorGUI.TextField(rect,"1");

            rect.x += rect.width;
            rect.width = w / 2;
            _condition.Operation = (H_EConditionOperation)EditorGUI.EnumPopup(rect, _condition.Operation);
        }

        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_conditions[index]._condition.Params == null)
                _conditions[index]._condition.CreateParamsByType(_conditions[index]._condition.Type);
            
            Rect origin = rect;
            float halfWidth = (origin.width / 2f);
            
            rect.width = halfWidth - 18f;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(rect, _conditions[index]._condition.UID);
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;
            rect.width = 18f;
            GUI.Button(rect, "E");

            rect.width = halfWidth / 2;
            rect.x += 18f;
            EditorGUI.BeginDisabledGroup(index == 0);
                _conditions[index]._condition.CombineOperation = (H_ECombineOperation)EditorGUI.EnumPopup(rect, _conditions[index]._condition.CombineOperation);
            if(index == 0 && _conditions[index]._condition.CombineOperation != H_ECombineOperation.JOIN)
            {
                _conditions[index]._condition.CombineOperation = H_ECombineOperation.JOIN;
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;

            EditorGUI.BeginChangeCheck();
            _conditions[index]._condition.Type = (H_EConditionType)EditorGUI.EnumPopup(rect, _conditions[index]._condition.Type);//types[EditorGUI.Popup(rect, Array.IndexOf(types, _conditions[index]._condition.Type), types)];

            if(EditorGUI.EndChangeCheck())
            {
                _conditions[index].Start(_conditions[index]._condition);
                _conditions[index]._condition.CreateParamsByType(_conditions[index]._condition.Type);

            }

            rect.x = origin.x;

            rect.y += EditorGUIUtility.singleLineHeight;

            DrawParamsByConditionType(rect, origin, index, _conditions[index]._condition.Type);
        }

        private void DrawParamsByConditionType(Rect rect, Rect origin, int index, H_EConditionType type)
        {
            if (type == H_EConditionType.CHECK_VAR || type == H_EConditionType.ON_CHANGE_VAR)
            {
                DrawVarCondition(rect, origin, index);
            }
            else if (type == H_EConditionType.ON_EVENT_DISPATCH)
            {
                DrawEventDispatch(rect, origin, index);
            }
            else if (type == H_EConditionType.CONDITION)
            {
                DrawCondition(rect, origin, index);
            }
            else if (type == H_EConditionType.LISTEN_QUEST)
            {
                DrawListenQuest(rect, origin, index);
            }
            else if (type == H_EConditionType.TIMER)
            {
                DrawTimerOption(rect, origin, index);
            }
        }

        private void DrawVarCondition(Rect rect, Rect origin, int index)
        {
            string[] groupsNames = CVarSystem.GetGroups().Select(e => e.Name).ToArray();
            string[] varTypes = CVarSystem.AllowedTypes;

            rect.width = 18;
            _conditions[index]._useStringFullname = EditorGUI.Toggle(rect, _conditions[index]._useStringFullname);
            rect.x += 18;

            rect.width = (origin.width - 18) / 5;

            if (!_conditions[index]._useStringFullname)
            {
                CheckableOptionPopup(rect, index, 0, groupsNames);

                rect.x += rect.width;

                int i = Array.FindIndex(varTypes, 0, varTypes.Length, e => e == (string)_conditions[index]._condition.Params[1]);
                int newValue = CheckableOptionPopup(rect, index, 1, varTypes);

                if (newValue != i)
                {
                    _conditions[index]._condition.UpdateParam(4, GetNewDefaultValue(varTypes[newValue]));
                }

                rect.x += rect.width;

                CheckableOptionPopup(rect, index, 2, CVarSystem.GetVarNamesByType((string)_conditions[index]._condition.Params[1], (string)_conditions[index]._condition.Params[0]));

                
            }
            else
            {
                rect.width = rect.width * 3f;

                string fullname = CVarSystem.GetFullName((string)_conditions[index]._condition.Params[2], (string)_conditions[index]._condition.Params[1], (string)_conditions[index]._condition.Params[0]);
                EditorGUI.BeginChangeCheck();
                fullname = EditorGUI.TextField(rect, fullname);
                if(EditorGUI.EndChangeCheck())
                {
                    string[] p = fullname.Split('.');
                    _conditions[index]._condition.UpdateParam(0, p[0]);
                    _conditions[index]._condition.UpdateParam(1, p[1]);
                    _conditions[index]._condition.UpdateParam(2, p[2]);
                }

            }

            rect.x += rect.width;
            rect.width = (origin.width - 18) / 5;
            EditorGUI.BeginChangeCheck();
            CVarCommands param3 = (CVarCommands)EditorGUI.EnumPopup(rect, (CVarCommands)_conditions[index]._condition.Params[3]);
            if (EditorGUI.EndChangeCheck())
                _conditions[index]._condition.UpdateParam(3, param3);

            rect.x += rect.width;

            EditorGUI.BeginChangeCheck();
            object param4 = DrawFieldByType(rect, _conditions[index]._condition.Params[4]);
            if (EditorGUI.EndChangeCheck())
                _conditions[index]._condition.UpdateParam(4, param4);
        }

        private void DrawCondition(Rect rect, Rect origin, int index)
        {
            rect.width = origin.width;

            _conditions[index]._reorderableList.DoList(rect);
        }
        
        private UnityEngine.Object _objTarget;
        private IS_InstanceSceneManager InstanceManager;

        private void DrawEventDispatch(Rect rect, Rect origin, int index)
        {
            if(!InstanceManager)
                InstanceManager = GameObject.FindObjectOfType<IS_InstanceSceneManager>();
            //rect.width = 18;
            //EditorGUI.Toggle(rect, false);
            //rect.x += 18;
            float targetSize = GUI.skin.label.CalcSize(new GUIContent("Target")).x;
            float identifierSize = GUI.skin.label.CalcSize(new GUIContent("Identifier")).x;
            float fieldSize = (origin.width - 18 - targetSize- identifierSize) / 3f;

            rect.width = targetSize;
            EditorGUI.LabelField(rect, "Target:");
            
            rect.x += rect.width+9;
            rect.width = fieldSize;
            string param0 = (string)_conditions[index]._condition.Params[0];
            
            EditorGUI.BeginChangeCheck();
            _conditions[index]._objTarget = EditorGUI.ObjectField(rect, _conditions[index]._objTarget, typeof(UnityEngine.Object), true);
            
            if (_conditions[index]._objTarget)
            {
                param0 = (_conditions[index]._objTarget.GetEditorInstanceName() == string.Empty) ? _conditions[index]._objTarget.SetEditorInstanceName("__instance__") : _conditions[index]._objTarget.GetEditorInstanceName(false);
            }
            else
            {
                _conditions[index]._objTarget = GetObjectByString(param0);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                if(!_conditions[index]._objTarget)
                    param0 = string.Empty;

                _conditions[index]._condition.UpdateParam(0, param0);
            }

            rect.x += rect.width;

            if (_conditions[index]._objTarget)
            {
                Component[] cs = null;
                string[] s = null;

                if (_conditions[index]._objTarget is Component ob)
                {
                    cs = ob.GetComponents<Component>();
                    s = new string[cs.Length + 1];
                }
                else if (_conditions[index]._objTarget is GameObject ob2)
                {
                    cs = ob2.GetComponents<Component>();
                    s = new string[cs.Length + 1];
                }

                if (cs != null)
                {
                    int ind = 0;
                    int newIndex = 0;

                    s[0] = "GameObject";
                    for (int i = 0; i < cs.Length; i++)
                    {
                        s[i + 1] = string.Format("{0}. {1}", i, cs[i].GetType().Name);
                        if (cs[i] == _conditions[index]._objTarget)
                            ind = i + 1;
                    }

                    newIndex = EditorGUI.Popup(rect, ind, s);
                    //newIndex = EditorGUILayout.Popup(index, s);

                    if (newIndex != ind)
                    {
                        if (newIndex != 0)
                        {
                            _conditions[index]._objTarget = cs[newIndex - 1];
                        }
                        else
                        {
                            if (_conditions[index]._objTarget is Component)
                            {
                                _conditions[index]._objTarget = (_conditions[index]._objTarget as Component).gameObject;
                            }
                            else if (_conditions[index]._objTarget is GameObject)
                            {
                                _conditions[index]._objTarget = (_conditions[index]._objTarget as GameObject);
                            }
                        }

                        param0 = (_conditions[index]._objTarget.GetEditorInstanceName() == string.Empty) ? _conditions[index]._objTarget.SetEditorInstanceName("__instance__") : _conditions[index]._objTarget.GetEditorInstanceName(false);
                        _conditions[index]._condition.UpdateParam(0, param0);
                        //_id.stringValue = (_objTarget.GetEditorInstanceName() == string.Empty) ? _objTarget.SetEditorInstanceName("__instance__") : _objTarget.GetEditorInstanceName(false);
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.Popup(rect, 0, new string[] { "none" });
                EditorGUI.EndDisabledGroup();
            }
            
            rect.x += rect.width;
            rect.width = identifierSize;
            EditorGUI.LabelField(rect, "Identifier:");

            rect.x += rect.width+9;
            rect.width = fieldSize;
            EditorGUI.BeginChangeCheck();
            param0 = EditorGUI.TextField(rect, param0);
            if (EditorGUI.EndChangeCheck())
            {
                if (InstanceManager != null)
                {
                    _conditions[index]._objTarget = GetObjectByString(param0);
                }
                _conditions[index]._condition.UpdateParam(0, param0);
            }

            rect.x = origin.x;
            rect.width = origin.width;
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.TextField(rect, "Event Name:", "ON_COMPLETE");
            //EditorGUI.Popup(rect, 0, new string[] { "ON_TUTORIAL_COMPLETE", "ON_GAME_OVER" });
        }

        private UnityEngine.Object GetObjectByString(string objID)
        {
            if (InstanceManager.ContainsValue(objID))
            {
                return (UnityEngine.Object)InstanceManager.GetKeyByValue(objID);
            }
            else
            {
                return null;
            }
        }

        private void DrawListenQuest(Rect rect, Rect origin, int index)
        {
            rect.width = 18;
            EditorGUI.Toggle(rect, false);
            rect.x += 18;
            rect.width = (origin.width - 18) / 2;
            EditorGUI.Popup(rect, 0, new string[] { "Quest (ID)" });
            rect.x += rect.width;
            EditorGUI.Popup(rect, 0, new string[] { "ON_COMPLETE", "ON_GOAL_UPDATE", "ON_FAIL" });
        }

        private void DrawTimerOption(Rect rect, Rect origin, int index)
        {
            rect.width = 18;
            EditorGUI.Toggle(rect, false);
            rect.x += 18;
            rect.width = (origin.width - 18) / 2;
            EditorGUI.Popup(rect, 0, new string[] { "ASC", "DESC" });
            rect.x += rect.width;
            EditorGUI.FloatField(rect, 100f);
        }

        private static object GetNewDefaultValue(string type)
        {
            if (type == typeof(string).Name)
                return "value";
            else if (type == typeof(int).Name)
                return 0;
            else if (type == typeof(float).Name)
                return 0.0f;
            else if (type == typeof(bool).Name)
                return false;
            else if (type == typeof(Vector3).Name)
                return Vector3.zero;

            return null;
        }

        private static object DrawFieldByType(Rect rect, object value)
        {
            if (value is string)
                return EditorGUI.TextField(rect, (string)value);
            else if (value is int)
                return EditorGUI.IntField(rect, (int)value);
            else if (value is float)
                return EditorGUI.FloatField(rect, (float)value);
            else if (value is bool)
                return EditorGUI.Toggle(rect, (bool)value);
            else if (value is Vector3)
                return EditorGUI.Vector3Field(rect, "", (Vector3)value);

            return null;
        }

        public int CheckableOptionPopup(Rect rect, int conditionIndex, int paramIndex, string[] displayableOptions)
        {
            int oldIndex = Array.FindIndex(displayableOptions, 0, displayableOptions.Length, (e) => e == (string)_conditions[conditionIndex]._condition.Params[paramIndex]);

            if(CheckablePopup(rect, oldIndex, displayableOptions, out int index))
            {
                _conditions[conditionIndex]._condition.UpdateParam(paramIndex, displayableOptions[index]);
                return index;
            }

            return oldIndex;
        }

        public bool CheckablePopup(Rect rect, int selectedIndex, string[] displayableOptions, out int index)
        {
            index = EditorGUI.Popup(rect, selectedIndex, displayableOptions);

            if(selectedIndex != index)
            {
                return true;
            }

            return false;
        }
    }

    

    public class RewardListEditor
    {
        ReorderableList reorderableList;
        List<string> list = new List<string>() { "reward a", "reward b", "reward c" };

        public void Start()
        {
            reorderableList = new ReorderableList(list, typeof(string))
            {
                drawHeaderCallback = OnDrawHeaderHandler,
                drawElementCallback = OnDrawElementHandler
            };
        }

        public void Draw(string label = "Conditions")
        {
            /*EditorGUILayout.LabelField(label);*/

            reorderableList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //EditorGUILayout.Space();
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            string name = "Rewards";
            EditorGUI.LabelField(rect, name);
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, list[index]);
        }
    }

    public class RewardEditor
    {
        public void Draw()
        {
            EditorGUILayout.LabelField("Reward field");
        }
    }
}