using SoldierSystem;
using UnityEngine;

namespace CardSystem
{
    public enum CardEffectType { Attack, Heal, Buff, Debuff }
    
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class CardData : ScriptableObject
    {
            public int ID;
            public string Name;
            public Sprite Icon;
            public string Description;
            public int Damage;
            public int Heal;
            public CardEffectType effectType;
            public int SoldierID;
    }
}