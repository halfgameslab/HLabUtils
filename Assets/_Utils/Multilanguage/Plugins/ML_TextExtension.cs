using UnityEngine;
using System.Collections;

using UnityEngine.UI;

namespace Mup.Multilanguage.Plugins
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class ML_TextExtension : MonoBehaviour
    {

        [SerializeField]
        private string _text = ML_Reader.UNDEFINE;

        private Text _textComponent;

        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                UpdateText();
            }
        }

        public string UpdateText()
        {
            string result = FormatText(_text);

            if (!_textComponent)
                _textComponent = GetComponent<Text>();
            
            _textComponent.text = result;
            
            return _textComponent.text;
        }

        private string FormatText(string text)
        {
            if (text == null)
                return string.Empty;

            text = text.Replace("<+/>","(;)");

            string[] keys = text.Split("+"[0]);
            string result = "";

            foreach (string key in keys)
            {
                string aux = ML_Reader.GetText(key);

                if (aux != ML_Reader.UNDEFINE)
                {
                    result += aux;
                }
                else
                {
                    result += key;
                }
            }

            result = result.Replace("\\n", "\n").Replace("<br/>", "\n").Replace("<br>", "\n").Replace("(;)","+");

            return result;
        }
        
        //adicionar argumentos ao texto
        //BUG - Quando trocar a linguagem é necessário recarregar
        //senão o texto aparecer com os caracteres de formatação
        public void InsertFormat(string text, params object[] args)
        {
            string result = FormatText(text);
            _text = text;

            if (args != null && args.Length > 0)
            {
                result = string.Format(result, args);
            }

            if (!_textComponent)
                _textComponent = GetComponent<Text>();
            
            _textComponent.text = result;
        }
        
        void OnEnable()
        {
            UpdateText();

            ML_Reader.OnLanguageChangeHandler += OnLanguageChangeHandler;
        }

        void OnDisable()
        {
            ML_Reader.OnLanguageChangeHandler -= OnLanguageChangeHandler;
        }

        void OnLanguageChangeHandler()
        {
            UpdateText();
        }

    }
}