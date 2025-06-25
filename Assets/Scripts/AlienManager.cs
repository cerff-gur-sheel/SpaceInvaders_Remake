using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the spawning, movement, and shooting of alien formations.
/// </summary>
public class AlienManager : MonoBehaviour
{
    #region Inspector Settings

    [Header("Formation Settings")]
    [SerializeField]
    private GameObject[] alienPrefabs;

    [SerializeField]
    private int columns = 11;

    [SerializeField]
    private int rows = 5;

    [SerializeField]
    private float spacingX = 0.5f;

    [SerializeField]
    private float spacingY = 0.5f;

    [SerializeField]
    private int spawnDelayMs = 100;

    [Header("Movement Settings")]
    [SerializeField]
    private float moveDistanceX = 0.1f;

    [SerializeField]
    private float moveDistanceY = 0.1f;

    [SerializeField]
    private float minX = -1.5f;

    [SerializeField]
    private float maxX = 6f;

    [Header("Alien Shooting")]
    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private float bulletSpeed = 6f;

    [SerializeField]
    private int simultaneousShots = 3;

    [Header("UFO Settings")]
    [SerializeField]
    private float ufoMinInterval = 15f;

    [SerializeField]
    private float ufoMaxInterval = 30f;

    #endregion

    #region Private Fields

    private GameObject[][] aliens;
    private int directionX = 1; // 1 for right, -1 for left
    private bool shouldChangeDirection = false;
    private bool bunkersActive = true;
    private bool isGameRunning = true;

    private GameObject activeUfo = null;

    private Coroutine[] ShotsCoroutine;

    private GameManager gameManager;

    private bool IsPaused => gameManager != null && gameManager.IsGamePaused;
    #endregion

    #region Public Methods

