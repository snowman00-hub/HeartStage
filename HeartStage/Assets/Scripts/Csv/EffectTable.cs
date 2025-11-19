using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectTable : DataTable
{
    public static readonly string Unknown = "이펙트 ID 없음";

    private readonly Dictionary<int, EffectCSVData> table = new Dictionary<int, EffectCSVData>();

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

        var list = LoadCSV<EffectCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.effect_ID))
            {
                table.Add(item.effect_ID, item);
            }
            else
            {
                Debug.LogError($"이펙트 ID 중복! effect_ID: {item.effect_ID}");
            }
        }

        Addressables.Release(handle);
    }

    public EffectCSVData Get(int effectId)
    {
        if (!table.ContainsKey(effectId))
        {
            Debug.LogWarning($"[ActiveEffectTable] effect_ID {effectId} 없음");
            return null;
        }

        return table[effectId];
    }

    public Dictionary<int, EffectData> GetAll()
    {
        Dictionary<int, EffectData> result = new Dictionary<int, EffectData>();

        foreach (var kvp in table)
        {
            var so = ResourceManager.Instance.Get<EffectData>(kvp.Value.effect_name);
            result.Add(kvp.Key, so);
        }

        return result;
    }
}
