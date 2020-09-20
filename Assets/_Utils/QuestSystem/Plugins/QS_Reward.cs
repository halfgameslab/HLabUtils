namespace Mup.QuestSystem
{
    /// <summary>
    /// support for quest system
    /// </summary>
    public class QS_Reward
    {
        public QS_Reward(string id)
        {
            ID = id;
        }

        private bool _wasCollected = false;

        /// <summary>
        /// reward identifier
        /// </summary>
        public string ID { get; set; }//reward identifier
        
        /// <summary>
        /// Tell if the reward was marked as collected
        /// </summary>
        public bool WasCollected { get { return _wasCollected; } }

        /// <summary>
        /// Mark the reward as collected
        /// </summary>
        public void Collect()
        {
            _wasCollected = true;
        }

        /// <summary>
        /// reset the reward
        /// </summary>
        public void Reset()
        {
            _wasCollected = false;
        }
    }
}