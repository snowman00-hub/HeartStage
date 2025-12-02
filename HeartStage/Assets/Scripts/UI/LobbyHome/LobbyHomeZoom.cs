using UnityEngine;

public class LobbyHomeZoom : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float minSize = 3f;
    public float maxSize = 10f;

    public Camera cam;

    private LobbyHomeEdgePan edgePan;

    private void Awake()
    {
        edgePan = GetComponent<LobbyHomeEdgePan>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleEditorZoom();
#endif
        HandleTouchZoom();
    }

    // 마우스 휠 줌
    private void HandleEditorZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // 줌 전
            Vector3 worldBefore = cam.ScreenToWorldPoint(Input.mousePosition);

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - scroll * zoomSpeed,
                minSize, maxSize
            );

            // 줌 후
            Vector3 worldAfter = cam.ScreenToWorldPoint(Input.mousePosition);

            // 줌으로 인해 달라진 만큼 카메라 이동
            cam.transform.position += worldBefore - worldAfter;

            edgePan.ClampCamera();
        }
    }

    // 모바일 핀치 줌
    private void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 mid = (t1.position + t2.position) * 0.5f;

            // 줌 전
            Vector3 worldBefore = cam.ScreenToWorldPoint(mid);

            Vector2 t1Prev = t1.position - t1.deltaPosition;
            Vector2 t2Prev = t2.position - t2.deltaPosition;

            float prevDist = Vector2.Distance(t1Prev, t2Prev);
            float currDist = Vector2.Distance(t1.position, t2.position);

            float diff = currDist - prevDist;

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - diff * zoomSpeed * 0.01f,
                minSize, maxSize
            );

            // 줌 후
            Vector3 worldAfter = cam.ScreenToWorldPoint(mid);

            // 차이만큼 이동
            cam.transform.position += worldBefore - worldAfter;

            edgePan.ClampCamera();
        }
    }
}