using System.Xml.Serialization;

public class CVarDataObject
{
    private static readonly string FLOAT_TYPE_NAME = typeof(float).Name.ToString();
    private static readonly string INT_TYPE_NAME = typeof(int).Name.ToString();
    private static readonly string BOOL_TYPE_NAME = typeof(bool).Name.ToString();

    [XmlAttribute("type")]
    public string VarType { get; set; }

    [XmlAttribute("address")]
    public int VarAddress { get; set; }

    [XmlAttribute("persistent")]
    public bool VarPersistent { get; set; }

    [XmlAttribute("group")]
    public string VarGroup { get; set; }

    [XmlAttribute("loocked")]
    public bool VarLocked { get; set; }

    [XmlElement("name")]
    public string VarName { get; set; }

    [XmlElement("data")]
    public string VarData { get; set; }


    public CVarObject ToCVarObject()
    {
        return new CVarObject 
        { 
            FullName = VarName, 
            Value = ParseValue(), 
            Address = VarAddress,
            IsPersistent = VarPersistent, 
            IsLocked = VarLocked,
            Group = CVarSystem.GetGroup(VarGroup),
        };
    }

    public object ParseValue()
    {
        if (VarType == INT_TYPE_NAME)
            return int.Parse(VarData);
        else if (VarType == FLOAT_TYPE_NAME)
            return float.Parse(VarData);
        else if (VarType == BOOL_TYPE_NAME)
            return bool.Parse(VarData);

        return VarData;
    }

    public static CVarDataObject ParseToCVarDataObject(CVarObject obj)
    {
        return ParseToCVarDataObject(obj.FullName, obj.Value, obj.Address, obj.IsPersistent, obj.IsLocked, obj.Group.Name);
    }

    public static CVarDataObject ParseToCVarDataObject(string name, object value, int address, bool persistent, bool locked, string group)
    {
        return new CVarDataObject()
        {
            VarName = name,
            VarData = value.ToString(),
            VarType = value.GetType().Name,
            VarAddress = address,
            VarPersistent = persistent,
            VarLocked = locked,
            VarGroup = group,
        };
    }
}