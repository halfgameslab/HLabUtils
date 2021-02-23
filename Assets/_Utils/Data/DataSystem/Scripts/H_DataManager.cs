using Mup.EventSystem.Events;
using UnityEngine;

namespace HLab.H_DataSystem
{
    public sealed class H_DataManager
    {
        private static H_DataManager _instance;

        public static H_DataManager Instance { get { return _instance ?? (_instance = new H_DataManager()); } }
        private H_DataManager() { /*block external initialization*/ }

        public bool CanLoadRuntimeDefault { get; set; }

        public H_AddressManager Address { get; set; } = new H_AddressManager();

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            Instance.Start();
        }

        public void Start()
        {
            Address.Load();
        }

        public void LoadGroupList<T, K>(string filename) where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
        {
            if (!Address.WasLoaded)
            {
                Address.AddEventListener(ES_Event.ON_LOAD, (ev) => Load<T, K>(filename));
                Address.Load();
            }
            else
            {
                Load<T, K>(filename);
            }
        }

        public void Load<T, K>(string filename) where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
        {
            if (CanLoadRuntimeDefault && System.IO.File.Exists(ParsePersistentDefaultDataPathWith(filename)))
                // load groups file        
                M_XMLFileManager.NewLoad<H_DataGroup<T, K>[]>(ParsePersistentDefaultDataPathWith(filename), (ev) => this.DispatchEvent(ES_Event.ON_LOAD, ev));
            else
                M_XMLFileManager.NewLoad<H_DataGroup<T, K>[]>(ParseStreamingDefaultDataPathWith(filename), (ev) => this.DispatchEvent(ES_Event.ON_LOAD, ev));
        }

        public static void SaveGroupListToFile<T>(string filename, T[] groups, bool useRuntimeDefault = false)
        {
            M_XMLFileManager.Save(ParseDataPathWith(filename, useRuntimeDefault), groups);
        }


        /// <summary>
        /// Used after build
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string ParsePersistentDefaultDataPathWith(string filename)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "Data", "Default", filename);
        }

        /// <summary>
        /// Used after build
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string ParsePersistentDataPathWith(string filename)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "Data", "Persistent", filename);
        }

        /// <summary>
        /// Used on editor
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string ParseStreamingDefaultDataPathWith(string filename)
        {
            return System.IO.Path.Combine(Application.streamingAssetsPath, "Data", filename);
        }

        public static string ParseDataPathWith(string filename, bool useRuntimeDefault = false)
        {
            if (!useRuntimeDefault)
                return ParseStreamingDefaultDataPathWith(filename);

            return ParsePersistentDefaultDataPathWith(filename);
        }
    }
}
