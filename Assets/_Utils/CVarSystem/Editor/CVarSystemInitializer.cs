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
        //PlayerPrefs.SetInt("FilesCopied", 0);
        CVarSystem.ActiveEditMode(true, true);
        //CVarSystem.Init();
        Debug.Log("CVarSystemInitializer");
        EditorApplication.playModeStateChanged += OnPlayModeStateChangeHandler;
    }
    private static void OnPlayModeStateChangeHandler(PlayModeStateChange state)
    {
        if ((state == PlayModeStateChange.EnteredEditMode))
        {
            Debug.Log("state == PlayModeStateChange.EnteredEditMode");
            //CVarSystem.UnloadGroups();
            //CVarSystem.IsEditModeActived = true;
            //CVarSystem.Init();
            CVarSystem.ActiveEditMode(true, true);
        }
        else if ((state == PlayModeStateChange.ExitingEditMode))
        {
            Debug.Log("state == PlayModeStateChange.ExitingEditMode");
            //CVarSystem.UnloadGroups();
            //CVarSystem.IsEditModeActived = false;
            //CVarSystem.Init();
            CVarSystem.ActiveEditMode(false, true);
        }
        /*else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            CVarSystem.Load();
        }*/

        
    }
}

#endif