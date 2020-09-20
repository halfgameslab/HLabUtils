using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.Ads
{
    public class M_BannerAdManager : MonoBehaviour
    {
        [SerializeField]
        protected string _androidAppKey;
        [SerializeField]
        protected string _iosAppKey;
        private static M_BannerAdManager _instance;
        public static M_BannerAdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<M_BannerAdManager>();
                }
                return _instance;
            }
        }

        [SerializeField]
        private string _placementName;

        public void ShowBanner()
        {
            IronSource.Agent.displayBanner();
        }

        public void HideBanner()
        {
            IronSource.Agent.hideBanner();
        }

        public void DestroyBanner()
        {
            IronSource.Agent.destroyBanner();
        }

        public void LoadBanner(IronSourceBannerSize size = IronSourceBannerSize.BANNER, IronSourceBannerPosition pos = IronSourceBannerPosition.BOTTOM)
        {
            if (_placementName != string.Empty)
            {
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, pos, _placementName);
            }
            else
            {
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, pos);
            }
        }

        void Awake()
        {
            if (Instance == this)
            {
                gameObject.name = "BannerAdManager";
                //ShowBanner();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject, 5f);
            }
        }

        protected void OnEnable()
        {
#if UNITY_ANDROID
            IronSource.Agent.init(_androidAppKey, IronSourceAdUnits.BANNER);
#elif UNITY_IOS
            IronSource.Agent.init(_iosAppKey, IronSourceAdUnits.BANNER);
#endif
            IronSource.Agent.shouldTrackNetworkState(true);
            IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
            IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
            IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;
        }

        protected void Start()
        {
            LoadBanner(IronSourceBannerSize.SMART_BANNER, IronSourceBannerPosition.BOTTOM);
        }

        //Invoked once the banner has loaded
        void BannerAdLoadedEvent()
        {
            ShowBanner();
        }
        //Invoked when the banner loading process has failed.
        //@param description - string - contains information about the failure.
        void BannerAdLoadFailedEvent(IronSourceError error)
        {
            Debug.Log(string.Concat("error: ",error.getErrorCode(), " ", error.getDescription()));
        }
        // Invoked when end user clicks on the banner ad
        void BannerAdClickedEvent()
        {
        }
        //Notifies the presentation of a full screen content following user click
        void BannerAdScreenPresentedEvent()
        {
        }
        //Notifies the presented screen has been dismissed
        void BannerAdScreenDismissedEvent()
        {
        }
        //Invoked when the user leaves the app
        void BannerAdLeftApplicationEvent()
        {
        }
    }
}