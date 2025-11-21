using UnityEngine;
using UnityEngine.EventSystems;

public class SlotRaycastProxy : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private DraggableSlot owner;

    private void Awake() => ResolveOwner();
    private void OnEnable() => ResolveOwner();

    private void ResolveOwner()
    {
        if (owner == null)
            owner = GetComponentInChildren<DraggableSlot>(true);
    }

    private DraggableSlot Owner
    {
        get { if (owner == null) ResolveOwner(); return owner; }
    }

    // ★ 부모 자신이 레이캐스트로 맞았을 때만 전달
    private bool IsHitSelf(PointerEventData e)
        => e != null && e.pointerCurrentRaycast.gameObject == gameObject;

    public void OnDrop(PointerEventData e)
    {
        if (!IsHitSelf(e)) return;
        Owner?.OnDrop(e);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!IsHitSelf(e)) return;
        Owner?.OnPointerEnter(e);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!IsHitSelf(e)) return;
        Owner?.OnPointerExit(e);
    }
}
