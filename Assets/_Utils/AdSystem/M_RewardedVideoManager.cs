using UnityEngine;
using Mup.EventSystem.Events;
using System;

namespace Mup.Ads
{
    public class M_RewardedVideoManager : M_NewAdManager
    {
        private static M_RewardedVideoManager _instance;
        public static M_RewardedVideoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<M_RewardedVideoManager>();
                }
                return _instance;
            }
        }

        [SerializeField]
        private string _placementName;


        public override bool IsReady()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public override void ShowAd()
        {
            if (_placementName != "")
            {
                IronSource.Agent.showRewardedVideo(_placementName);
            }
            else
            {
                IronSource.Agent.showRewardedVideo();
            }
        }

        void Awake()
        {
            if (Instance == this)
            {
                gameObject.name = "RewardedVideoManager";
                this.SetInstanceName(M_AdTarget.REWARD_VIDEO_MANAGER);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject, 5f);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
        }
        //Invoked when the RewardedVideo ad view has opened.
        //Your Activity will lose focus. Please avoid performing heavy 
        //tasks till the video ad will be closed.
        void RewardedVideoAdOpenedEvent()
        {
            print("Ad Opened");
            this.DispatchEvent(M_AdEvents.ON_OPEN);
        }
        //Invoked when the RewardedVideo ad view is about to be closed.
        //Your activity will now regain its focus.
        void RewardedVideoAdClosedEvent()
        {
            print("Ad video closed");
            this.DispatchEvent(M_AdEvents.ON_COMPLETE);
        }
        //Invoked when there is a change in the ad availability status.
        //@param - available - value will change to true when rewarded videos are available. 
        //You can then show the video by calling showRewardedVideo().
        //Value will change to false when no videos are available.
        void RewardedVideoAvailabilityChangedEvent(bool available)
        {
            print("Alou");
            //Change the in-app 'Traffic Driver' state according to availability.
            bool rewardedVideoAvailability = available;
        }
        //Invoked when the video ad starts playing.
        void RewardedVideoAdStartedEvent()
        {
            print("Ad Video Started");
            this.DispatchEvent(M_AdEvents.ON_START);
        }
        //Invoked when the video ad finishes playing.
        void RewardedVideoAdEndedEvent()
        {
            print("Ad Video finished");
            this.DispatchEvent(M_AdEvents.ON_VIDEO_ENDED);
        }
        //Invoked when the user completed the video and should be rewarded. 
        //If using server-to-server callbacks you may ignore this events and wait for 
        //the callback from the ironSource server.
        //@param - placement - placement object which contains the reward data
        void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
        {
            print("Giving Reward:" + placement.ToString());
            this.DispatchEvent(M_AdEvents.ON_REWARD);
        }
        //Invoked when the Rewarded Video failed to show
        //@param description - string - contains information about the failure.
        void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            print("AD Error: " + error.ToString());
            this.DispatchEvent(M_AdEvents.ON_FAIL);
        }
    }
}