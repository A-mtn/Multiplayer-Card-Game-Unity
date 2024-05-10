using System;
using System.Collections;
using System.Collections.Generic;
using CardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardSystem
{
    public class CardUI : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public TMP_Text damageText;
        public TMP_Text healText;

        private CardData m_CardData;
        [SerializeField] private Button m_CardPlayButton;


        private void Awake()
        {
            m_CardPlayButton.onClick.AddListener(DisplayTargetSelection);
        }

        private void DisplayTargetSelection()
        {
        }

        public void SetCardData(CardData cardData)
        {
            m_CardData = cardData;
            Debug.Log("rendering card: " + cardData.Name + " , " + cardData.Description + " , " + cardData.Damage + " , " + cardData.Heal);
            nameText.text = cardData.Name;
            descriptionText.text = cardData.Description;
            damageText.text = cardData.Damage.ToString();
            healText.text = cardData.Heal.ToString();
        }
    }
}

