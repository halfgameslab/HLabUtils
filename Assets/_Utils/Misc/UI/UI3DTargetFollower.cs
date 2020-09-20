using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI3DTargetFollower : MonoBehaviour {

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    [SerializeField]
    private bool _lookAtCamera = true;

    private RectTransform _rectTransform;

    public Transform Target {
        get { return _target; }
        set { _target = value; }
    }

    public Vector3 Offset
    {
        get
        {
            return _offset;
        }

        set
        {
            _offset = value;
        }
    }

    private void Awake()
    {
        _rectTransform = this.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (!Target)
            return;

        _rectTransform.localPosition = _target.position + Offset;

        if(_lookAtCamera)
            _rectTransform.LookAt(Camera.main.transform);
    }
}
