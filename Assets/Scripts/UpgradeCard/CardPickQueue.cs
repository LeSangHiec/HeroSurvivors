using UnityEngine;
using System.Collections;

public class CardPickQueue : MonoBehaviour
{
    public static CardPickQueue Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CardManager cardManager;

    [Header("Queue Settings")]
    [SerializeField] private float delayBetweenPicks = 0.3f;

    private int remainingPicks = 0;
    private bool isProcessingQueue = false;

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

    void Start()
    {
        if (cardManager == null)
        {
            cardManager = FindFirstObjectByType<CardManager>();
        }
    }

    // ========== QUEUE MANAGEMENT ==========

    public void QueueCardPicks(int count)
    {
        if (count <= 0) return;

        remainingPicks += count;

        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    public void OnCardSelected()
    {
        remainingPicks--;

        if (remainingPicks > 0 && !isProcessingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;

        while (remainingPicks > 0)
        {
            yield return new WaitForSecondsRealtime(delayBetweenPicks);

            if (cardManager != null)
            {
                cardManager.ShowCardSelection();
                yield return new WaitUntil(() => !cardManager.IsCardSelectionActive() || remainingPicks <= 0);
            }
           
        }

        isProcessingQueue = false;
    }

    // ========== QUERIES ==========

    public int GetRemainingPicks() => remainingPicks;
    public bool IsProcessing() => isProcessingQueue;

    // ========== UTILITY ==========

    public void ClearQueue()
    {
        remainingPicks = 0;
        StopAllCoroutines();
        isProcessingQueue = false;
    }
}