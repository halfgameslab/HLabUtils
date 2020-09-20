using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Xml;

#if UNITY_WINRT
using File = UnityEngine.Windows.File;
#else
using File = System.IO.File;
#endif

namespace Mup.Multilanguage.Plugins
{
    public enum Lang { NONE = -1,  EN = 0, PTBR = 1 };
    public delegate void OnLanguageChangeCallback();
    static public class ML_Reader
    {
        public static readonly string UNDEFINE = "undefine";

        static readonly string[] LANG = new string[] { "EN", "PTBR" };

        static private Dictionary<string, string> text = new Dictionary<string, string>();

        static private Lang currentLanguage = Lang.PTBR;

        static public Dictionary<string, string> Text
        {
            get
            {
                return text;
            }
        }

        static public OnLanguageChangeCallback OnLanguageChangeHandler { get; set; }

        static public Lang Language
        {
            get { return currentLanguage; }
            set
            {
                if (currentLanguage != value || text.Count == 0)
                {
                    if (ReadLangFile(value))
                    {
                        currentLanguage = value;

                        OnLanguageChangeHandler?.Invoke();
                    }
                }

            }
        }

        static public void Reload()
        {
            Lang aux = currentLanguage;
            currentLanguage = Lang.NONE;
            Language = aux;
        }

        static public string GetText(string id)
        {
            if (text.Count == 0)
                Reload();

            string result;

            if (text.TryGetValue(id, out result))
                return result;
            
            return UNDEFINE;
        }

        static public string Save(Dictionary<string, string> newList, Lang l = Lang.EN)
        {
            XmlDocument xml = new XmlDocument();
            
            xml.AppendChild(xml.CreateElement("strings"));

            foreach (KeyValuePair<string, string> s in newList)
            {
                XmlElement element = xml.CreateElement("string");
                element.SetAttribute("id", s.Key);
                //element.InnerText = "<![CDATA[" + s.Value + "]]>";
                element.AppendChild(xml.CreateCDataSection(s.Value));

                xml.FirstChild.AppendChild(element);
            }
            
            xml.Save(Application.dataPath+"/MUP/Multilanguage/Resources/"+LANG[(int)l]+".xml");
            Debug.Log("Arquivo salvo com sucesso em: "+ Application.dataPath + "/MUP/Multilanguage/Resources/" + LANG[(int)l] + ".xml");

            CreateTextListFromXML(xml);

            return "ok";
        }

        static private bool ReadLangFile(Lang lang)
        {
            TextAsset s = (TextAsset)Resources.Load(LANG[(int)lang], typeof(TextAsset));

            if (s == null)
                return false;

            XmlDocument xml = new XmlDocument();

            xml.LoadXml(s.text);
            
            return CreateTextListFromXML(xml);

        }

        static private bool CreateTextListFromXML(XmlDocument xml)
        {
            text = new Dictionary<string, string>();
            
            foreach (XmlNode node in xml.FirstChild.ChildNodes)
            {
                string key = node.Attributes["id"].InnerText;
                if (!text.ContainsKey(key))
                    text.Add(key, node.InnerText);
                else
                    Debug.LogWarning("A chave: " + key + " está repetida no arquivo e sua segunda ocorrencia foi ignorada.");
            }
            
            if (text.Count == 0)
                return false;

            return true;
        }
        
    }
}