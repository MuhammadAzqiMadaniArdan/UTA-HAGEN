using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DialogSystem : MonoBehaviour
{
    [Header("Dialog UI")]
    public GameObject dialogPanel;
    public TMPro.TextMeshProUGUI dialogText;
    public TMPro.TextMeshProUGUI speakerNameText;
    public Transform choicesParent;
    public GameObject choiceButtonPrefab;

    [Header("Typewriter Effect")]
    public float typewriterSpeed = 0.05f;
    public bool skipTypewriterOnClick = true;

    [Header("Audio")]
    public AudioSource dialogAudioSource;
    public AudioClip typingSound;
    public AudioClip choiceSelectSound;

    [Header("Selection System")]
    public bool enableMouseCursor = true;
    public bool enableKeyboardSelection = true;
    public Color selectedChoiceColor = Color.yellow;
    public Color normalChoiceColor = Color.white;

    [Header("NPC Response System")]
    public Color positiveResponseColor = Color.green;
    public Color negativeResponseColor = Color.red;
    public Color neutralResponseColor = Color.white;

    [Header("End Dialog Button")]
    public GameObject endDialogButtonPrefab;

    private DialogData currentDialog;
    private int currentNodeIndex = 0;
    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    private int selectedChoiceIndex = 0;
    private bool showingChoices = false;
    private bool showingResponse = false;

    public System.Action<DialogChoice> OnChoiceMade;
    public System.Action OnDialogEnded;
    public System.Action<DialogNode> OnNodeDisplayed;
    public System.Action<string> OnSpeakerChanged;

    private Dictionary<string, bool> dialogFlags = new Dictionary<string, bool>();
    private List<DialogChoice> dialogHistory = new List<DialogChoice>();

    private void Start()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        SetCursorState(false);
    }

    private void Update()
    {
        if (isTyping && skipTypewriterOnClick && Input.GetMouseButtonDown(0))
        {
            SkipTypewriter();
        }

        if (showingChoices && !showingResponse && enableKeyboardSelection && currentChoiceButtons.Count > 0)
        {
            HandleKeyboardSelection();
        }
    }

    private void HandleKeyboardSelection()
    {
        // Navigate up/down with arrow keys or WASD
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedChoiceIndex = (selectedChoiceIndex - 1 + currentChoiceButtons.Count) % currentChoiceButtons.Count;
            UpdateChoiceSelection();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedChoiceIndex = (selectedChoiceIndex + 1) % currentChoiceButtons.Count;
            UpdateChoiceSelection();
        }

        // Select with Enter or Space
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedChoiceIndex >= 0 && selectedChoiceIndex < currentChoiceButtons.Count)
            {
                UnityEngine.UI.Button selectedButton = currentChoiceButtons[selectedChoiceIndex].GetComponent<UnityEngine.UI.Button>();
                selectedButton.onClick.Invoke();
            }
        }

        // Number key selection (1-9)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int choiceIndex = i - 1;
                if (choiceIndex < currentChoiceButtons.Count)
                {
                    UnityEngine.UI.Button button = currentChoiceButtons[choiceIndex].GetComponent<UnityEngine.UI.Button>();
                    button.onClick.Invoke();
                }
            }
        }
    }

    private void UpdateChoiceSelection()
    {
        for (int i = 0; i < currentChoiceButtons.Count; i++)
        {
            TMPro.TextMeshProUGUI buttonText = currentChoiceButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            UnityEngine.UI.Image buttonImage = currentChoiceButtons[i].GetComponent<UnityEngine.UI.Image>();

            if (i == selectedChoiceIndex)
            {
                buttonImage.color = selectedChoiceColor;
                buttonText.fontStyle = TMPro.FontStyles.Bold;
            }
            else
            {
                buttonImage.color = normalChoiceColor;
                buttonText.fontStyle = TMPro.FontStyles.Normal;
            }
        }
    }

    private void SetCursorState(bool showCursor)
    {
        if (enableMouseCursor && showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void StartDialog(DialogData dialogData)
    {
        currentDialog = dialogData;
        currentNodeIndex = 0;

        GameManager.Instance.ChangeGameState(GameState.Dialog);
        dialogPanel.SetActive(true);

        SetCursorState(true);

        DisplayCurrentNode();
    }

    public void StartDialog(DialogData dialogData, int startNodeIndex)
    {
        currentDialog = dialogData;
        currentNodeIndex = startNodeIndex;

        GameManager.Instance.ChangeGameState(GameState.Dialog);
        dialogPanel.SetActive(true);

        SetCursorState(true);

        DisplayCurrentNode();
    }

    private void DisplayCurrentNode()
    {
        if (currentDialog == null || currentNodeIndex >= currentDialog.nodes.Count) return;

        DialogNode currentNode = currentDialog.nodes[currentNodeIndex];

        if (!CheckNodeConditions(currentNode))
        {
            // Skip to next available node or end dialog
            MoveToNextValidNode();
            return;
        }

        OnNodeDisplayed?.Invoke(currentNode);

        if (speakerNameText != null && !string.IsNullOrEmpty(currentNode.speakerName))
        {
            speakerNameText.text = currentNode.speakerName;
            OnSpeakerChanged?.Invoke(currentNode.speakerName);
        }

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentNode.dialogText));

        // Clear previous choices
        ClearChoices();

        StartCoroutine(ShowChoicesAfterTyping(currentNode));
    }

    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogText.text += letter;

            // Play typing sound
            if (dialogAudioSource != null && typingSound != null)
            {
                dialogAudioSource.PlayOneShot(typingSound, 0.1f);
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            DialogNode currentNode = currentDialog.nodes[currentNodeIndex];
            dialogText.text = currentNode.dialogText;
            isTyping = false;
        }
    }

    private IEnumerator ShowChoicesAfterTyping(DialogNode node)
    {
        yield return new WaitUntil(() => !isTyping);

        yield return new WaitUntil(() => !showingResponse);

        Debug.Log($"ShowChoicesAfterTyping: node has {node.choices.Count} choices, showingResponse = {showingResponse}");

        selectedChoiceIndex = 0;
        showingChoices = true;

        SetupChoicesLayout();

        // Create choice buttons
        int choiceNumber = 1;
        foreach (DialogChoice choice in node.choices)
        {
            if (!CheckChoiceConditions(choice)) continue;

            GameObject choiceButton = Instantiate(choiceButtonPrefab, choicesParent);
            TMPro.TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            buttonText.text = choice.choiceText;

            RectTransform buttonRect = choiceButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(400f, 50f); // Fixed width and height
            }

            if (choice.greedEffect > 0)
                buttonText.color = Color.red;
            else if (choice.greedEffect < 0)
                buttonText.color = Color.green;

            // Add click listener
            UnityEngine.UI.Button button = choiceButton.GetComponent<UnityEngine.UI.Button>();
            DialogChoice capturedChoice = choice;
            button.onClick.AddListener(() => MakeChoice(capturedChoice));

            currentChoiceButtons.Add(choiceButton);
            choiceNumber++;
        }

        Debug.Log($"Created {currentChoiceButtons.Count} choice buttons");

        if (currentChoiceButtons.Count == 0)
        {
            Debug.Log("No choices available, showing end dialog button");
            CreateEndDialogButton();
            showingChoices = false;
            yield break;
        }

        if (currentChoiceButtons.Count > 0)
        {
            UpdateChoiceSelection();
        }
    }

    private void CreateEndDialogButton()
    {
        if (endDialogButtonPrefab == null)
        {
            // Create a simple end button if no prefab is assigned
            GameObject endButton = Instantiate(choiceButtonPrefab, choicesParent);
            TMPro.TextMeshProUGUI buttonText = endButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            buttonText.text = "Continue...";
            buttonText.color = Color.cyan;

            RectTransform buttonRect = endButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(400f, 50f);
            }

            UnityEngine.UI.Button button = endButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => EndDialog());

            currentChoiceButtons.Add(endButton);
        }
        else
        {
            GameObject endButton = Instantiate(endDialogButtonPrefab, choicesParent);
            UnityEngine.UI.Button button = endButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => EndDialog());

            currentChoiceButtons.Add(endButton);
        }

        Debug.Log("Created end dialog button");
    }

    public void MakeChoice(DialogChoice choice)
    {
        showingChoices = false;

        if (dialogAudioSource != null && choiceSelectSound != null)
        {
            dialogAudioSource.PlayOneShot(choiceSelectSound);
        }

        dialogHistory.Add(choice);

        OnChoiceMade?.Invoke(choice);

        // Apply choice effects
        GameManager.Instance.AddGreed(choice.greedEffect);
        GameManager.Instance.AddSuspicion(choice.suspicionEffect);

        if (!string.IsNullOrEmpty(choice.flagToSet))
        {
            SetDialogFlag(choice.flagToSet, true);
        }

        if (!string.IsNullOrEmpty(choice.customAction))
        {
            ExecuteCustomAction(choice.customAction);
        }

        if (!string.IsNullOrEmpty(choice.npcResponseText))
        {
            StartCoroutine(ShowNPCResponse(choice));
        }
        else
        {
            // Move to next node or end dialog immediately
            ContinueAfterChoice(choice);
        }
    }

    private IEnumerator ShowNPCResponse(DialogChoice choice)
    {
        showingResponse = true;

        // Hide choice buttons
        ClearChoices();

        // Tampilkan panel dialog npc di dialog utama
        dialogText.text = choice.npcResponseText;

        // Warnai sesuai efek choice
        if (choice.greedEffect > 0 || choice.suspicionEffect > 0)
            dialogText.color = negativeResponseColor;
        else if (choice.greedEffect < 0 || choice.suspicionEffect < 0)
            dialogText.color = positiveResponseColor;
        else
            dialogText.color = neutralResponseColor;

        // Play response sound
        if (dialogAudioSource != null && choice.responseSound != null)
        {
            dialogAudioSource.PlayOneShot(choice.responseSound);
        }

        // Wait for specified time or until player clicks
        float timer = 0f;
        while (timer < choice.responseDisplayTime)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

       

        showingResponse = false;

        // Continue with dialog flow
        ContinueAfterChoice(choice);
    }

    private void ContinueAfterChoice(DialogChoice choice)
    {
        Debug.Log($"ContinueAfterChoice: nextNodeIndex = {choice.nextNodeIndex}, showingResponse = {showingResponse}");

        // Move to next node or end dialog
        if (choice.nextNodeIndex >= 0 && choice.nextNodeIndex < currentDialog.nodes.Count)
        {
            currentNodeIndex = choice.nextNodeIndex;
            StartCoroutine(DelayedDisplayNode());
        }
        else
        {
            EndDialog();
        }
    }

    private IEnumerator DelayedDisplayNode()
    {
        yield return new WaitForEndOfFrame(); // Wait one frame to ensure UI is reset
        Debug.Log($"Displaying node {currentNodeIndex}, showingResponse = {showingResponse}");
        DisplayCurrentNode();
    }

    private bool CheckNodeConditions(DialogNode node)
    {
        if (node.requiredFlags == null || node.requiredFlags.Count == 0) return true;

        foreach (string flag in node.requiredFlags)
        {
            if (!GetDialogFlag(flag)) return false;
        }

        return true;
    }

    private bool CheckChoiceConditions(DialogChoice choice)
    {
        Debug.Log($"Checking choice: '{choice.choiceText}'");
        Debug.Log($"Player Greed: {GameManager.Instance.GetGreed()}, Required: {choice.minGreedRequired}");
        Debug.Log($"Player Suspicion: {GameManager.Instance.GetSuspicion()}, Max Allowed: {choice.maxSuspicionAllowed}");

        // Check greed/suspicion requirements
        if (choice.minGreedRequired > 0 && GameManager.Instance.GetGreed() < choice.minGreedRequired)
        {
            Debug.Log($"Choice filtered out: Greed too low ({GameManager.Instance.GetGreed()} < {choice.minGreedRequired})");
            return false;
        }

        if (choice.maxSuspicionAllowed >= 0 && GameManager.Instance.GetSuspicion() > choice.maxSuspicionAllowed)
        {
            Debug.Log($"Choice filtered out: Suspicion too high ({GameManager.Instance.GetSuspicion()} > {choice.maxSuspicionAllowed})");
            return false;
        }

        // Check required flags
        if (choice.requiredFlags != null && choice.requiredFlags.Count > 0)
        {
            foreach (string flag in choice.requiredFlags)
            {
                if (!GetDialogFlag(flag))
                {
                    Debug.Log($"Choice filtered out: Missing required flag '{flag}'");
                    return false;
                }
            }
        }

        Debug.Log($"Choice passed all conditions: '{choice.choiceText}'");
        return true;
    }

    private void MoveToNextValidNode()
    {
        for (int i = currentNodeIndex + 1; i < currentDialog.nodes.Count; i++)
        {
            if (CheckNodeConditions(currentDialog.nodes[i]))
            {
                currentNodeIndex = i;
                DisplayCurrentNode();
                return;
            }
        }

        // No valid nodes found, end dialog
        EndDialog();
    }

    private void ExecuteCustomAction(string action)
    {
        switch (action.ToLower())
        {
            case "increase_money":
                // Custom logic for increasing money
                break;
            case "trigger_event":
                // Trigger specific game event
                break;
            case "change_npc_behavior":
                // Modify NPC behavior
                break;
            default:
                Debug.LogWarning($"Unknown custom action: {action}");
                break;
        }
    }

    public void SetDialogFlag(string flagName, bool value)
    {
        dialogFlags[flagName] = value;
    }

    public bool GetDialogFlag(string flagName)
    {
        return dialogFlags.ContainsKey(flagName) && dialogFlags[flagName];
    }

    public List<DialogChoice> GetDialogHistory()
    {
        return new List<DialogChoice>(dialogHistory);
    }

    public bool WasChoiceMade(string choiceText)
    {
        return dialogHistory.Exists(choice => choice.choiceText == choiceText);
    }

    private void ClearChoices()
    {
        foreach (GameObject button in currentChoiceButtons)
        {
            Destroy(button);
        }
        currentChoiceButtons.Clear();
    }

    private void EndDialog()
    {
        dialogPanel.SetActive(false);
        GameManager.Instance.ChangeGameState(GameState.Playing);
        OnDialogEnded?.Invoke();

        SetCursorState(false);
        showingChoices = false;

        currentDialog = null;
        ClearChoices();
    }

    private void SetupChoicesLayout()
    {
        if (choicesParent == null) return;

        // Add Vertical Layout Group if not present
        UnityEngine.UI.VerticalLayoutGroup layoutGroup = choicesParent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = choicesParent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        }

        // Configure layout settings
        layoutGroup.spacing = 10f; // Space between buttons
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        // Add Content Size Fitter if not present
        UnityEngine.UI.ContentSizeFitter sizeFitter = choicesParent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = choicesParent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        }

        sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
    }
}

[System.Serializable]
public class DialogData
{
    public string dialogName;
    public List<DialogNode> nodes = new List<DialogNode>();
}

[System.Serializable]
public class DialogNode
{
    public string dialogText;
    public string speakerName; // Added speaker name
    public List<DialogChoice> choices = new List<DialogChoice>();
    public List<string> requiredFlags = new List<string>(); // Added condition system
}

[System.Serializable]
public class DialogChoice
{
    public string choiceText;
    public int nextNodeIndex = -1;
    public float greedEffect = 0f;
    public float suspicionEffect = 0f;

    [Header("NPC Response")]
    public string npcResponseText = "";        // Response NPC gives after choice
    public float responseDisplayTime = 2f;     // How long to show response
    public AudioClip responseSound;            // Optional sound for response

    public string flagToSet = "";
    public string customAction = "";
    public List<string> requiredFlags = new List<string>();
    public float minGreedRequired = 0f;
    public float maxSuspicionAllowed = -1f; // -1 means no limit
}
