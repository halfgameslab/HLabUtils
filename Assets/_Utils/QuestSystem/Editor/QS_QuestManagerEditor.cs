using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Mup.QuestSystem
{
    [CustomEditor(typeof(QS_QuestManager))]
    public class QS_QuestManagerEditor : Editor
    {
        public static string DirPath = Environment.CurrentDirectory.Replace(@"\", "/");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_questData"));
            if (serializedObject.FindProperty("_questData") != null)
            {
                if(GUILayout.Button("Open Quest Data"))
                {
                    System.Diagnostics.Process.Start(DirPath+"/"+AssetDatabase.GetAssetPath(serializedObject.FindProperty("_questData").objectReferenceValue));
                }
            }
            if (serializedObject.FindProperty("_questData") != null)
            {
                if (GUILayout.Button("Open Saved Quest Data"))
                {
                    System.Diagnostics.Process.Start(Path.Combine(Application.persistentDataPath, "saved_quests.xml"));
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}