using UnityEngine;
using UnityEngine.SceneManagement;
public class sceneChanger : MonoBehaviour
{
 
    private void Start()
    {
        //Invoke("InitialMessage", 2);
    }

   

    public void changeScene()
    {
        SceneManager.LoadScene(1);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
