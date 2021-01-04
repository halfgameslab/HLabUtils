using Mup.EventSystem.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.SoundSystem
{

    public class M_SoundManager : MonoBehaviour
    {

        //basic singleton control
        private static M_SoundManager _instance;
        public static M_SoundManager Instance
        {
            get
            {
                if (!_instance)
                    new GameObject("SoundManager", typeof(M_SoundManager));

                return _instance;
            }
        }
        //end

        [SerializeField] private AudioSource _2dSoundFX;
        [SerializeField] private AudioClip[] _audioClips;

        private bool _isSoundActive = true;
        public bool IsSoundActive
        {
            get { return _isSoundActive; }
            set
            {
                if (_isSoundActive != value)
                {
                    _isSoundActive = value;

                    if (!value)
                        this.DispatchEvent(ES_Event.ON_DISABLE, "Sound");
                    else
                        this.DispatchEvent(ES_Event.ON_ENABLE, "Sound");
                }
            }
        }

        private bool _isMusicActive = true;
        public bool IsMusicActive
        {
            get { return _isMusicActive; }
            set
            {
                if (_isMusicActive != value)
                {
                    _isMusicActive = value;

                    if (!value)
                        this.DispatchEvent(ES_Event.ON_DISABLE, "Music");
                    else
                        this.DispatchEvent(ES_Event.ON_ENABLE, "Music");
                }
            }
        }

        public AudioClip[] AudioClips
        {
            get
            {
                return _audioClips;
            }

            set
            {
                _audioClips = value;
            }
        }

        public M_SoundManager()
        {
            _instance = this;
        }

        public void Play2DFXByIndex(int index)
        {
            Play2DFX(AudioClips[index]);
        }

        public void Play2DFXByName(string name)
        {
            Play2DFX(Array.Find(AudioClips, e => e.name == name));
        }

        public void Play2DFX(AudioClip clip)
        {
            if (IsSoundActive && _2dSoundFX && clip)
                _2dSoundFX.PlayOneShot(clip);
        }

        public void PlayClipAt(int index, Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(AudioClips[index], position);
        }

        public void Stop2DClip()
        {
            if (_2dSoundFX.isPlaying && IsSoundActive && _2dSoundFX)
                _2dSoundFX.Stop();
        }
    }
}
