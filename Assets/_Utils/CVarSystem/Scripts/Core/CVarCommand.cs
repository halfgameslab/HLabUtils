using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CVarCommands
{ 
    EQUAL = 0,
    GREATER = 1,
    SMALLER = 2,
    GREATER_EQUAL = 3,
    SMALLER_EQUAL = 4
}

public static class CVarCommand
{
    private static Func<object, object, bool>[] Actions { get; set; } = new Func<object, object, bool>[] { CEqual, Greater, Less, GreaterEqual, LessEqual };

    private static Dictionary<string, Func<object[], object>> ActionsTable { get; set; } = new Dictionary<string, Func<object[], object>>() { { "ADD", Add }, { "SUBTRACT", Subtract }, { "OVERWRITE", Overwrite } };// { { "",CEqual }, { "", Greater }, { "", Less }, { "", GreaterEqual }, { "", LessEqual } };

    /*
     * Default Actions
     */
    private static int CompareTo(IComparable a, IComparable b)
    {
        return a.CompareTo(b);
    }

    private static bool Greater(object a, object b)
    {
        return CompareTo((IComparable)a, (IComparable)b) > 0;
    }

    private static bool Less(object a, object b)
    {
        return CompareTo((IComparable)a, (IComparable)b) < 0;
    }

    /*private static bool CEqual(IComparable a, IComparable b)
    {
        return a.Equals(b);
    }*/

    private static bool GreaterEqual(object a, object b)
    {
        return CompareTo((IComparable)a, (IComparable)b) >= 0;
    }

    private static bool LessEqual(object a, object b)
    {
        return CompareTo((IComparable)a, (IComparable)b) <= 0;
    }

    private static bool CEqual(object a, object b)
    {
        if (a is IComparable)
            return a.Equals(b);
        else if(a is Vector3)
            return (Vector3)a == (Vector3)b;

        return a == b;
    }

    private static object Add(params object[] values)
    {
        if (values[0] is int)
            return (int)values[0] + (int)values[1];
        else if (values[0] is float)
            return (float)values[0] + (float)values[1];
        else if (values[0] is Vector3)
            return (Vector3)values[0] + (Vector3)values[1];

        return null;
    }

    private static object Subtract(params object[] values)
    {
        if (values[0] is int)
            return (int)values[0] - (int)values[1];
        else if (values[0] is float)
            return (float)values[0] - (float)values[1];
        else if (values[0] is Vector3)
            return (Vector3)values[0] - (Vector3)values[1];

        return null;
    }

    private static object Overwrite(params object[] values)
    {
        return values[1];
    }

    /*
     * End Default actions 
     */

    public static bool ExecuteAction(CVarCommands command, object a, object b)
    {
        return Actions[(int)command](a, b);
    }

    public static bool ExecuteAction(string command, object a, object b)
    {
        return Actions[(int)Enum.Parse(typeof(CVarCommands), command)](a, b);
    }

    public static bool TryExecuteAction(string action, out object result, params object[] args)
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
    public static bool ExecuteWithCVarSystemParam(string action, string[] vars)
    {
        return false;
    }

    public static void RegisterAction(string actionName, Func<object[], object> action)
    {
        if(!ActionsTable.ContainsKey(actionName))
            ActionsTable.Add(actionName, action);
    }

    public static void RemoveAction(string actionName)
    {
        if (!ActionsTable.ContainsKey(actionName))
            ActionsTable.Remove(actionName);
    }

}
