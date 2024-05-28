using SoldierSystem;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CombatSystem.UI
{
    public class HealthBarUI : NetworkBehaviour
    {
        private Slider m_Slider;
        private IDamageable m_Damageable;
        private Soldier m_Soldier;
        [SerializeField] private GameObject m_Owner;
        
        private void Awake()
        {
            m_Slider = GetComponent<Slider>();
            m_Damageable = m_Owner.GetComponent<IDamageable>();
            m_Soldier = m_Owner.GetComponent<Soldier>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsClient) return;
            
            m_Soldier.maxHealth_replicated.OnValueChanged += OnMaxHealthChanged;
            m_Soldier.health_replicated.OnValueChanged += OnHealthChanged;
            m_Slider.maxValue = m_Soldier.maxHealth_replicated.Value;
            m_Slider.value = m_Soldier.health_replicated.Value;

            m_Damageable.maxHealthChanged += OnSetInitialHealths;
        }
 
        private void OnSetInitialHealths()
        {
            m_Slider.maxValue = m_Soldier.maxHealth_replicated.Value;
            m_Slider.value = m_Soldier.health_replicated.Value;
        }
        private void OnHealthChanged(int oldValue, int newValue)
        {
            m_Slider.value = newValue;
        }

        private void OnMaxHealthChanged(int oldValue, int newValue)
        {
            m_Slider.maxValue = newValue;
        }
        
        private void OnValidate()
        {
            if (m_Owner != null)
            {
                if (m_Owner.GetComponent<Soldier>() == null)
                {
                    Debug.LogWarning("The owner must implement the Soldier class!");
                    m_Owner = null;
                }
            }
        }
    }
}