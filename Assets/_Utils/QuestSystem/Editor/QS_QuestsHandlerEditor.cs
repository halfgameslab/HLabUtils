using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mup.QuestSystem.Internal;
using UnityEditorInternal;
using System;
using System.IO;

namespace Mup.QuestSystem
{
    [CustomEditor(typeof(QS_QuestsHandler))]
    public class QS_QuestsHandlerEditor : Editor
    {
        private Dictionary<string, QS_Quest> _questTable;
        private List<string> _questsIds = new List<string>();
        private List<int> _selected;
        private ReorderableList _list;
        private SerializedProperty _property;

        public void OnEnable()
        {
            TextAsset quests = AssetDatabase.LoadAssetAtPath("Assets/_Utils/QuestSystem/Data/quest_data.xml", typeof(TextAsset)) as TextAsset;
            _questTable = QS_QuestLoader.Deserialize(quests.text);
            foreach (var item in _questTable)
            {
                _questsIds.Add(item.Value.Type+"/"+item.Key);
            }
            _property = serializedObject.FindProperty("_questsIds");
            _selected = new List<int>();
            _list = CreateList(_property);
            listResize(ref _selected, _list.count);
        }

        public ReorderableList CreateList(SerializedProperty property)
        {
            ReorderableList list = new ReorderableList(property.serializedObject, property, false, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Initial Quests");
                },
            };
            list.drawElementCallback += drawElement;
            list.elementHeightCallback += elementHeight;
            list.onAddCallback += AddListElementSelected;
            list.onRemoveCallback += RemoveListElement;
            return list;
        }

        private void AddListElementSelected(ReorderableList list)
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            list.serializedProperty.GetArrayElementAtIndex(index).stringValue = "";
        }
        private void RemoveListElement(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this quest?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        }

        public void drawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            listResize(ref _selected, _list.count);
            _selected[index] = _questsIds.FindIndex(T => T.Split('/')[1] == _property.GetArrayElementAtIndex(index).stringValue);
            if (_selected[index] == -1)
            {
                _selected[index] = 0;
                _property.GetArrayElementAtIndex(index).stringValue = _questsIds[_selected[index]].Split('/')[1];
            }
            rect.y += 2;
            EditorGUI.BeginChangeCheck();
            int _oldValue = _selected[index];
            _selected[index] = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _selected[index], _questsIds.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                int tempValue = _selected[index];
                for (int i = 0; i < _selected.Count; i++)
                {
                    if(i != index)
                    {
                        if (_selected[i] == tempValue)
                        {
                            if (EditorUtility.DisplayDialog("Warning!", "Quest already selected to start.", "Ok"))
                            {
                                _selected[index] = _oldValue;
                            }  
                        }
                    }
                }
                _property.GetArrayElementAtIndex(index).stringValue = _questsIds[_selected[index]].Split('/')[1];

            }
        }

        public float elementHeight(int index)
        {
            Repaint();
            float height = 0;
            height = (EditorGUIUtility.singleLineHeight+10);
            return height;
        }

        public override void OnInspectorGUI()
        {
            QS_QuestsHandler _target = (QS_QuestsHandler)target;
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(_target.DisplayTime);
                EditorGUILayout.LabelField("Remaining time:" + _target.RemainingTime);
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            if (File.Exists(Path.Combine(Application.persistentDataPath, "saved_quests.xml")))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Saved quest found! You can delet or edit.", MessageType.Info);
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Delete Saved Quests"))
                {
                    File.Delete(Path.Combine(Application.persistentDataPath, "saved_quests.xml"));
                }

                if (GUILayout.Button("Edit Saved Quests"))
                {
                    System.Diagnostics.Process.Start(Path.Combine(Application.persistentDataPath, "saved_quests.xml"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            }
            else
            {
                EditorGUILayout.HelpBox("Saved quest not found! System will create a new one based on base config after entering in playmode.", MessageType.Warning);
                _list = null;
                
            }
            serializedObject.Update();
            if(_list != null)
                _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void listResize<T>(ref List<T> list, int size)
        {
            if (size > list.Count)
                for (int i = 0; i <= size - list.Count; i++)
                    list.Add(default(T));
            else if (size < list.Count)
                for (int i = 0; i <= list.Count - size; i++)
                    list.RemoveAt(list.Count - 1);
        }
    }
}