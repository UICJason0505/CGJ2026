using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlotUI
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private GameObject selectedFrame;

    public bool IsEmpty { get; private set; } = true;
    public Sprite Icon { get; private set; }

    public bool Add(Sprite icon)
    {
        if (icon == null || !IsEmpty)
        {
            return false;
        }

        Icon = icon;
        IsEmpty = false;
        Refresh();
        return true;
    }

    public bool Remove()
    {
        if (IsEmpty)
        {
            return false;
        }

        Clear();
        return true;
    }

    public void Clear()
    {
        Icon = null;
        IsEmpty = true;
        Refresh();
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }
    }

    public void Refresh()
    {
        if (itemIconImage != null)
        {
            itemIconImage.enabled = !IsEmpty && Icon != null;
            itemIconImage.sprite = IsEmpty ? null : Icon;
            itemIconImage.preserveAspect = true;
        }
    }
}