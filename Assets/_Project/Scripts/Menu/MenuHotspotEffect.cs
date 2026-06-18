using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MenuHotspotEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Image targetImage;
    [SerializeField] private Vector2 hoverOffset = new Vector2(5f, 0f);
    [SerializeField] private float hoverScale = 1.012f;
    [SerializeField] private float pressedScale = 0.992f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Color hoverColor = new Color(0.25f, 0.56f, 1f, 0.12f);
    [SerializeField] private Color pressedColor = new Color(0.38f, 0.72f, 1f, 0.18f);

    private Vector2 initialPosition;
    private Vector3 initialScale;
    private Vector2 desiredPosition;
    private Vector3 desiredScale;
    private Color initialColor;
    private Color desiredColor;
    private bool isHovered;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (target == null)
            return;

        initialPosition = target.anchoredPosition;
        initialScale = target.localScale;
        desiredPosition = initialPosition;
        desiredScale = initialScale;
        if (targetImage != null)
        {
            initialColor = targetImage.color;
            desiredColor = initialColor;
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);
        target.anchoredPosition = Vector2.Lerp(target.anchoredPosition, desiredPosition, t);
        target.localScale = Vector3.Lerp(target.localScale, desiredScale, t);
        if (targetImage != null)
            targetImage.color = Color.Lerp(targetImage.color, desiredColor, t);
    }

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true);
    public void OnPointerExit(PointerEventData eventData) => SetHover(false);
    public void OnSelect(BaseEventData eventData) => SetHover(true);
    public void OnDeselect(BaseEventData eventData) => SetHover(false);

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target != null)
            desiredScale = initialScale * pressedScale;

        if (targetImage != null)
            desiredColor = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (target != null)
            desiredScale = initialScale * (isHovered ? hoverScale : 1f);

        if (targetImage != null)
            desiredColor = isHovered ? hoverColor : initialColor;
    }

    private void SetHover(bool hovered)
    {
        if (target == null)
            return;

        isHovered = hovered;
        desiredPosition = hovered ? initialPosition + hoverOffset : initialPosition;
        desiredScale = hovered ? initialScale * hoverScale : initialScale;
        if (targetImage != null)
            desiredColor = hovered ? hoverColor : initialColor;
    }
}
