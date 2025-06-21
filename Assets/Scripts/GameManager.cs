using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsGameOver { get; private set; } = false;
    public bool IsGamePaused { get; private set; } = false;
    public int Score { get; private set; } = 0;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("Game Manager Initialized");
    }
}
