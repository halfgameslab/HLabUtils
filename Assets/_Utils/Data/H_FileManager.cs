using System.Xml.Serialization;
using System.IO;
using System.Text;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net;

public static class H_FileManager
{
    public static void Save(string path, string data, bool checkDirectory = true)
    {
        if (checkDirectory && !Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, data);
    }

    public static string LocalLoad(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return string.Empty;
    }

    public static void Load(string path, Action<string> OnLoadComplete)
    {
        if (path.Contains("://") || path.Contains(":///"))
        {
            WWWLoadAsync(path, OnLoadComplete);
        }
        else
        {
            OnLoadComplete(LocalLoad(path));
        }
    }

    async static void WWWLoadAsync(string path, Action<string> OnLoadComplete)
    {
        UnityWebRequest data = UnityWebRequest.Get(path);

        data.SendWebRequest();
        while (!data.isDone)
            await Task.Delay(1);

        if (data?.downloadHandler != null && (data.error == null || data.error.Length == 0))
        {
            OnLoadComplete?.Invoke(data.downloadHandler.text);
        }
        else
        {
            Debug.Log("Download error: " + data.error + " " + path);
        }
    }

    public static void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /*
     *
     */
    public static void RenameOrMove(string path, string newPath)
    {
        if (File.Exists(path))
        {
            if (!File.Exists(newPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                // use copy instead move because u can rename some file so move will destroy the file so u can change the content
                File.Copy(path, newPath, true);
                File.Delete(path);
            }
            else
            {
                Debug.LogWarning(string.Format("There is another file with the name {0}", path));
            }
        }
        else
        {
            Debug.LogWarning(string.Format("File {0} doesnt exist!", path));
        }
    }

    public static void Copy(string path, string newPath, bool overwrite = false)
    {
        if (File.Exists(path))
        {
            if (!File.Exists(newPath) || overwrite)
            {
                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                File.Copy(path, newPath, true);
            }
            else
            {
                Debug.LogWarning(string.Format("There is another file with the name {0}", path));
            }
        }
        else
        {
            Debug.LogWarning(string.Format("File {0} doesnt exist!", path));
        }
    }
}
