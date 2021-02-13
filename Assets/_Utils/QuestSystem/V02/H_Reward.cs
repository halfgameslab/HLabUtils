namespace H_QuestSystem
{
    public class H_Reward
    {
        public string ID { get; set; }

        public bool IsCollect { get; set; }

        public float Value { get; set; }

        public bool AutoCollect { get; set; }

        //public H_QuestElementGroup Rewards { get; set; }

        public float Collect()
        {
            // for each var in rewards
            //Rewards.Invoke();

            IsCollect = true;
            return Value;
        }
    }

}