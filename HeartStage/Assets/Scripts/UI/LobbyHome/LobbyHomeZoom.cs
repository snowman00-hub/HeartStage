using UnityEngine;

public class LobbyHomeZoom : MonoBehaviour
{
    public RectTransform background;
    public RectTransform dragRect;
    public float zoomSpeed = 0.01f;
    public float minZoom = 0.7f;
    public float maxZoom = 1.8f;

    private void Update()
    {
#if UNITY_EDITOR  
        HandleEditorZoom();
#endif
        HandleTouchZoom();
    }

    private void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 t1Prev = t1.position - t1.deltaPosition;
            Vector2 t2Prev = t2.position - t2.deltaPosition;

            float prevDist = Vector2.Distance(t1Prev, t2Prev);
            float currDist = Vector2.Distance(t1.position, t2.position);

            float diff = currDist - prevDist;

            ApplyZoom(diff * zoomSpeed);
        }
    }

    private void HandleEditorZoom()
    {
        float scroll = Input.mouseScrollDelta.y; // 휠 위(+), 아래(-)

        if (Mathf.Abs(scroll) > 0.01f)
        {
            ApplyZoom(scroll * 0.1f); // PC에서는 좀 민감하니 배율 조정
        }
    }

    private void ApplyZoom(float delta)
    {
        float current = background.localScale.x;
        float newScale = Mathf.Clamp(current + delta, minZoom, maxZoom);
        background.localScale = new Vector3(newScale, newScale, 1);
        dragRect.localScale = background.localScale;  
    }
}