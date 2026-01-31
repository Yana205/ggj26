using UnityEngine;

public class GrandmaInteractable : MonoBehaviour
{
    public void OnPlayerInteract(CatPlayerController player)
    {
        if (player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Store the disguise ID before trying to feed (for marking YardCat)
        string disguiseId = player.CurrentDisguiseId;

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
                
                // Mark the corresponding yard cat as fed (visual indicator)
                MarkYardCatAsFed(disguiseId);
                
                // Clear player's disguise (grandma remembers this cat now)
                player.ClearDisguise();
            }
        }
        else
        {
            // Fallback if no GameManager (shouldn't happen)
            Debug.LogWarning("No GameManager found!");
        }
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
