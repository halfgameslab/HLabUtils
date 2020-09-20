using System;
using Mup.EventSystem.Events;
using Mup.QuestSystem;
using Mup.ShopSystem;
using UnityEngine;

[ScriptOrder(-500)]
public class UserData : MonoBehaviour
{
    public static UserData Instance;

    public int Level
    {
        get
        {
            if (Stars >= 34)
            {
                return 8;
            }
            else if (Stars >= 28)
            {
                return 7;
            }
            else if (Stars >= 22)
            {
                return 6;
            }
            else if (Stars >= 16)
            {
                return 5;
            }
            else if (Stars >= 10)
            {
                return 4;
            }
            else if (Stars >= 4)
            {
                return 3;
            }
            else if (Stars >= 2)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
    }

    public int Stars
    {
        get { return PlayerPrefs.GetInt("UserStars", 0); }
        private set { SaveData(value, "UserStars"); }
    }

    public int TotalStarsAvaliable { get { return PlayerPrefs.GetInt("TotalStarsAvaliable", 0); } }

    //private M_WebDataManager _dataManager;
    private QS_QuestManager _questManager;

    public void AddStar(int amount = 1)
    {
        Stars += amount;
    }

    public void Reset()
    {
        Stars = 0;
        PlayerPrefs.DeleteKey("UserStars");
        PlayerPrefs.DeleteKey("UserLevel");
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            //_dataManager = FindObjectOfType<M_WebDataManager>();
            //if (_dataManager != null)
            //{
            //    if (_dataManager.User != null)
            //    {
            //        PlayerPrefs.SetString("_userData", _dataManager.User.Numcad);
                    
            //        if (_dataManager.User.Progress != null)
            //        {
            //            foreach (var save in _dataManager.User.Progress)
            //            {
            //                int debug = 0;
            //                if (int.TryParse((save.Value * 10).ToString(), out debug))
            //                {
            //                    PlayerPrefs.SetInt(save.Key, debug);
            //                }
            //            }

            //            foreach (var save in _dataManager.User.Equip)
            //            {
            //                PlayerPrefs.SetString(save.Key, save.Value);
            //            }
            //        }
            //    }
            //}
            QS_QuestManager.Instance.SetInstanceName("QuestManager");
            if (!IS_InstanceManager.GetInstanceByID("QuestManager").HasEventListener(ES_Event.ON_COLLECT, M_ShopManager.Instance.OnQuestCollectHandler))
            {
                IS_InstanceManager.GetInstanceByID("QuestManager").AddEventListener(ES_Event.ON_COLLECT, M_ShopManager.Instance.OnQuestCollectHandler);
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //(this.gameObject);
            //_dataManager = null;
        }
    }

    public void InitSavedData()
    {
        QS_QuestManager.Instance.SetInstanceName("QuestManager");
        if (!IS_InstanceManager.GetInstanceByID("QuestManager").HasEventListener(ES_Event.ON_COLLECT, M_ShopManager.Instance.OnQuestCollectHandler))
        {
            IS_InstanceManager.GetInstanceByID("QuestManager").AddEventListener(ES_Event.ON_COLLECT, M_ShopManager.Instance.OnQuestCollectHandler);
        }

        QS_QuestManager.Instance.UpdateQuestsInfoFromSave();
    }

    public void SaveData(int progress,string treinamento)
    {
        //if (_dataManager != null)
        //{
        //    if (_dataManager.User != null)
        //    {
        //        if (_dataManager.User.Numcad != "")
        //        {
        //            _dataManager.SetProgress(DateTime.Now, (progress/10f).ToString(), treinamento);
        //        }
        //    }
        //}
        PlayerPrefs.SetInt(treinamento, progress);
    }

    public void SaveData(string progress, string treinamento)
    {
        //if (_dataManager != null)
        //{
        //    if (_dataManager.User != null)
        //    {
        //        if (_dataManager.User.Numcad != "")
        //        {
        //            _dataManager.SetProgress(DateTime.Now, progress.ToString(), treinamento);
        //        }
        //    }
        //}
        PlayerPrefs.SetString(treinamento, progress);

    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            print(Instance.ToString());
            print(Instance);
            
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            print(_dataManager);
            print(_dataManager.ToString());



        }
    }*/

}