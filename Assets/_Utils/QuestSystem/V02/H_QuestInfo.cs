using System.Xml.Serialization;

namespace H_QuestSystem
{
    public class QuestInfo
    {
        [XmlElement("n")]
        public string Name { get; set; }
        [XmlElement("d")]
        public string Description { get; set; }

        public QuestInfo Clone()
        {
            QuestInfo clone = new QuestInfo();
            clone.Name = Name;
            clone.Description = Description;

            return clone;
        }
    }
}