using System;
using CombatSystem;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SoldierSystem
{
    public class Soldier: NetworkBehaviour, IDamagable
    {
        public NetworkVariable<int> health { get; }
        public NetworkVariable<int> maxHealth { get; }
        public event Action healthChanged;
        public event Action maxHealthChanged;
        public event Action defeated;
        public event Action<int> healed;
        public event Action<int, bool> damaged;
        public event Action evaded;
        
        public void TakeDamage(IDamage rawDamage)
        {
            if (!IsServer) {
                return; 
            }

            if (Random.value < 0.0) { 
                evaded?.Invoke();
                return;
            }

            int damageTaken = rawDamage.magnitude;
            health.Value -= damageTaken;
            damaged?.Invoke(damageTaken, rawDamage.isCriticalHit);

            if (health.Value <= 0) {
                health.Value = 0;
                defeated?.Invoke();
            }

            UpdateHealthClientRpc(health.Value);
        }
        
        [ClientRpc]
        private void UpdateHealthClientRpc(int newHealth) {
            health.Value = newHealth;
            healthChanged?.Invoke();
        }

        public void Heal(int amount) {
            if (!IsServer) {
                return;
            }

            health.Value += amount;
            if (health.Value > maxHealth.Value) {
                health.Value = maxHealth.Value;
            }

            UpdateHealthClientRpc(health.Value);
        }
    }
}