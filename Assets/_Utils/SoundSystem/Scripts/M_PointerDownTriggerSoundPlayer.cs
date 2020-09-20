using UnityEngine.EventSystems;

namespace Mup.SoundSystem
{
    public class M_PointerDownTriggerSoundPlayer : M_SoundPlayer, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            this.Play();
        }
    }
}