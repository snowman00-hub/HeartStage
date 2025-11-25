using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SkillTable : DataTable
{
    public static readonly string Unknown = "스킬 ID 없음";

    private readonly Dictionary<int, SkillCSVData> table = new Dictionary<int, SkillCSVData>();

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

        var list = LoadCSV<SkillCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.skill_id))
            {
                table.Add(item.skill_id, item);
            }
            else
            {
                Debug.LogError($"스킬 ID 중복! skill_id: {item.skill_id}");
            }
        }

        Addressables.Release(handle);
    }

    public SkillCSVData Get(int skillId)
    {
        if (!table.ContainsKey(skillId))
        {
            Debug.LogWarning($"[ActiveSkillTable] skill_id {skillId} 없음");
            return null;
        }

        return table[skillId];
    }

    public Dictionary<int, SkillData> GetAll()
    {
        Dictionary<int, SkillData> result = new Dictionary<int, SkillData>();

        foreach (var kvp in table)
        {
            var so = ResourceManager.Instance.Get<SkillData>(kvp.Value.skill_name);
            result.Add(kvp.Key, so);
        }

        return result;
    }

    public List<int> GetEffectIds(int id)
    {
        var list = new List<int>();
        if (table.ContainsKey(id))
        {
            if (table[id].skill_eff1 != 0)
            {
                list.Add(table[id].skill_eff1);
            }

            if (table[id].skill_eff2 != 0)
            {
                list.Add(table[id].skill_eff2);
            }

            if (table[id].skill_eff3 != 0)
            {
                list.Add(table[id].skill_eff3);
            }
        }

        return list;
    }
}
