using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [Header("Dialog Configuration")]
    public DialogData dialogToTrigger;
    public bool triggerOnce = true;
    public bool requirePlayerLookAt = false;
    public float interactionDistance = 3f;

    [Header("UI")]
    public GameObject interactionPrompt;
    public KeyCode interactionKey = KeyCode.E;

    private bool hasTriggered = false;
    private bool playerInRange = false;
    private bool playerLookingAt = false;
    private DialogSystem dialogSystem;

    private void Start()
    {
        dialogSystem = FindObjectOfType<DialogSystem>();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        CheckPlayerInteraction();

        if (CanTriggerDialog() && Input.GetKeyDown(interactionKey))
        {
            TriggerDialog();
        }
    }

    private void CheckPlayerInteraction()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerInRange = distance <= interactionDistance;

        if (requirePlayerLookAt && playerInRange)
        {
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                Vector3 directionToNPC = (transform.position - playerCamera.transform.position).normalized;
                float dot = Vector3.Dot(playerCamera.transform.forward, directionToNPC);
                playerLookingAt = dot > 0.7f; // Player is looking roughly at the NPC
            }
        }
        else
        {
            playerLookingAt = true; // Not required, so always true
        }

        bool canInteract = CanTriggerDialog();
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(canInteract);
        }
    }

    private bool CanTriggerDialog()
    {
        if (triggerOnce && hasTriggered) return false;
        if (!playerInRange) return false;
        if (requirePlayerLookAt && !playerLookingAt) return false;
        if (GameManager.Instance.GetCurrentGameState() != GameState.Playing) return false;

        return true;
    }

    public void TriggerDialog()
    {
        if (dialogSystem != null && dialogToTrigger != null)
        {
            dialogSystem.StartDialog(dialogToTrigger);

            if (triggerOnce)
                hasTriggered = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
