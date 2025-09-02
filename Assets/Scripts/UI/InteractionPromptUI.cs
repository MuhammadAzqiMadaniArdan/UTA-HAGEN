using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI promptText;
    public Image keyIcon;
    public CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    public float fadeSpeed = 5f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;
    
    private Vector3 originalPosition;
    private bool isVisible = false;
    
    private void Start()
    {
        originalPosition = transform.localPosition;
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        // Start invisible
        canvasGroup.alpha = 0f;
    }
    
    private void Update()
    {
        // Animate visibility
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        
        // Bob animation when visible
        if (isVisible && canvasGroup.alpha > 0.5f)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = originalPosition + Vector3.up * bobOffset;
        }
    }
    
    public void ShowPrompt(string text = "Tekan E untuk berinteraksi")
    {
        if (promptText != null)
            promptText.text = text;
        isVisible = true;
    }
    
    public void HidePrompt()
    {
        isVisible = false;
    }
    
    public void SetKeyIcon(KeyCode key)
    {
        if (keyIcon != null && promptText != null)
        {
            promptText.text = $"Tekan {key} untuk berinteraksi";
        }
    }
}
