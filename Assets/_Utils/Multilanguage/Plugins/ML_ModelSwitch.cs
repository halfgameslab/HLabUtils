using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mup.Multilanguage.Plugins
{
    public class ML_ModelSwitch : MonoBehaviour
    {
        [SerializeField]
        private Lang _lang = Lang.PTBR;

        void UpdateModel()
        {
            if(!this.gameObject)
            {
                return;
            }

            if (_lang == ML_Reader.Language)
                this.gameObject.SetActive(true);
            else
                this.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            UpdateModel();

            ML_Reader.OnLanguageChangeHandler += OnLanguageChangeHandler;
        }

        private void OnDestroy()
        {
            ML_Reader.OnLanguageChangeHandler -= OnLanguageChangeHandler;
        }

        /*void OnDisable()
        {
            ML_Reader.OnLanguageChangeHandler -= OnLanguageChangeHandler;
        }*/

        void OnLanguageChangeHandler()
        {
            UpdateModel();
        }
    }
}