using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the overall game state, including player instantiation, score, and pause functionality.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Fields

    [Header("Player Settings")]
    [Tooltip("Prefab of the player to instantiate at the start of the game.")]
    [SerializeField]
    private GameObject playerPrefab;

    [Tooltip("Transform where the player will be instantiated.")]
    [SerializeField]
    private Transform playerSpawnPoint;

    [Header("Score UI")]
    [Tooltip("UI Text for displaying the primary score.")]
    [SerializeField]
    private TextMeshProUGUI primaryScoreText;

    [Tooltip("UI Text for displaying the secondary score (if used).")]
    [SerializeField]
    private TextMeshProUGUI secondaryScoreText;

    [Tooltip("UI Text for displaying the total score.")]
    [SerializeField]
    private TextMeshProUGUI totalScoreText;

    [Header("Life Manager")]
    [SerializeField]
    private Image[] lifeImages;

    [SerializeField]
    private TextMeshProUGUI lifeText;

    [Header("Alien Manager")]
    [Tooltip("Reference to the AlienManager responsible for managing invaders.")]
    [SerializeField]
    private AlienManager alienManager;

    [Header("Bunkers")]
    [SerializeField]
    private GameObject bunkerPrefab;

    [SerializeField]
    private Transform[] bunkerSpawnPoints;

    private int currentScore = 0;

    private int[] playerLife = new int[] { 3, 3 };

    private int currentPlayer = 0;

    private List<GameObject> bunkers;

    #endregion

    #region Properties

    /// <summary>
    /// Indicates whether the game is currently paused.
    /// </summary>
    public bool IsGamePaused { get; private set; } = false;

    private bool isGameRunning = false;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        StartGame();
    }

    #endregion

    #region Game Flow

    /// <summary>
    /// Initializes the game by spawning the player and starting the alien manager.
    /// </summary>
    public void StartGame()
    {
        if (isGameRunning)
            StopGame();

        TogglePauseGame(true);
        isGameRunning = true;

        SpawnBunkers();
        SpawnPlayer();
        alienManager.StartInvaderManager(this);
    }

    public void StopGame()
    {
        isGameRunning = false;

        // stop alien manager objects
        alienManager.StopAll();

        // stop bunkers
        bunkers.Where(b => b != null).ToList().ForEach(Destroy);

        // stop player
        var player = FindAnyObjectByType<Player>();
        player.gameObject.SetActive(false);
        Destroy(player.gameObject);
    }

    /// <summary>
    /// Instantiates the player at the designated spawn point.
    /// </summary>
    public void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            Instantiate(
                playerPrefab,
                playerSpawnPoint.transform.position,
                playerSpawnPoint.rotation,
                playerSpawnPoint
            );
        }
        else
            Debug.LogWarning("Player prefab or spawn point not assigned in GameManager.");
    }

    private void SpawnBunkers()
    {
        bunkers = new List<GameObject>();

        foreach (var trans in bunkerSpawnPoints)
        {
            var bunker = Instantiate(bunkerPrefab, trans.position, trans.rotation, trans);
            bunkers.Add(bunker);
        }
    }

    public void WinTheGame() { }
    #endregion

    #region Life Management
    public void ChangeLifePoints()
    {
        if (!isGameRunning)
            return;
        playerLife[currentPlayer]--;
        if (playerLife[currentPlayer] <= 0)
        {
            // TODO: game over
            isGameRunning = false;
        }

        UpdateLifeUI();
    }

    private void UpdateLifeUI()
    {
        var pl = playerLife[currentPlayer];
        lifeText.text = pl.ToString();
        for (var i = 0; i < lifeImages.Length; i++)
        {
            if (i < pl - 1)
                lifeImages[i].color = Color.green;
            else
                lifeImages[i].color = Color.clear;
        }
    }

    #endregion

    #region Score Management

    /// <summary>
    /// Adds points to the current score and updates the UI.
    /// </summary>
    /// <param name="points">The number of points to add.</param>
    public void AddPoints(int points)
    {
        if (!isGameRunning)
            return;
        currentScore += points;
        UpdateScoreUI();
    }

    /// <summary>
    /// Updates the score UI elements.
    /// </summary>
    private void UpdateScoreUI(int score = 0)
    {
        if (primaryScoreText != null && score == 0)
            primaryScoreText.text = currentScore.ToString();

        if (secondaryScoreText != null && score == 1)
            secondaryScoreText.text = currentScore.ToString();

        if (totalScoreText != null && score >= 2)
            totalScoreText.text = currentScore.ToString();
    }

    #endregion

    #region Pause Management

    /// <summary>
    /// Toggles the game's paused state.
    /// /// </summary>
    public void TogglePauseGame() => IsGamePaused = !IsGamePaused;

    public void TogglePauseGame(bool pause) => IsGamePaused = pause;
    #endregion
}
