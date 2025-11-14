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
        if(isCritical)
        {
            transform.position = hitPoint + Vector3.right;
            text.fontSize = criticalSize;
        }
        else
        {
            transform.position = hitPoint + Vector3.left;
            text.fontSize = normalSize;
        }
        text.text = damage.ToString();
        alpha = 1f;
    }
}