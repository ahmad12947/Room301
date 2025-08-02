using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DoorScript;
using AdvancedHorrorFPS;

public class DoorAndBookInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    private Door currentDoor;
    private GameObject currentBookWorld;
    private bool hasUnlockedBook = false;
    private bool isReadingBook = false;

    [Header("Camera Reference")]
    public Camera playerCamera;

    [Header("Book UI")]
    public GameObject bookUIObject;
    public GameObject inputFieldUI;
    private Animator bookAnimator;

    [Header("Audio")]
    public AudioClip openBookSound;
    public AudioClip closeBookSound;
    public AudioSource audioSource;
    public bool isLocked = false;

    [Header("UI Prompt")]
    public TextMeshProUGUI interactionPrompt;
    private GameObject obj;

    [Header("Hint UI")]
    public Text bookHintText;
    private bool bookHintShown = false;

    [Header("Fuse Interaction")]
    public GameObject fuseObject;
    public Light[] fuseEnableObjects;
    public Text objectiveText;
    public string newObjectiveAfterFuse = "Find a way to unlock Room 301";

    public AudioClip fuzeAudioClip, safeAudioClip;
    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.text = "";
            interactionPrompt.gameObject.SetActive(false);
        }

        if (!playerCamera)
            playerCamera = Camera.main;

        if (bookUIObject != null)
        {
            bookAnimator = bookUIObject.GetComponent<Animator>();
            bookUIObject.SetActive(false);
        }

        if (inputFieldUI != null)
            inputFieldUI.SetActive(false);
    }

    void Update()
    {
        // Toggle Book with Tab after first interaction
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isReadingBook)
            {
                ExitBookView();
                return;
            }
            else if (hasUnlockedBook)
            {
                OpenBookView();
                return;
            }
        }

        if (isReadingBook) return;

        HandleRaycast();
    }

    void HandleRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        interactionPrompt.gameObject.SetActive(false);
        currentDoor = null;
        currentBookWorld = null;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // DOOR INTERACTION
            Door door = hitObject.GetComponent<Door>();
            if (door != null )
            {
                if (door != null && door.enabled)
                {
                    currentDoor = door;
                    ShowPrompt("[E] Open Door");

                    if (Input.GetKeyDown(KeyCode.E))
                        currentDoor.OpenDoor();
                }
                else
                {
                    ShowPrompt("[E] Door is Locked");

                    if (Input.GetKeyDown(KeyCode.E))
                        Debug.Log("Door is locked.");
                }
                return;
            }

            // BOOK INTERACTION
            if (hitObject.CompareTag("Book"))
            {
                currentBookWorld = hitObject;
                ShowPrompt("[E] Read Book");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    objectiveText.text = "Find out where the ghost lives";
                    hasUnlockedBook = true;
                    OpenBookView();
                }
                return;
            }

            // SAFE INTERACTION
            // SAFE INTERACTION
            if (hitObject.CompareTag("safe"))
            {
                if (isLocked)
                {
                    ShowPrompt("It's locked.");
                }
                else
                {
                    ShowPrompt("[E] Open Safe");

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        hitObject.GetComponent<Animator>().Play("Take 001");
                        Invoke("SetActive", 3);
                        obj = hitObject.transform.GetChild(0).gameObject;
                        AudioSource.PlayClipAtPoint(safeAudioClip, transform.position);
                    }
                }
                return;
            }

            // FUSE INTERACTION
            if (fuseObject != null && hitObject == fuseObject)
            {
                ShowPrompt("[E] Fix Fuse");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    foreach (Light go in fuseEnableObjects)
                        go.enabled = true;

                    if (objectiveText != null)
                        objectiveText.text = newObjectiveAfterFuse;
                    fuseObject.GetComponent<BoxCollider>().enabled = false;
                    AudioSource.PlayClipAtPoint(fuzeAudioClip, transform.position);
                    /*  fuseObject.SetActive(false);*/ // optional: disable fuse after interaction
                }
                return;
            }
        }
    }

    private void SetActive()
    {
        obj.SetActive(true);
    }

    void ShowPrompt(string message)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.text = message;
            interactionPrompt.gameObject.SetActive(true);
        }
    }

    void OpenBookView()
    {
        if (!bookUIObject || !inputFieldUI)
        {
            Debug.LogWarning("[Book] Missing UI references.");
            return;
        }

        if (currentBookWorld != null)
            currentBookWorld.SetActive(false);

        bookUIObject.SetActive(true);
        transform.GetComponent<FirstPersonController>().enabled = false;
        CameraLook.Instance.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isReadingBook = true;

        if (openBookSound && audioSource)
            audioSource.PlayOneShot(openBookSound);

        if (bookAnimator)
        {
            bookAnimator.Play("Open");
            Invoke(nameof(EnableInputField), 1f);
        }
        else
        {
            EnableInputField();
        }
    }

    void EnableInputField()
    {
        if (inputFieldUI != null)
            inputFieldUI.SetActive(true);
    }

    void ExitBookView()
    {
        if (inputFieldUI != null)
            inputFieldUI.SetActive(false);

        if (closeBookSound && audioSource)
            audioSource.PlayOneShot(closeBookSound);

        if (bookAnimator)
        {
            bookAnimator.Play("Close");
            Invoke(nameof(CleanupBookUI), 1f);
        }
        else
        {
            CleanupBookUI();
        }

        transform.GetComponent<FirstPersonController>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isReadingBook = false;

        if (!bookHintShown)
        {
            bookHintShown = true;
            ShowBookHint("Press Tab to open the book and communicate with the ghost.");
        }
    }

    void ShowBookHint(string message)
    {
        if (bookHintText == null) return;

        bookHintText.text = message;
        bookHintText.gameObject.SetActive(true);
        Invoke(nameof(HideBookHint), 6f);
    }

    void HideBookHint()
    {
        if (bookHintText != null)
            bookHintText.gameObject.SetActive(false);
    }

    void CleanupBookUI()
    {
        CameraLook.Instance.enabled = true;

        if (bookUIObject != null)
            bookUIObject.SetActive(false);

        if (currentBookWorld != null)
            currentBookWorld.SetActive(true);
    }

    public void showFlashLightHint()
    {
        bookHintText.gameObject.SetActive(true);
        ShowBookHint("Press Right mouse button to switch between Lights");
        Invoke("HideBookHint()", 7);
    }
}
