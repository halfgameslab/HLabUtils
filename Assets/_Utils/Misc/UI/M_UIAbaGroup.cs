using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class M_UIAbaGroup : MonoBehaviour
{
    private Button _currentAba;
    private Button[] _buttons;

    private Button[] Buttons
    {
        get
        {
            if(_buttons == null)
            {
                _buttons = GetComponentsInChildren<Button>();

                foreach (Button button in Buttons)
                {
                    button.onClick.AddListener(delegate { Select(button); });
                }
            }

            return _buttons;
        }
    }

    public void Select(Button aba)
    {
        if (_currentAba)
            _currentAba.interactable = true;

        aba.interactable = false;

        _currentAba = aba;
    }

    public void Select(int index)
    {
        if(Buttons != null && index < Buttons.Length)
            Select(Buttons[index]);
    }
}
