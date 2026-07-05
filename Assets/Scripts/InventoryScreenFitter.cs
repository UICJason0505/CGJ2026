using UnityEngine;

[ExecuteAlways]
public class InventoryScreenFitter : MonoBehaviour
{
    [SerializeField] private float leftPadding = 16f;
    [SerializeField] private float verticalPadding = 24f;
    [SerializeField, Range(0.1f, 1f)] private float maxScreenWidth = 0.32f;
    [SerializeField] private bool allowUpscale = false;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    private void Start()
    {
        FitToScreen();
    }

    private void OnEnable()
    {
        FitToScreen();
    }

    private void OnRectTransformDimensionsChange()
    {
        FitToScreen();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            FitToScreen();
        }
    }
#endif

    private void FitToScreen()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (rectTransform == null || rectTransform.parent is not RectTransform parentRect)
        {
            return;
        }

        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform);

        if (bounds.size.x <= 0f || bounds.size.y <= 0f)
        {
            return;
        }

        float availableHeight = Mathf.Max(1f, parentRect.rect.height - verticalPadding * 2f);
        float availableWidth = Mathf.Max(1f, parentRect.rect.width * maxScreenWidth - leftPadding);
        float scale = Mathf.Min(availableHeight / bounds.size.y, availableWidth / bounds.size.x);

        if (!allowUpscale)
        {
            scale = Mathf.Min(1f, scale);
        }

        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 0.5f);
        rectTransform.localScale = new Vector3(scale, scale, 1f);
        rectTransform.anchoredPosition = new Vector2(
            leftPadding - bounds.min.x * scale,
            -bounds.center.y * scale
        );
    }
}
