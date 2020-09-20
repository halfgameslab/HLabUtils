using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

enum CVarWindowAction
{ 
    CREATE_GROUP,
    CREATE_VAR,
    EDIT_VAR_VALUES,
    EDIT_VAR_NAME,
    EDIT_GROUP,
}


public class CVarWindow : EditorWindow
{
    private const int SELECTED_ALL_INDEX = 5;

    private string _searchString = string.Empty;

    private string _editableName = string.Empty;
    private string _editableAuxName = string.Empty;
    private object _editableAuxValue;
    private int _selectedType = SELECTED_ALL_INDEX;
    private bool[] _fouldout = new bool[] { true, true, true, true };
    private bool _persistentAux = false;
    private bool _lockedAux = false;
    private GUIStyle style;

    private CVarWindowAction _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

    private static string _currentGroup = "global";

    [MenuItem("HLab/CVar")]
    public static void ShowWindow()
    {
        EditorWindow editorWindow = GetWindow(typeof(CVarWindow), false, "CVar");
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.minSize = new Vector2(250, editorWindow.minSize.y);
        editorWindow.Show();
        //EditorApplication.hierarchyChanged += HierarchyWindowChangedHandler;
    }

    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedoPerformedHandler;
        Undo.undoRedoPerformed += OnUndoRedoPerformedHandler;

        //_logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/HalfGamesLogoSymbol.png", typeof(Texture2D));

        //if (!CVarSystem.DataWasLoaded)
        //    CVarSystem.Load();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformedHandler;
        Undo.undoRedoPerformed -= OnUndoRedoPerformedHandler;
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
        DrawTopBar();
        if (_currentAction != CVarWindowAction.CREATE_GROUP)
            DrawGroupOptions();
        else
            DrawNewGroupOptions();

        DrawSearchAndAddHeader();
        DrawAbas();
        DrawVars();

        /*EditorGUILayout.BeginVertical();
        foreach (string k in CVarSystem.GetVarNamesByType<int>())
        {
            EditorGUILayout.LabelField(k+" "+CVarSystem.GetPersistent<int>(k));
        }
        EditorGUILayout.EndVertical();*/

