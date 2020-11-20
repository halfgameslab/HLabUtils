using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

enum CVarWindowAction
{ 
    CREATE_GROUP,
    CREATE_VAR,
    EDIT_VAR_VALUES,
    EDIT_VAR_NAME,
    EDIT_GROUP,
    CREATE_PERSISTENT_FILE,
    RENAME_PERSISTENT_FILE
}


public class CVarWindow : EditorWindow
{
    private const int SELECTED_ALL_INDEX = 6;

    private string _searchString = string.Empty;

    private string _editableName = string.Empty;
    private string _editableAuxName = string.Empty;
    private object _editableAuxValue;
    private int _selectedType = SELECTED_ALL_INDEX;
    private bool[] _fouldout = new bool[] { true, true, true, true, true };
    private bool _persistentAux = false;
    private bool _lockedAux = false;
    private bool _showAddress = false;
    private GUIStyle style;

    private CVarWindowAction _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

    private CVarGroup _currentGroup;
    //private string _currentGroupName;
    private string _currentGroupUID = "1";

    public CVarGroup CurrentGroup 
    {
        get 
        {
            return _currentGroup;
        }
        set
        {
            _currentGroup = value;

            if (value != null)
            {
                //_currentGroupName = value.Name;
                _currentGroupUID = value.UID;
            }
        }
    }


    [MenuItem("HLab/CVar")]
    public static void ShowWindow()
    {
        CVarWindow editorWindow = CreateWindow<CVarWindow>("CVar");//GetWindow(typeof(CVarWindow), false, "CVar");
        ConfigureWindow(editorWindow);
    }

    public static void SelectWindow(string currentGroupName)
    {
        CVarWindow editorWindow = GetWindow<CVarWindow>(false, "CVar", true);
        editorWindow.CurrentGroup = CVarSystem.GetGroupByName(currentGroupName);
        ConfigureWindow(editorWindow);
    }


    public static void ConfigureWindow(CVarWindow window)
    {
        window.autoRepaintOnSceneChange = true;
        window.minSize = new Vector2(250, window.minSize.y);
        window.Show();
    }


    private void OnEnable()
    {
        //CurrentGroup = EditorPrefs.GetString("CurrentGroup", CurrentGroup);
        Undo.undoRedoPerformed += OnUndoRedoPerformedHandler;
        Undo.undoRedoPerformed += OnUndoRedoPerformedHandler;
        SceneManager.sceneLoaded += OnSceneLoadedHandler;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformedHandler;
        Undo.undoRedoPerformed -= OnUndoRedoPerformedHandler;
        SceneManager.sceneLoaded -= OnSceneLoadedHandler;
    }
    private void OnSceneLoadedHandler(Scene s, LoadSceneMode m)
    {
        Repaint();
    }
    private void OnUndoRedoPerformedHandler()
    {
        Repaint();
    }


    /*void OnSelectionChange()
    {
        Repaint();
    }*/

    void OnGUI()
    {
        if((CurrentGroup != null && CurrentGroup != CVarSystem.GetGroupByUID(CurrentGroup.UID))
        || (CurrentGroup == null && _currentAction != CVarWindowAction.CREATE_GROUP))
        {   
            CurrentGroup = CVarSystem.GetGroupByUID(_currentGroupUID);
            if(CurrentGroup == null)
                CurrentGroup = CVarSystem.GetGroupByName("global");
        }

        CVarSystem.ClearDefaultOnPlay = GUILayout.Toggle(CVarSystem.ClearDefaultOnPlay, "Clear default on Play");
        EditorGUILayout.Space();        

        DrawTopBar();
        if (_currentAction != CVarWindowAction.CREATE_GROUP)
            DrawGroupOptions();
        else
            DrawNewGroupOptions();

        DrawSearchAndAddHeader();
        DrawAbas();
        DrawVars();
    }//Close on GUI

    private CVarGroup _currentGroupAux;

