using UnityEngine;

public class YardCat : MonoBehaviour
{
    [Header("Cat Identity")]
    [SerializeField] string catId;  // e.g., "Orange", "Black", "Gray"
    
    [Header("Appearance (for player to copy)")]
    [SerializeField] Color catColor = Color.white;  // Tint color for disguise
    [SerializeField] Sprite disguiseSprite;  // Optional: specific sprite for player when disguised
    
    [Header("Fed Indicator")]
    [SerializeField] float fedAlpha = 0.6f;  // Opacity when fed (subtle indicator)
    [SerializeField] float hungryAgainTime = 180f;  // 3 minutes until hungry again
    
    public string CatId => catId;
    public Color CatColor => catColor;
    public Sprite DisguiseSprite => disguiseSprite;
    public bool IsFed { get; private set; }

    SpriteRenderer spriteRenderer;
    Color originalColor;
    float fedTimer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-generate catId from GameObject name if not set
        if (string.IsNullOrEmpty(catId))
        {
            catId = gameObject.name;
        }

        // Store original color
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        // Count down fed timer
        if (IsFed)
        {
            fedTimer -= Time.deltaTime;
            if (fedTimer <= 0)
            {
                BecomeHungryAgain();
            }
        }
    }

    // Called when player interacts with this cat
    public void OnPlayerInteract(CatPlayerController player)
    {
        if (player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Player can disguise as ANY cat (even if fed) - the risk is on them!
        player.SetDisguise(catId, catColor, disguiseSprite);
        Debug.Log($"Player is now disguised as {catId}!");
    }

    // Called when this cat gets fed by Grandma (either as itself or player was disguised as it)
    public void MarkAsFed()
    {
        IsFed = true;
        fedTimer = hungryAgainTime;
        
        // Subtle visual indicator - reduce opacity
        if (spriteRenderer != null)
        {
            Color fedColor = originalColor;
            fedColor.a = fedAlpha;
            spriteRenderer.color = fedColor;
        }
        
        Debug.Log($"{catId} has been fed (will be hungry again in {hungryAgainTime} seconds)");
    }

    void BecomeHungryAgain()
    {
        IsFed = false;
        
        // Restore original appearance
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Remove from GameManager's fed list
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnfeedCat(catId);
        }
        
        Debug.Log($"{catId} is hungry again!");
    }
}
