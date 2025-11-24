using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class ShopData
{
    public int Shop_ID { get; set; }
    public int Shop_type { get; set; }
    public int Shop_item_type1 { get; set; }
    public int Shop_item_amount1 { get; set; }
    public int Shop_item_type2 { get; set; }
    public int Shop_item_amount2 { get; set; }
    public int Shop_item_type3 { get; set; }
    public int Shop_item_amount3 { get; set; }
    public int Shop_item_type4 { get; set; }
    public int Shop_item_amount4 { get; set; }
    public string Shop_item_name { get; set; }
    public int Shop_currency { get; set; } 
    public int Shop_price { get; set; }  
    public int Shop_multibuy { get; set; }
    public int Shop_24hr { get; set; }
    public string Shop_info { get; set; }
    public string Shop_icon { get; set; }
}

public class ShopTable : DataTable
{
    public static readonly string Unknown = "키 없음";

    private readonly Dictionary<int, ShopData> table = new Dictionary<int, ShopData>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
        }

        var list = LoadCSV<ShopData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Shop_ID))
            {
                table.Add(item.Shop_ID, item);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }

        Addressables.Release(handle);
    }

    public ShopData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }
}