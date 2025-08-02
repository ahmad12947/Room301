using AdvancedHorrorFPS;
using UnityEngine;

public class UVHintRevealer : MonoBehaviour
{
    public Transform rayOrigin; // usually aimPoint from flashlight
    public float revealDistance = 5f;
    public LayerMask mirrorLayerMask;
    public GameObject codeHintObject; // Assign UV code GameObject here
    public DoorAndBookInteractor interactor;
    private void Update()
    {
        if (!FlashLightScript.Instance.isGrabbed) return;

        bool blueLightActive = GameCanvas.Instance.isFlashBlueNow;
        bool lookingAtMirror = false;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, revealDistance, mirrorLayerMask))
        {
            if (hit.collider.CompareTag("MIRROR"))
            {
                lookingAtMirror = true;
                
            }
        }
        if (blueLightActive && lookingAtMirror)
        {
            interactor.isLocked = false;
        }
        codeHintObject.SetActive(blueLightActive && lookingAtMirror);
    }
}
