using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Diagnostics;

namespace Mup.ShopSystem.Data
{
    [CustomEditor(typeof(M_ItemDatabase))]
    public class M_ItemDatabaseEditor : Editor
    {
        private GUIContent _idContent = new GUIContent("ID:", "you cannot have the same id");
        private GUIContent _nameContent = new GUIContent("Name:", "Item Name, If used with the MultiLanguage system put the string ID. ");
        private GUIContent _descContent = new GUIContent("Description:", "Item Description, If used with the MultiLanguage system put the string ID.");
        Color proColor = (Color)new Color32(56, 56, 56, 255);
        Color plebColor = (Color)new Color32(194, 194, 194, 255);
        Color newBtonColor = (Color)new Color32(0x27, 0xAB, 0x27, 0xFF);
        Color deletBtnColor = (Color)new Color32(0xB1, 0x4E, 0xC3, 0xFF);
        Color deletItemColor = (Color)new Color32(0xA1, 0x0D, 0x0D, 0xFF);
        private string[] _choices;
        Process p = new Process();
        M_ItemDatabase _target;
        ReorderableList _list;

        public void OnEnable()
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.UseShellExecute = true;
            pi.FileName = Application.dataPath + "/Mup/ShopSystem/README.txt";
            p.StartInfo = pi;
            _list = GenerateDatabaseList();
        }

        protected override void OnHeaderGUI()
        {
            Vector2 tempIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            GUILayout.Label(EditorGUIUtility.IconContent("d_CustomSorting"));
            EditorGUIUtility.SetIconSize(tempIconSize);
        }

        public override void OnInspectorGUI()
        {
            GUIStyle gsAlterQuest = new GUIStyle();
            gsAlterQuest.normal.background = MakeTex(600, 1, Color.gray);
            _target = (M_ItemDatabase)target;
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Before populate the  item database READ the README.txt!", MessageType.Info, true);
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("README"))
            {
                p.Close();
                p.Start();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical(gsAlterQuest);
            EditorGUILayout.LabelField("Currency Types");
            Color old = GUI.backgroundColor;
            if (_target.PaymentMethods.Length > 0)
            {
                for (int i = 0; i < _target.PaymentMethods.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _target.PaymentMethods[i] = EditorGUILayout.TextArea(_target.PaymentMethods[i]);
                    GUI.backgroundColor = deletBtnColor;
                    if (GUILayout.Button("DELETE", GUILayout.Width(60)))
                    {
                        _target.PaymentMethods = _target.PaymentMethods.Where((source, index) => index != i).ToArray();
                    }
                    GUI.backgroundColor = old;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = newBtonColor;
            if (GUILayout.Button("NEW"))
            {
                _target.PaymentMethods = _target.PaymentMethods.Concat(new string[] { "" }).ToArray();
            }
            GUI.backgroundColor = old;
            _choices = _target.PaymentMethods;
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        public ReorderableList GenerateDatabaseList()
        {
            ReorderableList databaseList = new ReorderableList(serializedObject, serializedObject.FindProperty("_itemData"), true, true, true, true);

            databaseList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Item Database");
            };
            databaseList.drawElementCallback = (Rect rect, int index, bool active, bool focus) =>
            {
                Color old = GUI.backgroundColor;

                rect.y += 5;
                SerializedProperty element = databaseList.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty id = element.FindPropertyRelative("_id");
                SerializedProperty itemName = element.FindPropertyRelative("_itemName");
                SerializedProperty itemDescription = element.FindPropertyRelative("_itemDescription");
                SerializedProperty payInfo = element.FindPropertyRelative("_payInfo");
                SerializedProperty buyEvent = element.FindPropertyRelative("_buyEvent");
            #region first Row
            float x = rect.x;
                EditorGUI.LabelField(new Rect(x, rect.y, 25, EditorGUIUtility.singleLineHeight), _idContent);
                x += 25;
                EditorGUI.PropertyField(new Rect(x, rect.y, 50, EditorGUIUtility.singleLineHeight), id, GUIContent.none);
                id.intValue = index;
                x += 50;
                x += 10;
                EditorGUI.LabelField(new Rect(x, rect.y, 60, EditorGUIUtility.singleLineHeight), _nameContent);
                x += 60;
                EditorGUI.PropertyField(new Rect(x, rect.y, EditorGUIUtility.currentViewWidth - (x + 10), EditorGUIUtility.singleLineHeight), itemName, GUIContent.none);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
            #endregion
            #region second row
            x = rect.x;
                EditorGUI.LabelField(new Rect(x, rect.y, 80, EditorGUIUtility.singleLineHeight), _descContent);
                x += 80;
                EditorGUI.PropertyField(new Rect(x, rect.y, EditorGUIUtility.currentViewWidth - (x + 10), EditorGUIUtility.singleLineHeight), itemDescription, GUIContent.none);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
            #endregion
            #region third row
            for (int i = 0; i < payInfo.arraySize; i++)
                {
                    SerializedProperty payElemetn = payInfo.GetArrayElementAtIndex(i);
                    float w = rect.width / 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, w, EditorGUIUtility.singleLineHeight), "Currency:");
                    int choiceValueInt = _choices.ToList().FindIndex(T => T == payElemetn.FindPropertyRelative("CurrencyName").stringValue);
                    EditorGUI.BeginChangeCheck();
                    choiceValueInt = EditorGUI.Popup(new Rect(rect.x + 60, rect.y, w - 60, EditorGUIUtility.singleLineHeight), (choiceValueInt != -1) ? choiceValueInt : 0, _choices);
                    if (EditorGUI.EndChangeCheck())
                    {
                        payElemetn.FindPropertyRelative("CurrencyName").stringValue = _choices[choiceValueInt];
                    }
                    EditorGUI.LabelField(new Rect(rect.x + (w), rect.y, w, EditorGUIUtility.singleLineHeight), "Value");
                    EditorGUI.PropertyField(new Rect(rect.x + (w) + 60, rect.y, w - 80, EditorGUIUtility.singleLineHeight), payElemetn.FindPropertyRelative("Value"), GUIContent.none);
                    GUI.backgroundColor = deletItemColor;
                    if (GUI.Button(new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight), "-"))
                    {
                        payInfo.DeleteArrayElementAtIndex(i);
                    }
                    GUI.backgroundColor = old;
                    rect.y += EditorGUIUtility.singleLineHeight + 1;
                }
                if (payInfo.arraySize < _choices.Length)
                    if (GUI.Button(new Rect(rect.x, rect.y, rect.width, 15), "Add Currecny Type"))
                    {
                        payInfo.InsertArrayElementAtIndex(payInfo.arraySize);
                    }
            #endregion
        };
            databaseList.elementHeightCallback = (index) =>
            {
                Repaint();
                float height = 0;
                int childCount = databaseList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("_payInfo").arraySize;
                if (childCount == _choices.Length)
                {
                    childCount -= 1;
                }
                height = (EditorGUIUtility.singleLineHeight + 8) * (3 + childCount);
                return height;
            };

            return databaseList;
        }

        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}