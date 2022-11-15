using System;
using Fusion;
using UnityEngine;

namespace HP_
{
    public class HPHandler : NetworkBehaviour
    {
        [Networked(OnChanged = nameof(OnHpChanged))]
        public byte HP { get; set; }

        [Networked(OnChanged = nameof(OnStateChanged))]
        public bool isDead { get; set; }

        private bool isInitialized = false;

        private const byte startHp = 10;

        private void Start()
        {
            HP = startHp;
            isDead = false;
        }

        //Called only from server
        public void OnTakeDamage()
        {
            if(isDead) return;
            HP -= 1;
            Debug.Log($"{Time.time} Took damage {HP} left");

            if (HP <= 0)
            {
                Debug.Log($"{Time.time} {transform.name}, died");
                isDead = true;
            }
        }

        static void OnHpChanged(Changed<HPHandler> changed)
        {
            Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");
        }
        
        static void OnStateChanged(Changed<HPHandler> changed)
        {
            Debug.Log($"{Time.time} OnStateChange value {changed.Behaviour.isDead}");

        }

    }
}
