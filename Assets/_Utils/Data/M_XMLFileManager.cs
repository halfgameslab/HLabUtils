using System.Xml.Serialization;
using System.IO;
using System.Text;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Security.Cryptography;

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

    // Create a method to encrypt a text and save it to a specific file using a RSA algorithm public key   
    static void EncryptText(string publicKey, string text, string fileName)
    {
        // Convert the text to an array of bytes   
        UnicodeEncoding byteConverter = new UnicodeEncoding();
        byte[] dataToEncrypt = byteConverter.GetBytes(text);

        // Create a byte array to store the encrypted data in it   
        byte[] encryptedData;
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            // Set the rsa pulic key   
            rsa.FromXmlString(publicKey);

            // Encrypt the data and store it in the encyptedData Array   
            encryptedData = rsa.Encrypt(dataToEncrypt, false);
        }
        // Save the encypted data array into a file   
        File.WriteAllBytes(fileName, encryptedData);

        Console.WriteLine("Data has been encrypted");
    }

    // Method to decrypt the data withing a specific file using a RSA algorithm private key   
    static string DecryptData(string privateKey, string fileName)
    {
        // read the encrypted bytes from the file   
        byte[] dataToDecrypt = File.ReadAllBytes(fileName);

        // Create an array to store the decrypted data in it   
        byte[] decryptedData;
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            // Set the private key of the algorithm   
            rsa.FromXmlString(privateKey);
            decryptedData = rsa.Decrypt(dataToDecrypt, false);
        }

        // Get the string value from the decryptedData byte array   
        UnicodeEncoding byteConverter = new UnicodeEncoding();
        return byteConverter.GetString(decryptedData);
    }

    public static T LoadAndDecrypt<T>(string privateKey, string fileName)
    {
        string data = DecryptData(privateKey, fileName);

        return Deserialize<T>(data);
    }

    public static void EncryptAndSave<T>(string publicKey, T data, string fileName)
    {
        Debug.Log(Serialize<T>(data).Length);

        EncryptText(publicKey, Serialize<T>(data), fileName);
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
