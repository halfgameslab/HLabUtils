/// <summary>
/// Quest System
/// V 1.0
/// Developed by: Eder Moreira
/// Copyrights: MUP Studios 2017
/// A basic system to manage quests
/// DEPENDECY: Mup.EventSystem.Events
/// Version: 1.1
/// QuestManager received the responsibility to listen to the quest and dispacth a result event
/// Add list of saved quests
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using Mup.EventSystem.Events;
using System;
using Mup.QuestSystem.Internal;
using UnityEngine.SceneManagement;

namespace Mup.QuestSystem
{
    /// <summary>
    /// Manage the execution of quest
    /// </summary>
    [ScriptOrder(-30000)]
    public class QS_QuestManager : MonoBehaviour
    {
        /// <summary>
        /// poor singleton
        /// </summary>
        public static QS_QuestManager Instance { get; private set; }
        
        /// <summary>
        /// file with the quest list
        /// </summary>
        [SerializeField] private TextAsset _questData;

        /// <summary>
        /// table with quest info
        /// </summary>
        public Dictionary<string, QS_Quest> QuestTable { get; private set; }

        /// <summary>
        /// table with started quests
        /// </summary>
        public Dictionary<string, QS_Quest> StartedQuestsTable { get; private set; }

        public QS_QuestGameData SavedQuests;
        
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
            ReadAndStartQuests();
        }

        private void ReadAndStartQuests()
        {
            //never be changed after load
            QuestTable = QS_QuestLoader.Deserialize(_questData.text);
            StartedQuestsTable = new Dictionary<string, QS_Quest>();

            //read the started quest file
            QS_QuestGameData storedQuests = QS_QuestGameData.Load();
            if (storedQuests == null)
            {
                print("Will Init Quests");
                Init();
            }

            if (storedQuests != null)
            {

                print("loading quests");

                //start quests loaded from save file
                foreach (QS_QuestData quest in storedQuests.Quests)
                {
                    if (quest != null)
                    {
                        //start quest using data
                        StartQuest(quest);
                    }
                }

            }
            if (QuestTable.Count != storedQuests.Quests.Length)
            {
                print("missing quests init Again");
                Init();
            }
            SavedQuests = storedQuests;
        }

        private void OnDisable()
        {
            if (StartedQuestsTable != null)
            {
                foreach (KeyValuePair<string, QS_Quest> quest in StartedQuestsTable)
                {
                    if (UserData.Instance != null)
                    {
                        UserData.Instance.SaveData(quest.Value.Amount, quest.Value.ID);
                        UserData.Instance.SaveData(quest.Value.WasCollected ? 1 : 0, quest.Value.ID + "Collect");
                        //start quest using data
                    }

                    RemoveListeners(quest.Value);
                }
            }
        }

        public void UpdateQuestsInfoFromSave()
        {
            StartedQuestsTable = new Dictionary<string, QS_Quest>();
            Init();
            foreach (KeyValuePair<string, QS_Quest> quest in StartedQuestsTable)
            {
                if (PlayerPrefs.HasKey(quest.Value.ID))
                {
                    quest.Value.Reset();
                }
                int collect = PlayerPrefs.GetInt(quest.Value.ID+"Collect", 0);
                if (collect == 1)
                {
                    quest.Value.CollectAll();
                }
                quest.Value.Amount = PlayerPrefs.GetInt(quest.Value.ID,0);
                SaveQuestData();
            }
        }

        [ContextMenu("Init Quest")]
        private void Init()
        {
            if(QuestTable == null)
            {
                QuestTable = QS_QuestLoader.Deserialize(_questData.text);
            }
            foreach(QS_Quest quest in QuestTable.Values)
            {
                StartQuest(quest.ID);
            }
        }

