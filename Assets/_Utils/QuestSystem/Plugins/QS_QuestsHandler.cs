using Mup.EventSystem.Events;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace Mup.QuestSystem
{
    public class QS_QuestsHandler : MonoBehaviour
    {
        [SerializeField] private string[] _questsIds;
        private DateTime _actualTime;

        private TimeSpan _remainingTime;

        public string DisplayTime { get { return _actualTime.ToString(); } }

        public string RemainingTime { get { return _remainingTime.ToString(); } }

        private int _today = 0;

        private void Start()
        {
            StartCoroutine(I_GetRealTimeFromInternet());
            _today = PlayerPrefs.GetInt("MUP_LastDay");
            if(_today == 0)
            {
                _today = _actualTime.Day;
            }
            if (QS_QuestManager.Instance.StartedQuestsTable.Count == 0)
            {
                foreach (string quests in _questsIds)
                {
                    QS_QuestManager.Instance.StartQuest(quests);
                }
                AddDailyQuests();
                LoadAllAchievements();
                print("QUESTS BASE INITIALIZED");
            }
            StartCoroutine(I_CheckTime());
        }

        private IEnumerator I_CheckTime()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(1);
                _actualTime = _actualTime.AddSeconds(1);
                _remainingTime = TimeSpan.FromHours(24) - _actualTime.TimeOfDay;
                if (_today != _actualTime.Day)
                {
                    PlayerPrefs.SetInt("MUP_LastDay", _actualTime.Day);
                    _today = _actualTime.Day;
                    RestartDailyQuest();
                }
            }
        }

        IEnumerator I_GetRealTimeFromInternet()
        {
            UnityWebRequest www = UnityWebRequest.Get("http://www.google.com");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                _actualTime = DateTime.Now;
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                string todaysDates = www.GetResponseHeader("date");
                _actualTime = DateTime.ParseExact(todaysDates,
                           "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                           CultureInfo.InvariantCulture.DateTimeFormat,
                           DateTimeStyles.AssumeUniversal);
            }
        }

        private void RestartDailyQuest()
        {
            QS_Quest[] dailyQuests = QS_QuestManager.Instance.GetActiveQuestListByType("daily");
            foreach (QS_Quest daily in dailyQuests)
            {
                daily.Reset();
            }
            QS_QuestManager.Instance.SaveQuestData();
        }

        public static void AddDailyQuests()
        {
            QS_Quest[] dailyQuests = QS_QuestManager.Instance.GetQuestListByType("daily");
            foreach (QS_Quest daily in dailyQuests)
            {
                QS_QuestManager.Instance.StartQuest(daily.ID);
            }
        }

        public static void LoadAllAchievements()
        {
            QS_Quest[] achievements = QS_QuestManager.Instance.GetQuestListByType("achievement");
            foreach (QS_Quest achievement in achievements)
            {
                QS_QuestManager.Instance.StartQuest(achievement.ID);
            }
        }

        public static void AddRandomQuests(int amount,string type)
        {
            QS_Quest[] questsToAdd = QS_QuestManager.Instance.GetQuestListByType(type);
            while (QS_QuestManager.Instance.GetActiveQuestListByType(type,true).Length < amount)
            {
                QS_QuestManager.Instance.StartQuest(questsToAdd[UnityEngine.Random.Range(0, questsToAdd.Length)].ID);
                //ES_EventManager.DispatchEvent("QS_UIManager", "RELOAD_UI");
            }
        }
    }
}