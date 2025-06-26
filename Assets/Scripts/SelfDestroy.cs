using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [SerializeField]
    private float destroyDelay = 0.5f;

    [SerializeField]
    private bool pauseWhileAwait = false;

    private GameManager manager;

    [SerializeField]
    private Type type;

    [SerializeField]
    enum Type
    {
        player,
        bullet,
        alien,
    };

    private void Start()
    {
        manager = FindAnyObjectByType<GameManager>();

        if (destroyDelay <= 0)
            return;

        if (pauseWhileAwait)
            manager.TogglePauseGame(true);

        Destroy(gameObject, destroyDelay);
    }

    public void DestroySelf() => Destroy(gameObject);

    private void OnDestroy()
    {
        if (pauseWhileAwait)
            manager.TogglePauseGame(false);

        switch (type)
        {
            case Type.player:
                manager.SpawnPlayer();
                break;
            default:
                Debug.Log("not implemented");
                break;
        }
    }
}