        /// <summary>
        /// Start quest by ID
        /// </summary>
        /// <param name="questID">quest id (verify the quest_data.xml file for know ids)</param>
        public void StartQuest(string questID)
        {
            //if quest not started
            if(!StartedQuestsTable.ContainsKey(questID) && QuestTable.ContainsKey(questID))
            {
                //active quest
                ActiveQuest(QuestTable[questID].Clone());
            }
            else
            {
                if (StartedQuestsTable.ContainsKey(questID))
                    Debug.LogWarning(string.Concat("Quest ", questID," already started"));
                else Debug.LogWarning(string.Concat("Quest ", questID," not found!"));
            }
        }

        /// <summary>
        /// Start quest using quest data
        /// </summary>
        /// <param name="quest">quest data</param>
        public void StartQuest(QS_QuestData quest)
        {
            //verify if the quest exists
            if (QuestTable != null && QuestTable.ContainsKey(quest.ID))
            {
                //clone quest from list
                QS_Quest questData = QuestTable[quest.ID].Clone();
                //update quest info from quest data saved
                questData.Update(quest);
                //active quest
                ActiveQuest(questData);
            }
            else
            {
                Debug.LogWarning(string.Concat("Quest ", quest.ID, " not found!"));
            }
        }
        
        /// <summary>
        /// look for quest by the identifier
        /// </summary>
        /// <param name="id">quest identifier</param>
        /// <returns></returns>
        public QS_Quest GetStartedQuestByID(string id)
        {
            return (this.StartedQuestsTable.ContainsKey(id)) ? this.StartedQuestsTable[id] : null;//_questList.Find(x => x.ID == id);
        }

        /// <summary>
        /// look for original quest info by the identifier
        /// </summary>
        /// <param name="id">quest identifier</param>
        /// <returns></returns>
        public QS_Quest GetQuestByID(string id)
        {
            return (this.QuestTable.ContainsKey(id)) ? this.QuestTable[id] : null;//_questList.Find(x => x.ID == id);
        }

        public void SaveQuestData()
        {
            QS_QuestGameData.Save(StartedQuestsTable);
        }

        /// <summary>
        /// Get quest list original info by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public QS_Quest[] GetQuestListByType(string type, bool ignoreDeactive = false)
        {
            //create one list to store the quests
            List<QS_Quest> questList = new List<QS_Quest>();

            //for each quest in original quest table
            foreach(QS_Quest quest in this.QuestTable.Values)
            {
                //verify if quest type was equal to type param
                if(quest.Type == type && (!ignoreDeactive || quest.IsActive))
                {
                    //add finded quests
                    questList.Add(quest);
                }
            }

            //return the array with the infos
            return questList.ToArray();
        }

        /// <summary>
        /// Get quest active quest list original info by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public QS_Quest[] GetActiveQuestListByType(string type, bool ignoreDeactive = false)
        {
            //create one list to store the quests
            List<QS_Quest> questList = new List<QS_Quest>();

            //for each quest in original quest table
            foreach (QS_Quest quest in this.StartedQuestsTable.Values)
            {
                //verify if quest type was equal to type param
                if (quest.Type == type && (!ignoreDeactive || quest.IsActive))
                {
                    //add finded quests
                    questList.Add(quest);
                }
            }

            //return the array with the infos
            return questList.ToArray();
        }

        /// <summary>
        /// Remove listeners and remove the quest from started table.
        /// </summary>
        /// <param name="quest"></param>
        public void RemoveQuest(QS_Quest quest)
        {
            DeactiveQuest(quest);
            if (StartedQuestsTable.ContainsKey(quest.ID))
            {
                StartedQuestsTable.Remove(quest.ID);
            }
        }


        /// <summary>
        /// Add and Active quest
        /// </summary>
        /// <param name="quest"></param>
        private void ActiveQuest(QS_Quest quest)
        {
            if (!quest.IsActive)
            {
                if ((!quest.WasCompleted || !quest.WasCollected))
                {
                    //sempre que uma quest entrar no sistema ela será ativada
                    quest.AddEventListener(ES_Event.ON_ENABLE, OnQuestEnableHandler);
                    quest.AddEventListener(ES_Event.ON_DISABLE, OnQuestDisableHandler);
                    
                    quest.SetActive(true);
                }
                else
                {
                    if (quest.WasCompleted || quest.WasCollected)
                        Debug.LogWarning(string.Concat("The quest ", quest.ID, " was complete and could not be actived."));
                }
                if(!StartedQuestsTable.ContainsKey(quest.ID))
                {
                    StartedQuestsTable.Add(quest.ID, quest);
                }
                QS_QuestGameData.Save(StartedQuestsTable);
            }
        }

