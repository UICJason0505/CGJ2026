using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventorySlotUI[] slots;
    [SerializeField] private int selectedIndex;

    public Sprite SelectedIcon
    {
        get
        {
            if (selectedIndex < 0 || selectedIndex >= slots.Length)
            {
                return null;
            }

            return slots[selectedIndex].Icon;
        }
    }

    private void Awake()
    {
        RefreshAll();
        SelectSlot(selectedIndex);
    }

    public bool AddItem(Sprite icon)
    {
        if (icon == null)
        {
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty && slots[i].Add(icon))
            {
                return true;
            }
        }

        Debug.LogWarning("[Inventory] Not enough space.");
        return false;
    }

    public bool RemoveSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length)
        {
            return false;
        }

        return slots[selectedIndex].Remove();
    }

    public void SelectSlot(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, slots.Length - 1));

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetSelected(i == selectedIndex);
        }
    }

    public void SelectNextSlot()
    {
        if (slots.Length == 0)
        {
            return;
        }

        SelectSlot((selectedIndex + 1) % slots.Length);
    }

    public void SelectPreviousSlot()
    {
        if (slots.Length == 0)
        {
            return;
        }

        SelectSlot((selectedIndex - 1 + slots.Length) % slots.Length);
    }

    public void ClearAll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Clear();
        }
    }

    public void RefreshAll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Refresh();
        }
    }
}