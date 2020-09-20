using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.EventSystem.Events
{
    public class ES_EventTarget
    {
        //used when there isn't a object as target
        public const string GENERIC = "GENERIC_TARGET";
        public const string SYSTEM = "SYSTEM_TARGET";
        public const string ANY_GAME_OBJECT = "ANY_GAME_OBJECT";
        public const string ANY_UI_OBJECT = "ANY_UI_OBJECT";
        public const string ANY_3D_GAME_OBJECT = "ANY_3D_GAME_OBJECT";
        public const string ANY_2D_GAME_OBJECT = "ANY_2D_GAME_OBJECT";

        //
        public const string PLAYER = "PLAYER";
        public const string IAPMANGAER = "IAP_MANAGER";
    }
}