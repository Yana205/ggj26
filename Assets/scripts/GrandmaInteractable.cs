using UnityEngine;

public class GrandmaInteractable : MonoBehaviour
{
    GrandmaSpeechBubble speechBubble;

    void Start()
    {
        speechBubble = GetComponent<GrandmaSpeechBubble>();
    }

    public void OnPlayerInteract(CatPlayerController player)
    {
        if (player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Store the disguise ID before trying to feed (for marking YardCat)
        string disguiseId = player.CurrentDisguiseId;

        // No disguise - reject
        if (string.IsNullOrEmpty(disguiseId))
        {
            if (speechBubble != null)
            {
                speechBubble.ShowNoDisguiseMessage();
                AudioManager.Instance.Play(AudioManager.SoundType.AngryGrandma);
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
                
                // Show feeding message
                if (speechBubble != null){
                    speechBubble.ShowFeedingMessage(disguiseId);
                    AudioManager.Instance.Play(AudioManager.SoundType.Eating);
                }
                
                // Mark the corresponding yard cat as fed (visual indicator)
                MarkYardCatAsFed(disguiseId);
                
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
