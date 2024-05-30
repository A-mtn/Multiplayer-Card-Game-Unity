using System;
using System.Collections.Generic;
using CardSystem;
using CombatSystem;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace SoldierSystem
{
    public class Soldier: NetworkBehaviour, IDamageable, IPointerClickHandler
    {
        public NetworkVariable<int> health_replicated = new NetworkVariable<int>();
        public NetworkVariable<int> maxHealth_replicated = new NetworkVariable<int>();
        public int health => health_replicated.Value;
        public int maxHealth => maxHealth_replicated.Value;
        public event Action healthChanged;
        public event Action maxHealthChanged;
        public event Action<int> defeated;
        public event Action<int> healed;
        public event Action<int, bool> damaged;
        public event Action evaded;
        private bool m_IsInitialized;
        public bool isInitialized => m_IsInitialized;
        public event Action initialized;
        public event Action willUninitialize;
        [SerializeField] private int m_soldierId;
        
        [SerializeField] private List<CardData> m_soldierCards;
        public List<CardData> SoldierCards => m_soldierCards;
        public event Action selected;
        public string Name;
        
        public int SoldierId {get => m_soldierId;}

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();         
            health_replicated.OnValueChanged += (oldValue, newValue) => healthChanged?.Invoke();
            maxHealth_replicated.OnValueChanged += (oldValue, newValue) => maxHealthChanged?.Invoke();
            
            m_IsInitialized = true;
            initialized?.Invoke();
        }

        private void OnDisable()
        {
            willUninitialize?.Invoke();
        }

        public void SetInitialValues()
        {
            if (IsServer)
            {
                maxHealth_replicated.Value = 100;
                health_replicated.Value = 100;
                Debug.LogWarning("Setted initial values!!");
                maxHealthChanged?.Invoke();
                healthChanged?.Invoke();
            }
        }

        [ClientRpc]
        private void DamageClientRpc(int magnitude, bool isCriticalHit)
        {
            damaged?.Invoke(magnitude, isCriticalHit);
        }

        [ClientRpc]
        private void HealClientRpc(int amount)
        {
            healed?.Invoke(amount);
        }

        [ClientRpc]
        private void EvadeClientRpc()
        {
            evaded?.Invoke();
        }

        [ClientRpc]
        private void DefeatedClientRpc()
        {
            defeated?.Invoke(m_soldierId);
        }
        
        public void TakeDamage(IDamage rawDamage)
        {
            if (!IsServer) {
                return; 
            }

            if (Random.value < 0.0) { 
                evaded?.Invoke();
                EvadeClientRpc();
                return;
            }

            int damageTaken = rawDamage.magnitude;
            health_replicated.Value += damageTaken;
            if (damageTaken > 0)
            {
                healed?.Invoke(damageTaken);
                HealClientRpc(damageTaken);
            }
            else
            {
                damaged?.Invoke(damageTaken, rawDamage.isCriticalHit);
                DamageClientRpc(damageTaken, rawDamage.isCriticalHit);
            }

            if (health <= 0) {
                health_replicated.Value = 0;
                defeated?.Invoke(m_soldierId);
                //DefeatedClientRpc();
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogWarning("clicked to soldier: " + name);
            selected?.Invoke();
        }
    }
}