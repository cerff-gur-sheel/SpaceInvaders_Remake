using UnityEngine;
using UnityEngine.Events;

public class SelfDestroy : MonoBehaviour
{
    [SerializeField]
    private float destroyDelay = 0.5f;

    private void Start()
    {
        if (destroyDelay > 0)
            Destroy(gameObject, destroyDelay);
    }

    public void DestroySelf() => Destroy(gameObject);
}
