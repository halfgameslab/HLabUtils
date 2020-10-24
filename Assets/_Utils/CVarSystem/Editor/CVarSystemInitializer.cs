#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

// ensure class initializer is called whenever scripts recompile
[InitializeOnLoad]
internal static class CVarSystemInitializer
{
    //[InitializeOnLoadMethod]
    //static void RunOnStart()
    //{
    //    CVarSystem.DATA_PATH = Application.dataPath;
    //    Debug.Log("RunOnStart "+ Environment.CurrentDirectory);
    //}
    // register an event handler when the class is initialized
    static CVarSystemInitializer()
    {
        CVarSystem.Init();

        EditorApplication.playModeStateChanged += OnPlayModeStateChangeHandler;
    }
    private static void OnPlayModeStateChangeHandler(PlayModeStateChange state)
    {
        if (CVarSystem.EditorAutoSave = (state == PlayModeStateChange.EnteredEditMode))
        {
            Debug.Log("state == PlayModeStateChange.EnteredEditMode");
            //CVarSystem.UnloadGroups();
            //CVarSystem.IsEditModeActived = true;
            //CVarSystem.Init();
            CVarSystem.ActiveEditMode(true);
        }
        else if (CVarSystem.EditorAutoSave = (state == PlayModeStateChange.ExitingEditMode))
        {
            Debug.Log("state == PlayModeStateChange.ExitingEditMode");
            //CVarSystem.UnloadGroups();
            //CVarSystem.IsEditModeActived = false;
            //CVarSystem.Init();
            CVarSystem.ActiveEditMode(false);
        }
        /*else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            CVarSystem.Load();
        }*/

        
    }
}

#endif