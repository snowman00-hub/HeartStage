using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    private Dictionary<int, ItemData> itemDB = new Dictionary<int, ItemData>();
    public static readonly string expItemAssetName = "ExpItem";

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
    }

    public void SpawnExp(Vector3 spawnPos)
    {
        var expGo = PoolManager.Instance.Get(expItemAssetName);
        expGo.transform.position = spawnPos;
    }
}