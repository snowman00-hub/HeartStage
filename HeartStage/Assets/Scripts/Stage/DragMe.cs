using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool dragOnSurfaces = true;
    public CharacterData characterData;

    private readonly Dictionary<int, GameObject> m_DraggingIcons = new Dictionary<int, GameObject>();
    private readonly Dictionary<int, RectTransform> m_DraggingPlanes = new Dictionary<int, RectTransform>();
    void Awake()
    {
        if (characterData != null)
            DragSourceRegistry.Register(characterData, this);
    }
    void OnDestroy()
    {
        DragSourceRegistry.Unregister(characterData, this);
    }

    public void RebindCharacter(CharacterData cd)
    {
        if (characterData == cd)
            return;
        DragSourceRegistry.Unregister(characterData, this);
        if(characterData != null)
            DragSourceRegistry.Register(cd, this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var canvas = FindInParents<Canvas>(gameObject);
        if (canvas == null) return;
        var icon = new GameObject("icon");
        m_DraggingIcons[eventData.pointerId] = icon;

        icon.transform.SetParent(canvas.transform, false);
        icon.transform.SetAsLastSibling();

        var image = icon.AddComponent<Image>();
        image.sprite = GetComponent<Image>().sprite;
        // 필요 시 아이콘 크기를 원본과 동일하게
        // image.SetNativeSize();
        image.raycastTarget = false;              // <- 드롭 판정에 방해되지 않도록

        var group = icon.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;             // <- 이벤트 막지 않도록

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
            Object.Destroy(icon);

        m_DraggingIcons.Remove(eventData.pointerId);   // <- null 넣지 말고 제거
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
}
