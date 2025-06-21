using UnityEngine;

public class Alien : MonoBehaviour
{
    public int Points;

    [SerializeField]
    private Sprite[] alienSprites;

    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex;

    public enum State
    {
        Dead,
        Alive,
    }

    public State AlienState { get; private set; } = State.Alive;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpriteIndex = 0;
    }

    private void Start()
    {
        var bc2d = gameObject.AddComponent<BoxCollider2D>();
        bc2d.autoTiling = true;

        SetSprite(currentSpriteIndex);
    }

    /// <summary>
    /// Changes the sprite of the alien to the next one in the array.
    /// The sprite will loop back to the first one after the last sprite.
    /// </summary>
    public void ChangeSprite()
    {
        currentSpriteIndex = (currentSpriteIndex + 1) % alienSprites.Length;
        SetSprite(currentSpriteIndex);
    }

    private void SetSprite(int index)
    {
        if (alienSprites != null && alienSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = alienSprites[index];
        }
    }

    public void SetDead()
    {
        AlienState = State.Dead;
        spriteRenderer.enabled = false;
    }

    void OnDestroy() => SetDead();
}
