using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoverOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color hoverColor = Color.white;

    private Image _image;
    private Color _originalColor;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image == null)
            _image = GetComponentInChildren<Image>();

        if (_image != null)
        {
            _originalColor = _image.color;
            try { _image.alphaHitTestMinimumThreshold = 0.5f; }
            catch (System.Exception) { /* 需要 Sprite 开 Read/Write Enabled */ }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_image != null)
            _image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_image != null)
            _image.color = _originalColor;
    }
}
