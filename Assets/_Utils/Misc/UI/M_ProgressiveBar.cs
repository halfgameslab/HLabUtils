using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mup.Misc.Generic;
using Mup.EventSystem.Events;

namespace Mup.Misc.UI
{
    public class M_ProgressiveBar : MonoBehaviour
    {
        [SerializeField]
        public Image _barIMG;

        [SerializeField]
        public Image _middleBarIMG;

        [SerializeField]
        public Image _bgIMG;

        [SerializeField]
        private M_ValueInfo _value;

        [SerializeField]
        private Color _mainColor = Color.green;

        [SerializeField]
        private bool _activeOnValueChange = false;

        public bool Visible
        {
            set
            {
                _barIMG.enabled = value;

                if(_middleBarIMG)
                    _middleBarIMG.enabled = value;
                if(_bgIMG)
                    _bgIMG.enabled = value;

                this.DispatchEvent(ES_Event.ON_BECAME_VISIBLE, value);
            }

            get
            {
                return _barIMG.enabled;
            }
        }

        public bool ActiveOnValueChange
        {
            get
            {
                return _activeOnValueChange;
            }

            set
            {
                _activeOnValueChange = value;
            }
        }

        public M_ValueInfo Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        public Color MainColor
        {
            get
            {
                return _mainColor;
            }

            set
            {
                _mainColor = value;

                if(_barIMG)
                    _barIMG.color = _mainColor;
            }
        }

        public void OnEnable()
        {
            Init();   
        }

        void OnDisable()
        {
            if (Value)
            {
                Value.RemoveEventListener(ES_Event.ON_VALUE_CHANGE, UpdateLife);
                Value.RemoveEventListener(ES_Event.ON_DESTROY, OnHealthComponentDestroyHandler);
            }
        }

        private void LateUpdate()
        {
            if(_middleBarIMG)
                _middleBarIMG.fillAmount = Mathf.Lerp(_middleBarIMG.fillAmount, _barIMG.fillAmount, Time.deltaTime * 1.0f);
        }

        public void Init()
        {
            if (Value)
            {
                if (!Value.HasEventListener(ES_Event.ON_VALUE_CHANGE, UpdateLife))
                {
                    Value.AddEventListener(ES_Event.ON_VALUE_CHANGE, UpdateLife);
                }
                if (!Value.HasEventListener(ES_Event.ON_DESTROY, OnHealthComponentDestroyHandler))
                {
                    Value.AddEventListener(ES_Event.ON_DESTROY, OnHealthComponentDestroyHandler);
                }

                if (_barIMG)
                {
                    _barIMG.enabled = true;
                    _barIMG.fillAmount = Value.NormalizedAmount;
                }
                if (_middleBarIMG)
                    _middleBarIMG.fillAmount = Value.NormalizedAmount;
            }
            Visible = !ActiveOnValueChange;
            
        }

        void UpdateLife(ES_Event ev)
        {
            float normalizedLife = (float)ev.Data;
            _barIMG.fillAmount = normalizedLife;

            if (ActiveOnValueChange && !Visible)
            {
                Visible = true;
            }
        }

        public void StoreHealthBar()
        {
            if (this)
            {
                M_ObjectPool.Instance.Store(this.gameObject);

                OnDisable();
                Value = null;
            }
        }

        public void OnHealthComponentDestroyHandler(ES_Event ev)
        {
            StoreHealthBar();

            //Destroy(this.gameObject);
        }
        
    }
}