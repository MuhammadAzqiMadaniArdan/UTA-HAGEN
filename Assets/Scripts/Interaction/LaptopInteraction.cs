using UnityEngine;

public class LaptopInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI Elements")]
    public GameObject interactionPrompt; // "Press E to use laptop"
    public Canvas worldSpaceCanvas;

    private Transform player;
    private GameUI gameUI;
    private bool playerInRange = false;
    private bool laptopActive = false;

    void Start()
    {
        // Find player and GameUI
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        gameUI = FindObjectOfType<GameUI>();

        // Setup interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Setup world space canvas
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.worldCamera = Camera.main;
            worldSpaceCanvas.sortingOrder = 10;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactionRange;

        // Update prompt visibility
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(playerInRange && !laptopActive);
        }

        // Handle interaction input
        if (playerInRange && Input.GetKeyDown(interactionKey) && !laptopActive)
        {
            OpenLaptop();
        }

        // Emergency close laptop (when NPC approaches or Escape pressed)
        if (laptopActive && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLaptop();
        }
    }

    private void OpenLaptop()
    {
        if (gameUI != null)
        {
            laptopActive = true;

            // Hide interaction prompt
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player movement/camera control
            FirstPersonController playerController = player.GetComponent<FirstPersonController>();
            if (playerController != null)
                playerController.enabled = false;

            // Open laptop interface through GameUI
            gameUI.OpenLaptop();

            // Add suspicion for using laptop (risky behavior)
            if (GameManager.Instance != null)
                GameManager.Instance.AddSuspicion(2f);
        }
    }

    private void CloseLaptop()
    {
        if (gameUI != null)
        {
            laptopActive = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Re-enable player movement/camera control
            FirstPersonController playerController = player.GetComponent<FirstPersonController>();
            if (playerController != null)
                playerController.enabled = true;

            // Close laptop interface through GameUI
            gameUI.CloseLaptop();

            // Show prompt again if player still in range
            if (playerInRange && interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    // Called by GameUI when laptop is closed
    public void OnLaptopClosed()
    {
        laptopActive = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player movement
        FirstPersonController playerController = player.GetComponent<FirstPersonController>();
        if (playerController != null)
            playerController.enabled = true;

        // Show prompt again if player still in range
        if (playerInRange && interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        // Draw interaction range in editor
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
