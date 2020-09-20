using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using TMPro;

using Mup.Multilanguage.Plugins;

namespace Mup.Multilanguage.Editor
{
    [CustomEditor(typeof(ML_TextMeshProExtension))]
    [CanEditMultipleObjects]
    public class ML_TextMeshProExtensionEditor1 : UnityEditor.Editor
    {

        SerializedProperty stringKey;
        private string text;
        private TMP_Text textComponent;

        void OnEnable()
        {
            stringKey = serializedObject.FindProperty("_text");

            textComponent = (target as ML_TextMeshProExtension).GetComponent<TMP_Text>();
            
            text = textComponent.text;
            
            Undo.undoRedoPerformed += UpdateText;
            ML_Reader.OnLanguageChangeHandler += UpdateText;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UpdateText;
            ML_Reader.OnLanguageChangeHandler -= UpdateText;
        }

        public override void OnInspectorGUI()
        {
            if (Selection.objects.Length > 1)
            {
                EditorGUILayout.LabelField("You can't edit multiple instances!");

                return;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(stringKey, new GUILayoutOption[] { GUILayout.Width(EditorGUIUtility.currentViewWidth - 40), GUILayout.Height(100) });
            
            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateText();
            }

            if (text != textComponent.text)
            {
                UpdateText();
                Debug.LogWarning("Component Text managed by MultiLanguageTextExtension cant be changed manually!");
            }
            /*else if(GUI.changed)
            {
                UpdateText();
            }*/
        }

        private void UpdateText()
        {
            if (Selection.Contains(textComponent.gameObject))
            {
                text = (target as ML_TextMeshProExtension).UpdateText();
            }
        }

    }
}