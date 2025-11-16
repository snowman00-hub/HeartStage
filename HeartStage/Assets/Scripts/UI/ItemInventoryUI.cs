using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemInventoryUI : MonoBehaviour
{
    public static ItemInventoryUI Instance;

    public ItemInfoPanel ItemInfoPanel;

    private List<ItemInvenSlot> itemSlotList;

    private void Awake()
    {
        Instance = this;
        itemSlotList = GetComponentsInChildren<ItemInvenSlot>().ToList();
        SaveLoadManager.Load();
    }

    private void OnEnable()
    {
        var saveItemList = SaveLoadManager.Data.itemList;
        int index = 0;
        foreach (var item in saveItemList)
        {
            itemSlotList[index++].Init(item.Key, item.Value);
        }
    }

    public void OpenItemInfoPanel(int itemId)
    {
        ItemInfoPanel.itemId = itemId;
        ItemInfoPanel.gameObject.SetActive(true);
    }
}
