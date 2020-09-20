using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mup.EventSystem.Events;
using Mup.Misc.Generic;

namespace Mup.Ads
{
    public class M_NewAdManager : MonoBehaviour
    {
        [SerializeField]
        protected string _androidAppKey;
        [SerializeField]
        protected string _iosAppKey;
        [SerializeField]
        private M_Timer _adDelay;

        public float DelayToNextAd()
        {
            if (_adDelay.IsPlaying)
            {
                return _adDelay.Duration - _adDelay.ElapsedTime;
            }
            return 0;
        }

        public void StartVideoAdDelay(float delay)
        {
            _adDelay.Duration = delay;
            _adDelay.Play();
        }

        public virtual bool IsReady()
        {
            return false;
        }

        public virtual void ShowAd()
        {

        }

        public void ValidateIntegration()
        {
            IronSource.Agent.validateIntegration();
        }
        
        protected virtual void OnEnable()
        {          
#if UNITY_ANDROID
            IronSource.Agent.init(_androidAppKey);
#elif UNITY_IOS
            IronSource.Agent.init(_iosAppKey);
#endif
            IronSource.Agent.shouldTrackNetworkState(true);
        }        
    }
}