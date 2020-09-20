using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public enum M_TweenTypeV2
{
    MOVE      = 0,
    MOVE_LOCAL = 1,
    SCALE     = 2,
    ROTATE    = 3,
    COLOR     = 4,
    FADE      = 5,
    PUNCH_POS = 6,
    PUNCH_ROT = 7,
    PUNCH_SCA = 8,
    SHAKE_POS = 9,
    SHAKE_ROT = 10,
    SHAKE_SCA = 11,
    
}

public enum M_TweenSequenceAddType
{ 
    APPEND = 0,
    JOIN = 1
}

public enum M_TweenTargetType
{
    SELF = 0,
    OTHER = 1
}

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class M_TweenAction
{
    private static System.Func<M_TweenAction, Tween>[] Actions { get; set; } = new System.Func<M_TweenAction, Tween>[]
    {
        TMove, TMoveLocal, TScale, TRotate, TColor, TFade, TPunchPosition, TPunchRotation, TPunchScale, TShakePosition, TShakeRotation, TShakeScale
    };
    public M_TweenTypeV2 TweenType { get => _type; set => _type = value; }
    public M_TweenSequenceAddType AddType { get => _addType; set => _addType = value; }
    public float Duration { get => _duration; set => _duration = value; }
    public float Delay { get => _delay; set => _delay = value; }
    public int RepeatCount { get => _repeatCount; set => _repeatCount = value; }
    public LoopType LoopType { get => _loopType; set => _loopType = value; }
    public Object Target { get => _target; set => _target = value; }
    public Ease Ease { get => _ease; set => _ease = value; }
    public bool IsRelative { get => _isRelative; set => _isRelative = value; }
    public M_TweenTargetType TargetType { get => _targetType; set => _targetType = value; }

    [SerializeField] protected M_TweenTypeV2 _type;
    [SerializeField] private M_TweenSequenceAddType _addType;
    [SerializeField] private M_TweenTargetType _targetType = M_TweenTargetType.SELF;

    [SerializeField] protected Vector3 _toVector3 = Vector3.zero;
    [SerializeField] protected Vector3 _fromVector3 = Vector3.zero;

    [SerializeField] protected float _toFloat = 0f;
    [SerializeField] protected float _fromFloat = 0f;

    [SerializeField] protected Color _toColor = Color.white;
    [SerializeField] protected Color _fromColor = Color.white;

    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _delay = 0f;

    [SerializeField] private int _vibrato = 10;
    [SerializeField] private float _elasticity = 1f;
    [SerializeField] private float _randomness = 90f;

    [SerializeField] private int _repeatCount = 0;
    [SerializeField] private LoopType _loopType = LoopType.Restart;
    [SerializeField] private UnityEngine.Object _target;
    [SerializeField] private Ease _ease = Ease.Linear;
    [SerializeField] private AnimationCurve _customEaseCourve = new AnimationCurve(new Keyframe[] { new Keyframe(0,0), new Keyframe(1, 1) });
    [SerializeField] private RotateMode _rotationMode = RotateMode.FastBeyond360;
    [SerializeField] private bool _useEase = true;
    [SerializeField] private bool _isRelative = false;
    [SerializeField] private bool _useFrom = false;
    
    #region local
    public Tween Invoke()
    {
        if (Target == null)
            return null;

        Tween tween = Actions[(int)_type]?.Invoke(this)
            .SetDelay(_delay)
            .SetRelative(_isRelative)
            .OnStart(OnStartHandler)
            .OnComplete(OnCompleteHandler);

        if (_useEase)
            tween.SetEase(_ease);
        else
            tween.SetEase(_customEaseCourve);

        if (_repeatCount != 0)
        {
            tween.SetLoops(_repeatCount+1, _loopType);
        }
        
        return tween;
    }

    /// <summary>
    /// Callback called when the animation has started
    /// </summary>
    private void OnStartHandler()
    {
        
    }

    /// <summary>
    /// Callback called when the animation has finished
    /// </summary>
    private void OnCompleteHandler()
    {
        
    }

    #endregion local

    #region static
    
    /// <summary>
    /// Create a move animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TMove(M_TweenAction action)
    {
        Transform t = (action._target as Transform);

        if (!action._useFrom)
            return t.DOMove(action._toVector3, action._duration);

        return (action._target as Transform).DOMove(action._toVector3, action._duration).From(action._fromVector3);
    }

    /// <summary>
    /// Create a local move animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TMoveLocal(M_TweenAction action)
    {
        Transform t = (action._target as Transform);

        if (!action._useFrom)
            return t.DOLocalMove(!action._isRelative ? action._toVector3 : t.position + action._toVector3, action._duration);

        return (action._target as Transform).DOLocalMove(action._toVector3, action._duration).From(action._fromVector3);
    }

    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TScale(M_TweenAction action)
    {
        if (!action._useFrom)
            return (action._target as Transform).DOScale(action._toVector3, action._duration);

        return (action._target as Transform).DOScale(action._toVector3, action._duration).From(action._fromVector3);
    }

    /// <summary>
    /// Create a rotation animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TRotate(M_TweenAction action)
    {
        if (!action._useFrom)
            return (action._target as Transform).DORotate(action._toVector3, action._duration, action._rotationMode);

        return (action._target as Transform).DORotate(action._toVector3, action._duration, action._rotationMode).From(action._fromVector3);
    }

    /// <summary>
    /// Create a rotation animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TColor(M_TweenAction action)
    {
        if((action._target is Graphic))
        {
            if (!action._useFrom)
                return (action._target as Graphic).DOColor(action._toColor, action._duration).From((action._target as Graphic).color);
            
            return (action._target as Graphic).DOColor(action._toColor, action._duration).From(action._fromColor);
        }
                
        if (!action._useFrom)
            return (action._target as Renderer).material.DOColor(action._toColor, action._duration).From((action._target as Renderer).material.color);

        return (action._target as Renderer).material.DOColor(action._toColor, action._duration).From(action._fromColor);

    }

    /// <summary>
    /// Create a rotation animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TFade(M_TweenAction action)
    {
        if ((action._target is Graphic))
        {
            if (!action._useFrom)
                return (action._target as Graphic).DOFade(action._toFloat, action._duration);

            return (action._target as Graphic).DOFade(action._toFloat, action._duration).From(action._fromFloat);
        }
        
        if (!action._useFrom)
            return (action._target as Renderer).material.DOFade(action._toFloat, action._duration);

        return (action._target as Renderer).material.DOFade(action._toFloat, action._duration).From(action._fromFloat);

    }


    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TShakePosition(M_TweenAction action)
    {
        return (action._target as Transform).DOShakePosition(action._duration, action._elasticity, action._vibrato, action._randomness);
    }

    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TShakeRotation(M_TweenAction action)
    {
        return (action._target as Transform).DOShakeRotation(action._duration, action._elasticity, action._vibrato, action._randomness);
    }

    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TShakeScale(M_TweenAction action)
    {
        return (action._target as Transform).DOShakeScale(action._duration, action._elasticity, action._vibrato, action._randomness);
    }

    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TPunchPosition(M_TweenAction action)
    {
        return (action._target as Transform).DOPunchPosition(action._toVector3, action._duration, action._vibrato, action._elasticity);

    }

    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TPunchRotation(M_TweenAction action)
    {
        return (action._target as Transform).DOPunchRotation(action._toVector3, action._duration, action._vibrato, action._elasticity);

    }

    /// <summary>
    /// Create a scale animation using DOTween object
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static private Tween TPunchScale(M_TweenAction action)
    {
        return (action._target as Transform).DOPunchScale(action._toVector3, action._duration, action._vibrato, action._elasticity);

    }
    #endregion static
}