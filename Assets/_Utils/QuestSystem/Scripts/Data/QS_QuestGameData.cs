using Mup.QuestSystem;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class QS_QuestData
{
    [XmlAttribute("id")]
    public string ID { get; set; }

    [XmlAttribute("amount")]
    public int Amount { get; set; }

    [XmlAttribute("goal")]
    public int Goal { get; set; }

    [XmlAttribute("start_time")]
    public int StartTime { get; set; }

    [XmlAttribute("collected")]
    public bool Collected { get; set; }//reward index 1 2 4 8 16
}

[XmlRoot("saved_quests"), System.Serializable]
public class QS_QuestGameData
{
    [XmlArray("quests")]
    [XmlArrayItem("quest")]
    public QS_QuestData[] Quests { get; set; }

    public static QS_QuestGameData Load()
    {
        return M_XMLFileManager.Load<QS_QuestGameData>(System.IO.Path.Combine(Application.persistentDataPath,"saved_quests.xml"));
    }

    public static void Save(QS_QuestGameData data)
    {
        M_XMLFileManager.Save<QS_QuestGameData>(System.IO.Path.Combine(Application.persistentDataPath, "saved_quests.xml"), data);
    }

    public static void Save(Dictionary<string, QS_Quest> data)
    {
        int i = 0;
        QS_QuestData[] questDataList = new QS_QuestData[data.Values.Count];
        
        foreach (QS_Quest quest in data.Values)
        {
            QS_QuestData questData = new QS_QuestData()
            {
                ID = quest.ID,
                Amount = quest.Amount,
                Collected = quest.WasCollected,
                Goal = quest.Goal
            };
            questDataList[i] = questData;
            i++;
        };

        QS_QuestGameData questGameData = new QS_QuestGameData()
        {
            Quests = questDataList
        };

        Save(questGameData);
    }
}
