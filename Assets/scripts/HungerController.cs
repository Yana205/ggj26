using UnityEngine;

public class HungerController : MonoBehaviour
{
    [SerializeField] float maxHunger = 100f;
    [SerializeField] float depletionPerSecond = 5f;

    float currentHunger;

    public float HungerNormalized => maxHunger > 0 ? Mathf.Clamp01(currentHunger / maxHunger) : 0f;

    void Start()
    {
        currentHunger = maxHunger;
    }

    void Update()
    {
        currentHunger -= depletionPerSecond * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        Debug.Log(currentHunger);
    }

    public void GetFed()
    {
        currentHunger = maxHunger;
    }
}
