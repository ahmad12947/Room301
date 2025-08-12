using AdvancedHorrorFPS;
using UnityEngine;

public class ttsErrorFix : MonoBehaviour
{
  public static ttsErrorFix Instance;

    public GameObject errorCanvas;
    public CameraLook cameraLook;
    private void Awake()
    {
        Instance = this;
    }


    public void setUpCanvas()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        errorCanvas.SetActive(true);
        cameraLook.enabled = false;

    }
}
