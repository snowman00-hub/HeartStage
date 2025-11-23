using UnityEngine;

public class DropItem : MonoBehaviour
{
    [HideInInspector]
    public int itemId;
    [HideInInspector]
    public int amount;
    [HideInInspector]
    public Vector3 targetPos;

    private Vector3 startPos;
    private float delayTime = 1f;
    private float flyTime = 0.5f;
    private float timer = 0f;
    private bool isFlying = false;

    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        timer = 0f;
        isFlying = false;
    }

    public void Setup(int id, int amt, Vector3 spawnPos, Vector3 target)
    {
        this.itemId = id;
        this.amount = amt;
        transform.position = spawnPos;
        this.targetPos = target;

        // sprite setting
        var texture = ResourceManager.Instance.Get<Texture2D>(DataTableManager.ItemTable.Get(id).prefab);
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // delay time
        if (!isFlying)
        {
            if (timer >= delayTime)
            {
                isFlying = true;
                timer = 0f;
                startPos = transform.position;
            }
            return;
        }

        // flying
        float t = timer / flyTime;
        if (t >= 1f)
        {
            transform.position = targetPos;

            // 회수 + 아이템 사용
            PoolManager.Instance.Release(ItemManager.ItemPoolId, gameObject);
            ItemManager.Instance.UseItem(itemId, amount);
            return;
        }

        transform.position = Vector3.Lerp(startPos, targetPos, t);
    }
}