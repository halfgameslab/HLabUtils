/// <summary>
/// Event System
/// V 4.0
/// Developed by: Eder Moreira
/// Copyrights: Eder Moreira 2020
/// A basic system to manage comunication between objects
/// inside the sistem
/// OBS.: All added listeners must be removed to avoid waste in the table
/// Some Samples:
///  ES_EventManager.AddEventListener(ES_Event.ON_START, callback);
///  ES_EventManager.AddEventListener(this.gameObject, ES_Event.ON_START, callback);
///  ES_EventManager.AddEventListener("target identifier", ES_Event.ON_START, callback);
///  ES_EventManager.RemoveEventListener(this.gameObject, ES_Event.ON_START, callback);
///  ES_EventManager.DispatchEvent(this.gameObject, ES_Event.ON_START);
///  ES_EventManager.DispatchEvent(this.gameObject, ES_Event.ON_START, callbackparam);
///  ES_EventManager.DispatchEvent("target identifier", ES_Event.ON_COMPLETE,null, this);
/// </summary>

using System.Collections.Generic;
//using UnityEngine;
using System;
using System.Linq;
using UnityEngine;

namespace Mup.EventSystem.Events.Internal
{
    /// <summary>
    /// A manager for all events
    /// </summary>
    public static class ES_EventManager
    {
        public static bool IGNORE_PREFIX { get; set; } = true;

        /// <summary>
        /// A table with all of the listeners (eventinfo, callback)
        /// fast way to dispatch and add event listener
        /// </summary>
        static private Dictionary<string, ES_MupAction> _eventTable = new Dictionary<string, ES_MupAction>();

        /// <summary>
        /// Store the target identifier and a list of events for this target
        /// </summary>
        static private Dictionary<string, List<string>> _targetTable = new Dictionary<string, List<string>>();//targetName, list of target events
        //verificar se na lista de targets existe o target atual
        //se existir eu procuro na lista de eventos o evento atual
        //ter os dois dicionarios me permite evitar criar uma tuple ou criar nested dictionary (dicionario de dicionarios)
        
        /// <summary>
        /// Clear all listeners
        /// </summary>
        public static void Clear()
        {
            _eventTable.Clear();
            _targetTable.Clear();
        }

        /// <summary>
        /// Clear all null listeners
        /// </summary>
        public static void ClearNull()
        {
            List<string> nullKeys = _eventTable.Where(pair => pair.Value == null)
                            .Select(pair => pair.Key)
                            .ToList();

            _eventTable.Clear();

            foreach (string key in nullKeys)
            {
                _eventTable.Remove(key);
            }
        }

        /// <summary>
        /// Remove all instances from the old target and add to a new target
        /// </summary>
        /// <param name="oldTargetIdentifier"></param>
        /// <param name="newTargetIdentifier"></param>
        public static void SwapInstanceEvents(string oldTargetIdentifier, string newTargetIdentifier)
        {
            if (_targetTable.ContainsKey(oldTargetIdentifier))//if there is some listener for this object
            {
                foreach (string fullEventName in _targetTable[oldTargetIdentifier])//for each event
                {
                    //remove ther old target name from the full name
                    string eventName = fullEventName.Substring(oldTargetIdentifier.Length, fullEventName.Length-oldTargetIdentifier.Length);
                    //add the event to a new target identifier
                    AddEventListener(newTargetIdentifier, eventName, _eventTable[fullEventName]);
                    //remove old from list
                    _targetTable.Remove(oldTargetIdentifier);
                }
            }
        }

        /// <summary>
        /// Used with generic* events (Usually system events)
        /// *Events that dont belong to any object
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        static public void DispatchEvent(string eventName, object data = null)
        {
            DispatchEvent(ES_EventTarget.GENERIC, eventName, null, data);
        }
        

        /// <summary>
        /// Used when the event gonna be dispatch by some script within or without GameObject
        /// use null when require target but not require data
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <param name="unityObject"></param>
        static public void DispatchEvent(string targetIdentifier, string eventName, System.Object target = null, object data = null)
        {
            //create a key
            string key = GetTableKey(targetIdentifier, eventName);
            
            //if contains the key
            if (_eventTable.ContainsKey(key))
            {
                //if there is a method
                if (_eventTable[key] != null)
                {
                    //call the event
                    _eventTable[key](new ES_Event(target, eventName, targetIdentifier, data));
                }
                else
                {
                    //remove the method
                    _eventTable.Remove(key);
                }
            }
        }

        /// <summary>
        /// Used to listen generic* events
        /// *Events that dont belong to any object
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        static public void AddEventListener(string eventName, ES_MupAction method)
        {
            AddEventListener(ES_EventTarget.GENERIC, eventName, method);
        }

