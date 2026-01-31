using UnityEngine;

public class YardCatAnimator : MonoBehaviour
{
    [Header("Sprite Arrays - Drag sprites here")]
    [SerializeField] Sprite[] idleSprites;
    [SerializeField] Sprite[] layingSprites;
    [SerializeField] Sprite[] sleepingSprites;
    [SerializeField] Sprite[] walkSprites;

    [Header("Animation Settings")]
    [SerializeField] float frameRate = 8f;
    [SerializeField] float transitionDelay = 0.5f;

    [Header("State Timing")]
    [SerializeField] float minIdleTime = 4f;
    [SerializeField] float maxIdleTime = 10f;
    [SerializeField] float minRestTime = 5f;
    [SerializeField] float maxRestTime = 12f;

    [Header("Movement")]
    [SerializeField] float wanderSpeed = 1f;
    [SerializeField] float wanderRadius = 3f;
    [SerializeField] float minWanderTime = 2f;
    [SerializeField] float maxWanderTime = 5f;
    [SerializeField] Transform grandmaTransform;
    [SerializeField] float approachGrandmaChance = 0.5f;
    [SerializeField] float grandmaFeedDistance = 1.5f;

    SpriteRenderer spriteRenderer;
    Sprite[] currentSprites;
    int currentFrame;
    float frameTimer;
    float stateTimer;
    float nextStateChange;
    float transitionTimer;
    bool isTransitioning;
    bool animationFinished;
    Sprite[] gettingUpSprites;

    Vector3 startPosition;
    Vector3 targetPosition;
    float wanderTimer;
    bool isWandering;
    bool isApproachingGrandma;
    YardCat yardCat;

    float initialDelay;
    bool hasStarted;

    enum CatState { Idle, Laying, Sleeping, GettingUp, Walking }
    CatState currentState;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        yardCat = GetComponent<YardCat>();
        startPosition = transform.position;

        initialDelay = Random.Range(0f, 5f);
        hasStarted = false;
        
        SetState(CatState.Idle);
        ScheduleNextIdleChange();
        
        currentFrame = Random.Range(0, currentSprites != null ? currentSprites.Length : 1);

        if (grandmaTransform == null)
        {
            var grandma = FindFirstObjectByType<GrandmaInteractable>();
            if (grandma != null)
            {
                grandmaTransform = grandma.transform;
            }
        }

        Debug.Log($"{gameObject.name} ready (starts in {initialDelay:F1}s)");
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (!hasStarted)
        {
            initialDelay -= Time.deltaTime;
            if (initialDelay <= 0)
                hasStarted = true;
            else
            {
                AnimateSprites();
                return;
            }
        }

        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            if (transitionTimer >= transitionDelay)
                isTransitioning = false;
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
                currentFrame = (currentFrame + 1) % currentSprites.Length;
            }
            else if (currentState == CatState.GettingUp)
            {
                if (currentFrame < currentSprites.Length - 1)
                    currentFrame++;
                else
                {
                    SetState(CatState.Idle);
                    ScheduleNextIdleChange();
                    return;
                }
            }
            else
            {
                if (currentFrame < currentSprites.Length - 1)
                    currentFrame++;
                else
                    animationFinished = true;
            }
            
            spriteRenderer.sprite = currentSprites[currentFrame];
        }
    }

    void UpdateStateTimer()
    {
        stateTimer += Time.deltaTime;
        
        if (currentState == CatState.Idle)
        {
            if (stateTimer >= nextStateChange)
                DecideNextAction();
        }
        else if (currentState == CatState.Walking)
        {
            return;
        }
        else
        {
            if (stateTimer >= nextStateChange && animationFinished)
                TransitionToIdle();
        }
    }

    void TransitionToRandomRestState()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        float rand = Random.value;
        
        if (rand < 0.6f && layingSprites != null && layingSprites.Length > 0)
            SetState(CatState.Laying);
        else if (sleepingSprites != null && sleepingSprites.Length > 0)
            SetState(CatState.Sleeping);
        else if (layingSprites != null && layingSprites.Length > 0)
            SetState(CatState.Laying);
        else
            ScheduleNextIdleChange();
    }

    void TransitionToIdle()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        if (currentState == CatState.Laying && layingSprites != null && layingSprites.Length > 1)
            SetState(CatState.GettingUp);
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
                if (layingSprites != null && layingSprites.Length > 0)
                {
                    gettingUpSprites = new Sprite[layingSprites.Length];
                    for (int i = 0; i < layingSprites.Length; i++)
                        gettingUpSprites[i] = layingSprites[layingSprites.Length - 1 - i];
                    currentSprites = gettingUpSprites;
                }
                break;
        }

        if (currentSprites != null && currentSprites.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = currentSprites[0];
    }

    void ScheduleNextIdleChange()
    {
        nextStateChange = Random.Range(minIdleTime, maxIdleTime);
    }

    void UpdateMovement()
    {
        if (!isWandering && !isApproachingGrandma) return;
        if (currentState != CatState.Walking) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * wanderSpeed * Time.deltaTime;

        if (spriteRenderer != null)
        {
            if (direction.x < -0.1f)
                spriteRenderer.flipX = true;
            else if (direction.x > 0.1f)
                spriteRenderer.flipX = false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (isApproachingGrandma)
        {
            if (distanceToTarget < grandmaFeedDistance)
                OnReachedGrandma();
        }
        else
        {
            if (distanceToTarget < 0.1f)
                StopWandering();
        }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
            StopWandering();
    }

    void StartWandering()
    {
        if (yardCat != null && yardCat.IsFed) return;

        isWandering = true;
        isApproachingGrandma = false;

        bool shouldApproachGrandma = grandmaTransform != null && 
                                      Random.value < approachGrandmaChance &&
                                      yardCat != null && !yardCat.IsFed;

        if (shouldApproachGrandma)
        {
            isApproachingGrandma = true;
            targetPosition = grandmaTransform.position;
            wanderTimer = 30f;
            Debug.Log($"{gameObject.name} is going to Grandma!");
        }
        else
        {
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
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
        
        if (yardCat != null && !yardCat.IsFed && grandmaTransform != null)
        {
            var grandma = grandmaTransform.GetComponent<GrandmaInteractable>();
            if (grandma != null)
                grandma.FeedYardCat(yardCat);
            else
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.FeedYardCat(yardCat.CatId);
                yardCat.MarkAsFed();
            }
        }

        StopWandering();
    }

    void DecideNextAction()
    {
        float rand = Random.value;
        
        if (rand < 0.5f)
            StartWandering();
        else
            TransitionToRandomRestState();
    }
}
