using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] int mealsToWin = 5;

    [Header("State (Read Only)")]
    [SerializeField] int currentMeals;
    [SerializeField] string currentDisguise = "";
    [SerializeField] List<string> fedCats = new List<string>();

    bool gameOver;
    bool gameWon;

    public int MealsToWin => mealsToWin;
    public int CurrentMeals => currentMeals;
    public string CurrentDisguise => currentDisguise;
    public bool IsGameOver => gameOver;
    public bool IsGameWon => gameWon;

    public event System.Action OnGameStateChanged;
    public event System.Action<string> OnGameOver;  // reason
    public event System.Action OnGameWon;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Player starts as themselves (not disguised)
        currentDisguise = "";
        currentMeals = 0;
        fedCats.Clear();
        gameOver = false;
        gameWon = false;

        // Start background music looping for the main game scene
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Background);
        }
    }

    public void SetDisguise(string catId)
    {
        currentDisguise = catId;
        Debug.Log($"Player disguised as: {catId}");
        OnGameStateChanged?.Invoke();
    }

    public void ClearDisguise()
    {
        currentDisguise = "";
        OnGameStateChanged?.Invoke();
    }

    public bool IsCatFed(string catId)
    {
        return fedCats.Contains(catId);
    }

    // Called when a yard cat gets fed by Grandma (not the player)
    public void FeedYardCat(string catId)
    {
        if (gameOver || gameWon) return;
        if (fedCats.Contains(catId)) return;  // Already fed

        fedCats.Add(catId);
        Debug.Log($"Grandma fed yard cat: {catId}");
        OnGameStateChanged?.Invoke();
    }

    // Called when a yard cat becomes hungry again
    public void UnfeedCat(string catId)
    {
        if (fedCats.Contains(catId))
        {
            fedCats.Remove(catId);
            Debug.Log($"Cat {catId} is hungry again (removed from fed list)");
            OnGameStateChanged?.Invoke();
        }
    }

    public bool TryFeedPlayer()
    {
        if (gameOver || gameWon) return false;

        // Check if player has a disguise
        if (string.IsNullOrEmpty(currentDisguise))
        {
            Debug.Log("Grandma: I already fed you! (no disguise)");
            return false;
        }

        // Check if this disguise was already fed
        if (IsCatFed(currentDisguise))
        {
            Debug.Log($"Grandma: Wait... I already fed {currentDisguise}! You're the same cat!");
            TriggerGameOver("Caught! Grandma recognized your disguise!");
            return false;
        }

        // Feed successful
        fedCats.Add(currentDisguise);
        currentMeals++;
        Debug.Log($"Grandma feeds the cat! ({currentDisguise}) - Meals: {currentMeals}/{mealsToWin}");

        // Clear disguise after feeding (grandma remembers this cat now)
        ClearDisguise();

        // Check win condition
        if (currentMeals >= mealsToWin)
        {
            TriggerWin();
        }

        OnGameStateChanged?.Invoke();
        return true;
    }

    public void TriggerGameOver(string reason)
    {
        if (gameOver || gameWon) return;
        
        gameOver = true;
        Debug.Log($"GAME OVER: {reason}");
        OnGameOver?.Invoke(reason);
        OnGameStateChanged?.Invoke();
        SceneManager.LoadScene("LoseScene", LoadSceneMode.Single);
        AudioManager.Instance.Play(AudioManager.SoundType.LoseGame);
    }

    public void TriggerWin()
    {
        if (gameOver || gameWon) return;
        
        gameWon = true;
        Debug.Log("YOU WIN! Cat is fully satisfied!");
        OnGameWon?.Invoke();
        OnGameStateChanged?.Invoke();
        AudioManager.Instance.Play(AudioManager.SoundType.WinGame);
        SceneManager.LoadScene("WinScene", LoadSceneMode.Single);
    }

    public void RestartGame()
    {
        currentDisguise = "";
        currentMeals = 0;
        fedCats.Clear();
        gameOver = false;
        gameWon = false;
        OnGameStateChanged?.Invoke();
        
        // Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