        /*EditorGUILayout.BeginVertical();
        foreach(KeyValuePair<uint, string> k in CVarSystem.Address)
        {
            EditorGUILayout.LabelField(k.Key + " " + k.Value);
        }
        EditorGUILayout.EndVertical();*/

    }//Close on GUI

    private string _currentGroupAux = string.Empty;

    //Texture2D _logo;
    private void DrawTopBar()
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        EditorGUI.DrawRect(new Rect(0, EditorGUIUtility.singleLineHeight*1.5f, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.2f, 0.2f, 0.2f));
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.ColorField(Color.white , GUILayout.Width(18), GUILayout.Height(18));
        if(DefaultButton("+", "Criar Grupo"))
        {
            _editableName = ObjectNamesManager.GetUniqueName(CVarSystem.GetGroups().Select((CVarGroup g)=>g.Name).ToArray(),"new_group");
            _editableAuxName = "undefined";
            _currentAction = CVarWindowAction.CREATE_GROUP;
            _currentGroupAux = _currentGroup;
            _currentGroup = "<none>";
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        //GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true));

        /*DrawPathField("Default file path", "Application.streamingAssetsPath", "Data/cvar_data.xml");
        DrawPathField("Persist file path", "Application.persistentDataPath", "Data/cvar_data.xml");

        if (GUILayout.Button("Clear All"))
        {
            CVarSystem.Clear();
        }

        if (GUILayout.Button("Clear Persistent"))
        {
            CVarSystem.ClearPersistent();
        }

        EditorGUILayout.Space();*/
    }

    //private int _oldIndex;
    //private int _nIndex;

    private void DrawGroupOptions()
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_GROUP && _currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        string[] groups = CVarSystem.GetGroups().Select((e)=>e.Name).ToArray();//new string[] { "global", "group01", "group02", "group03" };

        if (_currentAction != CVarWindowAction.EDIT_GROUP)
        {
            int index = Array.IndexOf(groups, _currentGroup);
            int newIndex = EditorGUILayout.Popup("Group", index, groups);

            if(index != newIndex)
            {
                // change group
                _currentGroup = groups[newIndex];
                CVarSystem.LoadGroup(_currentGroup);   
            }

            EditorGUI.BeginDisabledGroup(_currentGroup == "global");
            if (DefaultButton("E", "Edit Group"))
            {
                _currentAction = CVarWindowAction.EDIT_GROUP;
                _editableName = _currentGroup;
                //////////_nIndex = _oldIndex = Mathf.Max(Array.IndexOf(groups, CVarSystem.GetGroup(_currentGroup).Base), 0);
            }
            if (DefaultButton("-", "Delete Group") && EditorUtility.DisplayDialog("Delete Var", "Want delete group?", "Delete", "Cancel"))
            {
                CVarSystem.RemoveGroup(_currentGroup);
                _currentGroup = "global";
            }
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            _editableName = EditorGUILayout.TextField("Group", _editableName);
            if (DefaultButton("V", "Save Changes"))
            {
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                CVarSystem.RenameGroup(_currentGroup, _editableName);
                _currentGroup = _editableName;

                /*if (_nIndex != _oldIndex)
                {
                    // change base
                    if(_nIndex != 0)
                        CVarSystem.SetGroupBase(_currentGroup, groups[_nIndex]);
                    else
                        CVarSystem.SetGroupBase(_currentGroup, string.Empty);
                }*/

                //_oldIndex = 0;
                //_nIndex = 0;
            }
            if (DefaultButton("X", "Cancel"))
            {
                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                //_oldIndex = 0;
                //_nIndex = 0;
            }

        }

        EditorGUI.EndDisabledGroup();

        // check if group change
        // unload current
        // load new

        //EditorGUI.BeginDisabledGroup(true);
        //EditorGUI.EndDisabledGroup();

        groups[0] = "<none>";

        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_GROUP);
        //_oldIndex = Mathf.Max(Array.IndexOf(groups, CVarSystem.GetGroup(_currentGroup).Base), 0);
        
        /*if (_currentAction != CVarWindowAction.EDIT_GROUP)
            EditorGUILayout.Popup("Base", Mathf.Max(Array.IndexOf(groups, CVarSystem.GetGroup(_currentGroup).Base), 0), groups);
        else
            _nIndex = EditorGUILayout.Popup("Base", _nIndex, groups);*/


        EditorGUI.EndDisabledGroup();

        //EditorGUILayout.BeginHorizontal();
        //GUILayout.Button("Override");
        //GUILayout.Button("RevertAll");
        //EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        float i_height = 2f;

        Rect rect = EditorGUILayout.GetControlRect(false, i_height);

        rect.height = i_height;

        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1));

        /*GUI.backgroundColor = Color.black;
        GUILayout.Box(GUIContent.none, GUILayout.Height(2), GUILayout.Width(EditorGUIUtility.currentViewWidth));
        GUI.backgroundColor = Color.white;*/
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
            CVarSystem.LoadGroup(_editableName);
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

            //unload current if
            if (_currentGroup != _editableName)
            {
                foreach (CVarObject var in CVarSystem.GetGroup(_currentGroup).Vars)
                {
                    // clone
                    CVarSystem.CloneVarToGroup(var, group);
                }

                CVarSystem.UnloadGroup(_currentGroup);
            }
            // set the current group to new group
            _currentGroup = _editableName;
            _editableName = string.Empty;
        }

        if(DefaultButton("X", "Cancel"))
        {
            _currentAction = CVarWindowAction.EDIT_VAR_VALUES;

            // set the current group to old group
            _currentGroup = _currentGroupAux;

            _editableName = string.Empty;

        }
        EditorGUILayout.EndHorizontal();

        string[] groups = CVarSystem.GetGroups().Select((e) => e.Name).ToArray();
        groups[0] = "<none>";

        //////talvez nesta parte aproveitarei algo para copiar o template
        int newIndex = EditorGUILayout.Popup("Copy From", Array.IndexOf(groups, _currentGroup), groups);
        
        if(newIndex != Array.IndexOf(groups, _currentGroup))
        {
            CVarSystem.UnloadGroup(_currentGroup);

            _currentGroup = groups[newIndex];

            CVarSystem.LoadGroup(_currentGroup);
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

        menu.ShowAsContext();
    }

    /*private void OnAddNewGroupHandler()
    {
        CVarSystem.CreateGroup("");
    }*/

    private void OnAddNewClickedHandler<T>(object value)
    {
        _editableAuxName = GetNewDefaultName<T>();
        _editableAuxValue = value;
        _persistentAux = false;
    }

    private void DrawAbas()
    {
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        _selectedType = EditorGUILayout.Popup(_selectedType, new string[] { "string", "int", "float", "bool", "", "all" });
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
        string[] names = CVarSystem.GetVarNamesByType<T>(_currentGroup);

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
        T value = CVarSystem.GetValue<T>(varName, default, _currentGroup);
        T aux;
        GUILayout.BeginHorizontal();

        DrawLocked<T>(varName);
        DrawPersistentField<T>(varName);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField(varName);
        EditorGUI.EndDisabledGroup();
        
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES || CVarSystem.GetLocked<T>(varName, _currentGroup));
        aux = (T)DrawFieldByType(value);//EditorGUILayout.TextField(value);

        
        if (aux != null && !aux.Equals(value))
        {
            CVarSystem.SetValue<T>(varName, aux, _currentGroup);
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
            CVarSystem.RemoveVar<T>(varName, _currentGroup);
            //repaint inspector
        }
        
                
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
    }

    private void DrawPersistentField<T>(string varName)
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES || CVarSystem.GetLocked<T>(varName));
        CVarSystem.SetPersistent<T>(varName, EditorGUILayout.Toggle(CVarSystem.GetPersistent<T>(varName, _currentGroup), GUILayout.MaxWidth(15)), _currentGroup);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLocked<T>(string name)
    {
        EditorGUI.BeginDisabledGroup(_currentAction != CVarWindowAction.EDIT_VAR_VALUES);
        CVarSystem.SetLocked<T>(name, EditorGUILayout.Toggle(CVarSystem.GetLocked<T>(name, _currentGroup), "IN LockButton", GUILayout.MaxWidth(15)), _currentGroup);
        EditorGUI.EndDisabledGroup();
    }

    /*private void DrawEditorVisibility<T>(string name)
    {
        EditorGUI.BeginDisabledGroup(_editableName.Length > 0);
        CVarSystem.SetLocked<T>(name, EditorGUILayout.Toggle(CVarSystem.GetLocked<T>(name), style, GUILayout.MaxWidth(15)));
        EditorGUI.EndDisabledGroup();
    }*/

    private object DrawFieldByType(object value)
    {
        if(value is string)
            return EditorGUILayout.TextField((string)value);
        else if (value is int)
            return EditorGUILayout.IntField((int)value);
        else if (value is float)
            return EditorGUILayout.FloatField((float)value);
        else if (value is bool)
            return EditorGUILayout.Toggle((bool)value);

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

        return null;
    }

    private static string GetNewDefaultName<T>()
    {
        if (typeof(T) == typeof(string))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(_currentGroup), "new_string");
        else if (typeof(T) == typeof(int))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(_currentGroup), "new_int");
        else if (typeof(T) == typeof(float))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(_currentGroup), "new_float");
        else if (typeof(T) == typeof(bool))
            return ObjectNamesManager.GetUniqueName(CVarSystem.GetVarNamesByType<T>(_currentGroup), "new_bool");

        return null;
    }

    private void DrawEditableVarName<T>(string varName)
    {
        T value = CVarSystem.GetValue<T>(varName, default, _currentGroup);
        
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
                if (!CVarSystem.ContainsVar<T>(_editableAuxName, _currentGroup))
                {
                    //CVarSystem.RemoveVar<T>(varName);
                    //CVarSystem.SetValue<T>(_editableAuxName, value);
                    CVarSystem.RenameVar<T>(varName, _editableAuxName, _currentGroup);

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
            if (!CVarSystem.ContainsVar<T>(_editableAuxName, _currentGroup))
            {
                CVarSystem.SetValue<T>(_editableAuxName, auxValue, _currentGroup);
                CVarSystem.SetPersistent<T>(_editableAuxName, _persistentAux, _currentGroup);
                CVarSystem.SetLocked<T>(_editableAuxName, _lockedAux, _currentGroup);

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
