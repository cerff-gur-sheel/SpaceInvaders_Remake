using UnityEngine;

public class Alien : MonoBehaviour
{
    #region Inspector Fields

    [Tooltip(
        "Points awarded for destroying this alien. If 'useRandomPoints' is true, a random value from 'possiblePoints' will be used."
    )]
    public int points = 100;

    [Tooltip(
        "Enable to use a random value from 'possiblePoints' instead of the fixed 'points' value."
    )]
    public bool useRandomPoints = false;

    [Tooltip("Possible point values for this alien (e.g., for UFOs).")]
    public int[] possiblePoints = new int[] { 50, 100, 150, 300 };

    [SerializeField, Tooltip("Sprites used for alien animation.")]
    private Sprite[] animationSprites;

    #endregion

    #region Private Fields
    private GameManager manager;
    private SpriteRenderer _spriteRenderer;
    private int _currentAnimationFrame;

    #endregion

    #region Alien State

    public enum AlienState
    {
        Dead,
        Alive,
    }

    public AlienState CurrentState { get; private set; } = AlienState.Alive;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentAnimationFrame = 0;
    }

    private void Start()
    {
        var boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.autoTiling = true;

        SetSprite(_currentAnimationFrame);
        manager = FindAnyObjectByType<GameManager>();
    }

    private void OnDestroy()
    {
        SetDead();
        manager.AddPoints(GetAwardedPoints());
    }

    #endregion

    #region Animation

    /// <summary>
    /// Advances to the next animation sprite, looping if necessary.
    /// </summary>
    public void AdvanceAnimationFrame()
    {
        if (animationSprites == null || animationSprites.Length == 0)
            return;

        _currentAnimationFrame = (_currentAnimationFrame + 1) % animationSprites.Length;
        SetSprite(_currentAnimationFrame);
    }

    private void SetSprite(int frameIndex)
    {
        if (animationSprites != null && animationSprites.Length > 0 && _spriteRenderer != null)
        {
            _spriteRenderer.sprite = animationSprites[frameIndex];
        }
    }

    #endregion

    #region State Management

    /// <summary>
    /// Sets the alien state to dead and disables its sprite.
    /// </summary>
    public void SetDead()
    {
        CurrentState = AlienState.Dead;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;
    }

    /// <summary>
    /// Returns the points to award for destroying this alien, using random if enabled.
    /// </summary>
    private int GetAwardedPoints()
    {
        if (useRandomPoints && possiblePoints != null && possiblePoints.Length > 0)
        {
            int randomIndex = Random.Range(0, possiblePoints.Length);
            return possiblePoints[randomIndex];
        }
        return points;
    }
    #endregion
}
