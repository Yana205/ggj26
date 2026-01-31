using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TMP_Text mealCountText;
    [SerializeField] TMP_Text disguiseText;
    [SerializeField] TMP_Text hungerText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text gameOverText;
    [SerializeField] GameObject winPanel;
    [SerializeField] TMP_Text winText;

    [Header("References")]
    [SerializeField] HungerController hungerController;

    void Start()
    {
        // Hide end game panels
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Subscribe to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += UpdateUI;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnGameWon += ShowWin;
        }

        UpdateUI();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= UpdateUI;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnGameWon -= ShowWin;
        }
    }

    void Update()
    {
        // Update hunger every frame
        UpdateHungerDisplay();
    }

    void UpdateUI()
    {
        UpdateMealCount();
        UpdateDisguiseDisplay();
    }

    void UpdateMealCount()
    {
        if (mealCountText == null || GameManager.Instance == null) return;
        mealCountText.text = $"Meals: {GameManager.Instance.CurrentMeals}/{GameManager.Instance.MealsToWin}";
    }

    void UpdateDisguiseDisplay()
    {
        if (disguiseText == null || GameManager.Instance == null) return;

        string disguise = GameManager.Instance.CurrentDisguise;
        if (string.IsNullOrEmpty(disguise))
        {
            disguiseText.text = "Disguise: None";
        }
        else
        {
            disguiseText.text = $"Disguised as: {disguise}";
        }
    }

    void UpdateHungerDisplay()
    {
        if (hungerText == null || hungerController == null) return;
        hungerText.text = $"Hunger: {Mathf.CeilToInt(hungerController.CurrentHunger)}";
    }

    void ShowGameOver(string reason)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\n{reason}";
        }
    }

    void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        if (winText != null)
        {
            winText.text = "YOU WIN!\nThe cat is satisfied!";
        }
    }

    // Called by restart button
    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
