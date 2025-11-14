using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool dragOnSurfaces = true;
    public CharacterData characterData;

    [SerializeField] private float directionThreshold = 10f;

    // 이번 드래그가 “세로 드래그”였는지 여부 (DraggableSlot에서 사용)
    public bool IsVerticalDrag { get; private set; }

    private readonly Dictionary<int, GameObject> m_DraggingIcons = new();
    private readonly Dictionary<int, RectTransform> m_DraggingPlanes = new();

    private enum DragDirection { Undecided, Horizontal, Vertical }
    private readonly Dictionary<int, DragDirection> m_Directions = new();
    private readonly Dictionary<int, Vector2> m_StartPointerPos = new();
    private readonly Dictionary<int, bool> m_ScrollDragging = new();

    private ScrollRect parentScrollRect;

    void Awake()
    {
        parentScrollRect = FindInParents<ScrollRect>(gameObject);

        if (characterData != null)
            DragSourceRegistry.Register(characterData, this);
    }

    void OnDestroy()
    {
        if (characterData != null)
            DragSourceRegistry.Unregister(characterData, this);
    }

    public void RebindCharacter(CharacterData cd)
    {
        if (characterData == cd)
            return;

        if (characterData != null)
            DragSourceRegistry.Unregister(characterData, this);

        characterData = cd;

        if (characterData != null)
            DragSourceRegistry.Register(characterData, this);
    }

    // ================= DRAG =================

    public void OnBeginDrag(PointerEventData eventData)
    {
        int id = eventData.pointerId;

        IsVerticalDrag = false;                        // 새 드래그 시작마다 초기화
        m_StartPointerPos[id] = eventData.position;
        m_Directions[id] = DragDirection.Undecided;
        m_ScrollDragging[id] = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        int id = eventData.pointerId;

        if (!m_Directions.TryGetValue(id, out var dir))
            return;

        // 방향 아직 결정 안 된 상태면 먼저 판정
        if (dir == DragDirection.Undecided)
        {
            var startPos = m_StartPointerPos[id];
            var delta = eventData.position - startPos;

            if (delta.sqrMagnitude < directionThreshold * directionThreshold)
                return; // 아직 너무 안 움직임

            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                // ===== 세로 드래그 확정 =====
                m_Directions[id] = DragDirection.Vertical;
                IsVerticalDrag = true;
                StartVerticalDrag(eventData);
            }
            else
            {
                // ===== 가로 드래그 확정 → ScrollRect에게 넘김 =====
                m_Directions[id] = DragDirection.Horizontal;

                if (parentScrollRect != null)
                {
                    parentScrollRect.OnBeginDrag(eventData);
                    parentScrollRect.OnDrag(eventData);
                    m_ScrollDragging[id] = true;
                }
                return;
            }
        }
        else if (dir == DragDirection.Vertical)
        {
            // 세로 드래그 중 → 아이콘 따라다니기
            if (m_DraggingIcons.TryGetValue(id, out var icon) && icon != null)
                SetDraggedPosition(eventData);
        }
        else if (dir == DragDirection.Horizontal)
        {
            // 가로 드래그 중 → ScrollRect 스크롤
            if (parentScrollRect != null && m_ScrollDragging.TryGetValue(id, out bool started) && started)
            {
                parentScrollRect.OnDrag(eventData);
            }
        }
    }

    private void StartVerticalDrag(PointerEventData eventData)
    {
        var canvas = FindInParents<Canvas>(gameObject);
        if (canvas == null) return;

        int id = eventData.pointerId;

        var icon = new GameObject("icon");
        m_DraggingIcons[id] = icon;

        icon.transform.SetParent(canvas.transform, false);
        icon.transform.SetAsLastSibling();

        var srcImage = GetComponent<Image>();

        var image = icon.AddComponent<Image>();
        image.sprite = srcImage.sprite;
        image.raycastTarget = false;

        var group = icon.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        if (dragOnSurfaces)
            m_DraggingPlanes[id] = transform as RectTransform;
        else
            m_DraggingPlanes[id] = canvas.transform as RectTransform;

        SetDraggedPosition(eventData);
    }

    private void SetDraggedPosition(PointerEventData eventData)
    {
        int id = eventData.pointerId;

        if (!m_DraggingIcons.TryGetValue(id, out var icon) || icon == null)
            return;

        if (dragOnSurfaces &&
            eventData.pointerEnter != null &&
            eventData.pointerEnter.transform is RectTransform)
        {
            m_DraggingPlanes[id] = eventData.pointerEnter.transform as RectTransform;
        }

        if (!m_DraggingPlanes.TryGetValue(id, out var plane) || plane == null)
            return;

        var rt = icon.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                plane,
                eventData.position,
                eventData.pressEventCamera,
                out var globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = plane.rotation;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int id = eventData.pointerId;

        // 세로 드래그였으면 아이콘 정리
        if (m_DraggingIcons.TryGetValue(id, out var icon) && icon != null)
            Destroy(icon);

        // 가로 드래그였으면 ScrollRect에 종료 알림
        if (m_Directions.TryGetValue(id, out var dir) &&
            dir == DragDirection.Horizontal &&
            parentScrollRect != null &&
            m_ScrollDragging.TryGetValue(id, out bool started) &&
            started)
        {
            parentScrollRect.OnEndDrag(eventData);
        }

        m_DraggingIcons.Remove(id);
        m_DraggingPlanes.Remove(id);
        m_StartPointerPos.Remove(id);
        m_Directions.Remove(id);
        m_ScrollDragging.Remove(id);

        // IsVerticalDrag는 여기서 굳이 false로 안 돌림
        // → DraggableSlot.OnDrop에서 이 값 보고 세로 드래그인지 판단
        // → 다음 OnBeginDrag에서 다시 false로 초기화됨
    }

    // =============== UTIL ===============

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
}
