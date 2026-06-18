using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimationEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Image targetImage;
    private Outline targetOutline;
    private Text targetText;
    private Color originalImageColor;
    private Color originalOutlineColor;
    private Color originalTextColor;
    private bool isHovered;
    private bool isPressed;

    [Header("Movimento")]
    public Vector2 hoverOffset = new Vector2(8f, 0f);
    public float hoverScaleMultiplier = 1.02f;
    public float clickScaleMultiplier = 0.985f;
    public float animationSpeed = 10f;
    public float idlePulseAmount = 0.0035f;
    public float idlePulseSpeed = 2.2f;

    [Header("Configurações Visuais")]
    public Color hoverTint = new Color(0.28f, 0.20f, 0.13f, 1f);
    public Color pressedTint = new Color(0.22f, 0.16f, 0.11f, 1f);
    public Color hoverOutlineTint = new Color(0.95f, 0.75f, 0.34f, 1f);
    public Color pressedOutlineTint = new Color(0.70f, 0.53f, 0.22f, 1f);
    public Color hoverTextTint = new Color(1f, 0.94f, 0.72f, 1f);
    public Color pressedTextTint = new Color(1f, 0.90f, 0.66f, 1f);

    private Vector3 targetScale;
    private Vector2 targetPosition;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        originalScale = transform.localScale;
        targetScale = originalScale;
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
            targetPosition = originalPosition;
        }

        targetImage = GetComponent<Image>();
        targetOutline = GetComponent<Outline>();
        targetText = GetComponentInChildren<Text>();

        if (targetImage != null)
            originalImageColor = targetImage.color;

        if (targetOutline != null)
            originalOutlineColor = targetOutline.effectColor;

        if (targetText != null)
            originalTextColor = targetText.color;
    }

    private void Update()
    {
        float pulse = (!isHovered && !isPressed) ? 1f + Mathf.Sin(Time.unscaledTime * idlePulseSpeed) * idlePulseAmount : 1f;
        Vector3 animatedScale = targetScale * pulse;
        float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, animatedScale, t);

        if (rectTransform != null)
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, t);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        isPressed = false;
        targetScale = originalScale * hoverScaleMultiplier;
        targetPosition = originalPosition + hoverOffset;
        ApplyVisualState(hoverTint, hoverOutlineTint, hoverTextTint);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        targetScale = originalScale;
        targetPosition = originalPosition;
        ApplyVisualState(originalImageColor, originalOutlineColor, originalTextColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = originalScale * clickScaleMultiplier;
        targetPosition = originalPosition + new Vector2(4f, -1.5f);
        ApplyVisualState(pressedTint, pressedOutlineTint, pressedTextTint);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;
        targetPosition = isHovered ? originalPosition + hoverOffset : originalPosition;
        ApplyVisualState(
            isHovered ? hoverTint : originalImageColor,
            isHovered ? hoverOutlineTint : originalOutlineColor,
            isHovered ? hoverTextTint : originalTextColor);
    }

    private void ApplyVisualState(Color imageColor, Color outlineColor, Color textColor)
    {
        if (targetImage != null)
            targetImage.color = imageColor;

        if (targetOutline != null)
            targetOutline.effectColor = outlineColor;

        if (targetText != null)
            targetText.color = textColor;
    }
}
