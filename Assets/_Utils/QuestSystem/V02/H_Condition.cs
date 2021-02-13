using System.Collections.Generic;
using System.Xml.Serialization;

namespace H_Misc
{
    public enum ConditionOperation
    {
        AND,
        OR
    }
    public class H_Condition
    {
        private string _type = "CheckVar";

        [XmlAttribute("uid")]
        public string UID { get; set; }

        [XmlAttribute("t")]
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    if (_type == "Condition")
                    {
                        Conditions = new List<H_Condition>();
                    }
                    else
                    {
                        Conditions = null;
                    }
                }
            }
        }

        [XmlAttribute("op")]
        public ConditionOperation Operation { get; set; } = ConditionOperation.AND;

        [XmlArray("pl")]
        [XmlArrayItem("pi")]
        public object[] _params;

        [XmlArray("cl")]
        [XmlArrayItem("ci")]
        public List<H_Condition> Conditions { get; set; }

        public bool IsLeaf { get { return !Type.Equals("Condition"); } }

        public H_Condition Clone(string sufix = " (clone)")
        {
            H_Condition clone = new H_Condition();
            clone.Type = _type;
            clone._params = _params;
            clone.Operation = Operation;
            clone.UID = string.Concat(UID, sufix);

            if (Conditions != null)
            {
                foreach (H_Condition c in Conditions)
                {
                    clone.Conditions.Add(c.Clone(string.Empty));
                }
            }

            return clone;
        }
    }
}
