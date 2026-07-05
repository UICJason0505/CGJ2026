using UnityEngine;
using UnityEngine.UI;

public class InventoryItemPickup : MonoBehaviour
{
    [SerializeField] private InventoryController inventory;
    [SerializeField] private Image item;
    [SerializeField] private bool destroyAfterPickup = true;

    public void Pickup()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<InventoryController>();
        }

        if (inventory == null)
        {
            return;
        }

        Sprite icon = GetPickupIcon();

        if (icon == null)
        {
            Debug.LogWarning($"[InventoryItemPickup] No sprite found on {name}.");
            return;
        }

        bool added = inventory.AddItem(icon);

        if (added && destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }

    private Sprite GetPickupIcon()
    {
        if (item != null)
        {
            return item.sprite;
        }

        Image image = GetComponent<Image>();
        if (image != null)
        {
            return image.sprite;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.sprite;
        }

        return null;
    }
}