using UnityEngine;
using UnityEngine.InputSystem;

public class CatPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Disguise")]
    [SerializeField] Color originalColor = Color.white;
    string currentDisguiseId = "";

    [SerializeField] float moveSpeed = 5f;

    [Header("Interaction")]
    [SerializeField] float interactRadius = 2f;
    [SerializeField] LayerMask interactLayerMask = -1;
    [SerializeField] bool showInteractRadius = true;  // Show radius in editor
    
    // [Header("Bounds")]
    private float minX = -25.3f;
    private float maxX = 25.6f;
    private float minY = -16f;
    private float maxY = 18f;

    InputAction interactAction;
    InputAction moveAction;

    // Animation
    Animator animator;
    SpriteRenderer spriteRenderer;
    float idleTime;

    public bool IsDisguised => !string.IsNullOrEmpty(currentDisguiseId);
    public string CurrentDisguiseId => currentDisguiseId;

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

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;

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

    void OnCollisionEnter(Collision collision)
    {
        // Log a message to the console
        Debug.Log("Collision detected with " + collision.gameObject.name);

        // You can access the other object's collider, rigidbody, etc.
        Collider otherCollider = collision.collider;

        // Example: Destroy the other object upon collision
        // Destroy(collision.gameObject);
    }

    void TryInteract()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayerMask);
        
        // DEBUG: Log how many colliders were found
        Debug.Log($"[Interact] Found {hits.Length} colliders within radius {interactRadius}");
        
        foreach (var hit in hits)
        {
            // DEBUG: Log each hit
            Debug.Log($"[Interact] Hit: {hit.gameObject.name} (Layer: {hit.gameObject.layer})");
            
            if (hit.gameObject == gameObject) continue;
            
            // Check for Grandma
            var grandma = hit.GetComponent<GrandmaInteractable>();
            if (grandma != null)
            {
                Debug.Log("[Interact] Found Grandma - interacting!");
                grandma.OnPlayerInteract(this);
                return;
            }

            // Check for Yard Cat (to copy disguise)
            var yardCat = hit.GetComponent<YardCat>();
            if (yardCat != null)
            {
                Debug.Log($"[Interact] Found YardCat {yardCat.CatId} - interacting!");
                yardCat.OnPlayerInteract(this);
                return;
            }
            
            // DEBUG: Object has no interactable script
            Debug.Log($"[Interact] {hit.gameObject.name} has no GrandmaInteractable or YardCat script!");
        }
    }

    public void SetDisguise(string catId, Color color, Sprite sprite = null)
    {
        currentDisguiseId = catId;
        
        // Change player color to match disguise
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDisguise(catId);
        }
    }

    public void ClearDisguise()
    {
        currentDisguiseId = "";
        
        // Reset to original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearDisguise();
        }
    }

    // Draw interact radius in Scene view (helps with debugging)
    void OnDrawGizmosSelected()
    {
        if (showInteractRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
