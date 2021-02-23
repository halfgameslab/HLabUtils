using HLab.H_DataSystem;
using Mup.EventSystem.Events;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HLab.H_QuestSystem.H_Editor
{
    public class H_QuestGroupEditor
    {
        public H_DataGroup<H_Quest, H_PersistentQuestData> CurrentQuestGroup { get; set; }
        public H_Quest SelectedQuest { get; set; }

        private ReorderableList _questList;
        private Vector2 _scrollPosition;

        private H_Quest _copiedQuest;

        public void Start(string currentGroup)
        {
            Start(H_QuestManager.Instance.QuestGroups.GetGroupByName(currentGroup));
        }

        public void Start(H_DataGroup<H_Quest, H_PersistentQuestData> currentGroup, H_Quest selected = null)
        {
            if (CurrentQuestGroup != currentGroup)
            {
                CurrentQuestGroup = currentGroup;

                if (currentGroup != null)
                    _questList = new ReorderableList(currentGroup.Data, typeof(H_Quest), true, true, true, true)
                    {
                        drawHeaderCallback = OnDrawHeaderHandler,
                        drawElementCallback = OnDrawElementHandler,
                        onAddCallback = OnAddElementHandler,
                        onRemoveCallback = OnRemoveElementHandler,
                        onSelectCallback = OnSelectElementHandler,
                        onReorderCallbackWithDetails = OnReorderListHandler
                    };
                else
                    _questList = null;

                _scrollPosition = Vector2.zero;
                SelectedQuest = selected;

                if (selected != null)
                    _questList.index = currentGroup.Data.IndexOf(selected);
            }
        }

        public void Draw()
        {
            if (CurrentQuestGroup == null)
                return;

            float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(firstCollumWidth));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUILayout.MinWidth(firstCollumWidth), GUILayout.MaxWidth(firstCollumWidth));

            _questList?.DoLayoutList();

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void SelectQuest(int index)
        {
            _questList.index = index;
            SelectedQuest = CurrentQuestGroup.Data[index];
            this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[index]);
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            EditorGUI.LabelField(rect, string.Format("Quests {0}", CurrentQuestGroup.Name));
            rect.x += rect.width - 40;
            rect.width = 20;
            EditorGUI.BeginDisabledGroup(SelectedQuest == null);
            if (GUI.Button(rect, new GUIContent("C", "Copy Quest")))
            {
                _copiedQuest = SelectedQuest;
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;
            EditorGUI.BeginDisabledGroup(_copiedQuest == null);

            if (GUI.Button(rect, new GUIContent("P", "Paste Quest")))
            {
                H_Quest clone = _copiedQuest.Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString());

                clone.Group = null; // remove group to avoid name error
                clone.UName = _copiedQuest.UName;// save the UName without changes

                if (_questList.index >= 0 && _questList.index < CurrentQuestGroup.Data.Count)
                {
                    CurrentQuestGroup.Insert(_questList.index + 1, clone);
                    clone.CheckAndUpdateUName();// process a new UName for the clone avoiding duplicated names in the group
                    _questList.index++;
                }
                else
                {
                    CurrentQuestGroup.Add(clone);
                    clone.CheckAndUpdateUName();// process a new UName for the clone avoiding duplicated names in the group
                    _questList.index = _questList.list.Count - 1;
                }

                SelectedQuest = clone;
                this.DispatchEvent(ES_Event.ON_CLICK, clone);
            }

            EditorGUI.EndDisabledGroup();
        }
        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 2;
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.3f));

            rect.width = rect.width - 20;
            GUI.Label(rect, string.Format("[#{0}][#{1}] {2}", (index + 1).ToString("#00"), CurrentQuestGroup.Data[index].UID, CurrentQuestGroup.Data[index].UName));

            rect.x += rect.width;
            rect.width = 19;
            if (GUI.Button(rect, new GUIContent("D", "Duplicate Quest")))
            {
                //duplicate quest
                CurrentQuestGroup.Insert(index + 1, CurrentQuestGroup.Data[index].Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString()));
                SelectQuest(index + 1);
                /*_questList.index = index + 1;
                this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[index+1]);*/
            }
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            H_Quest q = CurrentQuestGroup.Data[list.index];
            if (EditorUtility.DisplayDialog("Delete Quest", string.Format("Want delete quest {0}?\n\nYou can't revert this operation.", q.UName), "Delete", "Cancel"))
            {
                CurrentQuestGroup.Remove(q);
                this.DispatchEvent(ES_Event.ON_DESTROY, q);
                if (list.index > 0)
                {
                    SelectQuest(list.index - 1);
                    /*list.index = list.index - 1;
                    SelectedQuest = CurrentQuestGroup.Data[list.index];
                    this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);*/
                }
                else if (list.count > 0)
                {
                    SelectQuest(list.index);
                    //SelectedQuest = CurrentQuestGroup.Data[list.index];
                    //this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);
                }
                else
                {
                    SelectedQuest = null;
                }
            }
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            if (list.index > 0)
            {
                CurrentQuestGroup.Insert(list.index + 1, CurrentQuestGroup.Data[list.index].Clone(H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString()));
                list.index++;
            }
            else
            {
                CurrentQuestGroup.Add(
                    new H_Quest()
                    {
                        UID = H_DataManager.Instance.Address.GetNextAvaliableAddress().ToString(),
                        Group = CurrentQuestGroup,
                        UName = "quest_(0)"
                    });

                list.index = list.count - 1;
            }

            SelectQuest(list.index);
            //this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);
        }

        private void OnSelectElementHandler(ReorderableList list)
        {
            //list.index
            SelectQuest(list.index);
            /*SelectedQuest = CurrentQuestGroup.Data[list.index];
            this.DispatchEvent(ES_Event.ON_CLICK, CurrentQuestGroup.Data[list.index]);*/
        }

        private void OnReorderListHandler(ReorderableList list, int oldIndex, int newIndex)
        {
            CurrentQuestGroup.Save();
        }
    }
}
