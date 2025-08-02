using UnityEngine;
using UnityEngine.SceneManagement;
public class endGame : MonoBehaviour
{
    public AudioSource endGameAudio;
    public GameObject blackScreen;
    public void endGameFun()
    {
        blackScreen.SetActive(true);
        endGameAudio.Play();
        Invoke("changeScene", 6);
    }

    public void changeScene()
    {
        SceneManager.LoadScene(0);
    }
}
