using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // drag
    public bool dragOnSurfaces = true;
    private readonly Dictionary<int, GameObject> m_DraggingIcons = new Dictionary<int, GameObject>();
    private readonly Dictionary<int, RectTransform> m_DraggingPlanes = new Dictionary<int, RectTransform>();

    // drop
    public Image containerImage;
    public Image receivingImage;
    private Color normalColor;
    public Color highlightColor = Color.yellow;
    public CharacterData characterData;


    // ---------- Drag (슬롯 자체도 드래그 가능하도록) ----------
    public void OnBeginDrag(PointerEventData eventData)
    {
        var canvas = FindInParents<Canvas>(gameObject);
        if (canvas == null) return;
        // 캐릭터 올려놓지 않으면 드래그 불가
        if (characterData == null) return;

        var icon = new GameObject("icon");
        m_DraggingIcons[eventData.pointerId] = icon;

        icon.transform.SetParent(canvas.transform, false);
        icon.transform.SetAsLastSibling();

        var image = icon.AddComponent<Image>();
        // 슬롯에 표시된 이미지 우선 사용
        var src = receivingImage != null ? receivingImage : GetComponent<Image>();
        if (src != null) image.sprite = src.sprite;
        // image.SetNativeSize();
        image.raycastTarget = false;

        var group = icon.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        if (dragOnSurfaces)
            m_DraggingPlanes[eventData.pointerId] = transform as RectTransform;
        else
            m_DraggingPlanes[eventData.pointerId] = canvas.transform as RectTransform;

        SetDraggedPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_DraggingIcons.TryGetValue(eventData.pointerId, out var icon) && icon != null)
            SetDraggedPosition(eventData);
    }

    private void SetDraggedPosition(PointerEventData eventData)
    {
        if (dragOnSurfaces && eventData.pointerEnter != null && eventData.pointerEnter.transform is RectTransform)
            m_DraggingPlanes[eventData.pointerId] = eventData.pointerEnter.transform as RectTransform;

        var rt = m_DraggingIcons[eventData.pointerId].GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                m_DraggingPlanes[eventData.pointerId],
                eventData.position,
                eventData.pressEventCamera,
                out var globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = m_DraggingPlanes[eventData.pointerId].rotation;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_DraggingIcons.TryGetValue(eventData.pointerId, out var icon) && icon != null)
            Destroy(icon);

        m_DraggingIcons.Remove(eventData.pointerId);  // <- null 넣지 말고 제거
        m_DraggingPlanes.Remove(eventData.pointerId);
    }

    public static T FindInParents<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        var comp = go.GetComponent<T>();
        if (comp != null) return comp;

        var t = go.transform.parent;
        while (t != null && comp == null)
        {
            comp = t.gameObject.GetComponent<T>();
            t = t.parent;
        }
        return comp;
    }

    // ---------- Drop ----------
    private void OnEnable()
    {
        if (containerImage != null)
            normalColor = containerImage.color;
    }

    public void OnDrop(PointerEventData data)
    {
        if (containerImage != null) containerImage.color = normalColor;
        if (!TryGetDropPayload(data, out var dropSprite, out var droppedCD)) return;

        // (1) 먼저 DragMe -> Slot 케이스
        bool fromDragMe = data.pointerDrag != null && data.pointerDrag.GetComponent<DragMe>() != null;
        if (fromDragMe)
        {
            // 실제 배치
            if (receivingImage != null && dropSprite != null) receivingImage.sprite = dropSprite;
            if (droppedCD != null) characterData = droppedCD;

            // DragMe 잠금 및 레이캐스트 차단
            var src = data.pointerDrag.GetComponent<DragMe>();
            if (src != null)
            {
                var img = src.GetComponent<Image>();
                if (img) img.raycastTarget = false;
                DragSourceRegistry.Register(characterData, src);
            }

            if (characterData != null) Debug.Log($"{characterData.ch_name}");
            return;
        }

        // (2) Slot -> Slot 케이스: 자리 교환 또는 이동
        var sourceSlot = data.pointerDrag != null ? data.pointerDrag.GetComponent<DraggableSlot>() : null;
        if (sourceSlot != null)
        {
            HandleDropFromSlot(sourceSlot);
            if (characterData != null) Debug.Log($"{characterData.ch_name}");
        }
    }


    public void OnPointerEnter(PointerEventData data)
    {
        if (containerImage == null) return;
        if (TryGetDropPayload(data, out _, out _))
            containerImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (containerImage == null) return;
        containerImage.color = normalColor;
    }

    /// <summary>
    /// DragMe에서 끌어오든, 다른 DraggableSlot에서 끌어오든 공통으로 Sprite/CharacterData를 얻는다.
    /// </summary>
    private bool TryGetDropPayload(PointerEventData data, out Sprite sprite, out CharacterData cd)
    {
        sprite = null;
        cd = null;

        var srcObj = data?.pointerDrag;
        if (srcObj == null) return false;

        // 1) DragMe에서 끌어온 경우
        var dragMe = srcObj.GetComponent<DragMe>();
        if (dragMe != null)
        {
            var img = srcObj.GetComponent<Image>();
            if (img != null) sprite = img.sprite;
            cd = dragMe.characterData;
            return (sprite != null || cd != null);
        }

        // 2) 다른 DraggableSlot에서 끌어온 경우 (슬롯 → 슬롯)
        var slot = srcObj.GetComponent<DraggableSlot>();
        if (slot != null)
        {
            var img = slot.receivingImage != null ? slot.receivingImage : slot.GetComponent<Image>();
            if (img != null) sprite = img.sprite;
            cd = slot.characterData;
            return (sprite != null || cd != null);
        }

        return false;
    }

    public void ClearSlotAndUnlockSource(CharacterData leaving)
    {
        if (leaving == null) return;

        // 슬롯 비우기
        if (receivingImage) receivingImage.sprite = null;
        if (characterData == leaving) characterData = null;

        // DragMe 해제
        var src = DragSourceRegistry.GetSource(leaving);
        if (src != null)
        {
            var img = src.GetComponent<Image>();
            if (img)
            {
                img.raycastTarget = true;
            }
        }
    }
    private Sprite GetSlotSprite(DraggableSlot slot)
    {
        var img = slot.receivingImage != null ? slot.receivingImage : slot.GetComponent<Image>();
        return img != null ? img.sprite : null;
    }

    // 슬롯의 표시 스프라이트 설정
    private void SetSlotSprite(DraggableSlot slot, Sprite s)
    {
        var img = slot.receivingImage != null ? slot.receivingImage : slot.GetComponent<Image>();
        if (img != null) img.sprite = s;
    }

    // Slot -> Slot 드롭 처리 (스왑/이동)
    private void HandleDropFromSlot(DraggableSlot source)
    {
        if (source == null || source == this) return;        // 같은 슬롯이면 무시
        if (source.characterData == null) return;            // 소스 비었으면 무시

        var srcCD = source.characterData;
        var srcSprite = GetSlotSprite(source);

        if (this.characterData == null)
        {
            // ------- 이동 (타깃 빈칸) -------
            SetSlotSprite(this, srcSprite);
            this.characterData = srcCD;

            // 원 슬롯 비우기 (DragMe 해제는 삭제일 때만, 이동은 해제하지 않음)
            SetSlotSprite(source, null);
            source.characterData = null;
        }
        else
        {
            // ------- 스왑 (서로 데이터 있음) -------
            var dstCD = this.characterData;
            var dstSprite = GetSlotSprite(this);

            // 소스 -> 타깃
            SetSlotSprite(this, srcSprite);
            this.characterData = srcCD;

            // 타깃(원래 것) -> 소스
            SetSlotSprite(source, dstSprite);
            source.characterData = dstCD;
        }
    }
}
