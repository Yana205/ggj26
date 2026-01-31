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

    [Header("Base Timing (modified by personality)")]
    [SerializeField] float baseMinIdleTime = 4f;
    [SerializeField] float baseMaxIdleTime = 10f;
    [SerializeField] float baseMinRestTime = 5f;
    [SerializeField] float baseMaxRestTime = 12f;


    [Header("Movement (modified by personality)")]
    [SerializeField] float baseWanderSpeed = 1f;
    [SerializeField] float wanderRadius = 3f;
    [SerializeField] float baseMinWanderTime = 2f;
    [SerializeField] float baseMaxWanderTime = 5f;
    [SerializeField] Transform grandmaTransform;
    [SerializeField] float baseApproachGrandmaChance = 0.7f;
    [SerializeField] float grandmaFeedDistance = 1.5f;

    [Header("Personality (assigned randomly at start)")]
    [SerializeField] bool randomizePersonality = true;
    public CatPersonality personality;

    public enum CatPersonality
    {
        Lazy,       // More idle, laying, sleeping. Less wandering. Slower to get hungry.
        Active,     // More wandering. Less resting. Gets hungry faster.
        Balanced    // In between.
    }

    // Actual values (set based on personality)
    float minIdleTime;
    float maxIdleTime;
    float minRestTime;
    float maxRestTime;
    float wanderSpeed;
    float minWanderTime;
    float maxWanderTime;
    float approachGrandmaChance;
    float wanderChance;  // Chance to wander vs rest

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

        // Assign random personality if enabled
        if (randomizePersonality)
        {
            AssignRandomPersonality();
        }
        ApplyPersonalityValues();

        // Start with idle
        SetState(CatState.Idle);
        ScheduleNextIdleChange();
        
        // Randomize starting frame so cats aren't synchronized
        currentFrame = Random.Range(0, currentSprites != null ? currentSprites.Length : 1);

        // Try to find Grandma if not assigned
        if (grandmaTransform == null)
        {
            var grandma = FindFirstObjectByType<GrandmaInteractable>();
            if (grandma != null)
            {
                grandmaTransform = grandma.transform;
            }
        }

        Debug.Log($"{gameObject.name} personality: {personality})");
    }

    void AssignRandomPersonality()
    {
        // Get all yard cat animators and assign diverse personalities
        var allCats = FindObjectsByType<YardCatAnimator>(FindObjectsSortMode.None);
        int catIndex = System.Array.IndexOf(allCats, this);
        int totalCats = allCats.Length;

        if (totalCats <= 1)
        {
            personality = CatPersonality.Balanced;
        }
        else if (totalCats == 2)
        {
            personality = catIndex == 0 ? CatPersonality.Lazy : CatPersonality.Active;
        }
        else
        {
            // Distribute personalities: ensure at least 1 lazy, 1 active, rest balanced
            // But randomize which cats get which role
            float rand = Random.value;
            if (rand < 0.3f)
                personality = CatPersonality.Lazy;
            else if (rand < 0.6f)
                personality = CatPersonality.Active;
            else
                personality = CatPersonality.Balanced;
        }
    }

    void ApplyPersonalityValues()
    {
        switch (personality)
        {
            case CatPersonality.Lazy:
                // More idle/rest time, less wandering, slower hunger
                minIdleTime = baseMinIdleTime * 1.5f;
                maxIdleTime = baseMaxIdleTime * 2f;
                minRestTime = baseMinRestTime * 1.5f;
                maxRestTime = baseMaxRestTime * 2f;
                wanderSpeed = baseWanderSpeed * 0.7f;
                minWanderTime = baseMinWanderTime * 0.5f;
                maxWanderTime = baseMaxWanderTime * 0.5f;
                approachGrandmaChance = baseApproachGrandmaChance;  // Less likely to go to Grandma
                wanderChance = 0.3f;  // 15% chance to wander (mostly rests)
                // Adjust hunger timer on YardCat
                if (yardCat != null) yardCat.SetHungerMultiplier(1.5f);  // Takes 50% longer to get hungry
                break;

            case CatPersonality.Active:
                // Less idle/rest time, more wandering, faster hunger
                minIdleTime = baseMinIdleTime * 0.5f;
                maxIdleTime = baseMaxIdleTime * 0.6f;
                minRestTime = baseMinRestTime * 0.5f;
                maxRestTime = baseMaxRestTime * 0.5f;
                wanderSpeed = baseWanderSpeed * 1.3f;
                minWanderTime = baseMinWanderTime * 1.5f;
                maxWanderTime = baseMaxWanderTime * 1.5f;
                approachGrandmaChance = baseApproachGrandmaChance;  // More likely to go to Grandma
                wanderChance = 0.9f;  // 50% chance to wander
                // Adjust hunger timer on YardCat
                if (yardCat != null) yardCat.SetHungerMultiplier(0.6f);  // Gets hungry 40% faster
                break;

            case CatPersonality.Balanced:
            default:
                // Use base values with slight randomization
                float variance = Random.Range(0.9f, 1.1f);
                minIdleTime = baseMinIdleTime * variance;
                maxIdleTime = baseMaxIdleTime * variance;
                minRestTime = baseMinRestTime * variance;
                maxRestTime = baseMaxRestTime * variance;
                wanderSpeed = baseWanderSpeed * variance;
                minWanderTime = baseMinWanderTime * variance;
                maxWanderTime = baseMaxWanderTime * variance;
                approachGrandmaChance = baseApproachGrandmaChance;
                wanderChance = 0.7f;  // 30% chance to wander
                // Default hunger
                if (yardCat != null) yardCat.SetHungerMultiplier(1f);
                break;
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
        // Cat sprite faces left by default, flip when moving right
        if (spriteRenderer != null)
        {
            if (direction.x < -0.1f)
                spriteRenderer.flipX = true;   // Moving left - flip (or set false if sprite faces left)
            else if (direction.x > 0.1f)
                spriteRenderer.flipX = false;  // Moving right - no flip (or set true if sprite faces left)

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
        
        // Get fed by Grandma (through GrandmaInteractable for speech bubble)
        if (yardCat != null && !yardCat.IsFed && grandmaTransform != null)
        {
            var grandma = grandmaTransform.GetComponent<GrandmaInteractable>();
            if (grandma != null)
            {
                grandma.FeedYardCat(yardCat);
            }
            else
            {
                // Fallback if no GrandmaInteractable
                if (GameManager.Instance != null)
                    GameManager.Instance.FeedYardCat(yardCat.CatId);
                yardCat.MarkAsFed();
            }
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
        
        if (rand < wanderChance && walkSprites != null && walkSprites.Length > 0)

        {
            // Chance to wander (varies by personality)
            StartWandering();
        }
        else if (rand < wanderChance)
        {
            // Wants to wander but no walk sprites - use idle sprites and still move
            Debug.LogWarning($"{gameObject.name} wants to wander but no walk sprites assigned! Using idle sprites.");
            StartWandering();
        }
        else
        {
            // Rest (idle -> laying/sleeping)
            TransitionToRandomRestState();
        }
    }

    // Debug helper - call from Inspector or use for testing
    [ContextMenu("Force Approach Grandma")]
    public void DebugForceApproachGrandma()
    {
        if (grandmaTransform == null)
        {
            Debug.LogError($"{gameObject.name}: Grandma transform not assigned!");
            var grandma = FindFirstObjectByType<GrandmaInteractable>();
            if (grandma != null)
            {
                grandmaTransform = grandma.transform;
                Debug.Log($"Found Grandma at {grandmaTransform.position}");
            }
        }

        if (grandmaTransform != null)
        {
            isWandering = true;
            isApproachingGrandma = true;
            targetPosition = grandmaTransform.position;
            wanderTimer = 30f;
            SetState(CatState.Walking);
            Debug.Log($"{gameObject.name} forced to approach Grandma!");
        }
    }

    [ContextMenu("Debug Cat Status")]
    public void DebugCatStatus()
    {
        Debug.Log($"=== {gameObject.name} Status ===");
        Debug.Log($"Personality: {personality}");
        Debug.Log($"State: {currentState}");
        Debug.Log($"Is Wandering: {isWandering}, Approaching Grandma: {isApproachingGrandma}");
        Debug.Log($"Wander Chance: {wanderChance:F2}, Approach Grandma Chance: {approachGrandmaChance:F2}");
        Debug.Log($"Grandma Transform: {(grandmaTransform != null ? grandmaTransform.name : "NULL")}");
        Debug.Log($"Walk Sprites: {(walkSprites != null ? walkSprites.Length.ToString() : "NULL")}");
        Debug.Log($"YardCat IsFed: {(yardCat != null ? yardCat.IsFed.ToString() : "NULL")}");
    }
}
