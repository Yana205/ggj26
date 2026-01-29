using UnityEngine;

public class GrandmaInteractable : MonoBehaviour
{
    public void OnPlayerInteract(CatPlayerController player)
    {
        if (player == null) return;

        if (player.IsDisguised)
        {
            Debug.Log("Grandma feeds the cat!");
            var hunger = player.GetComponent<HungerController>();
            if (hunger != null)
                hunger.GetFed();
        }
        else
        {
            Debug.Log("Grandma shoos the cat away!");
        }
    }
}