        /// <summary>
        /// Add event listener
        /// </summary>
        /// <param name="targetIdendifier"></param>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        static public void AddEventListener(string targetIdendifier, string eventName, ES_MupAction method)
        {
            //create a table key
            string key = GetTableKey(targetIdendifier, eventName);
            
            //if there isn't the key
            if (!_eventTable.ContainsKey(key))
            {
                _eventTable.Add(key, method);//create a table element and add the method
                RegisterEventFromTargetTable(targetIdendifier, key);
            }
            else
            {
                _eventTable[key] += method;
            }
        }

        /// <summary>
        /// Register the event in the target table
        /// </summary>
        /// <param name="targetIdendifier"></param>
        /// <param name="key"></param>
        private static void RegisterEventFromTargetTable(string targetIdendifier, string key)
        {
            if (!_targetTable.ContainsKey(targetIdendifier))
            {
                _targetTable.Add(targetIdendifier, new List<string>());
            }

            _targetTable[targetIdendifier].Add(key);
        }

        /// <summary>
        /// Unregister the event int the target table
        /// </summary>
        /// <param name="targetIdendifier"></param>
        /// <param name="key"></param>
        private static void UnregisterEventFromTargetTable(string targetIdendifier, string key)
        {
            if (_targetTable.ContainsKey(targetIdendifier))//if the list contains the target
            {
                if(_targetTable[targetIdendifier].Remove(key) && _targetTable[targetIdendifier].Count == 0)//check if we can remove the event after remove check if there is some event on list
                {
                    _targetTable.Remove(targetIdendifier);//remove the target list from the table
                }
            }
        }

        /// <summary>
        /// Remove event listener
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        static public void RemoveEventListener(string targetIdentifier, string eventName, ES_MupAction method)
        {
            //create a table key
            string key = GetTableKey(targetIdentifier, eventName);

            //if contais the key
            if (_eventTable.ContainsKey(key))
            {
                //if there is a method remove the method
                if (_eventTable[key] != null)
                    _eventTable[key] -= method;

                //if there isn't a method remove the table element
                if (_eventTable[key] == null)
                {
                    //remove event
                    _eventTable.Remove(key);
                }

                UnregisterEventFromTargetTable(targetIdentifier, key);
            }
        }

        /// <summary>
        /// Remove a generic* event listener
        /// *Events that dont belong to any object
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        static public void RemoveEventListener(string eventName, ES_MupAction method)
        {
            RemoveEventListener(ES_EventTarget.GENERIC, eventName, method);
        }

        /// <summary>
        /// Remove a generic* event listener
        /// *Events that dont belong to any object
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        static public void RemoveEventListeners(string eventName)
        {
            RemoveEventListeners(ES_EventTarget.GENERIC, eventName);
        }

        /// <summary>
        /// Remove all listeners assign to event
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        static public void RemoveEventListeners(string targetIdentifier, string eventName)
        {
            //create a table key
            string key = GetTableKey(targetIdentifier, eventName);

            //if could remove the key
            if (_eventTable.Remove(key))
            {   
                UnregisterEventFromTargetTable(targetIdentifier, key);
            }
        }

        /// <summary>
        /// Remove all listeners form especific targets
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        static public void RemoveAllEventListeners(string targetIdentifier)
        {
            if (_targetTable.ContainsKey(targetIdentifier))
            {
                for (int i = _targetTable[targetIdentifier].Count - 1; i >= 0; i--)
                {
                    //remove event
                    //UnityEngine.Debug.Log(" -- count = " + _targetTable[targetIdentifier].Count + " i = " + i + " -- event " + _targetTable[targetIdentifier][i]);
                    string key = _targetTable[targetIdentifier][i];
                    if (_eventTable.Remove(key))
                    {
                        UnregisterEventFromTargetTable(targetIdentifier, key);
                    }

                }

                _targetTable.Remove(targetIdentifier);
            }
        }

        /// <summary>
        /// Check if that method has already add to this event
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        static public bool HasEventListener(string targetIdentifier, string eventName, ES_MupAction method)
        {
            // create a table key
            string key = GetTableKey(targetIdentifier, eventName);
            
            //if contains key
            if (_eventTable.ContainsKey(key))
            {
                foreach (ES_MupAction m in _eventTable[key].GetInvocationList())
                    if (m == method) return true;
            }

            //if not contains the key
            return false;
            
        }

        /// <summary>
        /// Check if someone has listem this event
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        static public bool HasEventListener(string targetIdentifier, string eventName)
        {
            if(_targetTable.ContainsKey(targetIdentifier))
            {
                return _targetTable[targetIdentifier].Contains(GetTableKey(targetIdentifier, eventName));
            }
            
            return false;
        }

        /// <summary>
        /// Check if someone has listeners
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        static public bool HasListeners(string targetIdentifier)
        {
            return _targetTable.ContainsKey(targetIdentifier);
        }

        /// <summary>
        /// Get a key by the target name and the name of the event
        /// </summary>
        /// <param name="targetIdentifier"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        static private string GetTableKey(string targetIdentifier, string eventName)
        {
            return string.Concat(targetIdentifier, eventName);
        }
    }
}
