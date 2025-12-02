using UnityEngine;

public class LobbyHomeEdgePan : MonoBehaviour
{
    public RectTransform background;   // 움직일 로비 배경
    public RectTransform dragRect;     // 드래그 중인 Rect
    public RectTransform viewport;     // 배경이 보이는 부모(스크린 크기 역할)
    public float moveSpeed = 400f;
    public float edgePercent = 0.15f;

    private float limitLeft;
    private float limitRight;

    private void Start()
    {
        CalcLimit();
    }

    private void CalcLimit()
    {
        float bgWidth = background.rect.width;
        float vpWidth = viewport.rect.width;

        float maxMove = bgWidth - vpWidth;

        if (maxMove < 0) maxMove = 0;

        // pivot 보정
        float pivot = background.pivot.x;

        limitLeft = -(maxMove * (1f - pivot));
        limitRight = maxMove * pivot;
    }

    private void Update()
    {
        float screenWidth = Screen.width;
        float leftArea = screenWidth * edgePercent;
        float rightArea = screenWidth * (1f - edgePercent);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, dragRect.position);

        Vector2 pos = background.anchoredPosition;

        if (screenPos.x < leftArea)
        {
            pos.x += moveSpeed * Time.deltaTime;
        }
        else if (screenPos.x > rightArea)
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, limitLeft, limitRight);

        background.anchoredPosition = pos;
    }
}