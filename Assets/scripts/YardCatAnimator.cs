using UnityEngine;

public class YardCatAnimator : MonoBehaviour
{
    [Header("Sprite Arrays - Drag sprites here")]
    [SerializeField] Sprite[] idleSprites;           // Loops continuously
    [SerializeField] Sprite[] layingSprites;         // Plays once, holds last frame
    [SerializeField] Sprite[] sleepingSprites;       // Plays once, holds last frame

    [Header("Animation Settings")]
    [SerializeField] float frameRate = 8f;
    [SerializeField] float transitionDelay = 0.5f;   // Pause before transitioning

    [Header("State Timing")]
    [SerializeField] float minIdleTime = 4f;         // Min time in idle before changing
    [SerializeField] float maxIdleTime = 10f;        // Max time in idle before changing
    [SerializeField] float minRestTime = 5f;         // Min time laying/sleeping
    [SerializeField] float maxRestTime = 12f;        // Max time laying/sleeping

    SpriteRenderer spriteRenderer;
    Sprite[] currentSprites;
    int currentFrame;
    float frameTimer;
    float stateTimer;
    float nextStateChange;
    float transitionTimer;
    bool isTransitioning;
    bool animationFinished;
    bool isGettingUp;
    Sprite[] gettingUpSprites;  // Reversed sprites for getting up

    enum CatState { Idle, Laying, Sleeping, GettingUp }
    CatState currentState;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start with idle
        SetState(CatState.Idle);
        ScheduleNextIdleChange();
        
        // Randomize starting frame so cats aren't synchronized
        currentFrame = Random.Range(0, currentSprites != null ? currentSprites.Length : 1);
        
        // Randomize initial timer so cats don't all change at once
        stateTimer = Random.Range(0f, minIdleTime * 0.5f);
    }

    void Update()
    {
        // Handle transition delay
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            if (transitionTimer >= transitionDelay)
            {
                isTransitioning = false;
            }
            return;
        }

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

            if (currentState == CatState.Idle)
            {
                // Idle loops
                currentFrame = (currentFrame + 1) % currentSprites.Length;
            }
            else if (currentState == CatState.GettingUp)
            {
                // Getting up plays once (reversed laying), then switches to idle
                if (currentFrame < currentSprites.Length - 1)
                {
                    currentFrame++;
                }
                else
                {
                    // Done getting up, now go to idle
                    SetState(CatState.Idle);
                    ScheduleNextIdleChange();
                    return;
                }
            }
            else
            {
                // Laying/Sleeping plays once then holds last frame
                if (currentFrame < currentSprites.Length - 1)
                {
                    currentFrame++;
                }
                else
                {
                    animationFinished = true;
                }
            }
            
            spriteRenderer.sprite = currentSprites[currentFrame];
        }
    }

    void UpdateStateTimer()
    {
        stateTimer += Time.deltaTime;
        
        if (currentState == CatState.Idle)
        {
            // From idle, can transition to laying or sleeping
            if (stateTimer >= nextStateChange)
            {
                TransitionToRandomRestState();
            }
        }
        else
        {
            // From laying/sleeping, go back to idle after time (and animation finished)
            if (stateTimer >= nextStateChange && animationFinished)
            {
                TransitionToIdle();
            }
        }
    }

    void TransitionToRandomRestState()
    {
        // Start transition delay
        isTransitioning = true;
        transitionTimer = 0f;
        
        // Pick laying or sleeping
        float rand = Random.value;
        
        if (rand < 0.6f && layingSprites != null && layingSprites.Length > 0)
            SetState(CatState.Laying);
        else if (sleepingSprites != null && sleepingSprites.Length > 0)
            SetState(CatState.Sleeping);
        else if (layingSprites != null && layingSprites.Length > 0)
            SetState(CatState.Laying);
        else
            ScheduleNextIdleChange(); // No rest sprites, stay idle
    }

    void TransitionToIdle()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        // If coming from laying, play reversed sprites (getting up)
        if (currentState == CatState.Laying && layingSprites != null && layingSprites.Length > 1)
        {
            SetState(CatState.GettingUp);
        }
        else
        {
            SetState(CatState.Idle);
            ScheduleNextIdleChange();
        }
    }

    void SetState(CatState newState)
    {
        currentState = newState;
        currentFrame = 0;
        frameTimer = 0f;
        stateTimer = 0f;
        animationFinished = false;

        switch (newState)
        {
            case CatState.Idle:
                currentSprites = idleSprites;
                break;
            case CatState.Laying:
                currentSprites = layingSprites;
                nextStateChange = Random.Range(minRestTime, maxRestTime);
                break;
            case CatState.Sleeping:
                currentSprites = sleepingSprites;
                nextStateChange = Random.Range(minRestTime, maxRestTime);
                break;
            case CatState.GettingUp:
                // Create reversed laying sprites for getting up animation
                if (layingSprites != null && layingSprites.Length > 0)
                {
                    gettingUpSprites = new Sprite[layingSprites.Length];
                    for (int i = 0; i < layingSprites.Length; i++)
                    {
                        gettingUpSprites[i] = layingSprites[layingSprites.Length - 1 - i];
                    }
                    currentSprites = gettingUpSprites;
                }
                break;
        }

        // Set first frame immediately
        if (currentSprites != null && currentSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = currentSprites[0];
        }
    }

    void ScheduleNextIdleChange()
    {
        nextStateChange = Random.Range(minIdleTime, maxIdleTime);
    }
}
