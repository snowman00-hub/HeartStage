using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public static class MonitoringDragHelper
{
    public static GameObject CreateDragIcon(Canvas canvas, Image sourceImage, RectTransform sourceRect, PointerEventData eventData, string iconName = "DragIcon")
    {
        if (canvas == null || sourceImage == null || sourceImage.sprite == null)
            return null;

        var dragIcon = new GameObject(iconName);
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        var image = dragIcon.AddComponent<Image>();
        image.sprite = sourceImage.sprite;
        image.raycastTarget = false;

        var dragCanvasGroup = dragIcon.AddComponent<CanvasGroup>();
        dragCanvasGroup.blocksRaycasts = false;
        dragCanvasGroup.alpha = 0.8f;

        var dragRectTransform = dragIcon.GetComponent<RectTransform>();
        dragRectTransform.sizeDelta = sourceRect.sizeDelta;

        // 초기 위치를 마우스 위치로 설정
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        dragIcon.transform.localPosition = localPoint;

        return dragIcon;
    }

    public static void UpdateDragIconPosition(GameObject dragIcon, Canvas canvas, PointerEventData eventData)
    {
        if (dragIcon != null && canvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            dragIcon.transform.localPosition = localPoint;
        }
    }

    public static void DestroyDragIcon(ref GameObject dragIcon)
    {
        if (dragIcon != null)
        {
            Object.Destroy(dragIcon);
            dragIcon = null;
        }
    }
}