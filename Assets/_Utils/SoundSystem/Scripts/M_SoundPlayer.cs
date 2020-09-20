using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.SoundSystem
{
    public class M_SoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private bool _playOnEnable = false;
        
        [SerializeField]
        private int _audioClipIndex = -1;
        
        void OnEnable()
        {
            if (_playOnEnable)
                Play();
        }

        public void Play()
        {
            if (_audioClipIndex != -1)
                M_SoundManager.Instance.Play2DFXByIndex(_audioClipIndex);
        }

        public void PlayClip(AudioClip clip)
        {
            M_SoundManager.Instance.Play2DFX(clip);
        }
    }
}