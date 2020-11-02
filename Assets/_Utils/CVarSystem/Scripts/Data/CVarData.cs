using System.Xml.Serialization;

[XmlRoot("cvar_data")]
public class CVarData
{
    [XmlArray("os")]
    [XmlArrayItem("o")]
    public CVarObject[] Objects { get; set; }
}