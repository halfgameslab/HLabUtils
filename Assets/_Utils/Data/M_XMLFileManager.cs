using System.Xml.Serialization;
using System.IO;
using System.Text;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;


/// <summary>
/// Para serializar Vector3 e outros tipos não serializados por padrão
/// https://stackoverflow.com/questions/11886290/use-the-xmlinclude-or-soapinclude-attribute-to-specify-types-that-are-not-known
/// </summary>
public static class M_XMLFileManager
{
    //ios error use this 
    //System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER","yes");


    public static void Save<T>(string path, T data, bool checkDirectory = true)
    {
        if (checkDirectory && !Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, Serialize<T>(data));
    }

    public static T Load<T>(string path)
    {
        if (File.Exists(path))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return (T)serializer.Deserialize(stream);
            }
        }

        return default;
    }

    public static void NewLoad<T>(string path, Action<T> OnLoadComplete)
    {
        if (path.Contains("://") || path.Contains (":///")) 
        {
            WWWLoadAsync<T>(path, OnLoadComplete);
        }
        else
        {
            OnLoadComplete(Load<T>(path));
        }
    }

    async static void WWWLoadAsync<T>(string path, Action<T> OnLoadComplete)
    {
        UnityWebRequest data = UnityWebRequest.Get(path);

        data.SendWebRequest();
        while(!data.isDone)
            await Task.Delay(1);

        if (data?.downloadHandler != null && (data.error == null || data.error.Length == 0))
        {
            OnLoadComplete?.Invoke(Deserialize<T>(data.downloadHandler.text));
        }
        else
        {
            Debug.Log("Download error: "+data.error+" "+path);
            OnLoadComplete?.Invoke(default);
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
        if(File.Exists(path))
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

    public static string Serialize<T>(T objectToSerialize)
    {
        MemoryStream mem = new MemoryStream();
        XmlSerializer ser = new XmlSerializer(typeof(T), new Type[] { typeof(UnityEngine.Vector3) });
        ser.Serialize(mem, objectToSerialize);
        ASCIIEncoding ascii = new ASCIIEncoding();

        return ascii.GetString(mem.ToArray()).Replace("???", "");
    }

    public static T Deserialize<T>(string xmlString)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(xmlString);
        MemoryStream mem = new MemoryStream(bytes);
        XmlSerializer ser = new XmlSerializer(typeof(T), new Type[] { typeof(UnityEngine.Vector3) });
        return (T)ser.Deserialize(mem);
    }
}
