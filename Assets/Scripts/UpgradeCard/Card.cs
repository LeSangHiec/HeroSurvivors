using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cardImageRenderer;
    [SerializeField] private TMP_Text cardTextRenderer;
    [SerializeField] private Button cardButton; // ← THÊM

    private CardSO cardInfo;
    private CardManager cardManager; // ← THÊM

    void Awake()
    {
        // ← THÊM: Get button
        if (cardButton == null)
        {
            cardButton = GetComponent<Button>();
        }

        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void Setup(CardSO card, CardManager manager)
    {
        cardInfo = card;
        cardManager = manager;

        if (cardImageRenderer != null)
        {
            cardImageRenderer.sprite = card.CardImage;
        }

        if (cardTextRenderer != null)
        {
            cardTextRenderer.text = card.cardText;
        }
    }

    // ← THÊM: Click handler
    void OnCardClicked()
    {
        if (cardManager != null && cardInfo != null)
        {
            cardManager.SelectCard(cardInfo);
        }
    }
}