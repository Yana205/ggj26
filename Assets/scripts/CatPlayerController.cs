using UnityEngine;
using UnityEngine.InputSystem;

public class CatPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Disguise")]
    [SerializeField] bool isDisguised;

    [SerializeField] float moveSpeed = 5f;

    [Header("Interaction")]
    [SerializeField] float interactRadius = 2f;
    [SerializeField] LayerMask interactLayerMask = -1;

    InputAction interactAction;
    InputAction moveAction;

    // Animation
    Animator animator;
    SpriteRenderer spriteRenderer;
    float idleTime;

    public bool IsDisguised => isDisguised;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                playerMap.Enable();
                interactAction = playerMap.FindAction("Interact");
                moveAction = playerMap.FindAction("Move");
            }
        }
    }

    void Update()
    {
        // Movement: implement WASD movement here (e.g. read Move action and apply to transform or Rigidbody2D)
        // Input: inputActions["Player"]["Move"].ReadValue<Vector2>()

        if (interactAction != null && interactAction.WasPressedThisFrame())
            TryInteract();

        if (moveAction == null)
        {
            Debug.LogWarning("moveAction is null! Check inputActions assignment in Inspector.");
        }
        else
        {
            Vector2 move = moveAction.ReadValue<Vector2>();
            Vector3 delta = new Vector3(move.x, move.y, 0f) * moveSpeed * Time.deltaTime;
            transform.position += delta;

            // Animation: set IsMoving parameter and track idle time
            bool isMoving = move.sqrMagnitude > 0.01f;
            
            if (isMoving)
            {
                idleTime = 0f;
            }
            else
            {
                idleTime += Time.deltaTime;
            }

            if (animator != null)
            {
                animator.SetBool("IsMoving", isMoving);
                animator.SetFloat("IdleTime", idleTime);
                
                // Reset idle time after meow triggers (so it doesn't keep triggering)
                if (idleTime > 3.1f)
                {
                    idleTime = 0f;
                }
            }

            // Flip sprite based on horizontal movement direction
            if (spriteRenderer != null)
            {
                if (move.x < -0.1f)
                    spriteRenderer.flipX = true;   // facing left (flipped)
                else if (move.x > 0.1f)
                    spriteRenderer.flipX = false;  // facing right (default)
            }
        }
    }

    void TryInteract()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayerMask);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            var grandma = hit.GetComponent<GrandmaInteractable>();
            if (grandma != null)
            {
                grandma.OnPlayerInteract(this);
                return;
            }
        }
    }
}
