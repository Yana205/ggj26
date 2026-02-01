using UnityEngine;

public class HungerController : MonoBehaviour
{
    [SerializeField] float maxHunger = 100f;
    [SerializeField] float depletionPerSecond = 5f;

    float currentHunger;
    bool starved;

    public float HungerNormalized => maxHunger > 0 ? Mathf.Clamp01(currentHunger / maxHunger) : 0f;
    public float CurrentHunger => currentHunger;

    void Start()
    {
        currentHunger = maxHunger;
        starved = false;
    }

    void Update()
    {
        // Don't deplete if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (starved) return;

        currentHunger -= depletionPerSecond * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        // Check for starvation
        if (currentHunger <= 0f && !starved)
        {
            starved = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("Starved! You ran out of hunger!");
            }
        }
    }

    public void GetFed()
    {
        currentHunger = Mathf.Clamp(currentHunger + 30f, 0f, maxHunger);;
    }
}
