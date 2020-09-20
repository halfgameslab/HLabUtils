using Mup.QuestSystem;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Mup.QuestSystem.Internal
{
    public class QS_QuestData
    {
        [XmlAttribute("id")]
        public string ID{ get; set; }
        [XmlAttribute("goal")]
        public int Goal { get; set; }
        [XmlAttribute("reward_id")]
        public string RewardID { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlElement("sender")]
        public string Sender { get; set; }
        [XmlElement("event")]
        public string Event { get; set; }
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("desc")]
        public string Desc { get; set; }
    }

    [XmlRoot("quest_data")]
    public class QS_QuestList
    {
        [XmlArray("quests")]
        [XmlArrayItem("quest")]
        public QS_QuestData[] QuestList { get; set; }
    }

    public class QS_QuestLoader
    {
        public static Dictionary<string, QS_Quest> Deserialize(string questData)
        {
            QS_QuestList ql = M_XMLFileManager.Deserialize<QS_QuestList>(questData);
            Dictionary<string, QS_Quest> q = new Dictionary<string, QS_Quest>();

            foreach(QS_QuestData qd in ql.QuestList)
            {
                string[] rewards = qd.RewardID.Split('|');
                QS_Reward[] qsRewards = new QS_Reward[rewards.Length];
                for (int i = 0; i < rewards.Length; i++)
                {
                    qsRewards[i] = new QS_Reward(rewards[i]);
                }

                QS_Quest quest = new QS_Quest()
                {
                    ID = qd.ID,
                    Goal = qd.Goal,
                    Rewards = qsRewards,
                    Name = qd.Name,
                    Description = qd.Desc,
                    EventSender = qd.Sender,
                    Type = qd.Type,
                    Event = qd.Event  
                };

                q.Add(quest.ID, quest);
            }

            return q;
        }
    }
}