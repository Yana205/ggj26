using UnityEngine;

public class YardCatAnimator : MonoBehaviour
{
    [Header("Sprite Arrays - Drag sprites here")]
    [SerializeField] Sprite[] idleSprites;
    [SerializeField] Sprite[] layingSprites;
    [SerializeField] Sprite[] sleepingSprites;

    [Header("Animation Settings")]
    [SerializeField] float frameRate = 8f;
    [SerializeField] float minStateTime = 3f;
    [SerializeField] float maxStateTime = 8f;

    SpriteRenderer spriteRenderer;
    Sprite[] currentSprites;
    int currentFrame;
    float frameTimer;
    float stateTimer;
    float nextStateChange;

    enum CatState { Idle, Laying, Sleeping }
    CatState currentState;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start with idle
        SetState(CatState.Idle);
        ScheduleNextStateChange();
        
        // Randomize starting frame so cats aren't synchronized
        currentFrame = Random.Range(0, currentSprites != null ? currentSprites.Length : 1);
    }

    void Update()
    {
        AnimateSprites();
        UpdateStateTimer();
    }

    void AnimateSprites()
    {
        if (currentSprites == null || currentSprites.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % currentSprites.Length;
            spriteRenderer.sprite = currentSprites[currentFrame];
        }
    }

    void UpdateStateTimer()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= nextStateChange)
        {
            ChangeToRandomState();
            ScheduleNextStateChange();
        }
    }

    void ChangeToRandomState()
    {
        // Pick a random state (weighted towards idle)
        float rand = Random.value;
        
        if (rand < 0.5f)
            SetState(CatState.Idle);
        else if (rand < 0.8f && layingSprites != null && layingSprites.Length > 0)
            SetState(CatState.Laying);
        else if (sleepingSprites != null && sleepingSprites.Length > 0)
            SetState(CatState.Sleeping);
        else
            SetState(CatState.Idle);
    }

    void SetState(CatState newState)
    {
        currentState = newState;
        currentFrame = 0;
        frameTimer = 0f;

        switch (newState)
        {
            case CatState.Idle:
                currentSprites = idleSprites;
                break;
            case CatState.Laying:
                currentSprites = layingSprites;
                break;
            case CatState.Sleeping:
                currentSprites = sleepingSprites;
                break;
        }

        // Set first frame immediately
        if (currentSprites != null && currentSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentSprites[0];
        }
    }

    void ScheduleNextStateChange()
    {
        stateTimer = 0f;
        nextStateChange = Random.Range(minStateTime, maxStateTime);
    }
}
