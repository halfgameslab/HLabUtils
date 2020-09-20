using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.SoundSystem
{
    public class M_TapButtonSoundPlayer : M_SoundPlayer
    {
        void Awake()
        {
            //adiciona o evento de click ao botão
            this.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => this.Play());
        }
    }
}