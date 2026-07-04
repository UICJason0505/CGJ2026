using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockWorldClickHandler : MonoBehaviour
{
    private NumberPadController controller;

    public void Initialize(NumberPadController padController)
    {
        controller = padController;
    }

    private void OnMouseDown()
    {
        controller?.ShowPasswordLock();
    }
}
