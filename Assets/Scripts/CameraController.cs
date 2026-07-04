using UnityEngine;
using UnityEngine.UI;

public class CameraCanvasMaskController : MonoBehaviour
{
    [System.Serializable]
    public struct CameraData
    {
        public float xminus;
        public float xplus;
        public float moveSpeed;
    }

    public CameraData data;

    public Camera mainCamera;

    public GameObject canvasToToggle;

    public bool pauseGameWhenCanvasOpen = true;

    public RectTransform maskImage;

    public RectTransform rawImage;

    private Vector3 cameraStartPos;
    private Vector2 rawImageStartAnchoredPos;

    void Start()
    {
        canvasToToggle.SetActive(false);
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            cameraStartPos = mainCamera.transform.position;
        }

        if (rawImage != null)
        {
            rawImageStartAnchoredPos = rawImage.anchoredPosition;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && canvasToToggle != null)
        {
            canvasToToggle.SetActive(!canvasToToggle.activeSelf);
        }

        bool isCanvasOpen = canvasToToggle == null || canvasToToggle.activeSelf;

        if (pauseGameWhenCanvasOpen && canvasToToggle != null)
        {
            Time.timeScale = isCanvasOpen ? 0f : 1f;
        }

        MoveCamera();

        KeepRawImageVisuallyStill();
    }

    void MoveCamera()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 pos = mainCamera.transform.position;

        if (Input.GetKey(KeyCode.A))
        {
            pos.x -= data.moveSpeed * Time.unscaledDeltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            pos.x += data.moveSpeed * Time.unscaledDeltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, data.xminus, data.xplus);

        mainCamera.transform.position = pos;
    }

    void KeepRawImageVisuallyStill()
    {
        if (mainCamera == null || rawImage == null)
        {
            return;
        }

        float cameraMoveX = mainCamera.transform.position.x - cameraStartPos.x;

        float uiMoveX = cameraMoveX * 100f;

        rawImage.anchoredPosition = rawImageStartAnchoredPos - new Vector2(uiMoveX, 0f);
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}