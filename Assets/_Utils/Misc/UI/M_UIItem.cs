using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mup.Misc.UI
{
    public class M_UIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static int _countItens = 0;

        public static bool IsPointerOverUIObject { get { return _countItens != 0; } }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _countItens++;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _countItens--;
        }
    }
}