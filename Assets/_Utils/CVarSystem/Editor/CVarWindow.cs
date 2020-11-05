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
    private GUIStyle style;

    private CVarWindowAction _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

    public string CurrentGroupName { get;set; } = "global";

    [MenuItem("HLab/CVar")]
    public static void ShowWindow()
    {
        CVarWindow editorWindow = CreateWindow<CVarWindow>("CVar");//GetWindow(typeof(CVarWindow), false, "CVar");
        ConfigureWindow(editorWindow);
    }

    public static void SelectWindow(string currentGroupName)
    {
        CVarWindow editorWindow = GetWindow<CVarWindow>(false, "CVar", true);
        editorWindow.CurrentGroupName = currentGroupName;
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
        if(CVarSystem.GetGroup(CurrentGroupName) == null)
        {
            CurrentGroupName = "global";
        }

        DrawTopBar();
        if (_currentAction != CVarWindowAction.CREATE_GROUP)
            DrawGroupOptions();
        else
            DrawNewGroupOptions();

        DrawSearchAndAddHeader();
        DrawAbas();
        DrawVars();
    }//Close on GUI

    private string _currentGroupAux = string.Empty;

    //Texture2D _logo;
    private void DrawTopBar()
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        EditorGUI.DrawRect(new Rect(0, EditorGUIUtility.singleLineHeight*1.5f, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.2f, 0.2f, 0.2f));
        EditorGUILayout.BeginHorizontal();
        
        if(DefaultButton("+", "Criar Grupo"))
        {
            _editableName = ObjectNamesManager.GetUniqueName(CVarSystem.GetGroups().Select((CVarGroup g)=>g.Name).ToArray(),"new_group");
            _editableAuxName = "undefined";
            _currentAction = CVarWindowAction.CREATE_GROUP;
            _currentGroupAux = CurrentGroupName;
            CurrentGroupName = "<none>";
        }

        if(GUILayout.Button(CVarSystem.IsEditModeActived?"Disable Edit Mode": "Enable Edit Mode"))
        {
            CVarSystem.ActiveEditMode(!CVarSystem.IsEditModeActived);
        }

        if (GUILayout.Button("Reload"))
        {
            CVarSystem.Reload();
        }

        if (GUILayout.Button("Copy Default to Pers"))
        {
            CVarSystem.CopyDefaultFilesToPersistentFolder();
        }

        if (GUILayout.Button("Open persistent folder"))
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

    }

    private void DrawFileManager()
    {
        EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived);

        EditorGUILayout.BeginHorizontal();

        bool boolAux = CVarSystem.CanLoadRuntimeDefault;
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        GUI.backgroundColor = Color.cyan;
        boolAux = EditorGUILayout.ToggleLeft("Show Runtime Default", CVarSystem.CanLoadRuntimeDefault);
        GUI.backgroundColor = Color.white;
        if (boolAux != CVarSystem.CanLoadRuntimeDefault)
        {
            CVarSystem.UnloadGroups();
            // change state
            CVarSystem.CanLoadRuntimeDefault = boolAux;
            CVarSystem.LoadGroups(false);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
        if (CVarSystem.CanLoadRuntimeDefault)
        {
            if (GUILayout.Button("Overwrite editor default") && EditorUtility.DisplayDialog("Overwrite default", "Want ovewrite default file?", "Ovewrite", "Cancel"))
            {
                CVarSystem.GetGroup(CurrentGroupName).Unload();
                M_XMLFileManager.Copy
                (
                    CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(CVarSystem.GetGroup(CurrentGroupName).Name, ".xml")),
                    CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(CVarSystem.GetGroup(CurrentGroupName).Name, ".xml")),
                    /*   //System.IO.Path.Combine(Application.streamingAssetsPath, "Data", string.Concat(group.Name, ".xml")),
                       file,
                       //System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default", string.Concat(group.Name, ".xml"))
                       CVarSystem.ParsePersistentDefaultDataPathWith(System.IO.Path.GetFileName(file)),*/
                    true
                );
                CVarSystem.GetGroup(CurrentGroupName).Load(false);
            }
        }
        else 
        {
            if (GUILayout.Button("Overwrite runtime default") && EditorUtility.DisplayDialog("Overwrite default", "Want ovewrite default file?", "Ovewrite", "Cancel"))
            {
                CVarSystem.GetGroup(CurrentGroupName).Unload();
                M_XMLFileManager.Copy
                (
                    CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(CVarSystem.GetGroup(CurrentGroupName).Name, ".xml")),
                    CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(CVarSystem.GetGroup(CurrentGroupName).Name, ".xml")),
                    /*   //System.IO.Path.Combine(Application.streamingAssetsPath, "Data", string.Concat(group.Name, ".xml")),
                       file,
                       //System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default", string.Concat(group.Name, ".xml"))
                       CVarSystem.ParsePersistentDefaultDataPathWith(System.IO.Path.GetFileName(file)),*/
                    true
                );
                CVarSystem.GetGroup(CurrentGroupName).Load(false);
            }
        }

        EditorGUILayout.EndHorizontal();
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
        }

        if (CVarSystem.GetGroup(CurrentGroupName).PersistentType != CVarGroupPersistentType.SHARED)
        {
            if (_currentAction != CVarWindowAction.CREATE_PERSISTENT_FILE && _currentAction != CVarWindowAction.RENAME_PERSISTENT_FILE)
            {
                // positive lookbehind ?<= ignore the \[ and get all between \]_ in a literal way
                Regex pattern = new Regex(@"(?<=\[)(.*?)(?=\]_)");

                string[] files = CVarSystem.ListAllPersistentFilesNameByGroup(CurrentGroupName);

                EditorGUI.BeginDisabledGroup(!CVarSystem.CanLoadRuntimePersistent);
                CVarGroup g = CVarSystem.GetGroup(CurrentGroupName);

                if (files.Length > 0)
                {
                    int id = Array.IndexOf(files, pattern.Match(g.PersistentPrefix).Value);//string.Format(g.PersistentPrefix.Replace("[", string.Empty).Replace("]", string.Empty).Replace("_", string.Empty)));
                    int currentId = id;

                    if (id >= 0 && id < files.Length)
                    {
                        currentId = EditorGUILayout.Popup(id, files);

                        if (id != currentId)
                        {
                            g.Unload();
                            g.PersistentPrefix = string.Format("[{0}]_", files[currentId]);
                            g.Load(false);
                        }
                    }
                    else
                    {

                        //g.PersistentPrefix = files[0];
                        g.Unload();
                        g.PersistentPrefix = string.Format("[{0}]_", files[EditorGUILayout.Popup(0, files)]);
                        g.Load(false);
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
                    g.ResetToDefault();
                }

                if (GUILayout.Button("E"))
                {
                    //rename
                    _currentAction = CVarWindowAction.RENAME_PERSISTENT_FILE;
                    _editableAuxName = pattern.Match(g.PersistentPrefix).Value;
                    /*g.ResetToDefault();
                    CVarSystem.GetGroup(_currentGroup).Save();
                    CVarSystem.GetGroup(_currentGroup).FlushPersistent();*/
                }
                if (GUILayout.Button("R") && EditorUtility.DisplayDialog("Reset persistent", "Want reset persistent file to default?", "Reset", "Cancel"))
                {
                    g.ResetToDefault();
                    CVarSystem.GetGroup(CurrentGroupName).Save();
                    CVarSystem.GetGroup(CurrentGroupName).FlushPersistent();
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
                    if (_editableAuxName.Replace(" ", "").Length > 0 && !System.IO.File.Exists(CVarGroup.GetPersistentFilePath(CurrentGroupName, CVarGroup.ParsePrefixName(_editableAuxName))))
                    {
                        if (_currentAction == CVarWindowAction.CREATE_PERSISTENT_FILE && CVarSystem.GetGroup(CurrentGroupName).PersistentPrefix.Length > 0)
                        {
                            // save the current file
                            CVarSystem.GetGroup(CurrentGroupName).Save();
                            CVarSystem.GetGroup(CurrentGroupName).FlushPersistent();
                        }
                        else if (_currentAction == CVarWindowAction.RENAME_PERSISTENT_FILE)
                        {
                            // delete the current file
                            CVarSystem.GetGroup(CurrentGroupName).DeletePersistentFile();
                        }

                        // set the new persistent prefix
                        CVarSystem.GetGroup(CurrentGroupName).SetGroupPersistentPrefix(_editableAuxName, CVarSystem.GetGroup(CurrentGroupName).PersistentType);
                        // save the new persistent file
                        CVarSystem.GetGroup(CurrentGroupName).Save();
                        // force create file
                        CVarSystem.GetGroup(CurrentGroupName).FlushPersistent();

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
            if (!System.IO.File.Exists(CVarGroup.GetPersistentFilePath(CurrentGroupName, string.Empty)))
            {
                if (GUILayout.Button("Create File"))
                {
                    CVarSystem.GetGroup(CurrentGroupName).Save();
                    CVarSystem.GetGroup(CurrentGroupName).FlushPersistent();
                    EditorUtility.DisplayDialog("Success", "Group Created", "Continue");
                }
            }
            else
            {
                if (GUILayout.Button("Delete File") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                {
                    CVarSystem.GetGroup(CurrentGroupName).ResetToDefault();
                }
                if (GUILayout.Button("Reset File") && EditorUtility.DisplayDialog("Reset persistent", "Want reset persistent file?", "Reset", "Cancel"))
                {
                    CVarSystem.GetGroup(CurrentGroupName).ResetToDefault();
                    CVarSystem.GetGroup(CurrentGroupName).Save();
                    CVarSystem.GetGroup(CurrentGroupName).FlushPersistent();
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

        string[] groups = CVarSystem.GetGroups().Select((e)=>e.Name).ToArray();//new string[] { "global", "group01", "group02", "group03" };

        if (_currentAction != CVarWindowAction.EDIT_GROUP)
        {
            int index = Array.IndexOf(groups, CurrentGroupName);
            int newIndex = EditorGUILayout.Popup("Group", index, groups);

            if(index != newIndex)
            {
                // change group
                CurrentGroupName = groups[newIndex];
                CVarSystem.GetGroup(CurrentGroupName)?.Load();   
            }

            EditorGUI.BeginDisabledGroup(CurrentGroupName == "global");
            if (DefaultButton("E", "Edit Group"))
            {
                _currentAction = CVarWindowAction.EDIT_GROUP;
                _editableName = CurrentGroupName;
                
            }
            if (DefaultButton("-", "Delete Group") && EditorUtility.DisplayDialog("Delete Var", "Want delete group?", "Delete", "Cancel"))
            {
                CVarSystem.RemoveGroup(CurrentGroupName);
                CurrentGroupName = "global";
            }
            EditorGUILayout.EndHorizontal();
            CVarSystem.GetGroup(CurrentGroupName).SetPersistentTypeAndSave((CVarGroupPersistentType)EditorGUILayout.EnumPopup(new GUIContent("Persistent Type"), CVarSystem.GetGroup(CurrentGroupName).PersistentType));


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
                CVarSystem.RenameGroup(CurrentGroupName, _editableName);
                CurrentGroupName = _editableName;

            }
            if (DefaultButton("X", "Cancel"))
            {
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup(new GUIContent("Persistent Type"), CVarSystem.GetGroup(CurrentGroupName).PersistentType);
            EditorGUI.EndDisabledGroup();
        }

        groups[0] = "<none>";
                
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
            CVarSystem.GetGroup(_editableName)?.Load();
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

            //unload current if
            if (CurrentGroupName != _editableName && CVarSystem.TryGetGroup(CurrentGroupName, out CVarGroup loadedGroup))
            {
                foreach (CVarObject var in group.Vars)
                {
                    // clone
                    CVarSystem.CloneVarToGroup(var, group);
                }

                loadedGroup.Unload();
            }
            // set the current group to new group
            CurrentGroupName = _editableName;
            _editableName = string.Empty;
        }

        if(DefaultButton("X", "Cancel"))
        {
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

            // set the current group to old group
            CurrentGroupName = _currentGroupAux;

            _editableName = string.Empty;

        }
        EditorGUILayout.EndHorizontal();

        string[] groups = CVarSystem.GetGroups().Select((e) => e.Name).ToArray();
        groups[0] = "<none>";

        //////talvez nesta parte aproveitarei algo para copiar o template
        int newIndex = EditorGUILayout.Popup("Copy From", Array.IndexOf(groups, CurrentGroupName), groups);
        
        if(newIndex != Array.IndexOf(groups, CurrentGroupName))
        {
            CVarSystem.GetGroup(CurrentGroupName)?.Unload();

            CurrentGroupName = groups[newIndex];

            CVarSystem.GetGroup(CurrentGroupName)?.Load();
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

    private void DrawGroupBar()
    {

    }

    private void DrawPathField(string label, string defaultPath, string editablePath)
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
    }

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
        _editableAuxName = GetNewDefaultName<T>(CurrentGroupName);
        _editableAuxValue = value;
        _persistentAux = false;
    }

    private void DrawAbas()
    {
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        _selectedType = EditorGUILayout.Popup(_selectedType, new string[] { "string", "int", "float", "bool", "vector3", "", "all" });
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
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
        string[] names = CVarSystem.GetVarNamesByType<T>(CurrentGroupName);

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
        T value = CVarSystem.GetValue<T>(varName, default, CurrentGroupName);
        T aux;
        GUILayout.BeginHorizontal();

        if (CVarSystem.CanLoadRuntimePersistent)
        {
            if (CVarSystem.GetPersistent<T>(varName, CurrentGroupName))
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
        
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES || CVarSystem.GetLocked<T>(varName, CurrentGroupName));
        aux = (T)DrawFieldByType(value);

        
        if (aux != null && !aux.Equals(value))
        {
            CVarSystem.SetValue<T>(varName, aux, CurrentGroupName);
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
            CVarSystem.RemoveVar<T>(varName, CurrentGroupName);
            //repaint inspector
        }
        
                
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
    }

    private void DrawPersistentField<T>(string varName)
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES || CVarSystem.GetLocked<T>(varName));
        CVarSystem.SetPersistent<T>(varName, EditorGUILayout.Toggle(CVarSystem.GetPersistent<T>(varName, CurrentGroupName), GUILayout.MaxWidth(15)), CurrentGroupName);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLocked<T>(string name)
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        CVarSystem.SetLocked<T>(name, EditorGUILayout.Toggle(CVarSystem.GetLocked<T>(name, CurrentGroupName), "IN LockButton", GUILayout.MaxWidth(15)), CurrentGroupName);
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
        T value = CVarSystem.GetValue<T>(varName, default, CurrentGroupName);
        
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
                if (!CVarSystem.ContainsVar<T>(_editableAuxName, CurrentGroupName))
                {
                    //CVarSystem.RemoveVar<T>(varName);
                    //CVarSystem.SetValue<T>(_editableAuxName, value);
                    CVarSystem.RenameVar<T>(varName, _editableAuxName, CurrentGroupName);

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
            if (!CVarSystem.ContainsVar<T>(_editableAuxName, CurrentGroupName))
            {
                CVarSystem.SetValue<T>(_editableAuxName, auxValue, CurrentGroupName);
                CVarSystem.SetPersistent<T>(_editableAuxName, _persistentAux, CurrentGroupName);
                CVarSystem.SetLocked<T>(_editableAuxName, _lockedAux, CurrentGroupName);

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
