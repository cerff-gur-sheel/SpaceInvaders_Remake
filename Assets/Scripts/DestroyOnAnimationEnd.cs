using UnityEngine;
using UnityEngine.Events;

public class DestroyOnAnimationEnd : MonoBehaviour
{
    public void DestroySelf() => Destroy(gameObject);
}
