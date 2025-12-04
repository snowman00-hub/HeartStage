using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GachaData
{
    public int Gacha_ID { get; set; }     
    public int Gacha_type { get; set; }   
    public int Gacha_item { get; set; }   
    public int Gacha_per { get; set; }    
    public int Gacha_have { get; set; }
    public int Gacha_have_amount { get; set; }
    public int Gacha_item_amount { get; set; }
}

public class GachaTable : DataTable
{
    private readonly static Dictionary<int, List<GachaData>> table = new Dictionary<int, List<GachaData>>();
    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();

        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
            return;
        }

        var list = LoadCSV<GachaData>(ta.text);

        foreach (var item in list)
        {
            if(!table.ContainsKey(item.Gacha_type))
            {
                table[item.Gacha_type] = new List<GachaData>();
            }
            table[item.Gacha_type].Add(item);
        }

        Addressables.Release(handle);
    }

    // 특정 가챠 타입의 모든 아이템 가져오기
    public static List<GachaData> GetGachaByType(int gatchaType)
    {
        return table.ContainsKey(gatchaType) ? table[gatchaType] : new List<GachaData>();
    }
}
