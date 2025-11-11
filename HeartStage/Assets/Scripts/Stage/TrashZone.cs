using UnityEngine;
using UnityEngine.EventSystems;

public class TrashZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 소스가 슬롯이었다면 원 슬롯 비우기
        var slot = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<DraggableSlot>() : null;
        if (slot && slot.characterData != null)
        {
            var leaving = slot.characterData;
            slot.ClearSlotAndUnlockSource(leaving); // 네가 만든 비우기 메서드 호출
        }
        // DragMe에서 온 건 단순 무시(원본은 그대로)
    }
}
