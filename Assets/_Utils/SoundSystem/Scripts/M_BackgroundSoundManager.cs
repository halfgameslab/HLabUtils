using Mup.EventSystem.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Mup.SoundSystem
{
    public class M_BackgroundSoundManager : MonoBehaviour
    {
        void Start()
        {
            M_SoundManager soundManager = M_SoundManager.Instance;

            if (soundManager && !soundManager.IsMusicActive)
                OnMusicActiveStateHandler(false);
        }

        private void OnEnable()
        {
            if (M_SoundManager.Instance)
            {
                M_SoundManager.Instance.AddEventListener(ES_Event.ON_DISABLE, OnMusicDislableHandler);
                M_SoundManager.Instance.AddEventListener(ES_Event.ON_ENABLE, OnMusicEnableHandler);
            }
        }

        private void OnDisable()
        {
            if (M_SoundManager.Instance)
            {
                M_SoundManager.Instance.RemoveEventListener(ES_Event.ON_DISABLE, OnMusicDislableHandler);
                M_SoundManager.Instance.RemoveEventListener(ES_Event.ON_ENABLE, OnMusicEnableHandler);
            }
        }

        public void OnMusicDislableHandler(ES_Event ev)
        {
            if (((string)ev.Data) == "Music")
                OnMusicActiveStateHandler(false);
        }

        public void OnMusicEnableHandler(ES_Event ev)
        {
            if (((string)ev.Data) == "Music")
                OnMusicActiveStateHandler(true);
        }

        public void OnMusicActiveStateHandler(bool state)
        {
            AudioSource audioSource = this.GetComponent<AudioSource>();

            if (audioSource.isPlaying && state == false)
            {
                audioSource.Stop();
            }
            else if (!audioSource.isPlaying && state == true)
            {
                audioSource.Play();
            }
        }
    }
}