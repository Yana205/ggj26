using Unity.Collections;
using UnityEngine;

public class GrandmaController : MonoBehaviour
{

    public GrandmaState state;
    
    float wanderSpeed = 0.4f;
    float baseWanderTimer = 10f;
    float baseIdleTimer = 5f;
    float baseShooingTimer = 2f;

    float wanderTimer;
    float idleTimer;
    float feedingTimer;
    float shooingTimer;

    Vector3 startPosition;
    Vector3 targetPosition;

    float wanderChance = 0.5f;

    public void setGrandmaState(GrandmaState _state)
    {
        state = _state;
        // TODO: Change to relevant sprite
    }

    // These will control her behavior and her active sprite
    public enum GrandmaState
    {
        Idle, // Doing nothing
        Wandering, // Wandering - still open to feeding cats but need to stand first
        Feeding, // Is currently feeding a cat - can't feed other cats
        Shooing
    }

    // Check not busy, then set state to wandering and randomize target position and start timer.
    public void startWandering()
    {

        if(state == GrandmaState.Feeding || state == GrandmaState.Shooing || (state == GrandmaState.Idle && idleTimer > 0))
        {
            return; // Don't start wandering if busy.
        }


        float offsetInX = Random.Range(3f, 7f);
        float offsetInY = Random.Range(3f, 7f);
        targetPosition = startPosition + new Vector3(offsetInX, offsetInY, 0);

        targetPosition.x = Mathf.Clamp(targetPosition.x, GameManager.minX, GameManager.maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, GameManager.minY, GameManager.maxY);

        wanderTimer = baseWanderTimer; // Set to wander time, will be reduced every call to update until reaches zero.
        setGrandmaState(GrandmaState.Wandering);
    }

    public void stopWandering()
    {
        targetPosition = transform.position;
        wanderTimer = 0;
        setGrandmaState(GrandmaState.Idle);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = GrandmaState.Idle; // Grandma starts idle.
        startPosition = transform.position;
    }

    // Movement only happens if the grandma is in Wandering mode.
    void UpdateMovement()
    {
        if(state == GrandmaState.Wandering)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if(wanderTimer <= 0 || (distanceToTarget <= 0.1))
            {
                stopWandering();
                return;
            }

            // Move towards target direction.
            else
            {
                Vector3 direction = (targetPosition - transform.position).normalized;

                Vector3 newPosition = transform.position += direction * wanderSpeed * Time.deltaTime;
                newPosition.x = Mathf.Clamp(newPosition.x, GameManager.minX, GameManager.maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, GameManager.minY, GameManager.maxY);

                transform.position = newPosition;
                wanderTimer -= Time.deltaTime;
            }       
        }

        // If grandma is Idling, set to wandering with some probability.
        if(GrandmaState.Idle == state)
        {   
            if(idleTimer <= 0)
            {   
                float rand = Random.value;
                if(rand <= wanderChance)
                {
                    startWandering();
                }
                else
                {
                    idleTimer = baseIdleTimer;
                }
            }

            else
            {
                idleTimer -= Time.deltaTime;
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
    }
}
