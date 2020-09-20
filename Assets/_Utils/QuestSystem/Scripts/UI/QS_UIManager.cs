using Mup.EventSystem.Events;
using Mup.Misc.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mup.QuestSystem.QuestUI
{
    public class QS_UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _questInfoUIPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private string[] _questType = new string[] { "daily", "level", "achievement" };
        [SerializeField] private M_UIAbaGroup _abaGroup;
        [SerializeField] private Sprite[] _questImages;

        private int _selected;

        public void Awake()
        {
            //this.SetInstanceName("QS_UIManager");
        }

        public void OnEnable()
        {
            this.AddEventListener("RELOAD_UI", OnRealoadUIHandler);
        }
        private void OnDisable()
        {
            this.RemoveEventListener("RELOAD_UI", OnRealoadUIHandler);
        }

        private void OnRealoadUIHandler(ES_Event ev)
        {
            Show(_selected);
        }

        public void Show(int id)
        {
            if (id < _questType.Length)
            {
                ShowPanel(_questType[id]);
                _selected = id;
            }
            _abaGroup.Select(id);

            gameObject.SetActive(true);
        }

        private void ShowPanel(string type)
        {
            Clear();

            QS_Quest[] questList = QS_QuestManager.Instance.GetActiveQuestListByType(type, false);

            foreach(QS_Quest quest in questList)
            {
                QS_UICell cell = M_ObjectPool.Instance.Create(_questInfoUIPrefab).GetComponent<QS_UICell>();
                if(quest.Type == "achievement")
                {
                    cell.UpdateDisplay(quest, _questImages[2]);
                }
                else if (quest.Type == "level")
                {
                    cell.UpdateDisplay(quest, _questImages[1]);
                }
                else if (quest.Type == "daily")
                {
                    cell.UpdateDisplay(quest, _questImages[0]);
                }
                else
                {
                    cell.UpdateDisplay(quest);
                }
                cell.transform.SetParent(_container);
                cell.transform.localScale = _questInfoUIPrefab.transform.localScale;
            }
        }
        
        public void Clear()
        {
            for (int i = _container.childCount - 1; i >= 0; i--)
            {
                _container.GetChild(i).gameObject.Store();
            }
        }
    }
}