using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Screens")]
    public GameObject deathScreen;
    public GameObject winScreen;

    bool _gameOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var playerHealth = GameObject.FindWithTag("Player")?.GetComponent<HealthComponent>();
        if (playerHealth != null)
            playerHealth.OnDeath += TriggerDeath;

        deathScreen?.SetActive(false);
        winScreen?.SetActive(false);
    }

    public void TriggerWin()
    {
        if (_gameOver) return;
        _gameOver = true;
        winScreen?.SetActive(true);
        Time.timeScale = 0f;
    }

    void TriggerDeath()
    {
        if (_gameOver) return;
        _gameOver = true;
        deathScreen?.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
