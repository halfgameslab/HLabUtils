using Mup.Misc.Generic;
using Mup.Misc.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(M_Health))]
public class AttachHealthBarControl : MonoBehaviour {
    
    [SerializeField]
    private M_ProgressiveBar _healthBarPrefab;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    [SerializeField]
    private Color _mainColor = Color.green;

    [SerializeField]
    private bool _activeOnValueChange = false;

    private M_ProgressiveBar _healthBar;

    private void OnEnable()
    {
        if (_healthBarPrefab)
        {
            _healthBar = M_ObjectPool.Instance.Create(_healthBarPrefab.gameObject).GetComponent<M_ProgressiveBar>();//Instantiate<M_ProgressiveBar>(_healthBarPrefab);
            _healthBar.Value = this.GetComponent<M_Health>();
            _healthBar.MainColor = _mainColor;
            _healthBar.ActiveOnValueChange = _activeOnValueChange;

            UI3DTargetFollower ws = _healthBar.GetComponent<UI3DTargetFollower>();
            ws.Target = this.transform;
            ws.Offset = _offset;

            _healthBar.transform.SetParent(GameObject.Find("3DCanvas").transform);
            _healthBar.transform.localScale = _healthBarPrefab.transform.localScale;

            _healthBar.Init();
        }
    }

    private void OnDisable()
    {
        DestroyHealthBar();
    }

    public void DestroyHealthBar()
    {
        if (_healthBar)
        {
            //if(M_ObjectPool.Instance)
            //    M_ObjectPool.Instance.Store(_healthBarPrefab.gameObject);
            //else
            //    Destroy(_healthBarPrefab.gameObject);
            _healthBar.GetComponent<M_Tween>().PlayBackward();

            //this.enabled = false;
        }
    }
}
