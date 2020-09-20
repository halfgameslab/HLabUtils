using Mup.EventSystem.Events;
using Mup.Multilanguage.Plugins;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mup.AlertSystem
{
    public class M_Alert : MonoBehaviour
    {
        public static M_Alert Instance { get; private set; }

        [SerializeField] private ML_TextExtension _label;

        [SerializeField] private ML_TextExtension _titleText;

        [SerializeField] private GameObject[] _buttons;
        [SerializeField] private ML_TextExtension[] _buttonTexts;

        public M_Alert()
        {
            Instance = this;
        }

        //0 = confirm/cancel 1 = ok
        public void Show(string message,string confirmgMsg,string resumeMSg, string cancelMsg, string title, int type = 0)
        {
            this.gameObject.SetActive(true);

            _label.Text = message;

            if (type == 0)
            {
                _buttons[0].SetActive(true);
                _buttons[1].SetActive(true);
                _buttons[2].SetActive(false);
                _buttonTexts[0].Text = confirmgMsg;
                _buttonTexts[1].Text = cancelMsg;
                _buttonTexts[2].Text = resumeMSg;
            }
            else if (type == 1)
            {
                _buttons[0].SetActive(false);
                _buttons[1].SetActive(false);
                _buttons[2].SetActive(true);
                _buttonTexts[0].Text = confirmgMsg;
                _buttonTexts[1].Text = cancelMsg;
                _buttonTexts[2].Text = resumeMSg;
            }
            else
            {
                _buttons[0].SetActive(true);
                _buttons[1].SetActive(true);
                _buttons[2].SetActive(true);
                _buttonTexts[0].Text = confirmgMsg;
                _buttonTexts[1].Text = cancelMsg;
                _buttonTexts[2].Text = resumeMSg;
            }

            _titleText.Text = title;
        }

        public void Show(string message, string title = "", int type = 0)
        {
            Show(message, "@generic_label_confirm", "@generic_label_ok", "@generic_label_cancel", title, type);
        }

        public void OnConfirmClickHandler()
        {
            this.DispatchEvent(ES_Event.ON_CONFIRM);
        }

        public void OnCancelClickHandler()
        {
            this.DispatchEvent(ES_Event.ON_CANCEL);
        }

        public void OnResumeClickHandler()
        {
            this.DispatchEvent(ES_Event.ON_RESUME);
        }
    }
}