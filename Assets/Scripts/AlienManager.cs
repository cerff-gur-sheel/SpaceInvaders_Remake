using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the spawning and movement of alien formations.
/// </summary>
public class AlienManager : MonoBehaviour
{
    #region Formation Settings
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
    #endregion

    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField]
    private float moveDistanceX = 0.1f;

    [SerializeField]
    private float moveDistanceY = 0.1f;

    [SerializeField]
    private float minX = -1.5f;

    [SerializeField]
    private float maxX = 6f;
    #endregion

    #region Private State
    private GameObject[][] aliens;
    private int dirX = 1; // 1 for right, -1 for left
    private bool changeDirection = false;
    private bool bunker = true;
    private bool isGamingRunning = true;
    #endregion

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private float bulletSpeed = 6f;

    [SerializeField]
    private float ufoMaxInterval = 15f;

    [SerializeField]
    private float ufoMinInterval = 30f;

    private int _lastRow = 0,
        _lastCol = 0;

    private GameObject _ufo = null;
    private int _dir;
    private bool paused = false;
    private bool last_paused = false;

    private GameManager manager;

    [SerializeField]
    private int shoots = 3;

    public void UnPause()
    {
        StartCoroutine(MoveFormationCoroutine(_lastRow, _lastCol));
        StartShootCoroutines();
        StartCoroutine(UfoSpawnCoroutine());
        if (_ufo != null)
            StartCoroutine(MoveUfo(_ufo, _dir));

        Debug.Log("ok");
    }

    private void Awake()
    {
        manager = FindFirstObjectByType<GameManager>();
        StartCoroutine(SpawnFormationCoroutine());
    }

    private void Update()
    {
        paused = manager.IsGamePaused;
        if (paused != last_paused)
        {
            if (!paused)
                UnPause();

            last_paused = paused;
        }
    }

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
        FindFirstObjectByType<Player>().UnPause();
        StartCoroutine(MoveFormationCoroutine());
        StartShootCoroutines();
        StartCoroutine(UfoSpawnCoroutine());
    }

    private void StartShootCoroutines()
    {
        for (var i = 0; i < shoots; i++)
            StartCoroutine(AlienShootCoroutine());
    }

    private IEnumerator MoveFormationCoroutine(int lastRow = 0, int lastCol = 0)
    {
        if (paused)
        {
            yield break;
        }
        bool shouldChangeDirection = false;
        bool anyAlive = false;
        float dirY = changeDirection ? -moveDistanceY : 0;

        bool resumed = lastRow != 0 || lastCol != 0;
        for (int row = rows - 1; row >= 0; row--)
        {
            int startCol = dirX == 1 ? 0 : columns - 1;
            int endCol = dirX == 1 ? columns : -1;
            int step = dirX;

            // If resuming, skip rows before lastRow
            if (resumed && row > lastRow)
                continue;

            for (int col = startCol; col != endCol; col += step)
            {
                // If resuming, skip columns before lastCol in the lastRow
                if (resumed && row == lastRow)
                {
                    if ((dirX == 1 && col < lastCol) || (dirX == -1 && col > lastCol))
                        continue;
                }

                if (paused)
                {
                    _lastRow = row;
                    _lastCol = col;
                    yield break;
                }

                GameObject alien = aliens[row][col];
                if (alien == null)
                    continue;

                Alien alienComponent = alien.GetComponent<Alien>();
                if (alienComponent == null || alienComponent.AlienState == Alien.State.Dead)
                    continue;

                anyAlive = true;

                alien.transform.Translate(new(moveDistanceX * dirX, dirY));
                alienComponent.ChangeSprite();

                if (bunker && alien.transform.position.y <= -4)
                {
                    Destroy(GameObject.Find("Bunkers"));
                    bunker = false;
                }

                shouldChangeDirection =
                    alien.transform.position.x < minX || alien.transform.position.x > maxX;

                yield return new WaitForSeconds(1 / 1000f);
            }
        }

        dirX = shouldChangeDirection ? -dirX : dirX;
        changeDirection = shouldChangeDirection;

        if (anyAlive)
            StartCoroutine(MoveFormationCoroutine());

        isGamingRunning = anyAlive;
    }

    private void AlienShoot(Vector3 spawnPosition)
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            if (bullet.TryGetComponent<Bullet>(out Bullet bulletComponent))
            {
                bulletComponent.speed = -Mathf.Abs(bulletSpeed);
                bulletComponent.bulletType = Bullet.BulletType.Alien;
                bulletComponent.BulletStyleType = Random.Range(0, 1);
            }
        }
    }

    private IEnumerator UfoSpawnCoroutine()
    {
        if (!isGamingRunning || paused)
            yield break;

        float waitTime = Random.Range(ufoMinInterval, ufoMaxInterval);
        yield return new WaitForSeconds(waitTime);

        if (!isGamingRunning || paused)
            yield break;

        bool fromLeft = Random.value < 0.5f;
        Vector2 spawnPos = fromLeft ? new Vector2(minX - 1f, 1.25f) : new Vector2(maxX + 1f, 1.25f);
        int dir = fromLeft ? 1 : -1;

        var ufo = Instantiate(alienPrefabs[5], spawnPos, Quaternion.identity);
        _ufo = ufo;
        _dir = dir;
        StartCoroutine(MoveUfo(ufo, dir));
        StartCoroutine(UfoSpawnCoroutine());
    }

    private IEnumerator AlienShootCoroutine()
    {
        if (paused)
            yield break;

        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        if (!isGamingRunning || paused)
            yield break;

        var aliveAliens = new System.Collections.Generic.List<GameObject>();
        Player player = FindFirstObjectByType<Player>();
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
                if (alienComponent != null && alienComponent.AlienState != Alien.State.Dead)
                {
                    aliveAliens.Add(alien);

                    if (Mathf.Abs(alien.transform.position.x - playerX) < alignThreshold)
                    {
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
            AlienShoot(spawnPos);
            StartCoroutine(AlienShootCoroutine());
        }
    }

    private IEnumerator MoveUfo(GameObject ufo, int dir)
    {
        float targetX = dir > 0 ? maxX + 2f : minX - 2f;
        float speed = 2f;
        while (
            ufo != null
            && (
                (dir > 0 && ufo.transform.position.x < targetX)
                || (dir < 0 && ufo.transform.position.x > targetX)
            )
        )
        {
            ufo.transform.Translate(dir * speed * Time.deltaTime * Vector3.right);
            if (paused)
            {
                ufo.transform.Translate(Vector2.zero);
                yield break;
            }
            yield return null;
        }
        if (ufo != null)
        {
            Destroy(ufo);
            _ufo = null;
        }
    }
}
