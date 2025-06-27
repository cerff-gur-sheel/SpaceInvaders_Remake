using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the player character, including movement and shooting.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    #region Inspector Fields

    [Header("Movement")]
    [Tooltip("Player movement speed.")]
    [SerializeField]
    private float movementSpeed = 5f;

    [Tooltip("Minimum X position (left boundary).")]
    [SerializeField]
    private float minX = -1.5f;

    [Tooltip("Maximum X position (right boundary).")]
    [SerializeField]
    private float maxX = 6.5f;

    [Header("Shooting")]
    [Tooltip("Prefab of the bullet to instantiate when shooting.")]
    [SerializeField]
    private GameObject bulletPrefab;

    [Tooltip("Transform where bullets are spawned.")]
    [SerializeField]
    private Transform bulletSpawnPoint;

    [Tooltip("Speed at which the bullet moves.")]
    [SerializeField]
    private float bulletMoveSpeed = 10f;

    #endregion

    #region Private Fields

    private GameManager gameManager;
    private Rigidbody2D rb2D;
    private Vector2 movementInput;
    private InputAction moveInputAction;
    private InputAction shootInputAction;
    private bool IsPaused => gameManager != null && gameManager.IsGamePaused;
    private Transform bulletTransform;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        var newBulletTransform = Instantiate(
            new GameObject("Player Bullet Transform"),
            Vector2.zero,
            transform.rotation,
            FindAnyObjectByType<GameManager>().transform
        );
        bulletTransform = newBulletTransform.transform;

        InitializeInputActions();
    }

    private void OnEnable()
    {
        moveInputAction.Enable();
        shootInputAction.Enable();
        shootInputAction.performed += HandleShoot;
    }

    private void OnDisable()
    {
        moveInputAction.Disable();
        shootInputAction.Disable();
        shootInputAction.performed -= HandleShoot;
    }

    private void Start()
    {
        CacheReferences();
        SetupRigidbody();
        SetupCollider();
        ValidateBulletPrefab();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Update()
    {
        if (IsPaused)
            return;

        HandleMovement();
        ClampPlayerPosition();
    }

    private void OnDestroy() => gameManager.ApplyLifeDamage();

    #endregion

    #region Initialization

    /// <summary>
    /// Sets up input actions for movement and shooting.
    /// </summary>
    private void InitializeInputActions()
    {
        moveInputAction = new InputAction(
            type: InputActionType.Value,
            binding: "<Gamepad>/leftStick"
        );
        moveInputAction
            .AddCompositeBinding("2DVector")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        shootInputAction = new InputAction(
            type: InputActionType.Button,
            binding: "<Keyboard>/space"
        );
        shootInputAction.AddBinding("<Gamepad>/buttonSouth");
    }

    /// <summary>
    /// Caches references to required components and managers.
    /// </summary>
    private void CacheReferences()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Configures the Rigidbody2D component.
    /// </summary>
    private void SetupRigidbody()
    {
        rb2D.gravityScale = 0;
        rb2D.freezeRotation = true;
    }

    /// <summary>
    /// Configures the BoxCollider2D component.
    /// </summary>
    private void SetupCollider()
    {
        var boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.autoTiling = true;
    }

    /// <summary>
    /// Validates that the bullet prefab is assigned.
    /// </summary>
    private void ValidateBulletPrefab()
    {
        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!", this);
    }

    #endregion

    #region Movement

    /// <summary>
    /// Handles player movement based on input.
    /// </summary>
    private void HandleMovement()
    {
        movementInput = moveInputAction.ReadValue<Vector2>();
        rb2D.linearVelocity = movementInput * movementSpeed;
    }

    /// <summary>
    /// Clamps the player's position within the defined X boundaries.
    /// </summary>
    private void ClampPlayerPosition()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
    }

    #endregion

    #region Shooting

    /// <summary>
    /// Handles shooting input and bullet instantiation.
    /// </summary>
    /// <param name="context">Input callback context.</param>
    private void HandleShoot(InputAction.CallbackContext context)
    {
        if (IsPaused)
            return;

        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bulletInstance = Instantiate(
                bulletPrefab,
                bulletSpawnPoint.position,
                Quaternion.identity,
                bulletTransform
            );

            if (bulletInstance.TryGetComponent(out Bullet bulletComponent))
            {
                bulletComponent.moveSpeed = bulletMoveSpeed;
                bulletComponent.owner = Bullet.BulletOwner.Player;
                bulletComponent.BulletStyle = 0;
            }
        }
    }

    #endregion
}
