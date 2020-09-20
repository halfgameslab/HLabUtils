using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct M_TweenUnityEvent
{
    [SerializeField] private string _name;
    [SerializeField] private UnityEvent _callback;

    public string EventName { get => _name; set => _name = value; }
    public UnityEvent Callback { get => _callback; set => _callback = value; }

    public void Invoke()
    {
        Callback.Invoke();
    }
}

/// <summary>
/// A visual editor to use with Dotween
/// </summary>
[HelpURL("http://dotween.demigiant.com/documentation.php")]
public class M_Tween : MonoBehaviour
{
    static public string[] EVENT_NAMES { get; private set; } = new string[]
    {
        "OnComplete",
        "OnKill",
        "OnPlay",
        "OnPause",
        "OnRewind",
        "OnStart",
        "OnStepComplete",
        "OnUpdate"
    };

    [SerializeField] private bool _playOnEnable = true;
    [SerializeField] private float _delay = 0f;
    [SerializeField] private float _timeScale = 1f;
    [SerializeField] private bool _autoKill = true;
    [SerializeField] private bool _ignoreTimeScale = true;
    [SerializeField] private UpdateType _updateType = UpdateType.Normal;
    
    [SerializeField] private int _repeatCount = 0;//if repeat count != 0 play backward dont work
    [SerializeField] private LoopType _repeatType = LoopType.Incremental;

    [SerializeField] private List<M_TweenAction> _actions = new List<M_TweenAction>();

    [SerializeField] private List<M_TweenUnityEvent> _eventList = new List<M_TweenUnityEvent>();

    public List<M_TweenAction> Actions { get => _actions; set => _actions = value; }
    public List<M_TweenUnityEvent> EventList { get => _eventList; set => _eventList = value; }
    public Sequence Sequence { get; private set; }

    protected void OnEnable()
    {
        if(_playOnEnable)
        {
            Play();
        }
    }
    
    /// <summary>
    /// Create the dotween sequence ready to play
    /// </summary>
    public void Setup()
    {
        Sequence = DOTween.Sequence();
        Sequence.Pause();

        Sequence.timeScale = _timeScale;
        Sequence.SetDelay(_delay);
        Sequence.SetAutoKill(_autoKill);
        //_sequence.SetId()
        Sequence.SetLoops(_repeatCount>=0?_repeatCount+1: _repeatCount, _repeatType);
        Sequence.SetUpdate(_updateType, _ignoreTimeScale);

        SetSequenceCallbacksByEvents(Sequence, _eventList);
        
        //append each action on the sequence
        foreach (M_TweenAction action in _actions)
        {
            if (action.AddType == M_TweenSequenceAddType.APPEND)
                Sequence.Append(action.Invoke());
            else if (action.AddType == M_TweenSequenceAddType.JOIN)
                Sequence.Join(action.Invoke());
        }
    }
        
    /// <summary>
    /// Play the sequence
    /// </summary>
    public void Play()
    {
        Setup();
        Sequence.Play();
    }

    /// <summary>
    /// Call sequence playbackwards
    /// I ever configure the sequence to update relative positions
    /// </summary>
    /// <param name="setup">if you want update the relative positions set to true.</param>
    public void PlayBackward(bool setup = true)
    {
        if (setup)
        {
            Setup();
        }
        //Play Backward only work with autokill disable
        Sequence.SetAutoKill(false);
        //complete the sequence without callbacks to play from the end of the sequence
        Sequence.Complete(false);
        //play backwards
        Sequence.PlayBackwards();
        
        //callback to kill the sequence after complete if needed
        if(_autoKill)
            Sequence.onRewind += OnPlayBackwardCompleteHandler;
    }

    /// <summary>
    /// Pause the sequence if the sequence is playing
    /// </summary>
    public void Pause()
    {
        if(Sequence != null && Sequence.IsActive() && Sequence.IsPlaying())
        {
            Sequence.Pause();
        }
    }

    /// <summary>
    /// Resume the current sequence if the sequence is paused
    /// </summary>
    public void Resume()
    {
        if (Sequence != null && Sequence.IsActive() && Sequence.ElapsedPercentage() < 1f)
        {
            Sequence.Play();
        }
    }

    /// <summary>
    /// Stop the current sequence
    /// </summary>
    /// <param name="complete"></param>
    public void Stop(bool complete = false)
    {
        if (Sequence != null && Sequence.IsActive() && Sequence.IsPlaying())
        {
            if (!complete)//if dont want complete the sequence 
            {
                Sequence.Pause();//just pause
                if(_autoKill)//if auto kill
                    Sequence.Kill();//kill the sequence
            }
            else
            {
                Sequence.Complete(true);//complete the sequence
            }
        }
    }

    /// <summary>
    /// Kill the sequence
    /// </summary>
    /// <param name="complete">tell the sequence if they will be completed before killed</param>
    public void Kill(bool complete = false)
    {
        if (Sequence != null && Sequence.IsActive())
        {
            Sequence.Kill(complete);
        }
        else
            Sequence.onRewind -= OnPlayBackwardCompleteHandler;
    }

    /// <summary>
    /// Called when the backward was complete
    /// </summary>
    private void OnPlayBackwardCompleteHandler()
    {
        Kill(false);
    }

    /// <summary>
    /// Append unity event to a tween call
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="eventList"></param>
    private static void SetSequenceCallbacksByEvents(Sequence sequence, List<M_TweenUnityEvent> eventList)
    {
        for (int i = 0; i < eventList.Count; i++)
        {
            if (eventList[i].EventName == EVENT_NAMES[0])
                sequence.onComplete += eventList[i].Invoke;
            else if (eventList[i].EventName == EVENT_NAMES[1])
                sequence.onKill += eventList[i].Invoke;
            else if (eventList[i].EventName == EVENT_NAMES[2])
                sequence.onPlay += eventList[i].Invoke;
            else if (eventList[i].EventName == EVENT_NAMES[3])
                sequence.onPause += eventList[i].Invoke;
            else if (eventList[i].EventName == EVENT_NAMES[4])
                sequence.onRewind += eventList[i].Invoke;
            else if (eventList[i].EventName == EVENT_NAMES[5])
                sequence.OnStart( eventList[i].Invoke);
            else if (eventList[i].EventName == EVENT_NAMES[6])
                sequence.onStepComplete += eventList[i].Invoke;
            else if (eventList[i].EventName == EVENT_NAMES[7])
                sequence.onUpdate += eventList[i].Invoke;
        }
    }
}

