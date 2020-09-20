using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.Ads
{
    public class M_AdEvents
    {
        public const string ON_START = "ON_START";
        public const string ON_LOAD = "ON_LOAD";
        public const string ON_FAIL_LOAD = "ON_FAIL_LOAD";
        public const string ON_OPEN = "ON_OPEN";
        public const string ON_CLOSE = "ON_CLOSE";
        public const string ON_COMPLETE = "ON_COMPLETE";
        public const string ON_SKIP = "ON_AD_SKIP";
        public const string ON_FAIL = "ON_FAIL";
        public const string ON_REWARD = "ON_REWARD";
        public const string ON_VIDEO_ENDED = "ON_VIDEO_ENDED";
        public const string ON_CLICKED = "ON_CLICKED";
    }
    public class M_AdTarget
    {
        public const string REWARD_VIDEO_MANAGER = "REWARD_VIDEO_MANAGER";
    }
}