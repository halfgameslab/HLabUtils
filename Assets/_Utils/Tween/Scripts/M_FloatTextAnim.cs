using DG.Tweening;
using Mup.Misc.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class M_FloatTextAnim : MonoBehaviour {

    RectTransform _rectTransform;
    //Vector2 _targetPosition = Vector2.zero;

    private void Awake()
    {
        _rectTransform = this.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        //_rectTransform = this.GetComponent<RectTransform>();
        //_targetPosition = _rectTransform.anchoredPosition + (Vector2.up * 50);

        //StartTween();

        //Color c = _rectTransform.GetComponent<Text>().color;
        Color c = Random.ColorHSV();
        c.a = 0;
        _rectTransform.GetComponent<Text>().color = c;
    }

    public void StartTween (bool isCritical)
    {
        float toScale = 1.3f;

        if (isCritical)
            toScale = 1.9f;

        _rectTransform.DOScale(Vector3.one * toScale, 0.2f).From(Vector3.one*0.3f);
        _rectTransform.GetComponent<Text>().DOFade(1, 0.2f).OnComplete(OnCompleteAlpha01Handler);
    }

    void OnCompleteTweenHandler()
    {
        M_ObjectPool.Instance.Store(this.gameObject);
    }

    void OnCompleteAlpha01Handler()
    {
        _rectTransform.GetComponent<Text>().DOFade(0, 0.49f);
        _rectTransform.DOMove(_rectTransform.anchoredPosition + (Vector2.up * 200), 0.5f).OnComplete(OnCompleteTweenHandler);
    }
}
