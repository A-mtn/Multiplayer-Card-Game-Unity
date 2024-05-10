using System;
using Unity.Netcode;

namespace CombatSystem
{
    public interface IDamagable
    {
        NetworkVariable<int> health { get; }
        NetworkVariable<int> maxHealth { get; }
        event Action healthChanged;
        event Action maxHealthChanged;
        event Action defeated;
        event Action<int> healed;
        event Action<int, bool> damaged;
        event Action evaded;
        void TakeDamage(IDamage rawDamage);
    }
}