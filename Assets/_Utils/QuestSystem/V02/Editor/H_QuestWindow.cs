using HLab.H_DataSystem;
using Mup.EventSystem.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using HLab.H_DataSystem.H_Editor;

namespace HLab.H_QuestSystem.H_Editor
{
    public class H_QuestWindow : EditorWindow
    {
        H_QuestGroupListEditor<H_Quest, H_PersistentQuestData> _questListEditor = new H_QuestGroupListEditor<H_Quest, H_PersistentQuestData>();
        H_QuestEditor _questEditor = new H_QuestEditor();
        H_QuestGroupEditor _questGroupEditor = new H_QuestGroupEditor();
        H_RuntimeFilesEditor<H_Quest, H_PersistentQuestData> _runtimeFilesEditor = new H_RuntimeFilesEditor<H_Quest, H_PersistentQuestData>();
        string _currentGroupUID = string.Empty;
        string _currentQuestUID = string.Empty;

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
            _questListEditor.AddEventListener(ES_Event.ON_CHANGE, OnChangeGroupsListHandler);
            _questListEditor.AddEventListener(ES_Event.ON_CONFIRM, OnCreateGroupHandler);
            _questListEditor.AddEventListener(ES_Event.ON_DESTROY, OnRemoveGroupHandler);

            _questGroupEditor.AddEventListener(ES_Event.ON_CLICK, OnSelectQuestHandler);
            _questGroupEditor.AddEventListener(ES_Event.ON_DESTROY, OnQuestDestroyHandler);
        }
        
        public void OnDisable()
        {
            if(_questListEditor != null)
            {
                _questListEditor.RemoveEventListener(ES_Event.ON_CHANGE, OnChangeGroupsListHandler);
                _questListEditor.RemoveEventListener(ES_Event.ON_CONFIRM, OnCreateGroupHandler);
                _questListEditor.RemoveEventListener(ES_Event.ON_DESTROY, OnRemoveGroupHandler);
            }
            if(_questGroupEditor != null)
            {
                _questGroupEditor.RemoveEventListener(ES_Event.ON_CLICK, OnSelectQuestHandler);
                _questGroupEditor.RemoveEventListener(ES_Event.ON_DESTROY, OnSelectQuestHandler);
            }
        }    

        public static void ConfigureWindow(H_QuestWindow window)
        {
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(450, window.minSize.y);
            window.Show();
        }

        public void OnGUI()
        {
            CheckToAvoidNullQuestList();

            float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

            _questListEditor.Draw(_questGroupEditor.CurrentQuestGroup, H_QuestManager.Instance.QuestGroups.GetGroups());

            if (_questGroupEditor.CurrentQuestGroup != null)
            {
                EditorGUI.BeginDisabledGroup(_questListEditor.CurrentState != 0);

                _runtimeFilesEditor.Draw(_questGroupEditor.CurrentQuestGroup, H_QuestManager.Instance.QuestGroups.GetGroups());

                EditorGUILayout.BeginHorizontal();

                _questGroupEditor.Draw();

                EditorGUILayout.BeginVertical(GUILayout.MinWidth((firstCollumWidth * 2) - 50), GUILayout.MaxWidth((firstCollumWidth * 2) - 50));

                _questEditor.Draw();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
        }

        public void SelectGroup(H_DataGroup<H_Quest, H_PersistentQuestData> group)
        {
            if (group != null)
            {
                _currentGroupUID = group.UID;
            }
            else
            {
                _currentGroupUID = string.Empty;
            }

            _questGroupEditor.Start(group);
            _questEditor.Clear();
        }

        public void SelectQuest(int index)
        {
            if(_questGroupEditor.CurrentQuestGroup.Data.Count > index)
                SelectQuest(_questGroupEditor.CurrentQuestGroup.Data[index]);
        }

        public void SelectQuest(H_Quest quest)
        {
            _questEditor.Start(quest);
            //_questGroupEditor.SelectedQuest = quest;
            _currentQuestUID = quest!=null?quest.UID:string.Empty;
        }

        private void CheckToAvoidNullQuestList()
        {
            if (_questGroupEditor.CurrentQuestGroup == null && _questListEditor.CurrentState != 1)
            {
                H_DataGroup<H_Quest, H_PersistentQuestData> currentQuestGroup = H_QuestManager.Instance.QuestGroups.GetGroupByUID(_currentGroupUID);
                if (currentQuestGroup != null)
                {
                    _questGroupEditor.Start(currentQuestGroup, currentQuestGroup.Data.FirstOrDefault(e => e.UID == _currentQuestUID));
                    SelectQuest(_questGroupEditor.SelectedQuest);
                }
                else
                {
                    _questGroupEditor.Start(H_QuestManager.Instance.QuestGroups.GetGroupByName("global"));
                    _currentGroupUID = _questGroupEditor.CurrentQuestGroup.UID;
                    _currentQuestUID = string.Empty;

                }
            }
        }

        private void OnRemoveGroupHandler(ES_Event ev)
        {
            H_QuestManager.Instance.QuestGroups.RemoveGroup((H_DataGroup<H_Quest, H_PersistentQuestData>)ev.Data);
            SelectGroup(H_QuestManager.Instance.QuestGroups.GetGroupByName("global"));
        }

        private void OnChangeGroupsListHandler(ES_Event ev)
        {
            SelectGroup((H_DataGroup<H_Quest, H_PersistentQuestData>)ev.Data);
        }

        private void OnCreateGroupHandler(ES_Event ev)
        {
            string name = (string)ev.Data;

            SelectGroup(H_QuestManager.Instance.QuestGroups.CreateGroup(name, _questGroupEditor.CurrentQuestGroup));
        }

        private void OnSelectQuestHandler(ES_Event ev)
        {
            SelectQuest((H_Quest)ev.Data);
            //_questEditor.Start((H_Quest)ev.Data);
        }

        private void OnQuestDestroyHandler(ES_Event ev)
        {
            if ((H_Quest)ev.Data == _questEditor.CurrentQuest)
                _questEditor.Clear();
        }
    }

    public class H_RewardListEditor
    {
        ReorderableList _reorderableList;
        List<string> _list = new List<string>() { "reward a", "reward b", "reward c" };

        public void Start()
        {
            _reorderableList = new ReorderableList(_list, typeof(string))
            {
                drawHeaderCallback = OnDrawHeaderHandler,
                drawElementCallback = OnDrawElementHandler
            };
        }

        public void Draw(string label = "Conditions")
        {
            /*EditorGUILayout.LabelField(label);*/

            _reorderableList.DoLayoutList();

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
            EditorGUI.LabelField(rect, _list[index]);
        }
    }

    public class H_RewardEditor
    {
        public void Draw()
        {
            EditorGUILayout.LabelField("Reward field");
        }
    }
}