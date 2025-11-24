using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GachaTypeCSVData
{
    public int Gacha_type_ID { get; set; }    
    public string Gacha_name { get; set; }    
    public int Gacha_currency { get; set; }   
    public int Gacha_price { get; set; }      
    public string info { get; set; }          
}

public class GachaTypeTable : DataTable
{
    private readonly Dictionary<int, GachaTypeCSVData> table = new Dictionary<int, GachaTypeCSVData>();

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

        var list = LoadCSV<GachaTypeCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Gacha_type_ID))
            {
                table.Add(item.Gacha_type_ID, item);
            }
            else
            {
                Debug.LogError($"가챠 타입 ID 중복: {item.Gacha_type_ID}");
            }
        }

        Addressables.Release(handle);
    }

    public GachaTypeCSVData Get(int gachaTypeId)
    {
        if (!table.ContainsKey(gachaTypeId))
        {
            Debug.LogWarning($"가챠 타입 ID를 찾을 수 없음: {gachaTypeId}");
            return null;
        }
        return table[gachaTypeId];
    }

    public IEnumerable<GachaTypeCSVData> GetAllData()
    {
        return table.Values;
    }
}