using Mup.EventSystem.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.Interactable
{
    public class M_SimpleRaycast : MonoBehaviour
    {
        [SerializeField] private float _distance = 10f;
        [SerializeField] private LayerMask _eventMask = -1;

        private RaycastHit _hit;
        private Ray _ray = new Ray();
        private Camera _camera;

        //private Transform _transform;

        private void Awake()
        {
           // _transform = transform;
            _camera = this.GetComponent<Camera>();
        }
        
        private void Update()
        {
            CheckRay();
        }

        private void CheckRay()
        {
                if (_camera != null)
                {
                    RaycastHit currentHit;
                    
                    _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    
                    //if hit cast something
                    if (Physics.Raycast(_ray, out currentHit, _distance, _eventMask))
                    {
                        //check if was diferent hit
                        if (_hit.transform != currentHit.transform)
                        {
                            //if hit wasn't null
                            if(_hit.transform)
                                _hit.transform.gameObject.DispatchEvent(ES_Event.ON_EXIT, _hit);//send exit event for last object

                            currentHit.transform.gameObject.DispatchEvent(ES_Event.ON_ENTER, currentHit);//send enter for the current objet
                        }

                        //if clicked
                        if (Input.GetMouseButtonUp(0))
                        {
                            currentHit.transform.gameObject.DispatchEvent(ES_Event.ON_CLICK, currentHit);//send clicked for object
                        }
                        
                    }
                    else if(_hit.transform)//if cast nothing and had some object 
                    {
                        _hit.transform.gameObject.DispatchEvent(ES_Event.ON_EXIT, _hit);//send exit event
                    }

                    _hit = currentHit;//store current hit
                }
        }
    }
}