using HLab.H_Common.H_Editor;
using Mup.Multilanguage.Plugins;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HLab.H_QuestSystem.H_Editor
{
    public class H_QuestEditor
    {
        public const string DEFAULT_QUEST_NAME = "quest_(0)";

        public H_Quest CurrentQuest { get; set; }

        private ReorderableList _infoList;

        private int _currentOption = 0;

        H_ConditionEditor _start = new H_ConditionEditor();
        H_ConditionEditor _goals = new H_ConditionEditor();
        H_ConditionEditor _fails = new H_ConditionEditor();

        H_RewardListEditor _rewardList = new H_RewardListEditor();

        Vector2 _scroolPosition;

        public void Start(H_Quest quest)
        {
            if (quest != null)
            {
                CurrentQuest = quest;

                _infoList = new ReorderableList(quest.Info, typeof(string))
                {
                    drawHeaderCallback = OnDrawHeaderHandler,
                    drawElementCallback = OnDrawElementHandler,
                    elementHeightCallback = OnElementHeightHandler,
                    onAddCallback = OnAddInfoElementCallback,
                    onRemoveCallback = OnRemoveInfoElementCallback
                };

                _start.Start(quest.StartCondition, "Start Conditions");
                _goals.Start(quest.TaskCondition, "Goals/Tasks");
                _fails.Start(quest.FailCondition, "Fails Conditions");
                _rewardList.Start();
            }
            else
            {
                Clear();
            }
        }

        private void OnAddInfoElementCallback(ReorderableList list)
        {
            string info = ObjectNamesManager.GetUniqueName(CurrentQuest.Info.ToArray(), string.Format("@quest_{0}_info(0)", CurrentQuest.GlobalUName));

            ML_Reader.SetText(info, "insert quest info here");

            if (list.index > 0)
            {
                CurrentQuest.InsertInfo(list.index + 1, info);
                list.index++;
            }
            else
            {
                CurrentQuest.AddInfo(info);
                list.index = list.count - 1;
            }
        }

        private void OnRemoveInfoElementCallback(ReorderableList list)
        {
            ML_Reader.RemoveText(CurrentQuest.Info[list.index]);
            CurrentQuest.RemoveInfoAt(list.index);
        }

        public void Clear()
        {
            _scroolPosition = Vector2.zero;
            _currentOption = 0;
            CurrentQuest = null;
            _infoList = null;
            _start.Clear();
            _goals.Clear();
            _fails.Clear();
        }

        public void Draw()
        {
            float size = ((EditorGUIUtility.currentViewWidth / 3f) * 2f) - 20f;

            if (CurrentQuest != null)
            {
                float w = size / 4f - 3f;

                _scroolPosition = EditorGUILayout.BeginScrollView(_scroolPosition, GUILayout.MinWidth(((EditorGUIUtility.currentViewWidth / 3f) * 2f)), GUILayout.MaxWidth(((EditorGUIUtility.currentViewWidth / 3f) * 2f)));
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));
                //EditorGUILayout.Space();

                GUIStyle style = new GUIStyle();
                //style.alignment = TextAnchor.MiddleLeft;
                style.richText = true;
                EditorGUILayout.SelectableLabel(string.Format("<color=#dddddd>Global UName:</color> <b><color=#81B4FF>{0}</color></b>", CurrentQuest.GlobalUName), style, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));//EditorStyles.linkLabel);
                
                EditorGUILayout.BeginHorizontal();
                string uname = EditorGUILayout.TextField(new GUIContent("Unique Name", "Used for internal control"), CurrentQuest.UName);
                if (uname != CurrentQuest.UName)
                {
                    uname = ObjectNamesManager.RemoveForbiddenCharacters(uname);

                    if (uname.Length == 0)
                        uname = DEFAULT_QUEST_NAME;

                    CurrentQuest.UName = uname;
                    /*if (ObjectNamesManager.ValidateName(uname, '.', '[', ']', ' ', '\0'))
                        CurrentQuest.UName = uname;
                    else
                        Debug.LogWarning("Avoid use . [ ] to name your strings. Names with only spaces are not alloweds to.");*/
                }

                /*if (GUILayout.Button("E", GUILayout.MinWidth(19), GUILayout.MaxWidth(19)))
                {

                }*/

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                _infoList.DoLayoutList();

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));

                DrawTabButton("Start", 0, size, w);
                DrawTabButton("Goals/Tasks", 1, size, w);
                DrawTabButton("Fails", 2, size, w);
                DrawTabButton("Rewards", 3, size, w);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));

                if (_currentOption == 0)
                    _start.Draw();
                else if (_currentOption == 1)
                    _goals.Draw();
                else if (_currentOption == 2)
                    _fails.Draw();
                else if (_currentOption == 3)
                    _rewardList.Draw();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.BeginVertical(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));

                EditorGUILayout.HelpBox("Select some quest to edit!", MessageType.Warning);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTabButton(string label, int option, float size, float width)
        {
            EditorGUI.BeginDisabledGroup(_currentOption == option);
            if (GUILayout.Button(label, GUILayout.MinWidth(width), GUILayout.MaxWidth(width)))
                _currentOption = option;
            EditorGUI.EndDisabledGroup();
        }

        private float OnElementHeightHandler(int index)
        {
            return EditorGUIUtility.singleLineHeight + 5f;
        }

        private bool _showStringID = false;
        void OnDrawHeaderHandler(Rect rect)
        {
            rect.width -= 19;
            EditorGUI.LabelField(rect, "Quest Info");
            rect.x += rect.width;
            rect.width = 19;
            _showStringID = EditorGUI.Toggle(rect, _showStringID);
        }
        //void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        //{
        //    rect.height = EditorGUIUtility.singleLineHeight;

        //    string name = CurrentQuest.Info[index].Name??string.Empty;
        //    string desc = CurrentQuest.Info[index].Description??string.Empty;

        //    if (!_showStringID)
        //    {
        //        name = ML_Reader.GetText(name);
        //        desc = ML_Reader.GetText(desc);
        //    }

        //    EditorGUI.BeginChangeCheck();
        //    name = EditorGUI.TextField(rect, "Name:", name);
        //    if(EditorGUI.EndChangeCheck())
        //    {
        //        if(_showStringID)
        //        {
        //            CurrentQuest.UpdateInfo(index, name, CurrentQuest.Info[index].Description);
        //        }
        //        else
        //        {
        //            ML_Reader.SetText(CurrentQuest.Info[index].Name, name);
        //        }
        //    }
        //    rect.y += rect.height;
        //    EditorGUI.BeginChangeCheck();
        //    desc = EditorGUI.TextField(rect, "Description:", desc);
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        if (_showStringID)
        //        {
        //            CurrentQuest.UpdateInfo(index, CurrentQuest.Info[index].Name, desc);
        //        }
        //        else
        //        {
        //            ML_Reader.SetText(CurrentQuest.Info[index].Description, desc);
        //        }
        //    }

        //}

        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;

            string info = CurrentQuest.Info[index] ?? string.Empty;

            if (!_showStringID)
            {
                info = ML_Reader.GetText(info);
            }

            EditorGUI.BeginChangeCheck();
            info = EditorGUI.TextField(rect, "Info:", info);
            if (EditorGUI.EndChangeCheck())
            {
                if (_showStringID)
                {
                    CurrentQuest.UpdateInfo(index, info);
                }
                else
                {
                    ML_Reader.SetText(CurrentQuest.Info[index], info);
                }
            }
        }

    }
}