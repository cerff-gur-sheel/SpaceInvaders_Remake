using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsGamePaused { get; private set; } = false;

    public void TogglePauseGame() => IsGamePaused = !IsGamePaused;
}
