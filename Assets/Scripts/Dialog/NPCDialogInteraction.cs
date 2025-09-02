using UnityEngine;

public class NPCDialogInteraction : MonoBehaviour
{
    [Header("Dialog Settings")]
    public DialogData npcDialogData;
    public string npcName = "NPC";
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI Indicators")]
    public GameObject interactionPrompt;
    public Canvas worldSpaceCanvas;

    private Transform player;
    private DialogSystem dialogSystem;
    private bool playerInRange = false;
    private bool isDialogActive = false;

    private void Start()
    {
        // Find player and dialog system
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        dialogSystem = FindObjectOfType<DialogSystem>();

        // Setup interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Setup world space canvas if needed
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.worldCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        // Show/hide interaction prompt
        if (playerInRange != wasInRange)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(playerInRange && !isDialogActive);
        }

        // Handle interaction input
        if (playerInRange && !isDialogActive && Input.GetKeyDown(interactionKey))
        {
            StartDialog();
        }
    }

    public void StartDialog()
    {
        if (dialogSystem == null || npcDialogData == null) return;

        isDialogActive = true;
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Subscribe to dialog end event
        dialogSystem.OnDialogEnded += OnDialogEnded;

        // Start the dialog
        dialogSystem.StartDialog(npcDialogData);
    }

    private void OnDialogEnded()
    {
        isDialogActive = false;

        // Unsubscribe from event
        if (dialogSystem != null)
            dialogSystem.OnDialogEnded -= OnDialogEnded;

        // Show interaction prompt again if player still in range
        if (playerInRange && interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
