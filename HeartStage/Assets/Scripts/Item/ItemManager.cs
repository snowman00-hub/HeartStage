using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    private Dictionary<int, ItemData> itemDB = new Dictionary<int, ItemData>();
    public static readonly string ItemPoolId = "ItemBase";

    public GameObject DropItem;
    public RectTransform itemBagTr;
    public RectTransform ExpTargetTr;

    public TextMeshProUGUI lightStickCount;
    private int AcquireItemCount = 0;

    private Vector3 inventoryItemTarget;
    private Vector3 expItemTarget;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 아이템테이블 데이터 -> SO에 덮어쓰기
        itemDB = DataTableManager.ItemTable.GetAll();
        foreach (var item in itemDB)
        {
            item.Value.UpdateData(DataTableManager.ItemTable.Get(item.Value.item_id));
        }

        inventoryItemTarget = GetUIToWorldPosition(itemBagTr);
        expItemTarget = GetUIToWorldPosition(ExpTargetTr);
        PoolManager.Instance.CreatePool(ItemPoolId, DropItem, 60);
        SetItemCountUI();
    }

    public void SpawnItem(int itemId, int amount, Vector3 spawnPos)
    {
        var go = PoolManager.Instance.Get(ItemPoolId);
        var dropItem = go.GetComponent<DropItem>();

        // 스폰 위치 랜덤값 +
        Vector2 offset = Random.insideUnitCircle * 0.4f;
        spawnPos += (Vector3)offset;

        if (itemId == ItemID.Exp) // 경험치
        {
            dropItem.Setup(itemId, amount, spawnPos, expItemTarget);
        }
        else
        {
            dropItem.Setup(itemId, amount, spawnPos, inventoryItemTarget);
        }
    }

    // 아이템 사용하기
    public void UseItem(int itemId, int amount)
    {
        if(itemId == ItemID.Exp) // 경험치
        {
            StageManager.Instance.ExpGet(amount);
        }
        else
        {
            AddToInventory(itemId, amount);
        }
    }

    // 실제 인벤토리에 데이터 저장
    public void AddToInventory(int itemId, int amount)
    {
        var itemList = SaveLoadManager.Data.itemList;
        if (itemList.ContainsKey(itemId))
        {
            itemList[itemId] += amount;
        }
        else
        {
            itemList.Add(itemId, amount);
        }

        AcquireItemCount += amount;
        SetItemCountUI();

        // 저장 타이밍 나중에 옮기기
        SaveLoadManager.Save();
    }

    // 획득 아이템 개수 UI Set
    private void SetItemCountUI()
    {
        lightStickCount.text = $"X{AcquireItemCount}";
    }

    // UI 위치의 월드좌표 얻기
    private Vector3 GetUIToWorldPosition(RectTransform ui)
    {
        // Overlay canvas라면 ui.position이 이미 스크린 좌표임
        Vector3 screenPos = ui.position;

        // 아이템과 카메라 사이 거리(양수!)
        float distance = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        screenPos.z = distance;

        // 스크린 → 월드 변환
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}