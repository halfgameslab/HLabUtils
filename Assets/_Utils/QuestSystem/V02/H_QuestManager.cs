using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace H_QuestSystem
{
    public class H_QuestManager : MonoBehaviour
    {
        public TextAsset[] _questsFiles;

        public Dictionary<string, H_Quest> QuestTable { get; private set; }

        private void OnEnable()
        {
            ES_EventManager.AddEventListener("CVarSystem", "OnLoadComplete", OnCVarSystemLoadCompleteHandler);
        }

        private void OnDisable()
        {
            ES_EventManager.RemoveEventListener("CVarSystem", "OnLoadComplete", OnCVarSystemLoadCompleteHandler);
        }

        private void OnCVarSystemLoadCompleteHandler(ES_Event ev)
        {
            H_QuestVar v = new H_QuestVar() { Fullname = CVarSystem.GetFullName<float>("new_float"), Command = "EQUAL", Value = 5f, StartValue = 0f, ID = "v01" };
            v.AddEventListener(ES_Event.ON_COMPLETE, (ES_Event e) => Debug.Log("Complete"));
            v.Start((ES_Event e) => Debug.Log("Complete"));

            H_QuestVar v2 = new H_QuestVar() { Fullname = CVarSystem.GetFullName<Vector3>("teste"), Command = "EQUAL", Value = new Vector3(55,1,1), StartValue = Vector3.zero, ID = "v01" };
            v2.AddEventListener(ES_Event.ON_COMPLETE, (ES_Event e) => Debug.Log("Complete2"));
            v2.Start((ES_Event e) => Debug.Log("Complete2"));
            //v.Invoke();
        }

        void Awake()
        {
            


            /*H_Quest[] quests = M_XMLFileManager.Deserialize<H_Quest[]>(_questsFiles[0].text);

            foreach(H_Quest quest in quests)
            {
                Add(quest);
            }*/

            // carregar as quests
            // ler o arquivo persistent
            // Configurar as quests de acordo com o arquivo persistent e seus valores padrões
        }

        public H_Quest[] GetAllActivatedQuests()
        {
            H_Quest[] quests = new H_Quest[QuestTable.Values.Count];

            QuestTable.Values.CopyTo(quests, 0);

            return quests;
        }

        public H_Quest GetQuestByID(string id)
        {
            return QuestTable.TryGetValue(id, out H_Quest quest) ? quest : null;
        }

        private void Add(H_Quest quest)
        {
            QuestTable.Add(quest.ID, quest);
        }

        private void Play()
        {

        }
    }
}
