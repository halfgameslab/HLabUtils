using System;

[Flags]
public enum CVarFlag
{
    NONE = 0x00,
    REMOVED = 0x01,
    CHANGED = 0x02,
    PERSISTENT = 0x04,
    LOCKED = 0x08
}


public class CVarObject
{
    //public string Name { get; set; }

    public string FullName { get; set; }

    public int Address { get; set; }

    public object Value { get; set; }

    public CVarFlag Flag { get; set; }
    public CVarGroup Group { get; set; }// qual grupo a variavel pertence
    
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
        

    private void SetFlagByBoolValue(CVarFlag flag, bool value)
    {
        if (value && !Flag.HasFlag(flag))
            Flag |= flag;
        else if (Flag.HasFlag(flag))
            Flag &= ~flag;
    }

    private bool HasFlag(CVarFlag flag)
    {
        return (Flag & flag) == flag;
    }
}