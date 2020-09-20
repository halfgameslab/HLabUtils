using UnityEngine;
using UnityEditor;
using System.Collections;

using System.Collections.Generic;
using Mup.Multilanguage.Plugins;

namespace Mup.Multilanguage.Editor
{

    class ML_WindowEditor : EditorWindow
    {
        
        private Dictionary<string, string> stringList;

        public Dictionary<string,string> StringList { set { stringList = value; } }

        private Vector2 scrollPosition = Vector2.zero;

        [MenuItem("MUP/String Editor")]
        public static void ShowWindow()
        {
            ML_Reader.Reload();

            ML_WindowEditor w = EditorWindow.GetWindow<ML_WindowEditor>("String Editor");
            w.StringList = ML_Reader.Text;
            
        }

        [MenuItem("MUP/ML/LoadEN")]
        public static void LoadENFile()
        {
            if (ML_Reader.Language != Lang.EN)
                ML_Reader.Language = Lang.EN;
            else
                ML_Reader.Reload();

            GameObject c = GameObject.Find("Canvas");
            if (c)
            {
                c.transform.Translate(Vector3.up);
                c.transform.Translate(Vector3.down);
            }
        }

        [MenuItem("MUP/ML/LoadPTBR")]
        public static void LoadPTBRFile()
        {
            if (ML_Reader.Language != Lang.PTBR)
                ML_Reader.Language = Lang.PTBR;
            else
                ML_Reader.Reload();

            GameObject c = GameObject.Find("Canvas");
            if (c)
            {
                c.transform.Translate(Vector3.up);
                c.transform.Translate(Vector3.down);
            }
        }

        [MenuItem("MUP/MLReload")]
        public static void ReloadLangFile()
        {
            ML_Reader.Reload();
            GameObject c = GameObject.Find("Canvas");
            if (c)
            {
                c.transform.Translate(Vector3.up);
                c.transform.Translate(Vector3.down);
            }
        }

        void OnGUI()
        {
            //Debug.Log(Application.systemLanguage);
            

            GUILayout.Label("String Settings", EditorStyles.boldLabel);

            if (stringList != null && stringList.Count != 0)
                GUILayout.Label("Key - Value", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            /*for (int i = 0; i < 10; i++)
            {
                if (GUILayout.Button("PT"))
                {

                }
            }*/

            GUILayout.EndHorizontal();

            Dictionary<string, string> copyList = new Dictionary<string, string>();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(300), GUILayout.Height(100));
            
            if(stringList == null || stringList.Count == 0)
            {
                
                GUILayout.Label("Nenhum arquivo foi encontrado", EditorStyles.boldLabel);
               

                GUILayout.EndScrollView();

                if (GUILayout.Button("Criar Arquivo"))
                {

                }
                return;
            }

            

            foreach (KeyValuePair<string, string> s in stringList)
            {
                //backup da chave utilizado no processo de remoção
                string key = s.Key;
                string value = s.Value;

                //inicio do desenho da interface
                
                
                EditorGUILayout.BeginHorizontal();
                key = EditorGUILayout.TextField(key);
                value = EditorGUILayout.TextField(value);

                copyList.Add(key, value);

                //botão para remover o elemento
                if (GUILayout.Button("-"))
                {
                    copyList.Remove(key);
                }

                /*if (GUILayout.Button("U"))
                {

                }
                if (GUILayout.Button("D"))
                {

                }*/
                EditorGUILayout.EndHorizontal();
                              
            }

            GUILayout.EndScrollView();

            stringList = copyList;

            AddStringElementOption();

            if (GUILayout.Button("Save"))
            {
                ML_Reader.Save(stringList);
            }

            if (GUILayout.Button("Revert"))
            {
                stringList = ML_Reader.Text;
            }

            if (GUILayout.Button("Reload"))
            {
                ML_Reader.Reload();

                StringList = ML_Reader.Text;
            }

            //SystemLanguage ss = Application.systemLanguage;

            //EditorGUILayout.EnumPopup(ss);
            

            /*foreach (SystemLanguage val in System.Enum.GetValues(typeof(SystemLanguage)))
            {
                
                Debug.Log(val);
            }*/

        }

        private string newKey = "Nova chave";
        private string newValue = "Novo valor";
        private void AddStringElementOption()
        {
            GUILayout.Label("Utilize o campo abaixo para adicionar um item:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            newKey = EditorGUILayout.TextField(newKey);
            newValue = EditorGUILayout.TextField(newValue);

            if (GUILayout.Button("+"))
            {
                if(!newKey.Equals("Nova chave") && !newValue.Equals("Novo valor"))
                {
                    stringList.Add(newKey, newValue);
                    newKey = "";
                    newValue = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}