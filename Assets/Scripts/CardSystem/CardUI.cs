using System;
using System.Collections;
using System.Collections.Generic;
using CardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardSystem
{
    public class CardUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public TMP_Text damageText;
        public TMP_Text healText;

        public Action<CardData, CardUI> clicked;

        private CardData m_CardData;
        [SerializeField] private Button m_CardPlayButton;

        private bool isSelected = false;

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

        public void ScaleUp()
        {
            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Change this to the scale you want
        }

        public void ResetScale()
        {
            transform.localScale = new Vector3(1, 1, 1); // Reset to original scale
            isSelected = false;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            //Debug.Log("card down");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log("card up");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("card click");
            if (isSelected)
                return;
            
            ScaleUp();
            clicked?.Invoke(m_CardData, this);
            isSelected = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("card enter");
            if (isSelected)
                return;
            this.gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("card exit");
            if (isSelected)
                return;
            this.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}

