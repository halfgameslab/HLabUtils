using Mup.EventSystem.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace H_QuestSystemV2
{
    public class H_QuestWindow : EditorWindow
    {
        QuestEditor e = new QuestEditor();
        QuestGroupEditor g = new QuestGroupEditor();

        [MenuItem("HLab/Quest Editor")]
        public static void ShowWindow()
        {
            H_QuestWindow ow = null;

            if (HasOpenInstances<H_QuestWindow>())
                ow = GetWindow<H_QuestWindow>(false, "Quest Editor", true);

            H_QuestWindow editorWindow = CreateWindow<H_QuestWindow>("Quest Editor");

            if (ow != null)
                editorWindow.position = new Rect(ow.position.x + 20f, ow.position.y + 20f, ow.position.width, ow.position.height);

            ConfigureWindow(editorWindow);
        }

        public void OnEnable()
        {
            g.AddEventListener(ES_Event.ON_CLICK, OnSelectQuestHandler);
            g.AddEventListener(ES_Event.ON_DESTROY, OnQuestDestroyHandler);

            //e.Start(new H_Quest());
            g.Start(H_QuestManager.Instance.QuestGroups.GetGroupByName("global"));
        }

        public void OnDisable()
        {
            if(g != null)
            {
                g.RemoveEventListener(ES_Event.ON_CLICK, OnSelectQuestHandler);
                g.RemoveEventListener(ES_Event.ON_DESTROY, OnSelectQuestHandler);
            }
        }

        private void OnSelectQuestHandler(ES_Event ev)
        {
            e.Start((H_Quest)ev.Data);
        }
        
        private void OnQuestDestroyHandler(ES_Event ev)
        {
            if((H_Quest)ev.Data == e.CurrentQuest)
                e.Clear();
        }

        public static void ConfigureWindow(H_QuestWindow window)
        {
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(450, window.minSize.y);
            window.Show();
        }

        private bool DefaultTextureButton(string textureName, string hint, float w = 50, float h = 40)
        {
            return GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture(textureName), hint), GUILayout.Width(w), GUILayout.Height(h));
        }

        public void OnGUI()
        {
            float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

            //EditorGUI.DrawRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight*2), Color.blue);

            EditorGUILayout.BeginHorizontal();
            DefaultTextureButton("d_Collab.FileAdded", "Create New Group");
            DefaultTextureButton("d_Refresh@2x", "Reload");
            DefaultTextureButton(CVarSystem.IsEditModeActived ? "d_PlayButton@2x" : "d_PauseButton@2x", CVarSystem.IsEditModeActived ? "Disable Edit Mode" : "Enable Edit Mode");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Popup("Group (id)", 0, new string[] { "group a", "group b", "group c" });
            GUILayout.Button("E", GUILayout.Width(18));
            GUILayout.Button("-", GUILayout.Width(18));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Popup("Persistent Type", 0, new string[] { "SHARED", "PER_SCENE", "CUSTOM" });
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            g.Draw();

            EditorGUILayout.BeginVertical(GUILayout.MinWidth((firstCollumWidth * 2) - 50), GUILayout.MaxWidth((firstCollumWidth * 2) - 50));

            //EditorGUI.DrawRect(new Rect(firstCollumWidth, EditorGUIUtility.singleLineHeight * 2, firstCollumWidth*2, EditorGUIUtility.singleLineHeight), Color.red);

            e.Draw();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        public void SelectQuest(int index)
        {
            if(g.CurrentQuestGroup.Data.Count > index)
                SelectQuest(g.CurrentQuestGroup.Data[index]);
        }

        public void SelectQuest(H_Quest quest)
        {
            e.Start(quest);
        }
    }

    public class QuestGroupEditor
    {
        public H_DataGroup<H_Quest> CurrentQuestGroup { get; set; }
        public H_Quest SelectedQuest { get; set; }
        private Vector2 _scrollPosition;
        public void Start(string currentGroup)
        {
            //H_QuestGroup group = H_QuestManager.GetGroup(currentGroup);
            //Start(group);
        }

        public void Start(H_DataGroup<H_Quest> currentGroup)
        {
            CurrentQuestGroup = currentGroup;
        }

        public void Draw()
        {
            float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(firstCollumWidth));

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quest", GUILayout.MinWidth(firstCollumWidth / 4));
            GUIStyle searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");

            EditorGUILayout.TextField("", searchStyle);

            if(GUILayout.Button("+", GUILayout.MinWidth(18)))
            {
                CurrentQuestGroup.Add(new H_Quest() { ID = CurrentQuestGroup.Data.Count.ToString() });
            }

            H_Quest[] displayableQuests = CurrentQuestGroup.Data.ToArray();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            Rect r = EditorGUILayout.GetControlRect(false, 1);

            EditorGUI.DrawRect(r,Color.grey);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUILayout.MinWidth(firstCollumWidth), GUILayout.MaxWidth(firstCollumWidth));

            if(displayableQuests.Length == 0)
            {
                EditorGUILayout.HelpBox("No quests to display!", MessageType.Warning);
            }

            for (int i = 0; i < displayableQuests.Length; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(firstCollumWidth - 40), GUILayout.MaxWidth(firstCollumWidth - 40));
                EditorGUI.BeginDisabledGroup(displayableQuests[i] == SelectedQuest);
                if(GUILayout.Button("quest "+ displayableQuests[i].ID, GUILayout.MinWidth(firstCollumWidth - 62)))
                {
                    //select quest and display on board
                    SelectedQuest = displayableQuests[i];
                    this.DispatchEvent(ES_Event.ON_CLICK, displayableQuests[i]);
                }
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button(new GUIContent("D", "Duplicate Quest"), GUILayout.MinWidth(19)))
                {
                    //duplicate quest
                    //CurrentQuestGroup.Data.Insert(CurrentQuestGroup.Data.IndexOf(displayableQuests[i])+1, displayableQuests[i].Clone() );
                    CurrentQuestGroup.Insert(CurrentQuestGroup.Data.IndexOf(displayableQuests[i]) + 1, displayableQuests[i].Clone());
                }
                if(GUILayout.Button(new GUIContent("-", "Delete Quest"), GUILayout.MinWidth(18), GUILayout.MaxWidth(18)) && EditorUtility.DisplayDialog("Delete Quest", String.Format("Want delete quest {0}?\n\nYou can't revert this operation.", displayableQuests[i].ID), "Delete", "Cancel"))
                {
                    //remove quest
                    CurrentQuestGroup.Remove(displayableQuests[i]);
                    this.DispatchEvent(ES_Event.ON_DESTROY, displayableQuests[i]);
                }
                
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
    }

    public class QuestEditor
    {
        public H_Quest CurrentQuest { get; set; }

        private ReorderableList _infoList;

        private int _currentOption = 0;

        ConditionEditor _start = new ConditionEditor();
        ConditionEditor _goals = new ConditionEditor();
        ConditionEditor _fails = new ConditionEditor();

        RewardListEditor _rewardList = new RewardListEditor();

        Vector2 _scroolPosition;

        public void Start(H_Quest quest)
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

        public void Clear()
        {
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

                _infoList.DoLayoutList();

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(size), GUILayout.MaxWidth(size));

                DrawTabButton("Start", 0, size, w);
                DrawTabButton("Tasks", 1, size, w);
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

                EditorGUILayout.HelpBox("Select one quest to edit!", MessageType.Warning);

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
            EditorGUI.LabelField(rect, "Quest Info (id)");
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            CurrentQuest.Info[index].Name = EditorGUI.TextField(rect, "Name:", CurrentQuest.Info[index].Name);
            rect.y += rect.height;
            CurrentQuest.Info[index].Description = EditorGUI.TextField(rect, "Description:", CurrentQuest.Info[index].Description);
        }

    }

    public class ConditionEditor
    {
        string _title = string.Empty;

        Condition _condition = new Condition() { Type = "Condition", ID = "q1" };

        ReorderableList reorderableList;

        List<ConditionEditor> _conditions = new List<ConditionEditor>();

        public void Start(Condition condition, string title = "Conditions")
        {
            //save title
            _title = title;
            //save condition
            _condition = condition;
            _conditions.Clear();
            if (_condition.Type == "Condition")
            {
                foreach(Condition c in condition.Conditions)
                {
                    ConditionEditor ce = new ConditionEditor();
                    _conditions.Add(ce);
                    ce.Start(c);
                }

                //create the reorderableList
                reorderableList = new ReorderableList(_conditions, typeof(ConditionEditor))
                {
                    drawHeaderCallback = OnDrawHeaderHandler,
                    drawElementCallback = OnDrawElementHandler,
                    elementHeightCallback = OnElementHeightHandler,
                    onAddCallback = OnAddElementHandler,
                    onRemoveCallback = OnRemoveElementHandler
                };
            }
            else
            {
                reorderableList = null;
            }
        }

        public void Clear()
        {
            _title = string.Empty;   
            _condition = null;
            reorderableList = null;
            _conditions.Clear();
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            _conditions.RemoveAt(list.index);

            // select the up next element to facilitate delete operation
            if (list.index != 0)
                list.index = list.index - 1;
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            ConditionEditor c = new ConditionEditor();
            Condition condition = new Condition() { Type = "CheckVar" };
            if (list.index < 0 || list.count == 0)
            {
                condition.ID = string.Concat(_condition.ID, "_c", 0);
                c._condition = condition;;
                _condition.Conditions.Add(condition);
                _conditions.Add(c);
                list.index = 0;
            }
            else
            {
                condition.ID = string.Concat(_condition.ID, "_c", list.index);
                c._condition = condition;
                _conditions.Insert(list.index + 1, c);
                _condition.Conditions.Insert(list.index + 1, condition);
                list.index++;
            }
            c.Start(condition);
        }

        private float OnElementHeightHandler(int index)
        {
            if (_conditions[index]._condition.Type == "Condition")
                return _conditions[index].reorderableList.GetHeight() + EditorGUIUtility.singleLineHeight * 1.5f;//(_conditions[index]._conditions.Count+6) * EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * 2 + 10;
        }

        public void Draw()
        {
            reorderableList?.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Button(string.Format("Create On {0} Complete Handler", _title));

        }

        void OnDrawHeaderHandler(Rect rect)
        {
            rect.width = rect.width / 2;
            EditorGUI.LabelField(rect, _title);
            rect.x += rect.width;
            rect.width = rect.width / 2;
            rect.x += rect.width;
            _condition.Operation = (ConditionOperation)EditorGUI.EnumPopup(rect, _condition.Operation);
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {

            //EditorGUI.LabelField(rect, list[index]);
            //EditorGUI.LabelField(rect, list[index]);
            Rect origin = rect;
            float halfWidth = (origin.width / 2f);
            string[] types = new string[] { "Condition", "CheckVar", "OnChangeVar", "OnEventDispatch", "ListenQuest", "Timer" };

            rect.width = halfWidth - 18f;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(rect, _conditions[index]._condition.ID);
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;
            rect.width = 18f;
            GUI.Button(rect, "E");

            rect.width = halfWidth / 2;
            rect.x += 18f;
            EditorGUI.Popup(rect, 0, new string[] { "Join", "Append" });
            rect.x += rect.width;
            string lastType = _conditions[index]._condition.Type;

            _conditions[index]._condition.Type = types[EditorGUI.Popup(rect, Array.IndexOf(types, _conditions[index]._condition.Type), types)];

            if(lastType != _conditions[index]._condition.Type)
            {
                _conditions[index].Start(_conditions[index]._condition);

            }

            //EditorGUI.Popup(rect, 0, new string[] { "AND", "OR" });

            rect.x = origin.x;



            rect.y += EditorGUIUtility.singleLineHeight;

            if (_conditions[index]._condition.Type.Equals("CheckVar") || _conditions[index]._condition.Type.Equals("OnChangeVar"))
            {
                rect.width = 18;
                EditorGUI.Toggle(rect, false);
                rect.x += 18;
                rect.width = (origin.width - 18) / 5;
                EditorGUI.Popup(rect, 0, new string[] { "Global", "Other Group" });
                rect.x += rect.width;
                EditorGUI.Popup(rect, 0, new string[] { "String", "Int", "Float", "Boolean", "Vector3" });
                rect.x += rect.width;
                EditorGUI.Popup(rect, 0, new string[] { "Var01", "Var02" });
                rect.x += rect.width;
                EditorGUI.EnumPopup(rect, CVarCommands.EQUAL);
                rect.x += rect.width;
                EditorGUI.TextField(rect, "value");
            }
            else if (_conditions[index]._condition.Type.Equals("OnEventDispatch"))
            {
                rect.width = 18;
                EditorGUI.Toggle(rect, false);
                rect.x += 18;
                rect.width = (origin.width - 18) / 3;
                EditorGUI.ObjectField(rect, (UnityEngine.Object)null, typeof(UnityEngine.Object), true);
                rect.x += rect.width;
                EditorGUI.Popup(rect, 0, new string[] { "Target" });
                rect.x += rect.width;
                EditorGUI.Popup(rect, 0, new string[] { "ON_TUTORIAL_COMPLETE", "ON_GAME_OVER" });
            }
            else if (_conditions[index]._condition.Type.Equals("Condition"))
            {
                rect.width = origin.width;

                _conditions[index].reorderableList.DoList(rect);

            }
            else if (_conditions[index]._condition.Type.Equals("ListenQuest"))
            {
                rect.width = 18;
                EditorGUI.Toggle(rect, false);
                rect.x += 18;
                rect.width = (origin.width - 18) / 2;
                EditorGUI.Popup(rect, 0, new string[] { "Quest (ID)" });
                rect.x += rect.width;
                EditorGUI.Popup(rect, 0, new string[] { "ON_COMPLETE", "ON_GOAL_UPDATE", "ON_FAIL" });
            }
            else if (_conditions[index]._condition.Type.Equals("Timer"))
            {
                rect.width = 18;
                EditorGUI.Toggle(rect, false);
                rect.x += 18;
                rect.width = (origin.width - 18) / 2;
                EditorGUI.Popup(rect, 0, new string[] { "ASC", "DESC" });
                rect.x += rect.width;
                EditorGUI.FloatField(rect, 100f);
            }
            //EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _title+" "+list[index]);
            //EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), list[index]);
        }

    }

    public class RewardListEditor
    {
        ReorderableList reorderableList;
        List<string> list = new List<string>() { "reward a", "reward b", "reward c" };

        public void Start()
        {
            reorderableList = new ReorderableList(list, typeof(string))
            {
                drawHeaderCallback = OnDrawHeaderHandler,
                drawElementCallback = OnDrawElementHandler
            };
        }

        public void Draw(string label = "Conditions")
        {
            /*EditorGUILayout.LabelField(label);*/

            reorderableList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //EditorGUILayout.Space();
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            string name = "Rewards";
            EditorGUI.LabelField(rect, name);
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, list[index]);
        }
    }

    public class RewardEditor
    {
        public void Draw()
        {
            EditorGUILayout.LabelField("Reward field");
        }
    }


    public class QuestList
    {

    }

}