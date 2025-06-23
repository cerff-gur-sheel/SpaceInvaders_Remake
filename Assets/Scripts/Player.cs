using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float minX = -1.5f;

    [SerializeField]
    private float maxX = 6.5f;

    [Header("Shooting")]
    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private Transform bulletSpawnPoint;

    [SerializeField]
    private float bulletSpeed = 10f;

    private GameManager manager;

    #region Local Attributes
    private Rigidbody2D rb2d;
    private Vector2 moveInput;
    private InputAction moveAction;
    private InputAction shootAction;

    private bool paused = true;
    #endregion

    public void UnPause() => paused = false;

    private void Awake()
    {
        moveAction = new InputAction(type: InputActionType.Value, binding: "<Gamepad>/leftStick");
        moveAction
            .AddCompositeBinding("2DVector")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        shootAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        shootAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        shootAction.Enable();
        shootAction.performed += OnShoot;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        shootAction.Disable();
        shootAction.performed -= OnShoot;
    }

    private void Start()
    {
        manager = FindFirstObjectByType<GameManager>();

        rb2d = gameObject.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0;
        rb2d.freezeRotation = true;

        var bc2d = gameObject.AddComponent<BoxCollider2D>();
        bc2d.autoTiling = true;

        if (bulletPrefab == null)
            Debug.LogError("Bullet Prefab is not assigned!");
    }

    private void Update()
    {
        paused = manager.IsGamePaused;

        if (paused)
            return;

        moveInput = moveAction.ReadValue<Vector2>();
        rb2d.linearVelocity = moveInput * speed;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (paused)
            return;
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                bulletSpawnPoint.position,
                Quaternion.identity
            );
            if (bullet.TryGetComponent(out Bullet bulletComponent))
                bulletComponent.speed = bulletSpeed;
        }
    }
}
