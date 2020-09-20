using Mup.EventSystem.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.Misc.UI
{
    public class M_PlayTweenOnVisible : MonoBehaviour
    {
        private void OnEnable()
        {
            this.AddEventListener(ES_Event.ON_BECAME_VISIBLE, OnVisibleHandler);
        }

        private void OnDisable()
        {
            this.RemoveEventListener(ES_Event.ON_BECAME_VISIBLE, OnVisibleHandler);
        }

        private void OnVisibleHandler(ES_Event ev)
        {
            if ((bool)ev.Data == true)
            {
                this.GetComponent<M_Tween>().Play();
                this.RemoveEventListener(ES_Event.ON_BECAME_VISIBLE, OnVisibleHandler);
            }
        }
    }

}