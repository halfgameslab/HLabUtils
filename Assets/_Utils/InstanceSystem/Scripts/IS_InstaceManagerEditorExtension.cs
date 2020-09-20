#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Util.InstanceSystem.Editor
{
    /// <summary>
    /// Used only in unity editor to help IS_InstanceWindow work
    /// </summary>
    public static class IS_InstaceManagerEditorExtension
    {
        /// <summary>
        /// Use only in editor mode, there isnt effect after when game playing
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public static string SetEditorInstanceName(this UnityEngine.Object obj, string instanceName)
        {
            if (obj == null)
                return instanceName;

            foreach (IS_InstanceSceneManager iManager in IS_InstanceSceneManager.Instances)
            {
                if (obj is GameObject)
                {
                    if (SetEditorInstanceName(obj as GameObject, ref instanceName, iManager))
                    {
                        return instanceName;
                    }
                }
                else
                {
                    if (SetEditorInstanceName(obj as Component, ref instanceName, iManager))
                    {
                        return instanceName;
                    }
                }
            }

            return instanceName;
        }

        public static string GetEditorInstanceName(this UnityEngine.Object obj, bool ignoreOrigenExtension = true)
        {
            if (obj != null)
            {
                string name;
                foreach (IS_InstanceSceneManager iManager in IS_InstanceSceneManager.Instances)
                {
                    name = GetEditorInstanceName(obj, iManager, ignoreOrigenExtension);

                    if (name != string.Empty)
                        return name;
                }
            }

            return string.Empty;
        }

        private static string GetEditorInstanceName(UnityEngine.Object obj, IS_InstanceSceneManager iManager, bool ignoreOrigenExtension = true)
        {
            if (iManager && iManager.ContainsKey(obj))
            {
                return ignoreOrigenExtension ? IS_InstanceManager.RemoveOrigenPrefix(iManager[obj]) : iManager[obj];
            }

            return string.Empty;
        }

        private static bool SetEditorInstanceName(UnityEngine.GameObject obj, ref string instanceName, IS_InstanceSceneManager iManager)
        {
            if (!obj.scene.IsValid() || obj.scene == iManager.gameObject.scene)
                return SetEditorInstanceName(obj, ref instanceName, IS_InstanceManager.GetPrefixByScene(iManager.gameObject.scene), iManager);

            return false;
        }

        private static bool SetEditorInstanceName(UnityEngine.Component obj, ref string instanceName, IS_InstanceSceneManager iManager)
        {
            if (!obj.gameObject.scene.IsValid() || obj.gameObject.scene == iManager.gameObject.scene)
                return SetEditorInstanceName(obj, ref instanceName, IS_InstanceManager.GetPrefixByScene(obj.gameObject.scene), iManager);

            return false;
        }

        private static bool SetEditorInstanceName(UnityEngine.Object obj, ref string instanceName, string prefix, IS_InstanceSceneManager iManager)
        {
            if (instanceName.Replace(" ", string.Empty).Length != 0)
            {
                Undo.RecordObject(iManager, "Set Instance Name");
                instanceName = string.Concat(prefix, IS_InstanceManager.DOT, instanceName);
                if (!iManager.ContainsKey(obj))
                {
                    iManager.Add(obj, ObjectNamesManager.GetUniqueName(iManager.Values.ToArray(), instanceName));
                    instanceName = iManager[obj];
                    return true;
                }
                else if (iManager[obj] != instanceName)
                {
                    instanceName = (iManager[obj] = ObjectNamesManager.GetUniqueName(iManager.Values.ToArray(), instanceName));
                    return true;
                }

            }

            return false;
        }
    }
}

#endif