#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Security.Cryptography;

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
        //CVarSystem.FilesHasBeenCopied = false;
        //PlayerPrefs.SetInt("FilesCopied", 0);
        
        //////// Create an instance of the RSA algorithm class  
        //////RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //////// Get the public keyy   
        //////string publicKey = rsa.ToXmlString(false); // false to get the public key   
        //////string privateKey = rsa.ToXmlString(true); // true to get the private key

        //////Debug.Log(publicKey);
        //////Debug.Log(privateKey);

        //////M_XMLFileManager.EncryptAndSave(publicKey, new CVarGroup() { Name = "teste", UID = "55"  }, CVarSystem.ParseStreamingDefaultDataPathWith("encryptedData.txt"));
        //////CVarGroup g = M_XMLFileManager.LoadAndDecrypt<CVarGroup>(privateKey, CVarSystem.ParseStreamingDefaultDataPathWith("encryptedData.txt"));

        //////Debug.Log(g.Name);
        //////Debug.Log(g.UID);

        CVarSystem.ActiveEditMode(true, true);
        H_QuestSystemV2.H_DataManager.Instance.Start();
        H_QuestSystemV2.H_QuestManager.Instance.Start();
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