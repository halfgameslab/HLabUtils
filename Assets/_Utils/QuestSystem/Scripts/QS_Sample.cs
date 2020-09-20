using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.QuestSystem
{
    public class QS_Sample : MonoBehaviour
    {
        private void OnEnable()
        {
            QS_QuestManager.Instance.AddEventListener(ES_Event.ON_COMPLETE, OnQuestCompleteHandler);
            QS_QuestManager.Instance.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnQuestValueChangeHandler);
        }

        private void OnDisable()
        {
            QS_QuestManager.Instance.RemoveEventListener(ES_Event.ON_COMPLETE, OnQuestCompleteHandler);
            QS_QuestManager.Instance.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, OnQuestValueChangeHandler);            
        }

        private void OnQuestCompleteHandler(ES_Event ev)
        {
            QS_Quest q = ev.Data as QS_Quest;

            print(ev.EventName+" "+q.ID + " " + q.Event + " " + q.Amount);
        }

        private void OnQuestValueChangeHandler(ES_Event ev)
        {
            QS_Quest q = ev.Data as QS_Quest;
            
            print(ev.TargetIdentifier + " " + ev.EventName + " " + q.ID + " " + q.Event + " " + q.Amount);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.I))
            {
                print("Clicou");
                ES_EventManager.DispatchEvent("Tutorial", ES_Event.ON_COMPLETE, null,1);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                print("Clicou");
                ES_EventManager.DispatchEvent("Player", ES_Event.ON_COMPLETE, null, 1);
            }
        }
    }
}