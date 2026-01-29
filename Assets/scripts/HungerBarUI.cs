using UnityEngine;
using UnityEngine.UI;

public class HungerBarUI : MonoBehaviour
{
    [SerializeField] HungerController hungerController;
    [SerializeField] Text hungerText;

    void Update()
    {
        if (hungerController != null && hungerText != null)
            hungerText.text = Mathf.CeilToInt(hungerController.CurrentHunger).ToString();
    }
}
