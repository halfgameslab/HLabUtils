using Mup.EventSystem.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.Misc.Generic
{
    [RequireComponent(typeof(M_Health))]
    public class M_HitDamage : MonoBehaviour
    {
        private M_Health _health;

        private void OnEnable()
        {
            if (!_health)
                _health = this.GetComponent<M_Health>();

            this.AddEventListener(ES_Event.ON_HIT, OnHitHandler);
        }

        private void OnDisable()
        {
            this.RemoveEventListener(ES_Event.ON_HIT, OnHitHandler);
        }

        private void OnHitHandler(ES_Event ev)
        {
            AddDamage((float)ev.Data);
        }

        public void AddDamage(float damage)
        {
            _health.Amount -= damage;
        }
    }
}