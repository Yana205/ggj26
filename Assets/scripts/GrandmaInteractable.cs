using UnityEngine;

public class GrandmaInteractable : MonoBehaviour
{
    GrandmaSpeechBubble speechBubble;
    SpriteRenderer spriteRenderer;
    Sprite baseSprite;
    Coroutine spriteRoutine;

    private float spriteDuration = 2f;  // Used for angry sprite; feeding uses cat's eating time
    float busyUntil;  // Grandma is busy (feeding) until this time
    float spriteRestoreDelay;  // Delay used by RestoreBaseSpriteAfterDelay

    [Header("Feeding sprite")]
    [SerializeField] Sprite feedingSprite;

    [Header("Angry sprite")]
    [SerializeField] Sprite angrySprite;

    void Start()
    {
        speechBubble = GetComponent<GrandmaSpeechBubble>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            baseSprite = spriteRenderer.sprite;
    }

    public bool IsBusy => Time.time < busyUntil;

    void SetBusyFor(float seconds)
    {
        busyUntil = Time.time + Mathf.Max(0f, seconds);
    }

    float GetEatingTimeForCat(string catId)
    {
        if (string.IsNullOrEmpty(catId)) return 2f;
        var yardCats = FindObjectsByType<YardCat>(FindObjectsSortMode.None);
        foreach (var cat in yardCats)
        {
            if (cat.CatId == catId) return cat.EatingTime;
        }
        return 2f;
    }

    public void OnPlayerInteract(CatPlayerController player)
    {
        if (player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Grandma is busy feeding another cat - can't feed now (no fed, no message)
        if (IsBusy) return;

        // Store the disguise ID before trying to feed (for marking YardCat)
        string disguiseId = player.CurrentDisguiseId;

        // No disguise - reject
        if (string.IsNullOrEmpty(disguiseId))
        {
            if (speechBubble != null)
            {
                speechBubble.ShowNoDisguiseMessage();
                AudioManager.Instance.Play(AudioManager.SoundType.AngryGrandma);
                ShowSprite(1);
            }
            return;
        }

        // Check if already fed (will trigger game over)
        bool alreadyFed = GameManager.Instance != null && GameManager.Instance.IsCatFed(disguiseId);

        // Try to feed through GameManager (handles disguise checking)
        if (GameManager.Instance != null)
        {
            bool fed = GameManager.Instance.TryFeedPlayer();
            
            if (fed)
            {
                // Successfully fed - restore hunger
                var hunger = player.GetComponent<HungerController>();
                if (hunger != null)
                    hunger.GetFed();
                
                // Show feeding sprite for this cat's eating time
                ShowSprite(0, GetEatingTimeForCat(disguiseId));
                
                // Show feeding message
                if (speechBubble != null){
                    speechBubble.ShowFeedingMessage(disguiseId);
                    AudioManager.Instance.Play(AudioManager.SoundType.Eating);
                }
                
                // Mark the corresponding yard cat as fed (visual indicator)
                MarkYardCatAsFed(disguiseId);
                
                // Grandma is busy for this cat's eating time
                SetBusyFor(GetEatingTimeForCat(disguiseId));
                
                // Clear player's disguise (grandma remembers this cat now)
                player.ClearDisguise();
            }
            else if (alreadyFed)
            {
                // Caught! Show recognition message
                if (speechBubble != null){
                    speechBubble.ShowAlreadyFedMessage(disguiseId);
                    AudioManager.Instance.Play(AudioManager.SoundType.AngryGrandma);
                }
            }
        }
        else
        {
            // Fallback if no GameManager (shouldn't happen)
            Debug.LogWarning("No GameManager found!");
        }
    }

    // Called when a yard cat approaches to get fed
    public void FeedYardCat(YardCat cat)
    {
        if (cat == null || cat.IsFed) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Grandma is busy - yard cat returns to place normally, not fed, no message
        if (IsBusy) return;

        // Show feeding sprite for this cat's eating time
        ShowSprite(0, cat.EatingTime);

        // Show message
        if (speechBubble != null)
        {
            speechBubble.ShowFeedingMessage(cat.CatId);
            AudioManager.Instance.Play(AudioManager.SoundType.Eating);
        }

        // Mark as fed
        if (GameManager.Instance != null)
            GameManager.Instance.FeedYardCat(cat.CatId);
        
        cat.MarkAsFed();

        // Grandma is busy for this cat's eating time
        SetBusyFor(cat.EatingTime);
    }

    void ShowSprite(int sprite_mode, float duration = -1f) // 0 = feeding, 1 = angry; duration < 0 = use default
    {
        if(spriteRenderer == null) return;
        if(0 == sprite_mode && feedingSprite == null) return;
        if(1 == sprite_mode && angrySprite == null) return;
        if(spriteRoutine != null)
        {
            StopCoroutine(spriteRoutine);
        }
        spriteRestoreDelay = duration >= 0f ? duration : spriteDuration;
        if(0 == sprite_mode)
        {
            spriteRenderer.sprite = feedingSprite;
        }
        else if(1 == sprite_mode)
        {
            spriteRenderer.sprite = angrySprite;
        }
        spriteRoutine = StartCoroutine(RestoreBaseSpriteAfterDelay());
    }

    System.Collections.IEnumerator RestoreBaseSpriteAfterDelay()
    {
        yield return new WaitForSeconds(spriteRestoreDelay);
        if (spriteRenderer != null && baseSprite != null)
            spriteRenderer.sprite = baseSprite;
        spriteRoutine = null;
    }

    // Find the yard cat with matching ID and mark it as fed
    void MarkYardCatAsFed(string catId)
    {
        if (string.IsNullOrEmpty(catId)) return;

        var yardCats = FindObjectsByType<YardCat>(FindObjectsSortMode.None);
        foreach (var cat in yardCats)
        {
            if (cat.CatId == catId)
            {
                cat.MarkAsFed();
                break;
            }
        }
    }
}
