using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPickupRaycaster : MonoBehaviour
{
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private float raycastDistance = 100f;

    private readonly List<RaycastResult> uiResults = new List<RaycastResult>();

    private void Awake()
    {
        if (raycastCamera == null)
        {
            raycastCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickupAtMouse();
        }
    }

    public void TryPickupAtMouse()
    {
        if (TryPickupUI())
        {
            return;
        }

        if (raycastCamera == null)
        {
            return;
        }

        Vector2 worldPoint = raycastCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit2D = Physics2D.OverlapPoint(worldPoint, pickupLayer);

        if (hit2D != null && TryPickup(hit2D.gameObject))
        {
            return;
        }

        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit3D, raycastDistance, pickupLayer))
        {
            TryPickup(hit3D.collider.gameObject);
        }
    }

    private bool TryPickupUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        uiResults.Clear();
        EventSystem.current.RaycastAll(pointerData, uiResults);

        for (int i = 0; i < uiResults.Count; i++)
        {
            GameObject target = uiResults[i].gameObject;

            if (!IsInPickupLayer(target))
            {
                continue;
            }

            if (TryPickup(target))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryPickup(GameObject target)
    {
        InventoryItemPickup pickup = target.GetComponentInParent<InventoryItemPickup>();

        if (pickup == null)
        {
            return false;
        }

        pickup.Pickup();
        return true;
    }

    private bool IsInPickupLayer(GameObject target)
    {
        return (pickupLayer.value & (1 << target.layer)) != 0;
    }
}