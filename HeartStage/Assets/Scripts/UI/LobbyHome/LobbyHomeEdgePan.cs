using UnityEngine;

public class LobbyHomeEdgePan : MonoBehaviour
{
    [SerializeField] private float panSpeed = 0.01f;
    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer background;

    private Vector2 lastPos;

    private void Update()
    {
#if UNITY_EDITOR
        HandleEditorPan();
#endif
        HandleTouchPan();
    }

    // 에디터용
    private void HandleEditorPan()
    {
        if (Input.GetMouseButtonDown(0))
            lastPos = (Vector2)Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector2 cur = (Vector2)Input.mousePosition;
            Vector2 delta = cur - lastPos;

            Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * panSpeed * cam.orthographicSize;
            transform.position += move;

            ClampCamera();

            lastPos = cur;
        }
    }

    // 핸드폰용
    private void HandleTouchPan()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
                lastPos = t.position;

            if (t.phase == TouchPhase.Moved)
            {
                Vector2 cur = t.position;
                Vector2 delta = cur - lastPos;

                Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * panSpeed * cam.orthographicSize;
                transform.position += move;

                ClampCamera();

                lastPos = cur;
            }
        }
    }

    // 배경 항상 찍도록 Clamp
    public void ClampCamera()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float halfW = camWidth / 2f;
        float halfH = camHeight / 2f;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x,
            background.bounds.min.x + halfW,
            background.bounds.max.x - halfW);

        pos.y = Mathf.Clamp(pos.y,
            background.bounds.min.y + halfH,
            background.bounds.max.y - halfH);

        transform.position = pos;
    }
}