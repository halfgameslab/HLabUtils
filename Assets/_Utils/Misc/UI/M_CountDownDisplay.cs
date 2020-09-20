using Mup.EventSystem.Events;
using Mup.Misc.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mup.Misc.UI
{
    public class M_CountDownDisplay : MonoBehaviour
    {
        [SerializeField] private M_Timer _timer;
        [SerializeField] private Text _counterLabel;
        [SerializeField] private Image _counterImage;

        //private void Awake()
        //{
        //    if(_counterImage)
        //        _counterImage.enabled = false;

        //    if(_counterLabel)
        //        _counterLabel.enabled = false;
        //}

        private void Update()
        {
            if (_timer.IsPlaying)
            {
                UpdateText(_timer.Duration - _timer.ElapsedTime);
                UpdateImage(_timer.NormalizedElapsedTime);
            }
        }

        private void UpdateText(float time)
        {
            if (_counterLabel)
            {
                int min = (int)(time / 60);
                int second = (int)(time % 60);

                string minString = string.Concat((min >= 10)?string.Empty: "0", min);
                string secondString = string.Concat((second >= 10) ? string.Empty : "0", second); ;
                
                _counterLabel.text = string.Concat(minString, ":", secondString);
            }
        }

        private void UpdateImage(float normalizedTime)
        {
            if(_counterImage)
                _counterImage.fillAmount = 1-normalizedTime;
        }
    }
}