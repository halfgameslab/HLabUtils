using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System;

public class QS_QuestTag
{

}

public class QS_QuestGroup
{
    [XmlAttribute("id")]
    public string ID { get; set; }

    [XmlAttribute("require_complete_quests")]
    public int RequireCompleteQuests { get; set; }

    [XmlArray("quests")]
    [XmlArrayItem("quest")]
    public QS_QuestTag[] QuestTag { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("desc")]
    public string Desc { get; set; }

    [XmlArray("rewards")]
    [XmlArrayItem("reward")]
    public string QS_Reward { get; set; }
}

[XmlRoot("quest_group")]
public class QS_QuestGroups
{
    [XmlArray("groups")]
    [XmlArrayItem("group")]
    QS_QuestGroup[] Groups { get; set; }
}