using HLab.H_Common.H_Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HLab.H_QuestSystem.H_Editor
{
    public class H_QuestEditor
    {
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

                _infoList = new ReorderableList(quest.Info, typeof(QuestInfo))
                {
                    drawHeaderCallback = OnDrawHeaderHandler,
                    drawElementCallback = OnDrawElementHandler,
                    elementHeightCallback = OnElementHeightHandler
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
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                string uname = EditorGUILayout.TextField(new GUIContent(string.Format("Unique Name ({0})", CurrentQuest.UID), "Used for internal control"), CurrentQuest.UName);
                if (uname != CurrentQuest.UName)
                {
                    if (ObjectNamesManager.ValidateName(uname, '.', '[', ']', ' ', '\0'))
                        CurrentQuest.UName = uname;
                    else
                        Debug.LogWarning("Avoid use . [ ] to name your strings. Names with only spaces are not alloweds to.");
                }

                if (GUILayout.Button("E", GUILayout.MinWidth(19), GUILayout.MaxWidth(19)))
                {

                }

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
            return EditorGUIUtility.singleLineHeight * 2 + 5f;
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            rect.width -= 19;
            EditorGUI.LabelField(rect, "Quest Info");
            rect.x += rect.width;
            rect.width = 19;
            EditorGUI.Toggle(rect, true);
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            CurrentQuest.Info[index].Name = EditorGUI.TextField(rect, "Name:", CurrentQuest.Info[index].Name);
            rect.y += rect.height;
            CurrentQuest.Info[index].Description = EditorGUI.TextField(rect, "Description:", CurrentQuest.Info[index].Description);
        }

    }
}