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

    public List<(int id, int amount)> GetValidItems()
    {
        var list = new List<(int, int)>();

        if (Shop_item_type1 != 0) list.Add((Shop_item_type1, Shop_item_amount1));
        if (Shop_item_type2 != 0) list.Add((Shop_item_type2, Shop_item_amount2));
        if (Shop_item_type3 != 0) list.Add((Shop_item_type3, Shop_item_amount3));
        if (Shop_item_type4 != 0) list.Add((Shop_item_type4, Shop_item_amount4));

        return list;
    }
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

    // 조각아이디들
    public List<int> GetPieceIds()
    {
        List<int> result = new List<int>();

        foreach (var id in table.Keys)
        {
            if (id >= 101000 && id <= 102000)
            {
                result.Add(id);
            }
        }

        return result;
    }

    // 데일리 샵에서 조각 세 개 랜덤으로 얻기
    public List<int> GetRandomThreePieceIds()
    {
        // 1) 먼저 범위 안의 ID들을 모아오기
        List<int> pieceIds = GetPieceIds();

        // 개수가 3개 미만이면 그대로 반환(혹은 에러 처리 가능)
        if (pieceIds.Count < 3)
            return pieceIds;

        // 2) 셔플해서 앞에 3개 가져오기
        for (int i = 0; i < pieceIds.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, pieceIds.Count);
            (pieceIds[i], pieceIds[randIndex]) = (pieceIds[randIndex], pieceIds[i]);
        }

        // 3) 첫 3개 반환
        return pieceIds.GetRange(0, 3);
    }
}