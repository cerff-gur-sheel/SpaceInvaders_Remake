using TMPro;
using UnityEngine;

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

    [Header("Alien Manager")]
    [Tooltip("Reference to the AlienManager responsible for managing invaders.")]
    [SerializeField]
    private AlienManager alienManager;

    private int currentScore = 0;

    #endregion

    #region Properties

    /// <summary>
    /// Indicates whether the game is currently paused.
    /// </summary>
    public bool IsGamePaused { get; private set; } = false;

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
    private void StartGame()
    {
        SpawnPlayer();
        if (alienManager != null)
            alienManager.StartInvaderManager();
    }

    /// <summary>
    /// Instantiates the player at the designated spawn point.
    /// </summary>
    private void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawnPoint != null)
            Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        else
            Debug.LogWarning("Player prefab or spawn point not assigned in GameManager.");
    }

    #endregion

    #region Score Management

    /// <summary>
    /// Adds points to the current score and updates the UI.
    /// </summary>
    /// <param name="points">The number of points to add.</param>
    public void AddPoints(int points)
    {
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
    /// </summary>
    public void TogglePauseGame() => IsGamePaused = !IsGamePaused;

    #endregion
}
