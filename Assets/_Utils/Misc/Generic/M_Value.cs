using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mup.EventSystem.Events;
using System.Text;
using Ultility;

namespace Mup.Misc.Generic
{
    public class M_Value: MonoBehaviour
    {
        private float _number;

        public float Value
        {
            get { return _number; }
            set
            {
                if(_number != value)
                    _number = value;
                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, this);
            }
        }

        public void Add(float amount)
        {
            Value += amount;
        }

        public override string ToString()
        {
            return Format(_number);
        }

        static public string Format(float a)
        {
            StringBuilder result = new StringBuilder();
            
            int count = 0;

            while (a > 1000)
            {
                a = a / 1000;
                count++;
            }

            string s = a.ToString();

            int dotIndex = s.LastIndexOf('.');

            if (dotIndex != -1)
                s = s.Substring(0, dotIndex+2);

            result.AppendFormat("{0}", s);

            if (count == 1)
                result.Append("K");
            else if (count == 2)
                result.Append("M");

            return result.ToString();
        }
    }
}