using UnityEngine;
using System.Collections;

using Mup.Multilanguage.Plugins;

namespace Mup.Multilanguage.Scripts
{
    public class ML_Manager : MonoBehaviour
    {
        //basic singleton control
        private static ML_Manager _instance;
        public static ML_Manager Instance { get { return _instance; } }
        //end

        public ML_Manager()
        {
            _instance = this;
        }

        public void SetLanguage(int lang)
        {
            ML_Reader.Language = (Lang)lang;
        }

        public void SetLanguage(Lang lang)
        {
            ML_Reader.Language = (Lang)lang;
        }
    }
}