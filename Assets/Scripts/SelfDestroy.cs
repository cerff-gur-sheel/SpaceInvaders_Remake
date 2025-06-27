using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    #region Input Fields

    [SerializeField]
    private float destroyDelay = 0.5f;

    [SerializeField]
    private bool pauseWhileAwait = false;

    [SerializeField]
    private Type type;

    [SerializeField]
    enum Type
    {
        player,
        bullet,
        alien,
    };

    #endregion

    #region Private Proprieties

    private GameManager manager;

    #endregion

    #region Unity Methods

    private void Start()
    {
        manager = FindAnyObjectByType<GameManager>();

        if (destroyDelay <= 0)
            return;

        if (pauseWhileAwait)
            manager.TogglePauseGame(true);

        Destroy(gameObject, destroyDelay);
    }

    private void OnDestroy()
    {
        if (pauseWhileAwait)
            manager.TogglePauseGame(false);

        switch (type)
        {
            case Type.player:
                manager.SpawnPlayer();
                break;
        }
    }

    #endregion

    #region Public Methods

    public void DestroySelf() => Destroy(gameObject);

    #endregion
}
