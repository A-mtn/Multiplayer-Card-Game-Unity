using UnityEngine;

namespace CombatSystem
{
    public class HealthModifier : IDamage
    {
        public bool isCriticalHit { get; set; }
        public int magnitude { get; set; }
        public GameObject instigator { get; set; }
        public object source { get; set; }
    }
}