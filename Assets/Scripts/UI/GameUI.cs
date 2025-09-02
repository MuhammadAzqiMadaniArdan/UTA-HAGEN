using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameUI : MonoBehaviour
{
    [Header("HUD Elements")]
    public Slider greedBar;
    public Slider suspicionBar;
    public TMPro.TextMeshProUGUI storyProgressText;
    public GameObject eventNotificationPanel;
    public TMPro.TextMeshProUGUI eventNotificationText;

    [Header("Status UI")]
    public GameObject statusUI;

    [Header("Laptop Interface")]
    public GameObject laptopInterface;
    public Button closeLaptopButton;

    [Header("Laptop Applications")]
    public GameObject laptopDesktop;
    public GameObject paymentApp;
    public Button paymentAppIcon;
    public Button closePaymentAppButton;

    [Header("Corruption UI")]
    public TMPro.TextMeshProUGUI corruptionText;

    [Header("Payment Form")]
    public GameObject paymentFormContent;
    public TMPro.TMP_InputField accountNumberField;
    public TMPro.TMP_InputField amountField;
    public TMPro.TMP_InputField reasonField;
    public Button transferButton;

    [Header("Transfer Loading")]
    public GameObject transferLoadingContent;
    public Slider transferProgressBar;
    public TMPro.TextMeshProUGUI transferStatusText;
    public TMPro.TextMeshProUGUI transferSuccessText;

    [Header("Ending UI")]
    public GameObject endingPanel;                 // Panel ending
    public TMPro.TextMeshProUGUI endingTitleText;  // Judul ending
    public TMPro.TextMeshProUGUI endingDescText;   // Deskripsi ending
    public Button restartButton;                   // Tombol restart

    private void Start()
    {
        // Subscribe to game manager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGreedLevelChanged += UpdateGreedBar;
            GameManager.Instance.OnSuspicionLevelChanged += UpdateSuspicionBar;
            GameManager.Instance.OnCorruptionChanged += UpdateCorruptionUI;
            GameManager.Instance.OnGameEnded += ShowEndingUI; // listen ending
        }

        // Setup laptop interface
        if (laptopInterface != null)
            laptopInterface.SetActive(false);

        if (paymentApp != null)
            paymentApp.SetActive(false);

        if (paymentFormContent != null)
            paymentFormContent.SetActive(true);

        if (transferLoadingContent != null)
            transferLoadingContent.SetActive(false);

        if (transferSuccessText != null)
            transferSuccessText.gameObject.SetActive(false);

        if (endingPanel != null)
            endingPanel.SetActive(false);

        if (paymentAppIcon != null)
            paymentAppIcon.onClick.AddListener(OpenPaymentApp);

        if (closePaymentAppButton != null)
            closePaymentAppButton.onClick.AddListener(ClosePaymentApp);

        if (transferButton != null)
            transferButton.onClick.AddListener(ProcessTransfer);

        if (closeLaptopButton != null)
            closeLaptopButton.onClick.AddListener(CloseLaptop);
    }

    private void Update()
    {
        // Emergency close laptop (when NPC approaches)
        if (Input.GetKeyDown(KeyCode.Escape) && laptopInterface.activeInHierarchy)
        {
            CloseLaptop();
        }
    }

    private void UpdateGreedBar(float greedLevel)
    {
        if (greedBar != null)
        {
            greedBar.value = greedLevel / GameManager.Instance.maxGreedLevel;
        }
    }

    private void UpdateSuspicionBar(float suspicionLevel)
    {
        if (suspicionBar != null)
        {
            suspicionBar.value = suspicionLevel / GameManager.Instance.maxSuspicionLevel;
        }
    }

    private void UpdateCorruptionUI(float total)
    {
        if (corruptionText != null)
            corruptionText.text = $"Total Korupsi: Rp{total:N0}";
    }

    public void ShowEventNotification(string eventText)
    {
        if (eventNotificationPanel != null && eventNotificationText != null)
        {
            eventNotificationPanel.SetActive(true);
            eventNotificationText.text = eventText;

            // Auto-hide after 3 seconds
            Invoke(nameof(HideEventNotification), 3f);
        }
    }

    private void HideEventNotification()
    {
        if (eventNotificationPanel != null)
            eventNotificationPanel.SetActive(false);
    }

    private void ShowEndingUI(EndingType ending)
    {
        if (endingPanel == null) return;

        // --- Matikan semua HUD & status bar ---
        if (statusUI != null) statusUI.SetActive(false);
        if (greedBar != null) greedBar.gameObject.SetActive(false);
        if (suspicionBar != null) suspicionBar.gameObject.SetActive(false);
        if (storyProgressText != null) storyProgressText.gameObject.SetActive(false);
        if (eventNotificationPanel != null) eventNotificationPanel.SetActive(false);

        // TODO: kalau kamu punya text "Anda terdeteksi CCTV", matikan di sini juga
        GameObject detectedText = GameObject.Find("DetectedText"); // ganti nama sesuai di Hierarchy
        if (detectedText != null) detectedText.SetActive(false);

        endingPanel.SetActive(true);

        string title = "";
        string desc = "";

        switch (ending)
        {
            case EndingType.Caught:
                title = "⚠ Kamu Tertangkap!";
                desc = "Kecurigaan terlalu tinggi, aparat berhasil menangkapmu.";
                break;
            case EndingType.TooGreedy:
                title = "💰 Terlalu Serakah";
                desc = "Keserakahanmu menghancurkan segalanya.";
                break;
            case EndingType.GoodEnding:
                title = "🌟 Good Ending";
                desc = "Kamu berhasil selamat dengan bersih!";
                break;
            case EndingType.NeutralEnding:
                title = "😐 Neutral Ending";
                desc = "Kamu bertahan, tapi bukan tanpa noda.";
                break;
            case EndingType.BadEnding:
                title = "☠ Bad Ending";
                desc = "Kesalahanmu berujung fatal.";
                break;
        }

        if (endingTitleText != null) endingTitleText.text = title;
        if (endingDescText != null) endingDescText.text = desc;

        // --- Setup Restart Button ---
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        // Unlock cursor supaya tombol bisa diklik
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestartGame()
    {
        if (endingPanel != null)
            endingPanel.SetActive(false);

        // Reset HUD
        if (statusUI != null) statusUI.SetActive(true);
        if (greedBar != null) greedBar.gameObject.SetActive(true);
        if (suspicionBar != null) suspicionBar.gameObject.SetActive(true);
        if (storyProgressText != null) storyProgressText.gameObject.SetActive(true);

        // Reset state dari GameManager
        GameManager.Instance.RestartGame();

        // Reload scene biar semua object kembali fresh
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }


    public void OpenLaptop()
    {
        if (laptopInterface != null && !laptopInterface.activeInHierarchy)
        {
            laptopInterface.SetActive(true);
            if (statusUI != null)
                statusUI.SetActive(false);

            if (laptopDesktop != null)
                laptopDesktop.SetActive(true);
            if (paymentApp != null)
                paymentApp.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.ChangeGameState(GameState.Event);
        }
    }

    public void CloseLaptop()
    {
        Debug.Log(" CloseLaptop called");

        if (laptopInterface != null)
        {
            laptopInterface.SetActive(false);
            if (statusUI != null)
                statusUI.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            GameManager.Instance.ChangeGameState(GameState.Playing);

            // Notify LaptopInteraction that laptop is closed
            LaptopInteraction laptopInteraction = FindObjectOfType<LaptopInteraction>();
            if (laptopInteraction != null)
                laptopInteraction.OnLaptopClosed();
        }
    }

    public void OpenPaymentApp()
    {
        Debug.Log(" OpenPaymentApp called");
        Debug.Log(" paymentApp reference: " + (paymentApp != null ? "Not null" : "NULL"));
        Debug.Log(" laptopInterface active: " + (laptopInterface != null ? laptopInterface.activeInHierarchy.ToString() : "null"));

        if (paymentApp != null)
        {
            Transform parent = paymentApp.transform.parent;
            Debug.Log(" paymentApp parent: " + (parent != null ? parent.name + " (active: " + parent.gameObject.activeInHierarchy + ")" : "null"));

            if (laptopDesktop != null)
                laptopDesktop.SetActive(false);

            if (laptopInterface != null && !laptopInterface.activeInHierarchy)
            {
                Debug.Log(" Laptop interface was inactive, reactivating...");
                laptopInterface.SetActive(true);
            }

            paymentApp.SetActive(true);

            ResetPaymentAppState();

            Debug.Log(" paymentApp active state after: " + paymentApp.activeInHierarchy);
            Debug.Log(" laptopInterface active state after: " + (laptopInterface != null ? laptopInterface.activeInHierarchy.ToString() : "null"));
        }
        else
        {
            Debug.LogError(" paymentApp is null! Check Inspector assignment.");
        }
    }

    public void ClosePaymentApp()
    {
        Debug.Log(" ClosePaymentApp called");

        if (paymentApp != null)
        {
            paymentApp.SetActive(false);
            if (laptopDesktop != null)
                laptopDesktop.SetActive(true);

            ResetPaymentAppState();

            // Clear fields when closing app
            if (accountNumberField != null) accountNumberField.text = "";
            if (amountField != null) amountField.text = "";
            if (reasonField != null) reasonField.text = "";
        }
    }

    private void ResetPaymentAppState()
    {
        if (paymentFormContent != null)
            paymentFormContent.SetActive(true);

        if (transferLoadingContent != null)
            transferLoadingContent.SetActive(false);

        if (transferSuccessText != null)
            transferSuccessText.gameObject.SetActive(false);

        if (transferProgressBar != null)
            transferProgressBar.value = 0f;
    }

    private void ProcessTransfer()
    {
        string accountNumber = accountNumberField.text;
        string amountText = amountField.text;
        string reason = reasonField.text;

        // Validate input
        if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(amountText) || string.IsNullOrEmpty(reason))
        {
            ShowEventNotification("Semua field harus diisi!");
            return;
        }

        if (float.TryParse(amountText, out float amount))
        {
            StartTransferProcess(accountNumber, amount, reason);
        }
        else
        {
            ShowEventNotification("Jumlah tidak valid!");
        }
    }

    private void StartTransferProcess(string accountNumber, float amount, string reason)
    {
        // Hide form and show loading
        if (paymentFormContent != null)
            paymentFormContent.SetActive(false);

        if (transferLoadingContent != null)
            transferLoadingContent.SetActive(true);

        if (transferStatusText != null)
            transferStatusText.text = "Uang sedang ditransfer...";

        if (transferSuccessText != null)
            transferSuccessText.gameObject.SetActive(false);

        // Start loading coroutine
        StartCoroutine(TransferLoadingCoroutine(accountNumber, amount, reason));
    }

    private IEnumerator TransferLoadingCoroutine(string accountNumber, float amount, string reason)
    {
        float loadingTime = 3f; // 3 seconds loading time
        float elapsedTime = 0f;

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loadingTime;

            if (transferProgressBar != null)
                transferProgressBar.value = progress;

            yield return null;
        }

        // Ensure progress bar is at 100%
        if (transferProgressBar != null)
            transferProgressBar.value = 1f;

        // Process the actual transfer
        ProcessMoneyTransfer(accountNumber, amount, reason);

        // Show success message
        if (transferStatusText != null)
            transferStatusText.gameObject.SetActive(false);

        if (transferSuccessText != null)
        {
            transferSuccessText.gameObject.SetActive(true);
            transferSuccessText.text = $"Transfer Berhasil!\nRp{amount:N0} telah dikirim";
        }

        // Auto close after 2 seconds
        yield return new WaitForSeconds(2f);
        ClosePaymentApp();
    }

    private void ProcessMoneyTransfer(string accountNumber, float amount, string reason)
    {
        // Calculate greed and suspicion based on amount and frequency
        float greedIncrease = amount / 1000f; // Adjust based on your game balance
        float suspicionIncrease = 0f;

        // Higher amounts increase suspicion
        if (amount > 5000f)
            suspicionIncrease += 10f;
        else if (amount > 2000f)
            suspicionIncrease += 5f;

        // Frequent transfers increase suspicion
        // (You would track this in a more complex system)

        GameManager.Instance.AddGreed(greedIncrease);
        GameManager.Instance.AddSuspicion(suspicionIncrease);
        GameManager.Instance.AddCorruption(amount); // Tambah ke total korupsi
    }
}