        /// <summary>
        /// Deactive and remove quest
        /// </summary>
        /// <param name="quest"></param>
        public void DeactiveQuest(QS_Quest quest)
        {
            RemoveListeners(quest);
            //deactive quest
            quest.SetActive(false);
            QS_QuestGameData.Save(StartedQuestsTable);
        }

        /// <summary>
        /// Deactive and remove quest by id
        /// </summary>
        /// <param name="id">quest identifier</param>
        public void DeactiveQuest(string id)
        {
            //look for the quest
            QS_Quest quest = GetStartedQuestByID(id);

            //if there is that quest
            if (quest != null)
                DeactiveQuest(quest);//remove
        }
        
        /// <summary>
        /// Remove all listeners from one quest
        /// </summary>
        /// <param name="quest"></param>
        private void RemoveListeners(QS_Quest quest)
        {
            quest.RemoveEventListener(ES_Event.ON_ENABLE, OnQuestEnableHandler);
            quest.RemoveEventListener(ES_Event.ON_DISABLE, OnQuestDisableHandler);

            if (quest.IsActive)
            {
                quest.RemoveEventListener(ES_Event.ON_COLLECT, OnQuestCollectHandler);
                quest.RemoveEventListener(ES_Event.ON_COLLECT_ALL, OnQuestCollectHandler);
                quest.RemoveEventListener(ES_Event.ON_COMPLETE, OnQuestCompleteHandler);
                quest.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnQuestValueChangeHandler);
            }
        }

        /// <summary>
        /// Add main listeners from one quest
        /// </summary>
        /// <param name="quest"></param>
        private void AddListeners(QS_Quest quest)
        {
            quest.AddEventListener(ES_Event.ON_COLLECT, OnQuestCollectHandler);
            quest.AddEventListener(ES_Event.ON_COLLECT_ALL, OnQuestCollectHandler);
            quest.AddEventListener(ES_Event.ON_COMPLETE, OnQuestCompleteHandler);
            quest.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnQuestValueChangeHandler);
        }

        /// <summary>
        /// Quest Handler
        /// </summary>
        /// <param name="ev"></param>
        private void OnQuestEnableHandler(ES_Event ev)
        {
            AddListeners(ev.Data as QS_Quest);

            this.DispatchEvent(ES_Event.ON_ENABLE, ev.Target);
        }

        /// <summary>
        /// Quest Handler
        /// </summary>
        /// <param name="ev"></param>
        private void OnQuestDisableHandler(ES_Event ev)
        {
            RemoveListeners(ev.Data as QS_Quest);
            this.DispatchEvent(ES_Event.ON_DISABLE, ev.Target);
        }

        /// <summary>
        /// Quest Handler
        /// </summary>
        /// <param name="ev"></param>
        private void OnQuestCompleteHandler(ES_Event ev)
        {
            if (this != null)
            {
                this.DispatchEvent(ES_Event.ON_COMPLETE, ev.Target);
            }
        }

        /// <summary>
        /// Quest Handler
        /// </summary>
        /// <param name="ev"></param>
        private void OnQuestValueChangeHandler(ES_Event ev)
        {
            SaveQuestData();
            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ev.Target);
        }

        /// <summary>
        /// Quest Handler
        /// </summary>
        /// <param name="ev"></param>
        private void OnQuestCollectHandler(ES_Event ev)
        {
            SaveQuestData();
            print("Quest Saved due Colect reward on "+ Application.persistentDataPath);
            this.DispatchEvent(ES_Event.ON_COLLECT, ev.Target);
        }
    }
}