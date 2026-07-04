using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject cameraMaskObject;
    public RectTransform cameraMaskRect;
    public Canvas canvas;

    public bool startOpen = false;

    public Vector2 xLimit = new Vector2(-800f, 800f);
    public Vector2 yLimit = new Vector2(-450f, 450f);

    void Start()
    {
        if (cameraMaskObject != null)
        {
            cameraMaskObject.SetActive(startOpen);
        }

        if (cameraMaskRect == null && cameraMaskObject != null)
        {
            cameraMaskRect = cameraMaskObject.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleCameraMask();
        }

        if (cameraMaskObject != null && cameraMaskObject.activeSelf)
        {
            FollowMouseUI();
        }
    }

    void ToggleCameraMask()
    {
        if (cameraMaskObject == null)
        {
            return;
        }

        cameraMaskObject.SetActive(!cameraMaskObject.activeSelf);
    }

    void FollowMouseUI()
    {
        if (cameraMaskRect == null)
        {
            return;
        }

        RectTransform parentRect = cameraMaskRect.parent as RectTransform;

        if (parentRect == null)
        {
            return;
        }

        Camera uiCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
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

        cameraMaskRect.anchoredPosition = localMousePos;
    }
}