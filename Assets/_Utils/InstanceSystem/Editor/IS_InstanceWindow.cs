using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using UnityEngine.SceneManagement;

namespace Util.InstanceSystem.Editor
{
    public class IS_InstanceWindow : EditorWindow
    {
        private readonly string UNDEFINED = "undefine";

        private bool[] _opened;
        private bool _showAll = false;
        private string _searchString = string.Empty;
        private Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("HLab/Instance")]
        public static void ShowWindow()
        {
            //IS_MonoInstance.Create();
            EditorWindow editorWindow = GetWindow(typeof(IS_InstanceWindow), false, "Instance");
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.minSize = new Vector2(250, editorWindow.minSize.y);
            editorWindow.Show();
            //EditorApplication.hierarchyChanged += HierarchyWindowChangedHandler;
        }

        [MenuItem("HLab/Clear")]
        public static void ClearRuntimeTable()
        {
            IS_InstanceManager.Clear();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformedHandler;
            Undo.undoRedoPerformed += OnUndoRedoPerformedHandler;
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

        void OnSelectionChange()
        {
            // m_selectedObj = GetSelection();
            Repaint();
        }

        void OnGUI()
        {
            if (IS_InstanceSceneManager.Instances.Count == 0)
            {
                return;
            }

            //DrawDragAndDropBox();

            DrawAbas();

            DrawInstanceManagerCreateNewBox();


            if (!_showAll)
            {
                DrawSelectedInstances();
            }//close not show all
            else
            {
                DrawAllInstances();

            }//close show all

            //DrawDragAndDropBox();

        }//Close on GUI

        private void DrawAbas()
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!_showAll);
            if (GUILayout.Button("Selected"))
            {
                _showAll = false;
                _scrollPosition = Vector2.zero;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_showAll);
            if (GUILayout.Button("All"))
            {
                _showAll = true;
                _scrollPosition = Vector2.zero;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void DrawInstanceManagerCreateNewBox()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)//for each scene loaded
            {
                //Check if there is some SceneManager inside the scene
                if (IS_InstanceSceneManager.Instances.Count == 0 || IS_InstanceSceneManager.Instances.FindIndex(iManager => iManager.gameObject.scene == SceneManager.GetSceneAt(i)) == -1)
                {
                    EditorGUILayout.HelpBox("IS_InstanceSceneManager not Found in " + SceneManager.GetSceneAt(i).name + " Scene", MessageType.Warning);
                    if (GUILayout.Button("Create"))
                    {
                        GameObject obj = new GameObject("InstanceSceneManager", typeof(IS_InstanceSceneManager));
                        Undo.RegisterCreatedObjectUndo(obj, "Create IS_InstanceSceneManager.GameObject");
                        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneAt(i));
                    }
                }
            }
        }

        private void DrawSelectedInstances()
        {
            string lastName = string.Empty;
            string aux = string.Empty;

            Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel);
            if (_opened == null || _opened.Length != transforms.Length)
            {
                _opened = new bool[transforms.Length];
            }

            if (transforms.Length > 0)
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                //EditorGUILayout.BeginVertical();
                int i = 0;
                foreach (Transform t in transforms)
                {
                    if (!t.gameObject.scene.IsValid() || IS_InstanceSceneManager.Instances.FindIndex(iManager => iManager.gameObject.scene == t.gameObject.scene) != -1)
                    {
                        lastName = aux = t.gameObject.GetEditorInstanceName();

                        if (aux != string.Empty)
                            _opened[i] = !EditorGUILayout.Foldout(!_opened[i], string.Concat(t.name, " (", t.gameObject.GetEditorInstanceName(false), ")"), EditorStyles.foldout);
                        else
                            _opened[i] = !EditorGUILayout.Foldout(!_opened[i], string.Concat(t.name, " (", t.gameObject.scene.name + ".<< " + UNDEFINED, " >>)"), EditorStyles.foldout);
                        //Mup.InstanceSystem.IS_MonoInstance.AddInstance(t.gameObject, "");
                        if (!_opened[i])
                        {
                            aux = EditorGUILayout.TextField("GameObject Id", aux);

                            if (lastName != aux)
                            {
                                //Undo.RecordObject(_iManager, "Rename instance");
                                t.gameObject.SetEditorInstanceName(aux);
                            }

                            Component[] components = t.GetComponents<Component>();

                            foreach (Component c in components)
                            {
                                if (c != null)
                                {
                                    lastName = aux = c.GetEditorInstanceName();

                                    aux = EditorGUILayout.TextField(string.Concat(c.GetType().Name, " Id"), aux);

                                    if (aux != lastName)
                                    {
                                        //Undo.RecordObject(_iManager, "Rename instance");
                                        c.SetEditorInstanceName(aux);
                                    }
                                }

                                //list.DoLayoutList();
                            }
                        }
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                        i++;
                    }
                    else
                        EditorGUILayout.HelpBox("Cant edit " + t.name + " because " + t.gameObject.scene.name + " Scene doesnt contains IS_InstanceSceneManager", MessageType.Info);
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a scene GameObject!", MessageType.Info);
            }
        }

        private void DrawAllInstances()
        {
            string lastName;
            string aux;
            int count = 0;
            int openedIndex = 0;
            GUIStyle searchStyle;
            int deleteIndex;

            if (IS_InstanceSceneManager.Instances.Count > 0)
            {
                searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
                searchStyle.fixedHeight = 15;
                _searchString = EditorGUILayout.TextField(_searchString, searchStyle);

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                if (_opened == null || _opened.Length != IS_InstanceSceneManager.Instances.Count)
                {
                    _opened = new bool[IS_InstanceSceneManager.Instances.Count];
                }

                foreach (IS_InstanceSceneManager iManager in IS_InstanceSceneManager.Instances)
                {
                    if (iManager.Keys.Count > 0)
                    {
                        _opened[openedIndex] = !EditorGUILayout.Foldout(!_opened[openedIndex], iManager.gameObject.scene.name, EditorStyles.foldout);
                        count++;
                        if (!_opened[openedIndex])
                        {
                            deleteIndex = -1;
                            for (int i = 0; i < iManager.Keys.Count; i++)
                            {
                                if (_searchString.Length == 0 || (iManager.Keys[i] != null && iManager.Values[i].Contains(_searchString)))
                                {
                                    if (iManager.Keys[i] != null)
                                    {
                                        EditorGUILayout.SelectableLabel(string.Concat("Full Name: ", iManager.Values[i]));
                                    }//close iManager.Keys[i] != null
                                    else//if iManager.Keys[i] == null
                                    {
                                        //draw error box

                                        //GUI.color = Color.yellow;
                                        EditorGUILayout.BeginHorizontal();
                                        //https://unitylist.com/p/5c3/Unity-editor-icons
                                        //EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon.sml"), GUILayout.MaxWidth(18));
                                        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.erroricon.sml"), GUILayout.MaxWidth(18));
                                        GUI.contentColor = new Color(0.9f, 0.2f, 0.2f);
                                        EditorGUILayout.SelectableLabel(String.Concat(iManager.Values[i], " - [Set the instance or delete key using (-) button!]"));
                                        EditorGUILayout.EndHorizontal();
                                        GUI.contentColor = Color.white;

                                        //end draw erro box
                                    }//close else iManager.Keys == null

                                    EditorGUILayout.BeginHorizontal();
                                    UnityEngine.Object obj;
                                    if (iManager.Keys[i])
                                    {
                                        obj = EditorGUILayout.ObjectField(iManager.Keys[i], iManager.Keys[i].GetType(), true);
                                    }//keys != null
                                    else//keys == null
                                    {
                                        //GUI.contentColor = Color.red;
                                        GUI.backgroundColor = Color.red;
                                        obj = EditorGUILayout.ObjectField(null, typeof(UnityEngine.Object), true);
                                        GUI.contentColor = Color.grey;
                                    }//close else keys == null

                                    if (obj != null && !iManager.ContainsKey(obj))
                                    {
                                        Undo.RecordObject(iManager, "Instance add");
                                        iManager.Keys[i] = obj;

                                        //obj.SetEditorInstanceName(obj.GetEditorInstanceName());
                                    }//close containsKey
                                    else if (obj != iManager.Keys[i])
                                        Debug.LogWarning("There is another instance name for this object!");

                                    lastName = IS_InstanceManager.RemoveOrigenPrefix(iManager.Values[i]);

                                    EditorGUI.BeginDisabledGroup(iManager.Keys[i] == null);

                                    aux = EditorGUILayout.TextField(lastName);
                                    EditorGUI.EndDisabledGroup();

                                    //if name change
                                    if (aux != lastName)
                                    {
                                        //Undo.RecordObject(_iManager, "Rename instance");
                                        iManager.Keys[i].SetEditorInstanceName(aux);
                                    }//close name change

                                    GUI.backgroundColor = Color.white;
                                    GUI.contentColor = Color.white;

                                    if (GUILayout.Button("-"))
                                    {
                                        deleteIndex = i;
                                    }//close if delete button

                                    EditorGUI.BeginDisabledGroup(iManager.Keys[i] == null);
                                    if (GUILayout.Button("S"))
                                    {
                                        Selection.activeObject = iManager.Keys[i];
                                        _showAll = false;
                                    }//close select button
                                    EditorGUI.EndDisabledGroup();

                                    EditorGUILayout.EndHorizontal();
                                }//close search

                            }//close for iManager.Keys
                            if (deleteIndex != -1)//if clicked to delete something
                            {
                                Undo.RecordObject(iManager, "Remove NUll Instance");
                                iManager.RemoveAt(deleteIndex);
                            }//close delete
                        }//close if opened

                    }//close iManager.Keys.Count
                    openedIndex++;
                }//close foreach iManager in Instances
                EditorGUILayout.EndScrollView();
            }//close if instance count > 0

            if (count == 0)
            {
                EditorGUILayout.HelpBox("Empty List!\nSelect a GameObject, click [Selected] Aba and have fun!", MessageType.Info);
            }
        }

        string _boxText = "Drag and Drop your objects here to give then a instance name!";

        private void DrawDragAndDropBox()
        {
            GUIStyle GuistyleBoxDND = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
                fontSize = 12
            };

            GUI.skin.box = GuistyleBoxDND;
            //GuistyleBoxDND.normal.background = MakeTex(2, 2, Color.white);
            Rect myRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 8f, GUILayout.ExpandWidth(true));
            GUI.Box(myRect, _boxText, GuistyleBoxDND);

            if (myRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    _boxText = "Drop The Objects";
                    Event.current.Use();
                }//close if Event.current.type == EventType.DragUpdated
                else if (Event.current.type == EventType.DragPerform)
                {
                    _boxText = "Drag and Drop your objects here to give then a instance name!";
                    Debug.Log(DragAndDrop.objectReferences.Length);
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        //myTarget.m_GameObjectGroups[groupIndex].Add(DragAndDrop.objectReferences[i] as GameObject);
                    }
                    Event.current.Use();
                }//close else if (Event.current.type == EventType.DragPerform)
            }//close myRect.Contains
            else if (DragAndDrop.objectReferences.Length == 0)
            {
                _boxText = "Drag and Drop your objects here to give then a instance name!";
            }//close else
        }//close DrawDragAndDropBox
    }//Close class
}