using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class TextCycler : MonoBehaviour
{

    public TextMeshProUGUI textComponent;

    private string[] lines =
    {
        "Grandma already fed me"
        ,"She won't feed me again :("
        ,"Time to get a disguise ;)"
        // ,""
    };

    public float delay = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CycleText());
    }

    IEnumerator CycleText()
    {
        foreach (string line in lines)
        {
            textComponent.text = line;
            yield return new WaitForSeconds(delay);
        }
        
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
