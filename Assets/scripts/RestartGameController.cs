using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGameController : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}
