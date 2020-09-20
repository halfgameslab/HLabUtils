using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.Misc.Generic
{
    public class M_ObjectPool : MonoBehaviour
    {
        //basic singleton control
        private static M_ObjectPool _instance;
        public static M_ObjectPool Instance
        {
            get
            {
                if (!_instance)
                    new GameObject("_ObjectPool(Cache)", typeof(M_ObjectPool));

                return _instance;
            }
        }
        //end

        Dictionary<string, List<GameObject>> _pool;

        public M_ObjectPool()
        {
            _instance = this;
            _pool = new Dictionary<string, List<GameObject>>();
        }

        public GameObject Create(GameObject unity_object, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            string key = unity_object.name;
            GameObject obj;

            if (_pool.ContainsKey(key))
            {
                if (_pool[key] != null && _pool[key].Count > 0)
                {
                    obj = _pool[key][0];
                    
                    obj.transform.SetParent(parent);

                    obj.gameObject.transform.position = position;
                    obj.gameObject.transform.rotation = rotation;
                    obj.gameObject.SetActive(true);

                    _pool[key].RemoveAt(0);

                    if (_pool[key].Count == 0)
                        _pool.Remove(key);

                    return obj;
                }

            }

            obj = Instantiate<GameObject>(unity_object, position, rotation);
            obj.name = unity_object.name;

            return obj;
        }

        public GameObject Create(GameObject unity_object, Transform parent)
        {
            return Create(unity_object, unity_object.transform.position, unity_object.transform.rotation, parent);
        }

        public GameObject Create(GameObject unity_object)
        {
            return Create(unity_object, unity_object.transform.position, unity_object.transform.rotation);
        }

        /*public GameObject Create(GameObject unity_object)
        {
            string key = unity_object.name;
            GameObject obj;

            if (_pool.ContainsKey(key))
            {
                if (_pool[key] != null && _pool[key].Count > 0)
                {
                    obj = _pool[key][0];

                    obj.transform.SetParent(null);
                    
                    obj.gameObject.SetActive(true);

                    _pool[key].RemoveAt(0);

                    if (_pool[key].Count == 0)
                        _pool.Remove(key);

                    return obj;
                }

            }

            obj = Instantiate<GameObject>(unity_object);
            obj.name = unity_object.name;

            return obj;
        }*/

        public void Store(GameObject unity_object)
        {
            if (unity_object.transform.parent == this.transform)
                return;

            //if (_pool.Count == 0)
            //{
            //    StartCoroutine(GarbageCollectorCoroutine());
            //}

            string key = unity_object.name;

            if (!_pool.ContainsKey(key))
            {
                _pool.Add(key, new List<GameObject>());
            }

            _pool[key].Add(unity_object);

            if (unity_object != null && this != null)
            {
                unity_object.transform.SetParent(this.transform);
                unity_object.gameObject.SetActive(false);
            }
        }

        public void Clear()
        {
            foreach(List<GameObject> objs in _pool.Values)
            {
                foreach(GameObject obj in objs)
                {
                    Destroy(obj);
                }
            }

            _pool.Clear();
        }

        //private IEnumerator GarbageCollectorCoroutine()
        //{
        //    foreach (List<GameObject> objs in _pool.Values)
        //    {
        //        for (int i = objs.Count-1; i > 0; i--)
        //        {
        //            Destroy(objs[i]);

        //            _pool.Remove();
        //            yield return new WaitForSeconds(5f);
        //        }
        //    }
            
        //}

    }
}
