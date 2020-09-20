using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CVarCommands
{ 
    EQUAL = 0,
    GREATER = 1,
    LESS = 2,
    GREATER_EQUAL = 3,
    LESS_EQUAL = 4
}

public class CVarCommand
{
    private Func<IComparable, IComparable, bool>[] Actions { get; set; } = new Func<IComparable, IComparable, bool>[] { CEqual, Greater, Less, GreaterEqual, LessEqual };

    private Dictionary<string, Func<object[], object>> ActionsTable = new Dictionary<string, Func<object[], object>>();// { { "",CEqual }, { "", Greater }, { "", Less }, { "", GreaterEqual }, { "", LessEqual } };

    /*
     * Default Actions
     */
    private static int CompareTo(IComparable a, IComparable b)
    {
        return a.CompareTo(b);
    }

    private static bool Greater(IComparable a, IComparable b)
    {
        return CompareTo(a, b) > 0;
    }

    private static bool Less(IComparable a, IComparable b)
    {
        return CompareTo(a, b) < 0;
    }

    private static bool CEqual(IComparable a, IComparable b)
    {
        return a.Equals(b);
    }

    private static bool GreaterEqual(IComparable a, IComparable b)
    {
        return CompareTo(a, b) >= 0;
    }

    private static bool LessEqual(IComparable a, IComparable b)
    {
        return CompareTo(a, b) <= 0;
    }
    /*
     * End Default actions 
     */

    public bool ExecuteAction(CVarCommands command, IComparable a, IComparable b)
    {
        return Actions[(int)command](a, b);
    }

    public bool ExecuteAction(string command, IComparable a, IComparable b)
    {
        return Actions[(int)Enum.Parse(typeof(CVarCommands), command)](a, b);
    }

    public bool ExecuteAction(string action, out object result, params object[] args)
    {
        if (ActionsTable.TryGetValue(action, out Func<object[], object> act))
        {
            result = act(args);

            return true;
        }

        result = null;
        
        return false;
    }

    /// <summary>
    /// Execute some action using CVars as params
    /// </summary>
    /// <param name="action"></param>
    /// <param name="vars"></param>
    /// <returns></returns>
    public bool ExecuteWithCVarSystemParam(string action, string[] vars)
    {
        return false;
    }

    public void RegisterAction(string actionName, Func<object[], object> action)
    {
        if(!ActionsTable.ContainsKey(actionName))
            ActionsTable.Add(actionName, action);
    }

    public void RemoveAction(string actionName)
    {
        if (!ActionsTable.ContainsKey(actionName))
            ActionsTable.Remove(actionName);
    }

}
