using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] private GameObject gameStartUI;

    [SerializeField] private GameObject gameEndUI;

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

    public void StartGame()
    {
        if (gameStartUI != null)
        {
            gameStartUI.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void EndGame()
    {
        if (gameEndUI != null)
        {
            gameEndUI.SetActive(true);
        }
        Time.timeScale = 0f;
    }
}