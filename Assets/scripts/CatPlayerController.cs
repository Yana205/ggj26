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

    public bool IsDisguised => isDisguised;

    void Start()
    {
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

            if (moveAction != null)
            {
                Vector2 move = moveAction.ReadValue<Vector2>();
                Vector3 delta = new Vector3(move.x, move.y, 0f) * moveSpeed * Time.deltaTime;
                transform.position += delta;
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
