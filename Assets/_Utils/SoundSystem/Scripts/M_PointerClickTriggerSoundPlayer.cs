using UnityEngine.EventSystems;

namespace Mup.SoundSystem
{
    public class M_PointerClickTriggerSoundPlayer : M_SoundPlayer, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            this.Play();
        }
    }
}