    //Texture2D _logo;
    private void DrawTopBar()
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        //EditorGUI.DrawRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, this.maxSize.y), new Color(0.15f, 0.15f, 0.15f));
        //EditorGUI.DrawRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 9.5f), new Color(0.15f, 0.15f, 0.15f));
        EditorGUI.DrawRect(new Rect(0, EditorGUIUtility.singleLineHeight * 9.5f, EditorGUIUtility.currentViewWidth, this.maxSize.y), new Color(0.2f, 0.2f, 0.2f));
        //EditorGUI.DrawRect(new Rect(0, 23f, EditorGUIUtility.currentViewWidth, 22f), new Color(0.15f, 0.15f, 0.15f));
        EditorGUI.DrawRect(new Rect(0, EditorGUIUtility.singleLineHeight * 2.5f, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.2f, 0.2f, 0.2f));
        
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(1, 1, 1, 0);
        if (DefaultTextureButton("d_Collab.FileAdded", "Create New Group"))
        //if(DefaultButton("+", "Criar Grupo"))
        {
            _editableName = ObjectNamesManager.GetUniqueName(CVarSystem.GetGroups().Select((CVarGroup g)=>g.Name).ToArray(),"new_group");
            _editableAuxName = "undefined";
            _currentAction = CVarWindowAction.CREATE_GROUP;
            _currentGroupAux = CurrentGroup;
            CurrentGroup = null;
        }
        
        if (DefaultTextureButton("d_Refresh@2x", "Reload"))
        {
            CVarSystem.Reload();
        }

        //if (GUILayout.Button(CVarSystem.IsEditModeActived?"Disable Edit Mode": "Enable Edit Mode"))
        if (DefaultTextureButton(CVarSystem.IsEditModeActived ? "d_PlayButton@2x" : "d_PauseButton@2x", CVarSystem.IsEditModeActived ? "Disable Edit Mode" : "Enable Edit Mode"))
        {
            CVarSystem.ActiveEditMode(!CVarSystem.IsEditModeActived);
        }

