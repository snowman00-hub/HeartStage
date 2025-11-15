using UnityEngine;

public class ExpItem : MonoBehaviour
{
    private Vector3 targetPos = new Vector3(4.65f, 9.84f, 0);

    private float delayTime = 0.5f;
    private float flyTime = 0.5f;

    private float timer = 0f;
    private bool isFlying = false;

    private Vector3 startPos;

    private void OnEnable()
    {
        timer = 0f;
        isFlying = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 딜레이 구간
        if (!isFlying)
        {
            if (timer >= delayTime)
            {
                // 이동 시작
                isFlying = true;
                timer = 0f;           // 이동 시간 타이머로 재사용
                startPos = transform.position;
            }
            else
            {
                return;
            }
        }

        // 이동 구간 (flyTime 동안)
        if (isFlying)
        {
            float t = timer / flyTime;
            if (t >= 1f)
            {
                transform.position = targetPos;
                isFlying = false;
                PoolManager.Instance.Release(ItemManager.expItemAssetName, gameObject);
                StageManager.Instance.ExpGet(3); // 임시 값
                return;
            }

            // Linear 이동
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
    }
}