using Mup.Misc.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class M_FloatNumberManager : MonoBehaviour
{
    static private M_FloatNumberManager _instance;
    static public M_FloatNumberManager Instance { get { return _instance; } }

    [SerializeField]
    private GameObject _numberPrefab;

    private void Awake()
    {
        if(_instance)
        {
            Destroy(this);
            return; 
        }

        _instance = this;
    }

    public void Create(Vector3 position, int number, bool isCritical = false)
    {
        GameObject ob = M_ObjectPool.Instance.Create(_numberPrefab);

        ob.SetActive(false);

        ob.transform.SetParent(GameObject.Find("3DCanvas").transform);

        //position = Camera.main.WorldToScreenPoint(position);

        //position = pos

        //position = position + (Random.insideUnitSphere*15f);
        
        //position.z = 0;

        ob.transform.position = position;

        ob.transform.localScale = _numberPrefab.transform.localScale;//Vector3.one;

        ob.transform.LookAt(Camera.main.transform.position);

        ob.GetComponent<Text>().text = number.ToString();

        ob.SetActive(true);

        ob.SendMessage("Play");
    }
	
}
