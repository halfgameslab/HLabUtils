using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[Flags]
public enum CVarFlag
{
    NONE = 0x00,
    REMOVED = 0x01,
    CHANGED = 0x02,
    PERSISTENT = 0x04,
    LOCKED = 0x08
}

[Serializable]
public class CVarObject
{
    [XmlElement("n")]
    public string Name { get; set; }

    [XmlAttribute("a")]
    public int Address { get; set; }

    [XmlElement("v")]
    public object Value { get; set; }

    [XmlAttribute("f")]
    public CVarFlag Flag { get; set; }
    
    //[IgnoreDataMember]
    [XmlIgnore]
    public bool IsPersistent 
    { 
        get 
        { 
            return HasFlag(CVarFlag.PERSISTENT); 
        } 

        set
        {
            SetFlagByBoolValue(CVarFlag.PERSISTENT, value);
        }
    }

    [XmlIgnore]
    public bool IsLocked
    {
        get
        {
            return HasFlag(CVarFlag.LOCKED);
        }

        set
        {
            SetFlagByBoolValue(CVarFlag.LOCKED, value);
        }
    }

    [XmlIgnore]
    public string FullName { get; set;  }

    [XmlIgnore]
    public CVarGroup Group { get; set; }

    public CVarObject(string fullName, object value, int address, CVarGroup group)
    {
        Value = value;
        Group = group;
        Address = address;
        FullName = fullName;
        Name = CVarSystem.RemoveTypeAndGroup(fullName);
    }

    public CVarObject()
    {

    }

    private void SetFlagByBoolValue(CVarFlag flag, bool value)
    {
        if (value && !Flag.HasFlag(flag))
            Flag |= flag;
        else if (!value && Flag.HasFlag(flag))
            Flag &= ~flag;
    }

    private bool HasFlag(CVarFlag flag)
    {
        return (Flag & flag) == flag;
    }
}