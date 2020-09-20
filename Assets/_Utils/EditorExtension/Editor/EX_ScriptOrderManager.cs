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
using UnityEditor;

namespace Mup.EditorExtension
{
    [InitializeOnLoad]
    public class EX_ScriptOrderManager
    {
        static EX_ScriptOrderManager()
        {
            foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (monoScript.GetClass() != null)
                {
                    foreach (var a in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ScriptOrder)))
                    {
                        int currentOrder = MonoImporter.GetExecutionOrder(monoScript);
                        int newOrder = ((ScriptOrder)a).Order;
                        if (currentOrder != newOrder)
                            MonoImporter.SetExecutionOrder(monoScript, newOrder);
                    }
                }
            }
        }
    }
}