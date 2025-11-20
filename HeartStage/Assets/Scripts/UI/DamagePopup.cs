using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float moveSpeed = 1f;
    public float fadeSpeed = 2f;

    private float alpha = 1f;

    public float normalSize = 40f;
    public float criticalSize = 80f;

    // 이동 및 페이드 아웃
    private void Update()
    {
        // 위로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 알파 감소
        alpha -= fadeSpeed * Time.deltaTime;

        var c = text.color;
        c.a = alpha;
        text.color = c;

        if (alpha <= 0f)
        {
            PoolManager.Instance.Release(DamagePopupManager.popupId, gameObject);
        }
    }

    // 대미지 팝업 세팅
    public void Setup(int damage, Vector3 hitPoint, bool isCritical = false)
    {
        // 월드 좌표 → 화면 좌표
        Vector2 screenPos = Camera.main.WorldToScreenPoint(hitPoint);

        // UI 위치로 설정
        transform.position = screenPos;

        // 크리티컬 여부에 따른 이동 좌우
        if (isCritical)
        {
            transform.position += Vector3.right * 70f;  // UI니까 픽셀 단위로 살짝 이동
            text.fontSize = criticalSize;
        }
        else
        {
            transform.position += Vector3.left * 70f;
            text.fontSize = normalSize;
        }

        text.text = damage.ToString();
        alpha = 1f;
    }
}