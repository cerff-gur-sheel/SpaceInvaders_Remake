using UnityEngine;

/// <summary>
/// Handles bullet movement, collision, and sprite animation.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float speed;

    [SerializeField]
    private float maxDistance = 10f;

    [SerializeField]
    private GameObject red,
        green;

    [Header("Sprite Animation")]
    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private float spriteChangeInterval = 0.1f;

    public enum BulletType
    {
        Player,
        Alien,
    }

    public BulletType bulletType = BulletType.Player;

    public int BulletStyleType = 1;

    private Vector3 startPosition;

    private Rigidbody2D rb2d;

    private GameManager manager;

    private Animator ac;

    [SerializeField]
    private Animator[] childrenAcs; // 0 red 1 green

    private bool lastPause = false;

    private bool IsPaused => manager != null && manager.IsGamePaused;

    public void Pause()
    {
        rb2d.linearVelocity = Vector2.zero;
        ac.speed = 0;
        foreach (var childrenAc in childrenAcs)
            childrenAc.speed = 0;
    }

    public void UnPause()
    {
        rb2d.linearVelocity = Vector2.up * speed;
        ac.speed = 1;
        foreach (var childrenAc in childrenAcs)
            childrenAc.speed = 0;
    }

    private void Start()
    {
        manager = FindFirstObjectByType<GameManager>();
        rb2d = gameObject.AddComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
        rb2d.gravityScale = 0;
        rb2d.linearVelocity = Vector2.up * speed;

        ac = GetComponent<Animator>();
        ac.SetInteger("bullet", BulletStyleType);
        foreach (var childrenAc in childrenAcs)
            childrenAc.SetInteger("bullet", BulletStyleType);

        startPosition = transform.position;
    }

    private void Update()
    {
        if (IsPaused != lastPause)
        {
            if (IsPaused)
                Pause();
            else
                UnPause();

            lastPause = IsPaused;
        }

        if (Vector3.Distance(transform.position, startPosition) > maxDistance)
            Destroy(gameObject);

        // Sprite color
        if (transform.position.y < 0)
        {
            green.SetActive(true);
            red.SetActive(false);
        }
        else
        {
            green.SetActive(false);
            red.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        const string PlayerTag = "Player";
        const string AlienTag = "Alien";

        bool isSameTypeCollision =
            (bulletType == BulletType.Player && other.CompareTag(PlayerTag))
            || (bulletType == BulletType.Alien && other.CompareTag(AlienTag));

        if (isSameTypeCollision)
            return;

        Destroy(gameObject);

        if (bulletType == BulletType.Alien && other.CompareTag(PlayerTag))
        {
            // TODO: kill the player
            return;
        }

        Destroy(other.gameObject);
    }
}
