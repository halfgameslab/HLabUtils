using System.Xml.Serialization;

[XmlRoot("cvar_data")]
public class CVarData
{
    [XmlArray("objects")]
    [XmlArrayItem("object")]
    public CVarDataObject[] Objects { get; set; }
}