    public void StartInvaderManager()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        StartCoroutine(SpawnFormationCoroutine());
    }

    #endregion

    #region Formation & Movement

    private IEnumerator SpawnFormationCoroutine()
    {
        aliens = new GameObject[rows][];
        for (int row = 0; row < rows; row++)
        {
            aliens[row] = new GameObject[columns];
            GameObject prefabToUse = alienPrefabs[row];
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = new(col * spacingX, -row * spacingY, 0);
                aliens[row][col] = Instantiate(
                    prefabToUse,
                    position,
                    Quaternion.identity,
                    transform
                );
                yield return new WaitForSeconds(spawnDelayMs / 1000f);
            }
        }
        FindAnyObjectByType<Player>().UnPause();
        StartCoroutine(MoveFormationCoroutine());
        StartAlienShootCoroutines();
        StartCoroutine(UfoSpawnCoroutine());

        FindAnyObjectByType<Player>().UnPause();
    }

    private IEnumerator MoveFormationCoroutine()
    {
        while (true)
        {
            if (IsPaused)
                yield return new WaitUntil(() => IsPaused == false);

            bool foundAlive = false;
            float moveY = shouldChangeDirection ? -moveDistanceY : 0;

            for (int row = rows - 1; row >= 0; row--)
            {
                int startCol = directionX == 1 ? 0 : columns - 1;
                int endCol = directionX == 1 ? columns : -1;
                int step = directionX;

                for (int col = startCol; col != endCol; col += step)
                {
                    if (IsPaused)
                        yield return new WaitUntil(() => IsPaused == false);

                    GameObject alien = aliens[row][col];
                    if (alien == null)
                        continue;

                    Alien alienComponent = alien.GetComponent<Alien>();
                    if (
                        alienComponent == null
                        || alienComponent.CurrentState == Alien.AlienState.Dead
                    )
                        continue;

                    foundAlive = true;

                    alien.transform.Translate(new Vector3(moveDistanceX * directionX, moveY));
                    alienComponent.AdvanceAnimationFrame();

                    if (bunkersActive && alien.transform.position.y <= -4)
                    {
                        Destroy(GameObject.Find("Bunkers"));
                        bunkersActive = false;
                    }

                    float posX = alien.transform.position.x;
                    shouldChangeDirection = posX < minX || posX > maxX;

                    yield return new WaitForSeconds(0.001f);
                }
            }

            if (shouldChangeDirection)
                directionX = -directionX;

            if (!foundAlive)
            {
                isGameRunning = false;
                yield break;
            }
        }
    }
    #endregion

    #region Alien Shooting

    private void StartAlienShootCoroutines()
    {
        if (ShotsCoroutine != null)
        {
            foreach (var coroutine in ShotsCoroutine)
                if (coroutine != null)
                    StopCoroutine(coroutine);
        }

        ShotsCoroutine = new Coroutine[simultaneousShots];
        for (int i = 0; i < simultaneousShots; i++)
            ShotsCoroutine[i] = StartCoroutine(AlienShootCoroutine());
    }

    private IEnumerator AlienShootCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        if (!isGameRunning || IsPaused)
            yield return new WaitUntil(() => IsPaused == false);

        var aliveAliens = new System.Collections.Generic.List<GameObject>();
        Player player = FindAnyObjectByType<Player>();
        float playerX = player != null ? player.transform.position.x : 0f;
        float alignThreshold = 0.5f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject alien = aliens[row][col];
                if (alien == null)
                    continue;
                Alien alienComponent = alien.GetComponent<Alien>();
                if (alienComponent != null && alienComponent.CurrentState != Alien.AlienState.Dead)
                {
                    aliveAliens.Add(alien);

                    if (Mathf.Abs(alien.transform.position.x - playerX) < alignThreshold)
                    {
                        // Increase chance to shoot if aligned with player
                        aliveAliens.Add(alien);
                        aliveAliens.Add(alien);
                    }
                }
            }
        }

        if (aliveAliens.Count > 0)
        {
            GameObject shooter = aliveAliens[Random.Range(0, aliveAliens.Count)];
            Vector3 spawnPos = shooter.transform.position + Vector3.down * 0.5f;
            SpawnAlienBullet(spawnPos);
            StartCoroutine(AlienShootCoroutine());
        }
    }

    private void SpawnAlienBullet(Vector3 spawnPosition)
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            if (bullet.TryGetComponent(out Bullet bulletComponent))
            {
                bulletComponent.moveSpeed = -Mathf.Abs(bulletSpeed);
                bulletComponent.owner = Bullet.BulletOwner.Alien;
                var type = Random.Range(0, 100) >= 50 ? 1 : 0;
                bulletComponent.BulletStyle = type;
            }
        }
    }
    #endregion

    #region UFO Logic

    private IEnumerator UfoSpawnCoroutine()
    {
        while (isGameRunning)
        {
            float waitTime = Random.Range(ufoMinInterval, ufoMaxInterval);
            yield return new WaitForSeconds(waitTime);

            if (IsPaused)
                yield return new WaitUntil(() => IsPaused == false);

            if (activeUfo == null)
            {
                bool fromLeft = Random.value < 0.5f;
                Vector2 spawnPos = fromLeft
                    ? new Vector2(minX - 1f, 1.25f)
                    : new Vector2(maxX + 1f, 1.25f);
                int dir = fromLeft ? 1 : -1;

                var ufo = Instantiate(alienPrefabs[5], spawnPos, Quaternion.identity);
                activeUfo = ufo;
                StartCoroutine(MoveUfoCoroutine(ufo, dir));
            }
        }
    }

    private IEnumerator MoveUfoCoroutine(GameObject ufo, int direction)
    {
        var targetX = direction > 0 ? maxX + 2f : minX - 2f;
        var speed = 2f;

        while (ufo != null)
        {
            if (IsPaused)
            {
                yield return new WaitUntil(() => IsPaused == false);
                continue;
            }

            var currentX = ufo.transform.position.x;
            var reachedTarget = direction > 0 ? currentX >= targetX : currentX <= targetX;
            if (reachedTarget)
                break;

            var move = direction * speed * Time.deltaTime * Vector3.right;
            ufo.transform.Translate(move);

            yield return null;
        }

        if (ufo != null)
        {
            Destroy(ufo);
            activeUfo = null;
        }
    }

    #endregion
}
