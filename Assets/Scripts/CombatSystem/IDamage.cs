using UnityEngine;

namespace CombatSystem
{
    public interface IDamage
    {
        bool isCriticalHit { get; }
        int magnitude { get; }
        GameObject instigator { get; }
        object source { get; }
    }
}