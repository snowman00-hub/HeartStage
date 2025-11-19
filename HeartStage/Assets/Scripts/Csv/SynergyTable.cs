using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SynergyTable : DataTable
{
    public static readonly string Unknown = "시너지 ID 없음";

    private readonly Dictionary<int, SynergyCSVData> table = new Dictionary<int, SynergyCSVData>();

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

        var list = LoadCSV<SynergyCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.synergy_id))
            {
                table.Add(item.synergy_id, item);
            }
            else
            {
                Debug.LogError($"시너지 ID 중복! synergy_id: {item.synergy_id}");
            }
        }

        Addressables.Release(handle);
    }

    public SynergyCSVData Get(int synergyId)
    {
        if (!table.ContainsKey(synergyId))
        {
            Debug.LogWarning($"[SynergyTable] synergy_id {synergyId} 없음");
            return null;
        }

        return table[synergyId];
    }

    public Dictionary<int, SynergyData> GetAll()
    {
        Dictionary<int, SynergyData> result = new Dictionary<int, SynergyData>();

        foreach (var kvp in table)
        {
            var so = ResourceManager.Instance.Get<SynergyData>(kvp.Value.synergy_name);
            result.Add(kvp.Key, so);
        }

        return result;
    }

    public List<int> GetEffectIds(int id)
    {
        var list = new List<int>();
        if (table.ContainsKey(id))
        {
            if (table[id].effect_type1 != 0)
            {
                list.Add(table[id].effect_type1);
            }

            if (table[id].effect_type2 != 0)
            {
                list.Add(table[id].effect_type2);
            }

            if (table[id].effect_type3 != 0)
            {
                list.Add(table[id].effect_type3);
            }
        }

        return list;
    }

    public List<CharacterType> GetRequireUnit(int id)
    {
        var list = new List<CharacterType>();
        if (table.ContainsKey(id))
        {
            if (table[id].synergy_Unit1 != 0)
            {
                list.Add((CharacterType)table[id].synergy_Unit1);
            }
            if (table[id].synergy_Unit2 != 0)
            {
                list.Add((CharacterType)table[id].synergy_Unit2);
            }
            if (table[id].synergy_Unit3 != 0)
            {
                list.Add((CharacterType)table[id].synergy_Unit3);
            }
        }

        return list;
    }
}
