using UnityEngine;

/// <summary>
/// Handles bullet movement, collision, and sprite animation.
/// </summary>
public class Bullet : MonoBehaviour
{
    #region Inspector Fields

    [Header("Movement")]
    [Tooltip("Bullet movement speed.")]
    public float moveSpeed = 10f;

    [Tooltip("Maximum distance bullet can travel before being destroyed.")]
    [SerializeField]
    private float maxTravelDistance = 10f;

    [Header("Bullet Visuals")]
    [Tooltip("GameObject for red bullet visual.")]
    [SerializeField]
    private GameObject redVisual;

    [Tooltip("GameObject for green bullet visual.")]
    [SerializeField]
    private GameObject greenVisual;

    [Header("Sprite Animation")]
    [Tooltip("Sprites for bullet animation.")]
    [SerializeField]
    private Sprite[] animationSprites;

    [Tooltip("Interval between sprite changes.")]
    [SerializeField]
    private float spriteAnimationInterval = 0.1f;

    #endregion

    #region Bullet Type

    public enum BulletOwner
    {
        Player,
        Alien,
    }

    [Tooltip("Who fired the bullet.")]
    public BulletOwner owner = BulletOwner.Player;

    private int _bulletStyle;

    public int BulletStyle
    {
        get => _bulletStyle;
        set
        {
            if (_bulletStyle != value)
            {
                _bulletStyle = value;
                childAnimators = new Animator[2];
                childAnimators[0] = redVisual.GetComponent<Animator>();
                childAnimators[1] = greenVisual.GetComponent<Animator>();

                _animator = GetComponent<Animator>();
                _animator.SetFloat("bullet", BulletStyle);

                foreach (var childAnimator in childAnimators)
                    childAnimator.SetFloat("bullet", BulletStyle);
            }
        }
    }

    private void OnBulletStyleChanged()
    {
        if (_animator != null)
            _animator.SetFloat("bullet", _bulletStyle);

        if (childAnimators != null)
        {
            foreach (var childAnimator in childAnimators)
                if (childAnimator != null)
                    childAnimator.SetFloat("bullet", _bulletStyle);
        }
    }

    #endregion

    #region Private Fields

    private Vector3 _spawnPosition;
    private Rigidbody2D _rigidbody2D;
    private GameManager _gameManager;
    private Animator _animator;
    private bool _wasPaused = false;
    private Animator[] childAnimators; // 0: red 1: green.
    #endregion

    #region Properties

    private bool IsGamePaused => _gameManager != null && _gameManager.IsGamePaused;

    #endregion

    #region Unity Methods

    private void Start()
    {
        _gameManager = FindAnyObjectByType<GameManager>();

        _rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        _rigidbody2D.freezeRotation = true;
        _rigidbody2D.gravityScale = 0;
        _rigidbody2D.linearVelocity = Vector2.up * moveSpeed;

        BulletStyle = _bulletStyle;

        _spawnPosition = transform.position;
    }

    private void Update()
    {
        HandlePauseState();

        if (Vector3.Distance(transform.position, _spawnPosition) > maxTravelDistance)
            Destroy(gameObject);

        UpdateVisualsByPosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    #endregion

    #region Pause Handling

    /// <summary>
    /// Handles pausing and resuming bullet movement and animation based on game pause state.
    /// </summary>
    private void HandlePauseState()
    {
        if (IsGamePaused != _wasPaused)
        {
            if (IsGamePaused)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _animator.speed = 0;
                foreach (var childAnimator in childAnimators)
                    childAnimator.speed = 0;
            }
            else
            {
                _rigidbody2D.linearVelocity = Vector2.up * moveSpeed;
                _animator.speed = 1;
                foreach (var childAnimator in childAnimators)
                    childAnimator.speed = 1;
            }

            _wasPaused = IsGamePaused;
        }
    }

    #endregion

    #region Visuals

    private void UpdateVisualsByPosition()
    {
        bool isBelowZero = transform.position.y < -2;
        greenVisual.SetActive(isBelowZero);
        redVisual.SetActive(!isBelowZero);
    }

    #endregion

    #region Collision

    private void HandleCollision(Collider2D other)
    {
        const string PlayerTag = "Player";
        const string AlienTag = "Alien";

        bool isFriendlyFire =
            (owner == BulletOwner.Player && other.CompareTag(PlayerTag))
            || (owner == BulletOwner.Alien && other.CompareTag(AlienTag));

        if (isFriendlyFire)
            return;

        Destroy(gameObject);

        if (owner == BulletOwner.Alien && other.CompareTag(PlayerTag))
        {
            // TODO: Implement player death logic
            return;
        }

        Destroy(other.gameObject);
    }
    #endregion
}
