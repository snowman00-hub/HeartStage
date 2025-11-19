using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    private Dictionary<int, ItemData> itemDB = new Dictionary<int, ItemData>();
    public static readonly string expItemAssetName = "ExpItem";

    public GameObject inventoryItem; // 경험치 이외의 아이템
    public static readonly string inventoryItemId = "inventoryItem";

    public TextMeshProUGUI lightStickCount;
    private int invenItemCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    // 아이템테이블 데이터 -> SO에 덮어쓰기
    private void Start()
    {
        itemDB = DataTableManager.ItemTable.GetAll();
        foreach (var item in itemDB)
        {
            item.Value.UpdateData(DataTableManager.ItemTable.Get(item.Value.item_id));
        }

        var expItemPrefab = ResourceManager.Instance.Get<GameObject>(expItemAssetName);
        PoolManager.Instance.CreatePool(expItemAssetName, expItemPrefab, 50);
        PoolManager.Instance.CreatePool(inventoryItemId, inventoryItem, 50);
        SetItemCountUI();
    }

    // 경험치 아이템
    public void SpawnExp(Vector3 spawnPos, int amount)
    {
        return; // 고치기

        var expGo = PoolManager.Instance.Get(expItemAssetName);
        var expItem = expGo.GetComponent<ExpItem>();
        expItem.amount = amount;
        expGo.transform.position = spawnPos;
    }

    // 보관함에 들어가는 아이템 얻기
    public void SpawnItem(int itemId, int amount, Vector3 spawnPos)
    {
        var itemBaseGo = PoolManager.Instance.Get(inventoryItemId);
        // 아이템 Id 및 수량 세팅
        var inventoryItem = itemBaseGo.GetComponent<InventoryItem>();
        inventoryItem.amount = amount;
        inventoryItem.itemId = itemId;
        itemBaseGo.SetActive(false);
        // 스프라이트 변경
        var renderer = itemBaseGo.GetComponentInChildren<SpriteRenderer>();
        var texture = ResourceManager.Instance.Get<Texture2D>(DataTableManager.ItemTable.Get(itemId).prefab);
        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        itemBaseGo.SetActive(true);
        // 위치 세팅
        itemBaseGo.transform.position = spawnPos;
    }

    // 실제 인벤토리에 데이터 저장
    public void AddToInventory(int  itemId, int amount)
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

        // UI 변경 근데 라이트 스틱만 떨어지나?
        invenItemCount += amount;
        SetItemCountUI();

        // 저장 타이밍 나중에 옮기기
        SaveLoadManager.Save();
    }

    private void SetItemCountUI()
    {
        lightStickCount.text = $"X{invenItemCount}";
    }
}