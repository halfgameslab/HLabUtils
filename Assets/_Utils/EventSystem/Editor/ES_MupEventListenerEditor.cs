using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

using Util.InstanceSystem.Editor;

namespace Mup.EventSystem.Events
{
    [CustomEditor(typeof(ES_MupEventListener))]
    public class ES_MupEventListenerEditor : Editor
    {
        ES_MupEventListener _target;
        UnityEngine.Object _objTarget;
        SerializedProperty _id;
        SerializedProperty _eventName;
        SerializedProperty _eventListener;
        IS_InstanceSceneManager InstanceManager;

        private void OnEnable()
        {
            InstanceManager = FindObjectOfType<IS_InstanceSceneManager>();
            _target = (ES_MupEventListener)target;
            if (InstanceManager == null)
            {
                Debug.LogError("[Error] IS_InstanceManager not found on the scene.");
            }
            else
            {
                _id = serializedObject.FindProperty("_targetIdentifier");
                _eventName = serializedObject.FindProperty("_eventName");
                _eventListener = serializedObject.FindProperty("_eventListener");
                _objTarget = GetObjectByString(_id.stringValue);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Target:", "Object target to listen"), GUILayout.Width(45));
            _objTarget = EditorGUILayout.ObjectField(_objTarget, typeof(UnityEngine.Object), true, GUILayout.Width(120));
            //if (EditorGUI.EndChangeCheck())
            //{
            //    _id.stringValue = (_objTarget.GetEditorInstanceName() == string.Empty) ? _objTarget.SetEditorInstanceName("__instance__") : _objTarget.GetEditorInstanceName();
            //}

            if(_objTarget)
            {
                _id.stringValue = (_objTarget.GetEditorInstanceName() == string.Empty) ? _objTarget.SetEditorInstanceName("__instance__") : _objTarget.GetEditorInstanceName(false);

                Component[] cs = null;
                string[] s = null;

                if (_objTarget is Component)
                {
                    cs = ((Component)_objTarget).GetComponents<Component>();
                    s = new string[cs.Length + 1];
                }
                else if(_objTarget is GameObject)
                {
                    cs = ((GameObject)_objTarget).GetComponents<Component>();
                    s = new string[cs.Length + 1];
                }

                if(cs != null)
                {
                    int index = 0;
                    int newIndex = 0;

                    s[0] = "GameObject";
                    for (int i = 0; i < cs.Length; i++)
                    {
                        s[i+1] = string.Format("{0}. {1}", i, cs[i].GetType().Name);
                        if (cs[i] == _objTarget)
                            index = i+1;
                    }

                    newIndex = EditorGUILayout.Popup(index, s);

                    if (newIndex != index)
                    {
                        if (newIndex != 0)
                        {
                            _objTarget = cs[newIndex - 1];
                        }
                        else
                        {
                            if (_objTarget is Component)
                            {
                                _objTarget = (_objTarget as Component).gameObject;
                            }
                            else if (_objTarget is GameObject)
                            {
                                _objTarget = (_objTarget as GameObject);
                            }
                        }

                        _id.stringValue = (_objTarget.GetEditorInstanceName() == string.Empty) ? _objTarget.SetEditorInstanceName("__instance__") : _objTarget.GetEditorInstanceName(false);
                    }
                }
            }
            else
            {
                if (InstanceManager != null)
                {
                    _objTarget = GetObjectByString(_id.stringValue);
                }

                EditorGUILayout.Popup(0, new string[] { "none" });
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Identifier:", "Identifier to listen"), GUILayout.Width(60));
            EditorGUILayout.PropertyField(_id, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                if (InstanceManager != null) {
                    _objTarget = GetObjectByString(_id.stringValue);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Event Name:", "Event name to listen"), GUILayout.Width(80));
            EditorGUILayout.PropertyField(_eventName, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.LabelField(new GUIContent("Event Listener", "Event to listener"));
            EditorGUILayout.PropertyField(_eventListener);
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkSceneDirty(_target.gameObject.scene);
            }
            serializedObject.ApplyModifiedProperties();
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

    }
}