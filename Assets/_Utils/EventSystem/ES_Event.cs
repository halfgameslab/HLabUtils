/// <summary>
/// Event
/// V 1.0
/// Developed by: Eder Moreira
/// Copyrights: MUP Studios 2017
/// Used to support the event system
/// Define consts and sets standards
/// </summary>

using System;

namespace Mup.EventSystem.Events
{
    /// <summary>
    /// The defaut method signature for a callback
    /// </summary>
    /// <param name=""></param>
    public delegate void ES_MupAction(ES_Event ev);

    /// <summary>
    /// Single part of a event system
    /// </summary>
    public class ES_Event
    {
        //events
        public const string ON_START = "ON_START";
        public const string ON_PLAY = "ON_PLAY";
        public const string ON_PAUSE = "ON_PAUSE";
        public const string ON_RESUME = "ON_RESUME";
        public const string ON_GAME_OVER = "ON_GAME_OVER";
        public const string ON_STOP = "ON_STOP";
        public const string ON_RESTART = "ON_RESTART";
        public const string ON_COMPLETE = "ON_COMPLETE";
        public const string ON_UPDATE = "ON_UPDATE";
        public const string ON_VALUE_CHANGE = "ON_VALUE_CHANGE";
        public const string ON_LOAD = "ON_LOAD";
        public const string ON_ENABLE = "ON_ENABLE";
        public const string ON_DISABLE = "ON_DISABLE";
        public const string ON_DESTROY = "ON_DESTROY";
        public const string ON_CLOSE = "ON_CLOSE";
        public const string ON_CONFIRM = "ON_CONFIRM";
        public const string ON_CANCEL = "ON_CANCEL";

        public const string ON_SKIP = "ON_SKIP";
        public const string ON_FAIL = "ON_FAIL";
        
        public const string ON_CLICK = "ON_CLICK";
        public const string ON_COMPLETE_ACTION = "ON_COMPLETE_ACTION";

        public const string ON_ENTER = "ON_ENTER";
        public const string ON_STAY = "ON_STAY";
        public const string ON_EXIT = "ON_EXIT";

        public const string ON_COLLECT = "ON_COLLECT";
        public const string ON_COLLECT_ALL = "ON_COLLECT_ALL";

        public const string ON_MOVE = "ON_MOVE";
        public const string ON_FIRE = "ON_FIRE";
        public const string ON_HIT = "ON_HIT";

        public const string ON_BECAME_VISIBLE = "ON_BECAME_VISIBLE";
        public const string ON_BECAME_INVISIBLE = "ON_BECAME_INVISIBLE";

        public const string ON_EXECUTE_ACTION = "ON_EXECUTE_ACTION";

        public const string ON_TIMER_COMPLETE = "ON_TIMER_COMPLETE";
        
        //Modificaoes
        public const string ON_PLAYER_DEATH = "ON_PLAYER_DEATH";
        public const string ON_COLOR_CHANGE = "ON_COLOR_CHANGE";
        public const string ON_MOVE_FINISH = "ON_MOVE_FINISH";
        public const string ON_MOVE_START = "ON_MOVE_START";
        
        //Icones
        public const string ON_CHANGE = "ON_CHANGE";
        public const string ON_STATE_CHANGE = "ON_STATE_CHANGE";


        /// <summary>
        /// The event dispatcher
        /// </summary>
        public Object Target { get; private set; }

        /// <summary>
        /// The event name
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// The info sended to the listener (func param)
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// The sender identifier
        /// </summary>
        public string TargetIdentifier { get; private set; }
        
        /// <summary>
        /// Event Constructor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="eventName"></param>
        public ES_Event(Object target, string eventName, string targetIdentifier, object data = null)
        {
            this.Target = target;
            this.EventName = eventName;
            this.Data = data;
            this.TargetIdentifier = targetIdentifier;
        }
    }
}