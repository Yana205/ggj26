using UnityEngine;
using UnityEngine.InputSystem;

public class CatPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Disguise")]
    [SerializeField] bool isDisguised;

    [Header("Interaction")]
    [SerializeField] float interactRadius = 2f;
    [SerializeField] LayerMask interactLayerMask = -1;

    InputAction interactAction;

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
            }
        }
    }

    void Update()
    {
        // Movement: implement WASD movement here (e.g. read Move action and apply to transform or Rigidbody2D)
        // Input: inputActions["Player"]["Move"].ReadValue<Vector2>()

        if (interactAction != null && interactAction.WasPressedThisFrame())
            TryInteract();
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
