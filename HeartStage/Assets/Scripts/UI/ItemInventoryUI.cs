using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemTypeID
{
    public static readonly int Consumable = 4;
    public static readonly int Cloth = 7;
    public static readonly int Piece = 9;
}

public class ItemInventoryUI : MonoBehaviour
{
    public static ItemInventoryUI Instance;

    public ItemInfoPanel ItemInfoPanel;

    private List<ItemInvenSlot> itemSlotList;

    private void Awake()
    {
        Instance = this;
        itemSlotList = GetComponentsInChildren<ItemInvenSlot>().ToList();
    }

    private void OnEnable()
    {
        ClearAll();
        ShowAll();
    }

    // 전부 보이기
    public void ShowAll()
    {
        ClearAll();
        var saveItemList = SaveLoadManager.Data.itemList;
        int index = 0;
        foreach (var item in saveItemList)
        {
            itemSlotList[index++].Init(item.Key, item.Value);
        }
    }

    // 소모품 보이기
    public void ShowConsumable()
    {
        ClearAll();
        var saveItemList = SaveLoadManager.Data.itemList;
        int index = 0;
        foreach (var item in saveItemList)
        {
            var itemData = DataTableManager.ItemTable.Get(item.Key);
            if (itemData.item_type != ItemTypeID.Consumable)
                continue;

            itemSlotList[index++].Init(item.Key, item.Value);
        }
    }

    // 의상 보이기
    public void ShowCloth()
    {
        ClearAll();
        var saveItemList = SaveLoadManager.Data.itemList;
        int index = 0;
        foreach (var item in saveItemList)
        {
            var itemData = DataTableManager.ItemTable.Get(item.Key);
            if (itemData.item_type != ItemTypeID.Cloth)
                continue;

            itemSlotList[index++].Init(item.Key, item.Value);
        }
    }

    // 조각 보이기
    public void ShowPiece()
    {
        ClearAll();
        var saveItemList = SaveLoadManager.Data.itemList;
        int index = 0;
        foreach (var item in saveItemList)
        {
            var itemData = DataTableManager.ItemTable.Get(item.Key);
            if (itemData.item_type != ItemTypeID.Piece)
                continue;

            itemSlotList[index++].Init(item.Key, item.Value);
        }
    }

    // 아이템 슬롯 비우기
    private void ClearAll()
    {
        foreach(var item in itemSlotList)
        {
            item.Clear();
        }
    }

    // 아이템 정보창 띄우기
    public void OpenItemInfoPanel(int itemId)
    {
        ItemInfoPanel.itemId = itemId;
        ItemInfoPanel.gameObject.SetActive(true);
    }
}