        if (DefaultTextureButton("FolderOpened Icon", "Open persistent folder"))
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        //if (PlayerPrefs.GetInt("FilesCopied", 0) == 0)
        //{
        //    //if (GUILayout.Button("Copy Default to Pers"))
        //    if (DefaultTextureButton("d_Collab.FolderMoved", "Copy default files to persistent folder"))
        //    {
        //        CVarSystem.CopyDefaultFilesToPersistentFolder();
        //        EditorUtility.DisplayDialog("Copy files", "Files coppied with successed!", "Continue");
        //    }
        //}
        //else
        //{
            //if (GUILayout.Button("Delete Pers Default "))
            if (DefaultTextureButton("Collab.FolderDeleted", CVarSystem.IsEditModeActived?"Delete all data files in default runtime folder": "Revert to default all files in default runtime folder"))
                //&& EditorUtility.DisplayDialog("Delete persistent data", "Are You sure you want delete persistent data?\nThis can be undone.", "Delete", "Cancel"))
            {
                //CVarSystem.ResetToDefault(false);

                GenericMenu menu = new GenericMenu();
                
                menu.AddItem(new GUIContent("Delete Runtime Default"), false, () => { if (EditorUtility.DisplayDialog("Reset runtime default data", "Are You sure you want reset runtime default data?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.DeleteRuntimeDefault(false); });
                menu.AddItem(new GUIContent("Delete Runtime Persistent"), false, () => { if (EditorUtility.DisplayDialog("Delete persistent data", "Are You sure you want delete persistent data?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.DeletePersistent(); } );
                menu.AddItem(new GUIContent("Delete All Runtime"), false, () => { if (EditorUtility.DisplayDialog("Delete persistent data", "Are You sure you want delete all runtime files?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.ResetToDefault(false); });
                menu.AddItem(new GUIContent("Delete All"), false, () => { if (EditorUtility.DisplayDialog("Delete all files", "Are You sure you want delete all created files?\nThis can be undone.", "Delete", "Cancel")) CVarSystem.DeleteAll(); });
                
                menu.ShowAsContext();

                //CVarSystem.CanLoadRuntimeDefault = false;
            }
        //}
        
        /*if (DefaultTextureButton("d_Grid.EraserTool@2x", "Delete all files") && EditorUtility.DisplayDialog("Delete all files", "Are You sure you want delete all created files?\nThis can be undone.", "Delete", "Cancel"))
        {
            CVarSystem.DeleteAll();
        }*/

        GUI.backgroundColor = new Color(1, 1, 1, 1);

        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

    }

    private bool DefaultTextureButton(string textureName, string hint, float w = 50, float h = 40)
    {
        return GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture(textureName), hint), GUILayout.Width(w), GUILayout.Height(h));
    }
    private void OverwriteData(CVarGroup currentGroup, string origenPath, string destinationPath, string readGroupsFrom, string saveGroupsTo)
    {
        // read all groups from path
        CVarGroup[] groups = M_XMLFileManager.Load<CVarGroup[]>(readGroupsFrom);
        // check if group exists
        CVarGroup group = groups.FirstOrDefault((g) => g.UID == currentGroup.UID);
        if (group != null)
        {
            //change group data
            currentGroup.Name = group.Name;
            currentGroup.PersistentType = currentGroup.PersistentType;

            currentGroup?.Unload();
            M_XMLFileManager.Copy
            (
                origenPath,
                destinationPath,
                true
            );
            currentGroup?.Load(false);

            // save new group list to runtime
            //M_XMLFileManager.Save(saveGroupsTo, groups.ToArray());
            CVarSystem.SaveGroupListToFile();
        }
        else
        {
            // add
            //groups.Add(currentGroup);
            Debug.LogWarning(string.Format("Can't revert data to default! There isn't Group with Name ({0}) and UID ({1}) on group list!", currentGroup.Name, currentGroup.UID));
        }
    }
    private void DrawDefaultFileManagerOptions(CVarGroup currentGroup, string loadedPath, string unloadedPath)
    {
        if (PlayerPrefs.GetInt("FilesCopied", 0) == 1)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived || _currentAction != CVarWindowAction.EDIT_VAR_VALUES);
            if (System.IO.File.Exists(loadedPath))
            {
                if (CVarSystem.CanLoadRuntimeDefault && System.IO.File.Exists(unloadedPath))
                {
                    // If group there isn't the group in the default (group create at runtime) revert button won't be showed
                    if (GUILayout.Button(new GUIContent("Revert", "Revert data to default")) && EditorUtility.DisplayDialog("Revert to default", "Want revert runtime default file with editor default data?", "Revert", "Cancel"))
                    {
                        OverwriteData(
                            currentGroup, 
                            unloadedPath, 
                            loadedPath, 
                            CVarSystem.ParseStreamingDefaultDataPathWith("groups_data.xml"),
                            CVarSystem.ParsePersistentDefaultDataPathWith("groups_data.xml")
                            );             
                    }
                }
                if (GUILayout.Button(new GUIContent(CVarSystem.CanLoadRuntimeDefault ? "Overwrite Editor" : "Overwrite Runtime", CVarSystem.CanLoadRuntimeDefault ? "Copy and overwrite default data on editor" : "Copy and overwrite default data on persistent folder"))
                    && EditorUtility.DisplayDialog("Overwrite default", "Want overwrite runtime default file?", "Ovewrite", "Cancel"))
                {
                    OverwriteData(currentGroup, loadedPath, unloadedPath);
                    /*currentGroup?.Unload();
                    M_XMLFileManager.Copy
                    (
                        loadedPath,
                        unloadedPath,
                        true
                    );
                    currentGroup?.Load(false);*/
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
        else
        {
            if (GUILayout.Button(new GUIContent("Copy default to runtime", "Copy default to runtime folder")))
            {
                CVarSystem.CopyDefaultFilesToPersistentFolder();
                EditorUtility.DisplayDialog("Copy files", "Files coppied with successed!", "Continue");
            }
        }
    }

    private void DrawFileManager()
    {
        //EditorGUI.BeginDisabledGroup(PlayerPrefs.GetInt("FilesCopied", 0) == 0);
        EditorGUI.BeginDisabledGroup(PlayerPrefs.GetInt("FilesCopied", 0) == 0 || Application.isPlaying || !CVarSystem.IsEditModeActived || _currentAction != CVarWindowAction.EDIT_VAR_VALUES);

        EditorGUILayout.BeginHorizontal();

        bool boolAux = CVarSystem.CanLoadRuntimeDefault;
        
        GUI.backgroundColor = Color.cyan;
        boolAux = EditorGUILayout.ToggleLeft("Show Runtime Default", CVarSystem.CanLoadRuntimeDefault);
        GUI.backgroundColor = Color.white;
        if (boolAux != CVarSystem.CanLoadRuntimeDefault)
        {
            //CVarSystem.UnloadGroups();
            CVarSystem.CanLoadRuntimeDefault = boolAux;
            CVarSystem.Reload();
            //CVarSystem.LoadGroups(false);
        }
        EditorGUI.EndDisabledGroup();
        //EditorGUI.EndDisabledGroup();
        if (CVarSystem.CanLoadRuntimeDefault)
        {
            DrawDefaultFileManagerOptions
                (
                    CurrentGroup, 
                    CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(CurrentGroup.UID, ".xml")), 
                    CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(CurrentGroup.UID, ".xml"))
                );
            
        }
        else 
        {
            DrawDefaultFileManagerOptions
                (
                    CurrentGroup,
                    CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(CurrentGroup.UID, ".xml")),
                    CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(CurrentGroup.UID, ".xml"))
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

        if (CurrentGroup.PersistentType != CVarGroupPersistentType.SHARED)
        {
            if (_currentAction != CVarWindowAction.CREATE_PERSISTENT_FILE && _currentAction != CVarWindowAction.RENAME_PERSISTENT_FILE)
            {
                // positive lookbehind ?<= ignore the \[ and get all between \]_ in a literal way
                Regex pattern = new Regex(@"(?<=\[)(.*?)(?=\]_)");

                string[] files = CurrentGroup.ListAllPersistentFileNames();

                EditorGUI.BeginDisabledGroup(!CVarSystem.CanLoadRuntimePersistent);
                
                if (files.Length > 0)
                {
                    int id = Array.IndexOf(files, pattern.Match(CurrentGroup.PersistentPrefix).Value);//string.Format(g.PersistentPrefix.Replace("[", string.Empty).Replace("]", string.Empty).Replace("_", string.Empty)));
                    int currentId;

                    if (id >= 0 && id < files.Length)
                    {
                        currentId = EditorGUILayout.Popup(id, files);

                        if (id != currentId)
                        {
                            CurrentGroup?.Unload();
                            CurrentGroup.PersistentPrefix = string.Format("[{0}]_", files[currentId]);
                            CurrentGroup?.Load(false);
                        }
                    }
                    else
                    {

                        //g.PersistentPrefix = files[0];
                        CurrentGroup?.Unload();
                        CurrentGroup.PersistentPrefix = string.Format("[{0}]_", files[EditorGUILayout.Popup(0, files)]);
                        CurrentGroup?.Load(false);
                    }


                }
                else
                {
                    EditorGUILayout.Popup(0, new string[] { "<none>" });
                }

                if (GUILayout.Button("+"))
                {
                    _currentAction = CVarWindowAction.CREATE_PERSISTENT_FILE;
                    _editableAuxName = SceneManager.GetActiveScene().name;
                }
                EditorGUI.BeginDisabledGroup(files.Length == 0);
                if (GUILayout.Button("-") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                {
                    CurrentGroup.ResetToDefault();
                }

                if (GUILayout.Button("E"))
                {
                    //rename
                    _currentAction = CVarWindowAction.RENAME_PERSISTENT_FILE;
                    _editableAuxName = pattern.Match(CurrentGroup.PersistentPrefix).Value;
                    /*g.ResetToDefault();
                    CVarSystem.GetGroup(_currentGroup).Save();
                    CVarSystem.GetGroup(_currentGroup).FlushPersistent();*/
                }
                if (GUILayout.Button("R") && EditorUtility.DisplayDialog("Reset persistent", "Want reset persistent file to default?", "Reset", "Cancel"))
                {
                    CurrentGroup.ResetToDefault();
                    CurrentGroup.Save();
                    CurrentGroup.FlushPersistent();
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
                    if (_editableAuxName.Replace(" ", "").Length > 0 && !System.IO.File.Exists(CVarGroup.GetPersistentFilePath(CurrentGroup.Name, CVarGroup.ParsePrefixName(_editableAuxName))))
                    {
                        if (_currentAction == CVarWindowAction.CREATE_PERSISTENT_FILE && CurrentGroup.PersistentPrefix.Length > 0)
                        {
                            // save the current file
                            CurrentGroup?.Save();
                            CurrentGroup?.FlushPersistent();
                        }
                        else if (_currentAction == CVarWindowAction.RENAME_PERSISTENT_FILE)
                        {
                            // delete the current file
                            CurrentGroup?.DeletePersistentFile();
                        }

                        // set the new persistent prefix
                        CurrentGroup?.SetGroupPersistentPrefix(_editableAuxName, CurrentGroup.PersistentType);
                        // save the new persistent file
                        CurrentGroup?.Save();
                        // force create file
                        CurrentGroup?.FlushPersistent();

                        _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                        _editableAuxName = string.Empty;
                    }
                    else
                    {
                        if (_editableAuxName.Replace(" ", "").Length == 0)
                            EditorUtility.DisplayDialog("Invalid name", "Invalid name try another one!\nThe name can't have only spaces!", "OK");
                        else
                            EditorUtility.DisplayDialog("Invalid name", "There is another file with this name!", "OK");
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
            if (!System.IO.File.Exists(CVarGroup.GetPersistentFilePath(CurrentGroup.UID, string.Empty)))
            {
                if (GUILayout.Button("Create File"))
                {
                    CurrentGroup?.Save();
                    CurrentGroup?.FlushPersistent();
                    EditorUtility.DisplayDialog("Success", "File Created", "Continue");
                }
            }
            else
            {
                if (GUILayout.Button("Delete File") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                {
                    CurrentGroup?.ResetToDefault();
                }
                if (GUILayout.Button("Reset File") && EditorUtility.DisplayDialog("Reset persistent", "Want reset persistent file?", "Reset", "Cancel"))
                {
                    CurrentGroup?.ResetToDefault();
                    CurrentGroup?.Save();
                    CurrentGroup?.FlushPersistent();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGroupOptions()
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_GROUP && _currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        CVarGroup[] groups = CVarSystem.GetGroups();
        //string[] groups = CVarSystem.GetGroups().Select((e)=>e.Name).ToArray();//new string[] { "global", "group01", "group02", "group03" };

        if (_currentAction != CVarWindowAction.EDIT_GROUP)
        {
            int index = Array.IndexOf(groups, CurrentGroup);
            //int index = Array.FindIndex(groups, 0, e=>e.UID == CurrentGroup.UID);
            int newIndex = EditorGUILayout.Popup(string.Format("Group ({0})", CurrentGroup.UID), index, groups.Select((e=>e.Name)).ToArray());

            if(index != newIndex)
            {
                // change group
                CurrentGroup = groups[newIndex];
                CurrentGroup?.Load();   
            }

            EditorGUI.BeginDisabledGroup(CurrentGroup.Name == "global");
            if (DefaultButton("E", "Edit Group"))
            //if(DefaultTextureButton("d_editicon.sml", "Edit Group", 19, 19))
            {
                _currentAction = CVarWindowAction.EDIT_GROUP;
                _editableName = CurrentGroup.Name;
                
            }
            if (DefaultButton("-", "Delete Group") && EditorUtility.DisplayDialog("Delete Var", "Want delete group?", "Delete", "Cancel"))
            {
                CVarSystem.RemoveGroup(CurrentGroup);
                CurrentGroup = CVarSystem.GetGroupByName("global");
            }
            EditorGUILayout.EndHorizontal();
            CurrentGroup.SetPersistentTypeAndSave((CVarGroupPersistentType)EditorGUILayout.EnumPopup(new GUIContent("Persistent Type"), CurrentGroup.PersistentType));


            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //EditorGUILayout.BeginHorizontal();

            EditorGUI.EndDisabledGroup();

            DrawFileManager();

        }
        else
        {
            _editableName = EditorGUILayout.TextField("Group", _editableName);
            if (DefaultButton("V", "Save Changes"))
            {
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                CVarSystem.TryRenameGroup(CurrentGroup.Name, _editableName);
                //if (CVarSystem.TryRenameGroup(CurrentGroup.Name, _editableName))
                //    _currentGroupName = _editableName;
            }
            if (DefaultButton("X", "Cancel"))
            {
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup(new GUIContent("Persistent Type"), CurrentGroup.PersistentType);
            EditorGUI.EndDisabledGroup();
        }

        groups[0] = null;// "<none>";
                
        EditorGUI.EndDisabledGroup();

        //EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_GROUP);



        //EditorGUI.EndDisabledGroup();


        //EditorGUILayout.Space();
        EditorGUILayout.Space();

        float i_height = 2f;

        Rect rect = EditorGUILayout.GetControlRect(false, i_height);

        rect.height = i_height;

        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1));

    }

    private void DrawNewGroupOptions()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        
        _editableName = EditorGUILayout.TextField("Name", _editableName);
        
        if(DefaultButton("V", "Create Group"))
        {
            // create new group
            CVarGroup group = CVarSystem.CreateGroup(_editableName);

            // load new group
            //CVarSystem.GetGroupByName(_editableName).Load();
            group.Load();
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

            //unload current if
            if (CurrentGroup != null && CurrentGroup.Name != _editableName)
            {
                foreach (CVarObject var in CurrentGroup.Vars)
                {
                    // clone
                    CVarSystem.CloneVarToGroup(var, group);
                }

                //CurrentGroup.Unload();
            }
            // set the current group to new group
            CurrentGroup = group;
            _editableName = string.Empty;
        }

        if(DefaultButton("X", "Cancel"))
        {
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

            // set the current group to old group
            CurrentGroup = _currentGroupAux;

            _editableName = string.Empty;

        }
        EditorGUILayout.EndHorizontal();

        List<string> groups = new List<string>();
        
        groups.Add("<none>");

        groups.AddRange(CVarSystem.GetGroups().Select((e) => e.Name));
        //groups[0] = "<none>";

        int currentIndex = CurrentGroup != null ? Array.IndexOf(groups.ToArray(), CurrentGroup.Name) : 0;
        //////talvez nesta parte aproveitarei algo para copiar o template
        int newIndex = EditorGUILayout.Popup("Copy From", currentIndex, groups.ToArray());
        
        if(newIndex != currentIndex)
        {
            //CurrentGroup?.Unload();

            CurrentGroup = CVarSystem.GetGroupByName(groups[newIndex]);

            //CurrentGroup?.Load();
        }

        /*EditorGUILayout.BeginHorizontal();
        GUILayout.Button("Override");
        GUILayout.Button("RevertAll");
        EditorGUILayout.EndHorizontal();*/

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        float i_height = 2f;

        Rect rect = EditorGUILayout.GetControlRect(false, i_height);

        rect.height = i_height;

        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1));
    }

    /*private void DrawGroupBar()
    {

    }*/

    /*private void DrawPathField(string label, string defaultPath, string editablePath)
    {
        EditorGUILayout.BeginHorizontal();

        //EditorGUILayout.TextField(Application.streamingAssetsPath);
        EditorGUILayout.LabelField(label, GUILayout.Width(130));
        EditorGUILayout.LabelField(defaultPath+ " + ");
        EditorGUILayout.TextField(editablePath);
        //GUILayout.Button(new GUIContent("E", Path), GUILayout.Width(19), GUILayout.Height(19));
        DefaultButton("E", "Editar Caminho");
        DefaultButton("-", "Deletar Arquivo");
        //GUILayout.Box(_logo, GUIStyle.none, GUILayout.Width(80), GUILayout.Height(80));
        //GUILayout.Button(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(40), GUILayout.Height(40));
        //GUILayout.Button(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(40), GUILayout.Height(40));
        //GUILayout.Button(EditorGUIUtility.IconContent("Collab.FileDeleted"), GUILayout.Width(40), GUILayout.Height(40));
        //GUILayout.Button(EditorGUIUtility.IconContent("Collab.FileDeleted"), GUILayout.Width(40), GUILayout.Height(40));
        EditorGUILayout.EndHorizontal();
    }*/

    private void DrawSearchAndAddHeader()
    {
        EditorGUILayout.LabelField("Vars");

        GUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        GUIStyle searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
        //searchStyle.fixedHeight = 15;
        _searchString = EditorGUILayout.TextField(_searchString, searchStyle);
        
        // start the process to create a new var
        
        if (DefaultButton("+", "Add New Var"))
        {
            CreateMenu();
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();
    }

    private void CreateMenu()
    {
        GenericMenu menu = new GenericMenu();

        if (_selectedType == SELECTED_ALL_INDEX || _selectedType == 0)
        {
            menu.AddItem(new GUIContent("string"), false, OnAddNewClickedHandler<string>, GetNewDefaultValue<string>());
        }
        if (_selectedType == SELECTED_ALL_INDEX || _selectedType == 1)
        {
            menu.AddItem(new GUIContent("int"), false, OnAddNewClickedHandler<int>, GetNewDefaultValue<int>());
        }
        if (_selectedType == SELECTED_ALL_INDEX || _selectedType == 2)
        {
            menu.AddItem(new GUIContent("float"), false, OnAddNewClickedHandler<float>, GetNewDefaultValue<float>());
        }
        if (_selectedType == SELECTED_ALL_INDEX || _selectedType == 3)
        {
            menu.AddItem(new GUIContent("bool"), false, OnAddNewClickedHandler<bool>, GetNewDefaultValue<bool>());
        }
        if (_selectedType == SELECTED_ALL_INDEX || _selectedType == 4)
        {
            menu.AddItem(new GUIContent("vector3"), false, OnAddNewClickedHandler<Vector3>, GetNewDefaultValue<Vector3>());
        }

        menu.ShowAsContext();
    }

    /*private void OnAddNewGroupHandler()
    {
        CVarSystem.CreateGroup("");
    }*/


    private void OnAddNewClickedHandler<T>(object value)
    {
        _editableAuxName = GetNewDefaultName<T>(CurrentGroup.Name);
        _editableAuxValue = value;
        _persistentAux = false;
    }

    private void DrawAbas()
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        GUILayout.BeginHorizontal();

        _selectedType = EditorGUILayout.Popup(_selectedType, new string[] { "string", "int", "float", "bool", "vector3", "", "all" });
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        _showAddress = EditorGUILayout.ToggleLeft("Show Var Address", _showAddress);
        EditorGUILayout.Space();
        EditorGUI.EndDisabledGroup();
    }
    Vector2 _scrollPosition = Vector2.zero;

    public void DrawVars()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawHeaderFooterAndVars<string>("String Vars", _selectedType, 0);
        DrawHeaderFooterAndVars<int>("Int Vars", _selectedType, 1);
        DrawHeaderFooterAndVars<float>("Float Vars", _selectedType, 2);
        DrawHeaderFooterAndVars<bool>("Boolean Vars", _selectedType, 3);
        DrawHeaderFooterAndVars<Vector3>("Vector3 Vars", _selectedType, 4);

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeaderFooterAndVars<T>(string title, int selectedType, int index)
    {
        // header
        if (selectedType == SELECTED_ALL_INDEX || _selectedType == index)
        {
            _fouldout[index] = EditorGUILayout.Foldout(_fouldout[index], title);
            
            // end header
            if (_fouldout[index])
            {
                // vars
                DrawVars<T>();

                //end vars
                // footer
                DrawVarFooter<T>();

                //end footer
            }
        }
    }

    private void DrawVarFooter<T>()
    {
        // footer
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        if (DefaultButton("+", "Add New Var"))
        {
            OnAddNewClickedHandler<T>(GetNewDefaultValue<T>());
            _currentAction = CVarWindowAction.CREATE_VAR;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    private bool DefaultButton(string label, string tooltip)
    {
        return GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(19));
    }

    string _editableType;

    

    private void DrawVars<T>()
    {
        if (CurrentGroup == null)
            return;

        string[] names = CVarSystem.GetVarNamesByType<T>(CurrentGroup.Name);

        // if was creating a new variable
        // disable all
        EditorGUI.BeginDisabledGroup(_currentAction == CVarWindowAction.CREATE_GROUP || _currentAction == CVarWindowAction.CREATE_VAR);
        foreach (string n in names)
        {
            // search logic
            if (_searchString == string.Empty || n.Contains(_searchString))
            {
                if (_currentAction != CVarWindowAction.EDIT_VAR_NAME || (n != _editableName || _editableType != typeof(T).Name))
                {
                    DrawEditableVarValue<T>(n);
                }
                else
                    DrawEditableVarName<T>(n);
            }
        }
        EditorGUI.EndDisabledGroup();

        DrawAddNewVarInfo<T>();
    }
    
    private void DrawEditableVarValue<T>(string varName)
    {
        T value = CVarSystem.GetValue<T>(varName, default, CurrentGroup.Name);
        T aux;
        GUILayout.BeginHorizontal();

        if (CVarSystem.CanLoadRuntimePersistent)
        {
            if (CVarSystem.GetPersistent<T>(varName, CurrentGroup.Name))
            {
                GUI.backgroundColor = Color.red;
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
            }
        }
        else if (CVarSystem.CanLoadRuntimeDefault)
            GUI.backgroundColor = Color.cyan;

        DrawLocked<T>(varName);
        DrawPersistentField<T>(varName);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField(varName);
        EditorGUI.EndDisabledGroup();
        
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES || CVarSystem.GetLocked<T>(varName, CurrentGroup.Name));
        if (!_showAddress)
        {
            aux = (T)DrawFieldByType(value);

            if (aux != null && !aux.Equals(value))
            {
                CVarSystem.SetValue<T>(varName, aux, CurrentGroup.Name);
                //repaint inspector
            }
            if (DefaultButton("E", "Edit Var Name"))
            {
                _editableAuxName = _editableName = varName;
                _editableType = typeof(T).Name;
                _currentAction = CVarWindowAction.EDIT_VAR_NAME;
            }
            if (DefaultButton("-", "Delete Var") && EditorUtility.DisplayDialog("Delete Var", "Want delete var?", "Delete", "Cancel"))
            {
                CVarSystem.RemoveVar<T>(varName, CurrentGroup.Name);
                //repaint inspector
            }
        }
        else
        {
            EditorGUILayout.LabelField(" Address: "+CVarSystem.GetAddress<T>(varName, CurrentGroup.Name).ToString(), GUILayout.Height(19));
        }
        
                
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
    }

    private void DrawPersistentField<T>(string varName)
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES || CVarSystem.GetLocked<T>(varName));
        CVarSystem.SetPersistent<T>(varName, EditorGUILayout.Toggle(CVarSystem.GetPersistent<T>(varName, CurrentGroup.Name), GUILayout.MaxWidth(15)), CurrentGroup.Name);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLocked<T>(string name)
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        CVarSystem.SetLocked<T>(name, EditorGUILayout.Toggle(CVarSystem.GetLocked<T>(name, CurrentGroup.Name), "IN LockButton", GUILayout.MaxWidth(15)), CurrentGroup.Name);
        EditorGUI.EndDisabledGroup();
    }

    private object DrawFieldByType(object value)
    {
        if (value is string)
            return EditorGUILayout.TextField((string)value);
        else if (value is int)
            return EditorGUILayout.IntField((int)value);
        else if (value is float)
            return EditorGUILayout.FloatField((float)value);
        else if (value is bool)
            return EditorGUILayout.Toggle((bool)value);
        else if (value is Vector3)
            return EditorGUILayout.Vector3Field("", (Vector3)value);

        return null;
    }

    private static object GetNewDefaultValue<T>()
    {
        if (typeof(T) == typeof(string))
            return "new_string";
        else if (typeof(T) == typeof(int))
            return 0;
        else if (typeof(T) == typeof(float))
            return 0.0f;
        else if (typeof(T) == typeof(bool))
            return false;
        else if (typeof(T) == typeof(Vector3))
            return Vector3.zero;

        return null;
    }

    private static string GetNewDefaultName<T>(string currentGroup)
    {
        if (typeof(T) == typeof(string))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(currentGroup), "new_string");
        else if (typeof(T) == typeof(int))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(currentGroup), "new_int");
        else if (typeof(T) == typeof(float))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(currentGroup), "new_float");
        else if (typeof(T) == typeof(bool))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(currentGroup), "new_bool");
        else if (typeof(T) == typeof(Vector3))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(currentGroup), "new_vector3");

        return null;
    }

    private void DrawEditableVarName<T>(string varName)
    {
        T value = CVarSystem.GetValue<T>(varName, default, CurrentGroup.Name);
        
        GUILayout.BeginHorizontal();

        DrawLocked<T>(varName);
        DrawPersistentField<T>(varName);

        _editableAuxName = EditorGUILayout.TextField(_editableAuxName);
        
        EditorGUI.BeginDisabledGroup(true);
        DrawFieldByType(value);
        EditorGUI.EndDisabledGroup();

        if (DefaultButton("V", "Save"))
        {
            if (_editableAuxName != varName)
            {
                if (!CVarSystem.ContainsVar<T>(_editableAuxName, CurrentGroup.Name))
                {
                    //CVarSystem.RemoveVar<T>(varName);
                    //CVarSystem.SetValue<T>(_editableAuxName, value);
                    CVarSystem.RenameVar<T>(varName, _editableAuxName, CurrentGroup.Name);

                    _editableName = string.Empty;
                    _editableAuxName = string.Empty;
                    _editableType = string.Empty;
                    _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                }
                else
                {
                    Debug.LogWarning(string.Concat("The CVar ", _editableAuxName, " already exist! Try another one."));
                }

                //repaint inspector
            }
            else
            {
                _editableName = string.Empty;
                _editableAuxName = string.Empty;
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
            }
        }
        if (DefaultButton("X", "Cancel"))
        {
            _editableName = string.Empty;
            _editableAuxName = string.Empty;
            EditorGUI.FocusTextInControl("");
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawNewVar<T>()
    {
        GUILayout.BeginHorizontal();

        style = "IN LockButton";

        _lockedAux = EditorGUILayout.Toggle(_lockedAux, style, GUILayout.MaxWidth(15));

        _persistentAux = EditorGUILayout.Toggle(_persistentAux, GUILayout.MaxWidth(15));
        
        _editableAuxName = EditorGUILayout.TextField(_editableAuxName);
        T auxValue = (T)DrawFieldByType(_editableAuxValue);//EditorGUILayout.TextField(_editableAuxValue);
        
        if (DefaultButton("V", "Save"))
        {
            if (!CVarSystem.ContainsVar<T>(_editableAuxName, CurrentGroup.Name))
            {
                CVarSystem.SetValue<T>(_editableAuxName, auxValue, CurrentGroup.Name);
                CVarSystem.SetPersistent<T>(_editableAuxName, _persistentAux, CurrentGroup.Name);
                CVarSystem.SetLocked<T>(_editableAuxName, _lockedAux, CurrentGroup.Name);

                _editableAuxName = string.Empty;
                _editableAuxValue = null;
                _persistentAux = false;
                _lockedAux = false;
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
            }
            else
            {

                Debug.LogWarning(string.Concat("The CVar ", _editableAuxName, " already exist! Try another one."));
                _editableAuxValue = auxValue;
            }
            
            //repaint inspector
        }
        else
        {
            _editableAuxValue = auxValue;
        }
        if (DefaultButton("X", "Cancel"))
        {
            _editableAuxName = string.Empty;
            _editableAuxValue = null;
            _persistentAux = false;
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawAddNewVarInfo<T>()
    {
        if (_currentAction == CVarWindowAction.CREATE_VAR && typeof(T) == _editableAuxValue.GetType())
        {
            DrawNewVar<T>();
        }
    }
}
