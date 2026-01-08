using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    // Singleton
    public static SceneController Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private bool isTransitioning = false;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== SCENE LOADING ==========

    public void LoadMainMenu()
    {
        if (isTransitioning) return;


        StartCoroutine(LoadSceneAsync(mainMenuSceneName, () =>
        {
            // Play menu music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }));
    }

    public void LoadGameScene()
    {
        if (isTransitioning) return;


        StartCoroutine(LoadSceneAsync(gameSceneName, () =>
        {
            // Play gameplay music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameplayMusic();
            }
        }));
    }

    public void ReloadCurrentScene()
    {
        if (isTransitioning) return;

        string currentScene = SceneManager.GetActiveScene().name;

        StartCoroutine(LoadSceneAsync(currentScene));
    }

    public void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // ========== ASYNC LOADING ==========

    IEnumerator LoadSceneAsync(string sceneName, System.Action onComplete = null)
    {
        isTransitioning = true;

        // Fade out music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(fadeOutDuration);
        }

        yield return new WaitForSeconds(fadeOutDuration);

        // Start loading scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until scene is loaded
        while (!asyncLoad.isDone)
        {
            // Progress: asyncLoad.progress (0.0 to 1.0)
            yield return null;
        }

       

        // Callback
        onComplete?.Invoke();

        yield return new WaitForSeconds(fadeInDuration);

        isTransitioning = false;
    }

    // ========== GETTERS ==========

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public bool IsInMainMenu()
    {
        return GetCurrentSceneName() == mainMenuSceneName;
    }

    public bool IsInGame()
    {
        return GetCurrentSceneName() == gameSceneName;
    }

    public bool IsTransitioning() => isTransitioning;
}