using UnityEngine;

public class LobbyHomeZoom : MonoBehaviour
{
    public RectTransform target; // 줌할 UI (배경)
    public float zoomSpeed = 0.01f;
    public float minZoom = 0.7f;
    public float maxZoom = 1.8f;

    private void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 t1Prev = t1.position - t1.deltaPosition;
            Vector2 t2Prev = t2.position - t2.deltaPosition;

            float prevDist = Vector2.Distance(t1Prev, t2Prev);
            float currDist = Vector2.Distance(t1.position, t2.position);

            float diff = currDist - prevDist; // 확대 + / 축소 -

            Vector3 scale = target.localScale;
            float newScale = Mathf.Clamp(scale.x + diff * zoomSpeed, minZoom, maxZoom);

            target.localScale = new Vector3(newScale, newScale, 1);
        }
    }
}