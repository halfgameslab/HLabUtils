/// <summary>
/// Event
/// V 4.0
/// Developed by: Eder Moreira
/// Copyrights: Eder Moreira 2020
/// Used to support the event system
/// </summary>
/// 
using Mup.EventSystem.Events.Internal;
using Mup.EventSystem.Events;

public static class ES_ObjectExtension
{
    #region GLOBAL_CONFIGURATION

    /// <summary>
    /// Used with generic* events (Usually system events)
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="data"></param>
    public static void DispatchEvent(this System.Object self, string eventName, object data = null)
    {
        ES_EventManager.DispatchEvent(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX), eventName, self, data);
    }

    /// <summary>
    /// Add a listener at the current object to get all dispatch from event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    public static void AddEventListener(this System.Object self, string eventName, ES_MupAction method)
    {
        ES_EventManager.AddEventListener(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX), eventName, method);
    }

    /// <summary>
    /// Remove a generic* event listener
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    public static void RemoveEventListener(this System.Object self, string eventName, ES_MupAction method)
    {
        ES_EventManager.RemoveEventListener(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX), eventName, method);
    }

    /// <summary>
    /// Remove a generic* event listener
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    public static void RemoveEventListeners(this System.Object self, string eventName)
    {
        ES_EventManager.RemoveEventListeners(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX), eventName);
    }

    /// <summary>
    /// Remove all listeners form especific event
    /// </summary>
    public static void RemoveAllEventListeners(this System.Object self)
    {
        ES_EventManager.RemoveAllEventListeners(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX));
    }

    /// <summary>
    /// Check if that method has already add to this event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool HasEventListener(this System.Object self, string eventName, ES_MupAction method)
    {
        return ES_EventManager.HasEventListener(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX), eventName, method);
    }

    /// <summary>
    /// Check if that method has already add to this event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool HasListeners(this System.Object self, string eventName, ES_MupAction method)
    {
        return ES_EventManager.HasListeners(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX));
    }

    /// <summary>
    /// Check if someone has listem this event
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public static bool HasEventListener(this System.Object self, string eventName)
    {
        return ES_EventManager.HasEventListener(self.GetInstanceName(ES_EventManager.IGNORE_PREFIX), eventName);
    }

    #endregion


    #region LOCAL_CONFIGURATION
    /// <summary>
    /// Used with generic* events (Usually system events)
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="data"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    public static void DispatchEvent(this System.Object self, string eventName, bool ignoreOrigenSceneName, object data = null)
    {
        ES_EventManager.DispatchEvent(self.GetInstanceName(ignoreOrigenSceneName), eventName, self, data);
    }

    /// <summary>
    /// Used to listen generic* events
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    public static void AddEventListener(this System.Object self, string eventName, ES_MupAction method, bool ignoreOrigenSceneName)
    {
        ES_EventManager.AddEventListener(self.GetInstanceName(ignoreOrigenSceneName), eventName, method);
    }

    /// <summary>
    /// Remove a generic* event listener
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    public static void RemoveEventListener(this System.Object self, string eventName, ES_MupAction method, bool ignoreOrigenSceneName)
    {
        ES_EventManager.RemoveEventListener(self.GetInstanceName(ignoreOrigenSceneName), eventName, method);
    }

    /// <summary>
    /// Remove a generic* event listener
    /// *Events that dont belong to any object
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    public static void RemoveEventListeners(this System.Object self, string eventName, bool ignoreOrigenSceneName)
    {
        ES_EventManager.RemoveEventListeners(self.GetInstanceName(ignoreOrigenSceneName), eventName);
    }

    /// <summary>
    /// Remove all listeners form especific event
    /// </summary>
    /// <param name="ignoreOrigenSceneName"></param>
    public static void RemoveAllEventListeners(this System.Object self, bool ignoreOrigenSceneName)
    {
        ES_EventManager.RemoveAllEventListeners(self.GetInstanceName(ignoreOrigenSceneName));            
    }

    /// <summary>
    /// Check if that method has already add to this event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    /// <returns></returns>
    public static bool HasEventListener(this System.Object self, string eventName, ES_MupAction method, bool ignoreOrigenSceneName)
    {
        return ES_EventManager.HasEventListener(self.GetInstanceName(ignoreOrigenSceneName), eventName, method);
    }

    /// <summary>
    /// Check if that method has already add to this event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="method"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    /// <returns></returns>
    public static bool HasListeners(this System.Object self, string eventName, ES_MupAction method, bool ignoreOrigenSceneName)
    {
        return ES_EventManager.HasListeners(self.GetInstanceName(ignoreOrigenSceneName));
    }

    /// <summary>
    /// Check if someone has listem this event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="ignoreOrigenSceneName"></param>
    /// <returns></returns>
    public static bool HasEventListener(this System.Object self, string eventName, bool ignoreOrigenSceneName)
    {
        return ES_EventManager.HasEventListener(self.GetInstanceName(ignoreOrigenSceneName), eventName);
    }
    #endregion
}