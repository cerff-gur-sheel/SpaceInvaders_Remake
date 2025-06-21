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
    #endregion

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private float bulletSpeed = 6f;

    [SerializeField]
    private float ufoMaxInterval = 15f;

    [SerializeField]
    private float ufoMinInterval = 30f;

    private void Awake()
    {
        StartCoroutine(SpawnFormationCoroutine());
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
        StartCoroutine(AlienShootCoroutine());
        StartCoroutine(UfoSpawnCoroutine());
    }

    private IEnumerator MoveFormationCoroutine()
    {
        bool shouldChangeDirection = false;
        bool anyAlive = false;
        float dirY = changeDirection ? -moveDistanceY : 0;

        for (int row = rows - 1; row >= 0; row--)
        {
            int startCol = dirX == 1 ? 0 : columns - 1;
            int endCol = dirX == 1 ? columns : -1;
            int step = dirX;

            for (int col = startCol; col != endCol; col += step)
            {
                GameObject alien = aliens[row][col];
                if (alien == null)
                    continue;

                Alien alienComponent = alien.GetComponent<Alien>();
                if (alienComponent == null || alienComponent.AlienState == Alien.State.Dead)
                    continue;

                anyAlive = true;

                alien.transform.Translate(new(moveDistanceX * dirX, dirY));
                alienComponent.ChangeSprite();

                if (bunker && alien.transform.position.y <= -3)
                {
                    Destroy(GameObject.Find("Bunkers"));
                    bunker = false;
                }

                if (alien.transform.position.x < minX || alien.transform.position.x > maxX)
                    shouldChangeDirection = true;

                yield return new WaitForSeconds(1 / 1000f);
            }
        }

        dirX = shouldChangeDirection ? -dirX : dirX;
        changeDirection = shouldChangeDirection;

        if (anyAlive)
            StartCoroutine(MoveFormationCoroutine());
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

    private IEnumerator AlienShootCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

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

    private IEnumerator UfoSpawnCoroutine()
    {
        float waitTime = Random.Range(ufoMinInterval, ufoMaxInterval);
        yield return new WaitForSeconds(waitTime);

        bool fromLeft = Random.value < 0.5f;
        Vector2 spawnPos = fromLeft ? new Vector2(minX - 1f, 1.25f) : new Vector2(maxX + 1f, 1.25f);
        int dir = fromLeft ? 1 : -1;

        var ufo = Instantiate(alienPrefabs[5], spawnPos, Quaternion.identity);
        StartCoroutine(MoveUfo(ufo, dir));
        StartCoroutine(UfoSpawnCoroutine());
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
            yield return null;
        }
        if (ufo != null)
        {
            Destroy(ufo);
        }
    }
}
