using System.Collections.Generic;

public static class ItemInvenHelper
{
    private static Dictionary<int, int> Items => SaveLoadManager.Data.itemList;
    
    // 아이템 개수 추가
    public static void AddItem(int id, int amount)
    {
        if (Items.ContainsKey(id))
            Items[id] += amount;
        else
            Items[id] = amount;

        SaveLoadManager.Save();
        LobbyManager.Instance?.MoneyUISet();

    }

    // 아이템 소비 시도, 보유 개수보다 적으면 실패
    public static bool TryConsumeItem(int id, int amount)
    {
        if (!Items.ContainsKey(id) || Items[id] < amount)
            return false;

        Items[id] -= amount;

        if (Items[id] <= 0)
            Items.Remove(id);

        SaveLoadManager.Save();
        LobbyManager.Instance?.MoneyUISet();
        return true;
    }

    // 개수 얻기
    public static int GetAmount(int id)
    {
        if (!Items.ContainsKey(id))
            return 0;

        return Items[id];
    }
}