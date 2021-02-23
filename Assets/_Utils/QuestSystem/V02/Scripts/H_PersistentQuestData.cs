using HLab.H_DataSystem;

namespace HLab.H_QuestSystem 
{
    public class H_PersistentQuestData : H_Cloneable<H_PersistentQuestData>, H_Processable<H_PersistentQuestData>, H_Groupable<H_Quest, H_PersistentQuestData>
    {
        public H_DataGroup<H_Quest, H_PersistentQuestData> Group { get; set; }

        public H_PersistentQuestData Clone(string cloneUID)
        {
            return this;
        }

        public void Process()
        {

        }
    }
}