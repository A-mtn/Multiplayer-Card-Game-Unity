using UnityEngine;

namespace CombatSystem
{
    public class Damage : IDamage
    {
        public bool isCriticalHit { get; private set; }
        public int magnitude { get; private set; }
        public GameObject instigator { get; private set; }
        public object source { get; private set; }

        public Damage(bool critical, int dmg, GameObject instigator, object source) {
            isCriticalHit = critical;
            magnitude = dmg;
            this.instigator = instigator;
            this.source = source;
        }
    }
}