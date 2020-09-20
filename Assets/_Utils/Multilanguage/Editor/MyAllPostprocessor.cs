using UnityEngine;
using UnityEditor;

class MyAllPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            if(str.Contains("PTBR.xml") || str.Contains("EN.xml"))
            {
                Mup.Multilanguage.Plugins.ML_Reader.Reload();
            }
            //Debug.Log("Reimported Asset: " + str);
        }
        /*foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }*/
    }
}