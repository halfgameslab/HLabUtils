using Mup.EventSystem.Events;

namespace H_DataSystem
{
    public class H_AddressManager
    {
        private int _currentAddress = -1;

        public string Filename { get; set; } = "current_address.xml";

        public bool WasLoaded { get { return _currentAddress != -1; } }

        public int CurrentAddress
        {
            get
            {
                return _currentAddress;
            }
            private set
            {
                if (_currentAddress != value)
                {
                    _currentAddress = value;

#if UNITY_EDITOR
                    //save
                    H_FileManager.Save(H_DataManager.ParseStreamingDefaultDataPathWith(Filename), _currentAddress.ToString());

#else
                H_FileManager.Save(ParsePersistentDefaultDataPathWith(FileName), _currentAddress.ToString());
#endif
                }
            }
        }

        public int GetNextAvaliableAddress()
        {
            CurrentAddress++;
            return CurrentAddress;
        }

        public void Load()
        {
#if UNITY_EDITOR
            H_FileManager.Load(H_DataManager.ParseStreamingDefaultDataPathWith(Filename), OnLoadCompleteHandler);
#else
            if(System.IO.File.Exists(ParsePersistentDefaultDataPathWith(filename)))
                H_FileManager.Load(ParsePersistentDefaultDataPathWith(filename), OnLoadCompleteHandler);
            else
                H_FileManager.Load(ParseStreamingDefaultDataPathWith(filename), OnLoadCompleteHandler);
#endif
        }

        public void OnLoadCompleteHandler(string obj)
        {
            if (int.TryParse(obj, out _currentAddress))
            {
                this.DispatchEvent(ES_Event.ON_LOAD, this);
            }
            else
            {
                CurrentAddress = 0;
            }
        }
    }
}