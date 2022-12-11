using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace HP_
{
    public class HPHandler : NetworkBehaviour
    {
        [SerializeField] private bool isZombie;
        [SerializeField] private float startHp;

        [Networked(OnChanged = nameof(OnHpChanged))] public byte HP { get; set; }
        [Networked(OnChanged = nameof(OnStateChanged))] public bool isDead { get; set; }
        
        
        public Color uiOnHitColor;
        public Image uiOnHitImage;
        public SkinnedMeshRenderer bodyMeshRenderer;
        public Color defaultMeshBodyColor;
        private bool isInitialized = false;
        private IHealth iHealth;

        private void Start()
        {
            HP = (byte) startHp;
            isDead = false;

            defaultMeshBodyColor = bodyMeshRenderer.material.color;
            iHealth = GetComponent<IHealth>();
            isInitialized = true;
        }

        IEnumerator OnHit()
        {
            bodyMeshRenderer.material.color = Color.red;
          
            if (Object.HasInputAuthority && !isZombie)
            {
                uiOnHitImage.color = uiOnHitColor;
            }
            
            yield return new WaitForSeconds(0.2f);
            
            bodyMeshRenderer.material.color = defaultMeshBodyColor;
            if (Object.HasInputAuthority && !isZombie)
            {
                uiOnHitImage.color = new Color(0, 0, 0, 0);
            }
        }

        //Called only from server
        public void OnTakeDamage()
        {
            if(isDead) return;
            HP -= 1;

            if (HP <= 0)
            {
                isDead = true;
            }
        }

        static void OnHpChanged(Changed<HPHandler> changed)
        {
            byte newHp = changed.Behaviour.HP;
            changed.LoadOld();
            byte oldHp = changed.Behaviour.HP;

            if (newHp < oldHp)
            {
                changed.Behaviour.OnHpReduce();
            }
        }
        private void OnHpReduce()
        {
            if (!isInitialized) return;
            StartCoroutine(OnHit());
        }
        
        static void OnStateChanged(Changed<HPHandler> changed)
        {
            bool isCurrentlyDead = changed.Behaviour.isDead;
            changed.LoadOld();

            bool isDeadOld = changed.Behaviour.isDead;

            if (isCurrentlyDead)
            {
                changed.Behaviour.OnDeath();
            }
        }

        private void OnDeath()
        {
            if (iHealth == null) return;
            iHealth.Death();
        }

    }
}
