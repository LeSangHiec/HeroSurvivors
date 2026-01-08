using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardProgress
{
    public CardSO card;
    public int currentLevel;

    public CardProgress(CardSO card)
    {
        this.card = card;
        this.currentLevel = 1;
    }
}

public class CardTracker : MonoBehaviour
{
    public static CardTracker Instance { get; private set; }

    [Header("Card Progress")]
    [SerializeField] private List<CardProgress> pickedCards = new List<CardProgress>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== CARD MANAGEMENT ==========

    public void AddCard(CardSO card)
    {
        CardProgress progress = GetCardProgress(card);

        if (progress != null)
        {
            UpgradeCard(progress, card);
        }
        else
        {
            UnlockCard(card);
        }
    }

    void UpgradeCard(CardProgress progress, CardSO card)
    {
        if (progress.currentLevel < card.maxLevel)
        {
            progress.currentLevel++;
        }
       
    }

    void UnlockCard(CardSO card)
    {
        pickedCards.Add(new CardProgress(card));
    }

    // ========== QUERIES ==========

    public CardProgress GetCardProgress(CardSO card)
    {
        return pickedCards.Find(p => p.card == card);
    }

    public int GetCardLevel(CardSO card)
    {
        CardProgress progress = GetCardProgress(card);
        return progress != null ? progress.currentLevel : 0;
    }

    public bool HasCard(CardSO card)
    {
        return GetCardProgress(card) != null;
    }

    public bool IsCardMaxLevel(CardSO card)
    {
        CardProgress progress = GetCardProgress(card);
        return progress != null && progress.currentLevel >= card.maxLevel;
    }

    public List<CardProgress> GetAllPickedCards()
    {
        return new List<CardProgress>(pickedCards);
    }

    // ========== UTILITY ==========

    public void ResetCards()
    {
        pickedCards.Clear();
    }

    //public void LogAllCards()
    //{
    //    Debug.Log("========== PICKED CARDS ==========");
    //    foreach (var progress in pickedCards)
    //    {
    //        Debug.Log($"{progress.card.cardText}: Level {progress.currentLevel}/{progress.card.maxLevel}");
    //    }
    //    Debug.Log("==================================");
    //}
}