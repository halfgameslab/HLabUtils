using Mup.EventSystem.Events;
using Mup.Misc.Generic;
using Mup.Multilanguage.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mup.QuestSystem.QuestUI
{
    public class QS_UICell : MonoBehaviour
    {
        [SerializeField] private ML_TextExtension _title;
        [SerializeField] private ML_TextExtension _description;
        [SerializeField] private Text _rewardAmount;
        [SerializeField] private Image _rewardImage;
        [SerializeField] private Image _questImage;
        [SerializeField] private ML_TextExtension _progressText;
        [SerializeField] private Image _progressBarImage;
        [SerializeField] private Button _collectButton;
        

        private QS_Quest _myQuest;

        public void UpdateDisplay(QS_Quest quest,Sprite questSprite = null)
        {
            _myQuest = quest;
            _questImage.sprite = questSprite;
            _title.Text = _myQuest.Name;
            _description.Text = _myQuest.Description;
            _progressBarImage.gameObject.SetActive(true);
            _rewardAmount.text = " x ";
            if (_myQuest.Rewards != null)
            {
                foreach (QS_Reward reward in _myQuest.Rewards)
                {
                    _rewardAmount.text += reward.ID;
                }
            }
            else
            {
                _rewardAmount.text = " Coletado ";
            }
            UpdateInfo();
            _myQuest.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChanged);
            _myQuest.AddEventListener(ES_Event.ON_COMPLETE, OnComplete);
            _myQuest.AddEventListener(ES_Event.ON_COLLECT_ALL, OnCollect);
            //_icon.sprite = Sprite;
        }

        private void OnCollect(ES_Event ev)
        {
            _collectButton.onClick = new Button.ButtonClickedEvent();
            _collectButton.gameObject.SetActive(false);
        }

        private void OnComplete(ES_Event ev)
        {
            if (_myQuest.WasCompleted)
            {
                _collectButton.gameObject.SetActive(true);
                _progressText.Text = "@generic_quest_finished";
                    _progressBarImage.fillAmount = (float)_myQuest.Amount / _myQuest.Goal;
                _progressBarImage.gameObject.SetActive(false);

                _collectButton.enabled = true;
                _collectButton.GetComponentInChildren<ML_TextExtension>().Text = "@generic_quest_collect";
                _collectButton.onClick = new Button.ButtonClickedEvent();

                if (_myQuest.Type == "level")
                {
                    _collectButton.onClick.AddListener(() => _myQuest.CollectAll());
                    _collectButton.onClick.AddListener(() => QS_QuestManager.Instance.RemoveQuest(_myQuest));
                    _collectButton.onClick.AddListener(() => QS_QuestsHandler.AddRandomQuests(3, "level"));
                }
                else
                {
                    _collectButton.onClick.AddListener(() => _myQuest.CollectAll());
                    _collectButton.onClick.AddListener(() => QS_QuestManager.Instance.SaveQuestData());
                }
            }
        }

        private void OnValueChanged(ES_Event ev)
        {
            _progressText.Text = string.Concat(_myQuest.Amount, "/", _myQuest.Goal);
                _progressBarImage.fillAmount = (float)_myQuest.Amount / _myQuest.Goal;
        }

        private void UpdateInfo()
        {
            if (_myQuest.WasCompleted)
            {
                _progressText.Text = "@generic_quest_finished";
                _progressBarImage.fillAmount = (float)_myQuest.Amount / _myQuest.Goal;
                _collectButton.gameObject.SetActive(true);
                _collectButton.interactable = true;
                if (!_myQuest.WasCollected)
                {
                    _collectButton.GetComponentInChildren<ML_TextExtension>().Text = "@generic_quest_collect";
                    _collectButton.onClick = new Button.ButtonClickedEvent();
                    _collectButton.onClick.AddListener(() => UpdateInfo());
                    _collectButton.onClick.AddListener(() => _myQuest.CollectAll());
                    if (_myQuest.Type == "level")
                    {
                        _collectButton.onClick.AddListener(() => QS_QuestManager.Instance.RemoveQuest(_myQuest));
                        _collectButton.onClick.AddListener(() => QS_QuestsHandler.AddRandomQuests(3, "level"));
                    }
                    else
                    {
                        _collectButton.onClick.AddListener(() => QS_QuestManager.Instance.SaveQuestData());
                    }
                }
                else
                {
                    _progressBarImage.gameObject.SetActive(true);
                    _progressText.Text = "@generic_quest_collected";
                    _collectButton.onClick = new Button.ButtonClickedEvent();
                    _collectButton.gameObject.SetActive(false);
                }
            }
            else if (!_myQuest.WasCollected)
            {
                _progressText.Text = string.Concat(_myQuest.Amount, "/", _myQuest.Goal);
                    _progressBarImage.fillAmount = (float)_myQuest.Amount / _myQuest.Goal;
                _collectButton.gameObject.SetActive(false);
                _collectButton.GetComponentInChildren<ML_TextExtension>().Text = "@generic_quest_incompleted";
            }
        }

        public void ShowInfo()
        {

        }

        private void OnDisable()
        {
            if (_myQuest != null)
            {
                _myQuest.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnValueChanged);
                _myQuest.RemoveEventListener(ES_Event.ON_COMPLETE, OnComplete);
                _myQuest.RemoveEventListener(ES_Event.ON_COLLECT_ALL, OnCollect);
            }
        }

    }
}