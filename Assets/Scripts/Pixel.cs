using UnityEngine;

public class Pixel : MonoBehaviour
{
    public Pixel left;
    public Pixel right;
    public Pixel top;
    public Pixel down;

    [SerializeField] private float destroyNeighborChance = 0.3f;

    void Start()
    {
        var bc2d = gameObject.AddComponent<BoxCollider2D>();
        bc2d.autoTiling = true;
    }

    void OnDestroy()
    {
        TryDestroyNeighbor(left);
        TryDestroyNeighbor(right);
        TryDestroyNeighbor(top);
        TryDestroyNeighbor(down);
    }

    void TryDestroyNeighbor(Pixel neighbor)
    {
        if (neighbor != null && Random.value < destroyNeighborChance)
            Destroy(neighbor.gameObject);
    }

}