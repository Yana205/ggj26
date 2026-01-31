using UnityEngine;

public class YardCatAnimator : MonoBehaviour
{
    [Header("Sprite Arrays - Drag sprites here")]
    [SerializeField] Sprite[] idleSprites;           // Loops continuously
    [SerializeField] Sprite[] layingSprites;         // Plays once, holds last frame
    [SerializeField] Sprite[] sleepingSprites;       // Plays once, holds last frame
    [SerializeField] Sprite[] walkSprites;           // Walking animation

    [Header("Animation Settings")]
    [SerializeField] float frameRate = 8f;
    [SerializeField] float transitionDelay = 0.5f;   // Pause before transitioning

    [Header("State Timing")]
    [SerializeField] float minIdleTime = 4f;         // Min time in idle before changing
    [SerializeField] float maxIdleTime = 10f;        // Max time in idle before changing
    [SerializeField] float minRestTime = 5f;         // Min time laying/sleeping
    [SerializeField] float maxRestTime = 12f;        // Max time laying/sleeping

    [Header("Movement")]
    [SerializeField] float wanderSpeed = 1f;
    [SerializeField] float wanderRadius = 3f;        // How far cat wanders from start
    [SerializeField] float minWanderTime = 2f;
    [SerializeField] float maxWanderTime = 5f;
    [SerializeField] Transform grandmaTransform;     // Assign Grandma in Inspector
    private float approachGrandmaChance = 0.7f;  // 70% chance to go to Grandma
    [SerializeField] float grandmaFeedDistance = 1.5f;    // How close to get to Grandma

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

    // Movement
    Vector3 startPosition;
    Vector3 targetPosition;
    float wanderTimer;
    bool isWandering;
    bool isApproachingGrandma;
    bool isGoingAwayFromGrandma;
    YardCat yardCat;

    enum CatState { Idle, Laying, Sleeping, GettingUp, Walking }
    CatState currentState;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        yardCat = GetComponent<YardCat>();
        startPosition = transform.position;
        
        // Start with idle
        SetState(CatState.Idle);
        ScheduleNextIdleChange();
        
        // Randomize starting frame so cats aren't synchronized
        currentFrame = Random.Range(0, currentSprites != null ? currentSprites.Length : 1);
        
        // Randomize initial timer so cats don't all change at once
        stateTimer = Random.Range(0f, minIdleTime * 0.5f);

        // Try to find Grandma if not assigned
        if (grandmaTransform == null)
        {
            var grandma = FindFirstObjectByType<GrandmaInteractable>();
            if (grandma != null)
            {
                grandmaTransform = grandma.transform;
            }
        }
    }

    void Update()
    {
        // Don't do anything if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

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
        UpdateMovement();
    }

    void AnimateSprites()
    {
        if (currentSprites == null || currentSprites.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;

            if (currentState == CatState.Idle || currentState == CatState.Walking)
            {
                // Idle and Walking loop
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
            // From idle, can transition to laying, sleeping, or walking
            if (stateTimer >= nextStateChange)
            {
                DecideNextAction();
            }
        }
        else if (currentState == CatState.Walking)
        {
            // Walking is handled by UpdateMovement
            return;
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
            case CatState.Walking:
                currentSprites = walkSprites != null && walkSprites.Length > 0 ? walkSprites : idleSprites;
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

    void UpdateMovement()
    {
        if (!isWandering && !isApproachingGrandma && !isGoingAwayFromGrandma) return;
        if (currentState != CatState.Walking) return;

        // Move toward target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * wanderSpeed * Time.deltaTime;

        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            if (direction.x < -0.1f)
                spriteRenderer.flipX = true;
            else if (direction.x > 0.1f)
                spriteRenderer.flipX = false;
        }

        // Check if reached target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (isApproachingGrandma)
        {
            // Check if close enough to Grandma to get fed
            if (distanceToTarget < grandmaFeedDistance)
            {
                OnReachedGrandma();
            }
        }
        else if (isGoingAwayFromGrandma)
        {
            if (distanceToTarget < 1)
            {
                OnReachedStartPosition();
            }
        }
        else
        {
            // Regular wandering - reached target
            if (distanceToTarget < 0.1f)
            {
                StopWandering();
            }
        }

        // Timeout for wandering
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            StopWandering();
        }
    }

    void StartWandering()
    {
        // Don't wander if already fed, unless to go away from grandma
        if (yardCat != null && yardCat.IsFed && !isGoingAwayFromGrandma) return;

        isWandering = true;
        isApproachingGrandma = false;

        // Decide: wander randomly or approach Grandma?
        bool shouldApproachGrandma = !isGoingAwayFromGrandma &&
                                      grandmaTransform != null && 
                                      Random.value < approachGrandmaChance &&
                                      yardCat != null && !yardCat.IsFed;

        if (shouldApproachGrandma)
        {
            // Go to Grandma
            isApproachingGrandma = true;
            targetPosition = grandmaTransform.position;
            wanderTimer = 30f;  // Long timeout for approaching Grandma
            Debug.Log($"{gameObject.name} is going to Grandma!");
        }
        else
        {
            // Random wander within radius
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
            if (isGoingAwayFromGrandma)
            {
                Debug.Log($"{gameObject.name} is going away from Grandma!");        
            }
        }

        SetState(CatState.Walking);
    }

    void StopWandering()
    {
        isWandering = false;
        isApproachingGrandma = false;
        SetState(CatState.Idle);
        ScheduleNextIdleChange();
    }

    void OnReachedGrandma()
    {
        Debug.Log($"{gameObject.name} reached Grandma and wants food!");
        
        // Get fed by Grandma
        if (yardCat != null && !yardCat.IsFed)
        {
            // Mark this cat as fed
            if (GameManager.Instance != null)
            {
                GameManager.Instance.FeedYardCat(yardCat.CatId);
            }
            yardCat.MarkAsFed();
        }

        isApproachingGrandma = false;

        isGoingAwayFromGrandma = true;
        targetPosition = startPosition;
        StopWandering();
    }

    void OnReachedStartPosition()
    {
        Debug.Log($"{gameObject.name} reached back to start position!");

        // Stop and go back to idle
        StopWandering();
    }

    // Modified: Sometimes wander instead of laying/sleeping
    void DecideNextAction()
    {
        float rand = Random.value;
        
        if (rand < 0.7f && walkSprites != null && walkSprites.Length > 0)
        {
            // 30% chance to wander
            StartWandering();
        }
        else
        {
            // 70% chance to rest
            TransitionToRandomRestState();
        }
    }
}
