using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the game's lifecycle, including player, score, pause state, and environment setup.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("Player Configuration")]
    [Tooltip("Prefab of the player to instantiate at game start.")]
    [SerializeField]
    private GameObject playerPrefab;

    [Tooltip("Location where the player will spawn.")]
    [SerializeField]
    private Transform playerSpawnPoint;

    [Header("Score UI")]
    [SerializeField]
    private TextMeshProUGUI primaryScoreUI;

    [SerializeField]
    private TextMeshProUGUI secondaryScoreUI;

    [SerializeField]
    private TextMeshProUGUI totalScoreUI;

    [Header("Life UI")]
    [SerializeField]
    private Image[] lifeIndicators;

    [SerializeField]
    private TextMeshProUGUI lifeCountText;

    [Header("Alien Configuration")]
    [Tooltip("Reference to the AlienManager script.")]
    [SerializeField]
    private AlienManager alienManager;

    [Header("Bunker Configuration")]
    [SerializeField]
    private GameObject bunkerPrefab;

    [SerializeField]
    private Transform[] bunkerSpawnPoints;

    [Header("Game Over UI")]
    [SerializeField]
    private TextMeshProUGUI gameOverUI;

    [SerializeField]
    private int gameOverDelaySeconds = 2;

    #endregion

    #region === Private Fields ===

    private int currentPlayerIndex = 0;
    private int[] playerLives = new int[] { 3, 3 };
    private int currentScore = 0;

    private bool isGamePaused = false;
    private bool isGameRunning = false;

    private List<GameObject> activeBunkers = new();

    #endregion

    #region === Properties ===

    public bool IsGamePaused => isGamePaused;

    #endregion

    #region === Unity Events ===

    private void Awake()
    {
        StartGame();
    }

    #endregion

    #region === Game Control ===

    private void StartGame()
    {
        if (isGameRunning)
            StopGame();

        PauseGame(true);
        isGameRunning = true;

        InitializeBunkers();
        SpawnPlayer();
        alienManager.StartInvaderManager(this);
    }

    private void StopGame()
    {
        isGameRunning = false;

        alienManager.StopAll();
        activeBunkers.Where(b => b != null).ToList().ForEach(Destroy);

        var player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            player.gameObject.SetActive(false);
            Destroy(player.gameObject);
        }
    }

    public void WinGame()
    {
        StartGame();
    }

    private async Task HandleGameOver()
    {
        gameOverUI.gameObject.SetActive(true);
        await Task.Delay(gameOverDelaySeconds * 1000);
        StopGame();
        // TODO: Load Main Menu
    }

    #endregion

    #region === Player Handling ===

    public void SpawnPlayer()
    {
        if (!isGameRunning)
            return;

        if (playerPrefab == null || playerSpawnPoint == null)
        {
            Debug.LogWarning("Player prefab or spawn point is not assigned.");
            return;
        }

        Instantiate(
            playerPrefab,
            playerSpawnPoint.position,
            playerSpawnPoint.rotation,
            playerSpawnPoint
        );
    }

    #endregion

    #region === Bunker Handling ===

    private void InitializeBunkers()
    {
        activeBunkers.Clear();

        foreach (var spawn in bunkerSpawnPoints)
        {
            var bunker = Instantiate(bunkerPrefab, spawn.position, spawn.rotation, spawn);
            activeBunkers.Add(bunker);
        }
    }

    #endregion

    #region === Life Management ===

    public void ApplyLifeDamage()
    {
        if (!isGameRunning)
            return;

        playerLives[currentPlayerIndex]--;
        UpdateLifeUI();

        if (playerLives[currentPlayerIndex] <= 0)
        {
            _ = HandleGameOver();
            isGameRunning = false;
        }
    }

    private void UpdateLifeUI()
    {
        int lives = playerLives[currentPlayerIndex];
        lifeCountText.text = lives.ToString();

        for (int i = 0; i < lifeIndicators.Length; i++)
        {
            lifeIndicators[i].color = (i < lives) ? Color.green : Color.clear;
        }
    }

    #endregion

    #region === Score Management ===

    public void AddScore(int points)
    {
        if (!isGameRunning)
            return;

        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        primaryScoreUI.text = currentScore.ToString();
        secondaryScoreUI.text = currentScore.ToString();
        totalScoreUI.text = currentScore.ToString();
    }

    #endregion

    #region === Pause Control ===

    public void TogglePause() => isGamePaused = !isGamePaused;

    public void PauseGame(bool pause)
    {
        isGamePaused = pause;
    }

    #endregion
}
