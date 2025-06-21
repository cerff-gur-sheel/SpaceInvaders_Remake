using UnityEngine;
using System.Collections;

/// <summary>
/// Handles bullet movement, collision, and sprite animation.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float speed;
    [SerializeField] private float maxDistance = 10f;

    [SerializeField] private GameObject red, green;

    [Header("Sprite Animation")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float spriteChangeInterval = 0.1f;
 
    private SpriteRenderer sr;

    public enum BulletType
    {
        Player,
        Alien
    }

    public BulletType bulletType = BulletType.Player;

    public int BulletStyleType = 1;

    private Vector3 startPosition;

    private void Start()
    {
        var rb2d = gameObject.AddComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
        rb2d.gravityScale = 0;
        rb2d.linearVelocity = Vector2.up * speed;

        var ac = GetComponent<Animator>();
        ac.SetInteger("bullet", BulletStyleType);

        startPosition = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, startPosition) > maxDistance)
            Destroy(gameObject);

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
        // Prevent bullet from hitting objects of the same type
        if ((bulletType == BulletType.Player && other.CompareTag("Player")) ||
            (bulletType == BulletType.Alien && other.CompareTag("Alien")))
            return;


        if (bulletType == BulletType.Alien && other.CompareTag("Player"))
        {
            // TODO: kill the player
            return;
        }
         Debug.Log($"{other.tag}");
       Destroy(gameObject);
        Destroy(other.gameObject);
    }
}