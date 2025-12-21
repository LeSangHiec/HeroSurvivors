using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject cardSelectionUI;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardPositionOne;
    [SerializeField] private Transform cardPositionTwo;
    [SerializeField] private Transform cardPositionThree;

    [Header("Card Data")]
    [SerializeField] private List<CardSO> deck = new List<CardSO>();

    // Tracking
    private List<CardSO> alreadySelectedCards = new List<CardSO>();

    // Current cards
    private GameObject cardOne;
    private GameObject cardTwo;
    private GameObject cardThree;

    // ========== CARD SELECTION ==========

    public void ShowCardSelection()
    {
        if (cardSelectionUI == null)
        {
            Debug.LogError("CardManager: Card Selection UI is not assigned!");
            return;
        }

        cardSelectionUI.SetActive(true);
        RandomizeNewCards();

        // Pause game
        Time.timeScale = 0f;

        Debug.Log("Card selection shown - Game paused");
    }

    public void HideCardSelection()
    {
        if (cardSelectionUI != null)
        {
            cardSelectionUI.SetActive(false);
        }

        // Resume game
        Time.timeScale = 1f;

        Debug.Log("Card selection hidden - Game resumed");
    }

    // ========== RANDOMIZE CARDS ==========

    void RandomizeNewCards()
    {
        // Destroy old cards
        if (cardOne != null) Destroy(cardOne);
        if (cardTwo != null) Destroy(cardTwo);
        if (cardThree != null) Destroy(cardThree);

        // Get available cards
        List<CardSO> availableCards = new List<CardSO>(deck);

        // Remove unique cards already selected
        availableCards.RemoveAll(card => card.isUnique && alreadySelectedCards.Contains(card));

        // Remove weapon cards if max weapons reached
        if (WeaponManager.Instance != null && !WeaponManager.Instance.CanAddWeapon())
        {
            availableCards.RemoveAll(card => card.cardType == CardType.Weapon);
            Debug.Log("Max weapons reached - removed weapon cards from pool");
        }

        // Check if enough cards
        if (availableCards.Count < 3)
        {
            Debug.LogWarning($"Not enough available cards! Only {availableCards.Count} cards available.");

            // Fill with non-unique cards if needed
            List<CardSO> nonUniqueCards = deck.FindAll(card => !card.isUnique);
            while (availableCards.Count < 3 && nonUniqueCards.Count > 0)
            {
                CardSO randomCard = nonUniqueCards[Random.Range(0, nonUniqueCards.Count)];
                if (!availableCards.Contains(randomCard))
                {
                    availableCards.Add(randomCard);
                }
            }

            if (availableCards.Count < 3)
            {
                Debug.LogError("Still not enough cards even after adding non-unique cards!");
                return;
            }
        }

        // Randomize 3 cards
        List<CardSO> randomizedCards = new List<CardSO>();

        while (randomizedCards.Count < 3 && availableCards.Count > 0)
        {
            CardSO randomCard = availableCards[Random.Range(0, availableCards.Count)];

            if (!randomizedCards.Contains(randomCard))
            {
                randomizedCards.Add(randomCard);
                availableCards.Remove(randomCard); // Prevent duplicates
            }
        }

        // Instantiate cards
        if (randomizedCards.Count >= 3)
        {
            cardOne = InstantiateCard(randomizedCards[0], cardPositionOne);
            cardTwo = InstantiateCard(randomizedCards[1], cardPositionTwo);
            cardThree = InstantiateCard(randomizedCards[2], cardPositionThree);

            Debug.Log("Cards randomized successfully!");
        }
        else
        {
            Debug.LogError("Failed to randomize 3 cards!");
        }
    }

    GameObject InstantiateCard(CardSO cardData, Transform position)
    {
        if (cardPrefab == null || position == null)
        {
            Debug.LogError("CardManager: Card prefab or position is null!");
            return null;
        }

        // Instantiate card
        GameObject newCard = Instantiate(cardPrefab, position);

        // Setup card
        Card cardComponent = newCard.GetComponent<Card>();
        if (cardComponent != null)
        {
            cardComponent.Setup(cardData, this);
        }
        else
        {
            Debug.LogError("Card prefab doesn't have Card component!");
        }

        return newCard;
    }

    // ========== CARD SELECTION ==========

    public void SelectCard(CardSO selectedCard)
    {
        if (selectedCard == null)
        {
            Debug.LogError("Selected card is null!");
            return;
        }

        Debug.Log($"Card selected: {selectedCard.cardText}");

        // Add to selected list if unique
        if (selectedCard.isUnique && !alreadySelectedCards.Contains(selectedCard))
        {
            alreadySelectedCards.Add(selectedCard);
        }

        // Apply card effect based on type
        switch (selectedCard.cardType)
        {
            case CardType.Upgrade:
                ApplyCardEffect(selectedCard);
                break;

            case CardType.Weapon:
                UnlockWeapon(selectedCard);
                break;
        }

        // Hide card selection
        HideCardSelection();
    }

    // ========== APPLY UPGRADE EFFECTS ==========

    void ApplyCardEffect(CardSO card)
    {
        // ← THÊM: Check nếu None thì return
        if (card.effectType == CardEffect.None)
        {
            Debug.LogWarning($"Card {card.cardText} has no effect type (None)");
            return;
        }

        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        PlayerXP playerXP = FindAnyObjectByType<PlayerXP>();

        switch (card.effectType)
        {
            case CardEffect.None:
                // Do nothing
                break;

            case CardEffect.MaxHealth:
                if (playerStats != null)
                {
                    playerStats.IncreaseMaxHealth(card.effectValue);
                }
                break;

            case CardEffect.MoveSpeed:
                if (playerController != null)
                {
                    playerController.IncreaseMoveSpeed(card.effectValue);
                }
                break;

            case CardEffect.Damage:
                if (playerStats != null)
                {
                    playerStats.IncreaseDamage(card.effectValue);
                }
                break;

            case CardEffect.AttackSpeed:
                if (playerController != null)
                {
                    playerController.IncreaseAttackSpeed(card.effectValue);
                }
                break;

            case CardEffect.CritChance:
                if (playerStats != null)
                {
                    playerStats.IncreaseCritChance(card.effectValue);
                }
                break;

            case CardEffect.HealthRegen:
                if (playerStats != null)
                {
                    playerStats.IncreaseHealthRegen(card.effectValue);
                }
                break;

            case CardEffect.XPGain:
                if (playerXP != null)
                {
                    playerXP.IncreaseXPGain(card.effectValue);
                }
                break;
        }
    }

    // ========== UNLOCK WEAPON ==========

    void UnlockWeapon(CardSO card)
    {
        if (card.weaponData == null || card.weaponPrefab == null)
        {
            Debug.LogError("Card missing weapon data or prefab!");
            return;
        }

        // Use WeaponManager Singleton
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.AddWeapon(card.weaponData, card.weaponPrefab);
        }
        else
        {
            Debug.LogError("WeaponManager.Instance is null!");
        }
    }
}