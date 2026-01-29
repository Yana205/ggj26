using UnityEngine;
using UnityEngine.UI;

public class HungerBarUI : MonoBehaviour
{
    [SerializeField] HungerController hungerController;
    [SerializeField] Image fillImage;

    void Start()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }
    void Update()
    {
        if (hungerController != null && fillImage != null)
            fillImage.fillAmount = hungerController.HungerNormalized;
    }
}
