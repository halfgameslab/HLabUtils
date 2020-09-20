using UnityEngine.EventSystems;
using UnityEngine;
using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using Mup.Misc.UI;

namespace Mup.Interactable
{
    public class M_Pointer3DClickTrigger : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            //especific dispatchEvent
            this.DispatchEvent(ES_Event.ON_CLICK);

            //generic dispatchEvent
            ES_EventManager.DispatchEvent(ES_EventTarget.ANY_3D_GAME_OBJECT, ES_Event.ON_CLICK, this.gameObject, eventData.pointerCurrentRaycast.worldPosition);
        }
    }
}