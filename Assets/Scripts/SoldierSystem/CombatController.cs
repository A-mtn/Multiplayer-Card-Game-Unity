using System;
using CombatSystem;
using MainGame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace SoldierSystem
{
    public class CombatController : NetworkBehaviour
    {
        [SerializeField] private FloatingText m_FloatingTextPrefab;
        private ObjectPool<FloatingText> m_Pool;
        private IDamageable m_Damageable;

        private void Awake()
        {
            m_Damageable = GetComponent<IDamageable>();
            m_Pool = new ObjectPool<FloatingText>(OnCreate, OnGet, OnRelease);
        }

        private void OnEnable()
        {
            m_Damageable.initialized += OnDamageableInitialized;
            m_Damageable.willUninitialize += OnDamageableWillUninitialize;
            if (m_Damageable.isInitialized)
                OnDamageableInitialized();
        }

        private void OnDamageableWillUninitialize()
        {
            m_Damageable.damaged -= DisplayDamage;
            m_Damageable.healed -= DisplayRestorationAmount;
            m_Damageable.defeated -= OnDefeated;
            m_Damageable.evaded -= OnEvaded;
        }


        private void OnDamageableInitialized()
        {
            m_Damageable.damaged += DisplayDamage;
            m_Damageable.healed += DisplayRestorationAmount;
            m_Damageable.defeated += OnDefeated;
            m_Damageable.evaded += OnEvaded;
        }
        
        private void DisplayRestorationAmount(int amount)
        {
            FloatingText floatingText = m_Pool.Get();
            floatingText.Set(amount.ToString(), Color.green);
        }

        private void DisplayDamage(int magnitude, bool isCriticalHit)
        {
            FloatingText damageText = m_Pool.Get();
            damageText.Set(magnitude.ToString(), isCriticalHit ? Color.black : Color.red);
            if (isCriticalHit)
                damageText.transform.localScale *= 2;
        }
        
        private void OnEvaded()
        {
            FloatingText damageText = m_Pool.Get();
            damageText.Set("Evaded", Color.yellow);
        }
        
        private void OnDefeated()
        { 
            Destroy(this.gameObject);
        }
        
        private void OnRelease(FloatingText floatingText)
        {
            floatingText.gameObject.SetActive(false);
        }

        private void OnGet(FloatingText floatingText)
        {
            floatingText.transform.position = transform.position + GetCenterOfCollider(transform);
            floatingText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            floatingText.gameObject.SetActive(true);
            floatingText.Animate();
        }

        private FloatingText OnCreate()
        {
            FloatingText floatingText = Instantiate(m_FloatingTextPrefab);
            floatingText.finished += m_Pool.Release;
            return floatingText;
        }
        
        public static Vector3 GetCenterOfCollider(Transform target)
        {
            Vector3 center;
            Collider collider = target.GetComponent<Collider>();
            switch (collider)
            {
                case CapsuleCollider capsuleCollider:
                    center = capsuleCollider.center;
                    break;
                case CharacterController characterController:
                    center = characterController.center;
                    break;
                case SphereCollider sphereCollider:
                    center = sphereCollider.center;
                    break;
                default:
                    center = Vector3.zero;
                    Debug.LogWarning("Could not find center");
                    break;
            }

            return center;
        }
    }
}