using Mup.EventSystem.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Mup.Misc.Generic
{
    public class M_Timer : MonoBehaviour
    {
        [SerializeField] private float _duration = 2f;
        [SerializeField] private bool _playOnEnable = false;

        [SerializeField] private UnityEvent _onPlay;
        [SerializeField] private UnityEvent _onStop;
        [SerializeField] private UnityEvent _onComplete;

        private float _startTime = -1;
        private Coroutine _updateCoroutine;
        private float _originalDuration;
        public float ElapsedTime
        {
            get
            {
                if (IsPlaying)
                    return Mathf.Min(Time.realtimeSinceStartup - _startTime, Duration);
                
                return -1;
            }
        }

        public float NormalizedElapsedTime
        {
            get
            {
                return ElapsedTime / Duration;
            }
        }

        public float Duration
        {
            get
            {
                return _duration;
            }

            set
            {
                _duration = value;
            }
        }

        public bool IsPlaying
        {
            get { return _updateCoroutine != null; }
        }
        private void Awake()
        {
            _originalDuration = _duration;
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
        }

        public void Play(float duration = -1)
        {
            _onPlay.Invoke();
            //if durantion has been passed as parameter
            if (duration >= 0)
                Duration = duration;
            else//else use the duration passed through the editor
                Duration = _originalDuration;

            _startTime = Time.realtimeSinceStartup;
            _updateCoroutine = StartCoroutine(UpdateTimer());
            
            this.DispatchEvent(ES_Event.ON_PLAY, this);
        }

        public void Stop()
        {
            if(_updateCoroutine != null)
                StopCoroutine(_updateCoroutine);

            _updateCoroutine = null;
            _startTime = -1;
            this.DispatchEvent(ES_Event.ON_STOP, this);
            _onStop.Invoke();
        }

        private IEnumerator UpdateTimer()
        {
            yield return new WaitForSecondsRealtime(Duration);

            OnCompleteTimerHandler();
        }

        private void OnCompleteTimerHandler()
        {
            _onComplete.Invoke();

            if (_updateCoroutine != null)
                StopCoroutine(_updateCoroutine);

            _startTime = -1;
            _updateCoroutine = null;
            this.DispatchEvent(ES_Event.ON_COMPLETE, this);
        }
    }
}