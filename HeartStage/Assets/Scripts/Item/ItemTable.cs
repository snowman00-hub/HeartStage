using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemTable : DataTable
{
    public static readonly string Unknown = "키 없음";

    private readonly Dictionary<int, ItemCSVData> table = new Dictionary<int, ItemCSVData>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
        }

        var list = LoadCSV<ItemCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.ID))
            {
                table.Add(item.ID, item);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }


        Addressables.Release(handle);
    }

    public ItemCSVData Get(int key)
    {
        if (!table.ContainsKey(key))
        {
            return null;
        }
        return table[key];
    }
}
