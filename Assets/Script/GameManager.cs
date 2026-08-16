using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] private GameObject gameStartUI;

    [SerializeField] private GameObject gameEndUI;

    public bool IsGameRunning { get; private set; } = false;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
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

    private void Start()
    {
        Time.timeScale = 0f; // Pause the game at the start
        if (gameStartUI != null)
        {
            gameStartUI.SetActive(true);
        }
        if (gameEndUI != null)
        {
            gameEndUI.SetActive(false);
        }
    }

    public void StartGame()
    {
        if (gameStartUI != null)
        {
            gameStartUI.SetActive(false);
        }
        Time.timeScale = 1f;
        IsGameRunning = true;
    }

    public void EndGame()
    {
        if (gameEndUI != null)
        {
            gameEndUI.SetActive(true);
        }
        Time.timeScale = 0f;
        IsGameRunning = false;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}