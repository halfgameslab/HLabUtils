/// <summary>
/// EX_ScriptOrderManager
/// V 1.0
/// Developed by: Eder Moreira
/// Copyrights: MUP Studios 2018
/// Used to update unity script order
/// Reference: https://stackoverflow.com/questions/27928474/unity3d-execution-order-of-scripts-programmatically-added/36724871
/// Reference: https://docs.unity3d.com/Manual/class-MonoManager.html
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;

public class ScriptOrder : Attribute
{
    public int Order { get; set; }

    public ScriptOrder(int order)
    {
        this.Order = order;
    }
}