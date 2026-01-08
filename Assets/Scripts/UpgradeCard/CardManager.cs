using System.Collections.Generic;
using UnityEngine;

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

    private List<CardSO> alreadySelectedCards = new List<CardSO>();

    private GameObject cardOne;
    private GameObject cardTwo;
    private GameObject cardThree;

    // ========== SHOW/HIDE ==========

    public void ShowCardSelection()
    {
        if (cardSelectionUI == null)
        {
            return;
        }

        cardSelectionUI.SetActive(true);
        RandomizeNewCards();

        Time.timeScale = 0f;

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerCardSelectionShown();
            GameEvents.Instance.TriggerGamePause();
        }
    }

    public void HideCardSelection()
    {
        if (cardSelectionUI != null)
        {
            cardSelectionUI.SetActive(false);
        }

        Time.timeScale = 1f;

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerCardSelectionHidden();
            GameEvents.Instance.TriggerGameResume();
        }
    }

    // ========== CARD RANDOMIZATION ==========

    void RandomizeNewCards()
    {
        DestroyCurrentCards();

        List<CardSO> validCards = GetValidCards();

        if (validCards.Count == 0)
        {
            ResetAndRetry();
            return;
        }

        List<CardSO> selectedCards = SelectRandomCards(validCards);

        InstantiateSelectedCards(selectedCards);
    }

    void DestroyCurrentCards()
    {
        if (cardOne != null) Destroy(cardOne);
        if (cardTwo != null) Destroy(cardTwo);
        if (cardThree != null) Destroy(cardThree);
    }

    List<CardSO> GetValidCards()
    {
        List<CardSO> validCards = new List<CardSO>();

        foreach (CardSO card in deck)
        {
            if (!IsCardValid(card)) continue;
            validCards.Add(card);
        }

        return validCards;
    }

    bool IsCardValid(CardSO card)
    {
        // Skip unique + picked
        if (card.isUnique && alreadySelectedCards.Contains(card)) return false;

        // Skip max level
        if (CardTracker.Instance != null && CardTracker.Instance.IsCardMaxLevel(card)) return false;

        // Check weapon unlock
        if (card.cardType == CardType.Weapon)
        {
            if (WeaponManager.Instance.HasWeapon(card.weaponData)) return false;
        }

        // Check weapon upgrade
        if (card.cardType == CardType.WeaponUpgrade)
        {
            if (!WeaponTracker.Instance.HasWeapon(card.targetWeapon)) return false;
        }

        return true;
    }

    void ResetAndRetry()
    {
        if (CardTracker.Instance != null)
        {
            CardTracker.Instance.ResetCards();
        }
        RandomizeNewCards();
    }

    List<CardSO> SelectRandomCards(List<CardSO> validCards)
    {
        List<CardSO> selectedCards = new List<CardSO>();
        int count = Mathf.Min(3, validCards.Count);

        for (int i = 0; i < count; i++)
        {
            if (validCards.Count == 0) break;

            int idx = Random.Range(0, validCards.Count);
            selectedCards.Add(validCards[idx]);
            validCards.RemoveAt(idx);
        }

        return selectedCards;
    }

    void InstantiateSelectedCards(List<CardSO> selectedCards)
    {
        if (selectedCards.Count > 0) cardOne = InstantiateCard(selectedCards[0], cardPositionOne);
        if (selectedCards.Count > 1) cardTwo = InstantiateCard(selectedCards[1], cardPositionTwo);
        if (selectedCards.Count > 2) cardThree = InstantiateCard(selectedCards[2], cardPositionThree);
    }

    GameObject InstantiateCard(CardSO cardData, Transform position)
    {
        if (cardPrefab == null || position == null)
        {
            Debug.LogError("Card prefab or position is null!");
            return null;
        }

        GameObject newCard = Instantiate(cardPrefab, position);

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
            return;
        }

        TriggerCardEvents(selectedCard);
        TrackCard(selectedCard);
        ApplyCard(selectedCard);
        HideCardSelection();
        NotifyQueue();
    }

    void TriggerCardEvents(CardSO selectedCard)
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerCardSelected(selectedCard);
        }
    }

    void TrackCard(CardSO selectedCard)
    {
        if (CardTracker.Instance != null)
        {
            CardTracker.Instance.AddCard(selectedCard);
        }

        if (selectedCard.isUnique && !alreadySelectedCards.Contains(selectedCard))
        {
            alreadySelectedCards.Add(selectedCard);
        }
    }

    void ApplyCard(CardSO selectedCard)
    {
        switch (selectedCard.cardType)
        {
            case CardType.Upgrade:
                ApplyCardEffect(selectedCard);
                break;
            case CardType.Weapon:
                UnlockWeapon(selectedCard);
                break;
            case CardType.WeaponUpgrade:
                UpgradeWeapon(selectedCard);
                break;
        }
    }

    void NotifyQueue()
    {
        if (CardPickQueue.Instance != null)
        {
            CardPickQueue.Instance.OnCardSelected();
        }
    }

    // ========== CARD EFFECTS ==========

    void ApplyCardEffect(CardSO card)
    {
        if (card.effectType == CardEffect.None) return;

        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        PlayerXP playerXP = FindAnyObjectByType<PlayerXP>();

        switch (card.effectType)
        {
            case CardEffect.MaxHealth:
                playerStats?.IncreaseMaxHealth(card.effectValue);
                break;
            case CardEffect.MoveSpeed:
                playerController?.IncreaseMoveSpeed(card.effectValue);
                break;
            case CardEffect.Damage:
                playerStats?.IncreaseDamage(card.effectValue);
                break;
            case CardEffect.AttackSpeed:
                playerController?.IncreaseAttackSpeed(card.effectValue);
                break;
            case CardEffect.CritChance:
                playerStats?.IncreaseCritChance(card.effectValue);
                break;
            case CardEffect.HealthRegen:
                playerStats?.IncreaseHealthRegen(card.effectValue);
                break;
            case CardEffect.XPGain:
                playerXP?.IncreaseXPGain(card.effectValue);
                break;
        }
    }

    void UnlockWeapon(CardSO card)
    {
        if (card.weaponData == null || card.weaponPrefab == null)
        {
            return;
        }

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.AddWeapon(card.weaponData, card.weaponPrefab);
        }
      
    }

    void UpgradeWeapon(CardSO card)
    {
        if (card.targetWeapon == null)
        {
            return;
        }

        if (WeaponTracker.Instance != null)
        {
            WeaponTracker.Instance.UpgradeWeapon(card.targetWeapon);
        }
      
    }

    // ========== QUERIES ==========

    public bool IsCardSelectionActive()
    {
        return cardSelectionUI != null && cardSelectionUI.activeSelf;
    }
}