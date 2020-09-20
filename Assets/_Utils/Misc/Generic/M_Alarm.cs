using Mup.EventSystem.Events;
using Mup.EventSystem.Events.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace Mup.Misc.Generic
{
    public class M_Alarm
    {
        private string ID { get; set; }

        private Timer _timer;
        private double _startTime = -1;
        private double _totalTime = -1;
        private double _elapsedTime = 0;

        /// <summary>
        /// return elapsed time in seconds
        /// </summary>
        public double ElapsedTime
        {
            get { return (IsPlaying && _startTime != -1) ? (Time.time - _startTime) + _elapsedTime : 0; }
        }

        /// <summary>
        /// return timeLeft in seconds
        /// </summary>
        public double TimeLeft
        {
            get
            {
                if (IsPlaying && _totalTime != -1)
                {
                    double t = _totalTime - (Time.time - _startTime);

                    if (t >= 0)
                        return t;
                    else
                        return 0;
                }

                return 0;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _timer != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">in seconds</param>
        public void Start(double time, ES_MupAction onAlarmCompleteHandler = null, double elapsedTime = 0)
        {
            ID = string.Concat("ALARM_", Time.time.ToString());
            _timer = new Timer(time * 1000);

            _totalTime = time;
            _startTime = Time.time;
            _elapsedTime = elapsedTime;

            _timer.Elapsed += OnTimedEvent;

            if (onAlarmCompleteHandler != null)
                this.AddEventListener(ES_Event.ON_COMPLETE, onAlarmCompleteHandler);

            _timer.Start();
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                _elapsedTime = ElapsedTime;
                _totalTime = TimeLeft;

                _timer.Stop();
                _timer.Dispose();

                _timer = null;
            }
        }

        public void Resume()
        {
            if (!IsPlaying && _totalTime != -1)
            {
                Start(_totalTime, null, _elapsedTime);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            ES_EventManager.RemoveEventListeners(ID, ES_Event.ON_COMPLETE);

            _timer.Stop();
            _timer.Dispose();
            _startTime = -1;
            _totalTime = -1;

            _timer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(System.Object source, ElapsedEventArgs e)
        {
            ES_EventManager.DispatchEvent(this.GetInstanceName(), ES_Event.ON_COMPLETE, null, _totalTime);

            Stop();
        }
    }
}
