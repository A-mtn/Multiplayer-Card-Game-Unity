using System;
using Unity.Netcode;

namespace CombatSystem
{
    public interface IDamageable
    {
        public int health { get; }
        public int maxHealth { get; }
        event Action healthChanged;
        event Action maxHealthChanged;
        event Action<int> defeated;
        event Action<int> healed;
        event Action<int, bool> damaged;
        event Action evaded;
        bool isInitialized { get; }
        event Action initialized;
        event Action willUninitialize;
        void TakeDamage(IDamage rawDamage);
    }
}