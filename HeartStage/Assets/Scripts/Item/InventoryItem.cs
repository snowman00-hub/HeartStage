using UnityEngine;

// 인벤토리에 쌓이는 아이템
public class InventoryItem : MonoBehaviour
{
    private Vector3 targetPos = new Vector3(-4.84f, 8.19f, 0);

    private float delayTime = 0.5f;
    private float flyTime = 0.5f;

    private float timer = 0f;
    private bool isFlying = false;

    private Vector3 startPos;

    [HideInInspector]
    public int amount = 1;
    [HideInInspector]
    public int itemId = 0;

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
                PoolManager.Instance.Release(ItemManager.inventoryItemId, gameObject);
                ItemManager.Instance.AddToInventory(itemId, amount);
                return;
            }

            // Linear 이동
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
    }
}
