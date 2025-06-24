using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsGamePaused { get; private set; } = false;

    private int score = 0;

    [SerializeField]
    private TextMeshProUGUI score1Txt;

    [SerializeField]
    private TextMeshProUGUI score2Txt;

    [SerializeField]
    private TextMeshProUGUI scoreTxt;

    public void AddPoints(int value)
    {
        score += value;
        score1Txt.text = score.ToString();
    }

    public void TogglePauseGame() => IsGamePaused = !IsGamePaused;
}
