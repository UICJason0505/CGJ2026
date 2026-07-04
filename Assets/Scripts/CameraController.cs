using UnityEngine;

public class MaskCameraController : MonoBehaviour
{
    public GameObject maskObject;
    public RectTransform maskRect;

    public RectTransform referenceBackground;
    public RectTransform hiddenImage;

    public Canvas canvas;

    public bool startOpen = false;

    public Vector2 xLimit = new Vector2(-800f, 800f);
    public Vector2 yLimit = new Vector2(-450f, 450f);

    private Camera uiCamera;

    void Start()
    {
        if (maskRect == null && maskObject != null)
        {
            maskRect = maskObject.GetComponent<RectTransform>();
        }

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        if (maskObject != null)
        {
            maskObject.SetActive(startOpen);
        }

        InitHiddenImage();
        MatchHiddenImageToBackground();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleMask();
        }

        if (maskObject != null && maskObject.activeSelf)
        {
            FollowMouse();
        }
    }

    void LateUpdate()
    {
        if (maskObject != null && maskObject.activeSelf)
        {
            MatchHiddenImageToBackground();
        }
    }

    void ToggleMask()
    {
        if (maskObject == null)
        {
            return;
        }

        maskObject.SetActive(!maskObject.activeSelf);

        if (maskObject.activeSelf)
        {
            InitHiddenImage();
            MatchHiddenImageToBackground();
        }
    }

    void FollowMouse()
    {
        if (maskRect == null)
        {
            return;
        }

        RectTransform parentRect = maskRect.parent as RectTransform;

        if (parentRect == null)
        {
            return;
        }

        Vector2 localMousePos;

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            Input.mousePosition,
            uiCamera,
            out localMousePos
        );

        if (!success)
        {
            return;
        }

        localMousePos.x = Mathf.Clamp(localMousePos.x, xLimit.x, xLimit.y);
        localMousePos.y = Mathf.Clamp(localMousePos.y, yLimit.x, yLimit.y);

        maskRect.anchoredPosition = localMousePos;
    }

    void InitHiddenImage()
    {
        if (hiddenImage == null || referenceBackground == null)
        {
            return;
        }

        hiddenImage.anchorMin = new Vector2(0.5f, 0.5f);
        hiddenImage.anchorMax = new Vector2(0.5f, 0.5f);
        hiddenImage.pivot = new Vector2(0.5f, 0.5f);

        hiddenImage.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            referenceBackground.rect.width
        );

        hiddenImage.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            referenceBackground.rect.height
        );
    }

    void MatchHiddenImageToBackground()
    {
        if (hiddenImage == null || referenceBackground == null)
        {
            return;
        }

        hiddenImage.position = referenceBackground.position;
        hiddenImage.rotation = referenceBackground.rotation;

        Vector3 parentScale = hiddenImage.parent.lossyScale;
        Vector3 backgroundScale = referenceBackground.lossyScale;

        hiddenImage.localScale = new Vector3(
            backgroundScale.x / parentScale.x,
            backgroundScale.y / parentScale.y,
            backgroundScale.z / parentScale.z
        );

        hiddenImage.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            referenceBackground.rect.width
        );

        hiddenImage.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            referenceBackground.rect.height
        );
    }
}