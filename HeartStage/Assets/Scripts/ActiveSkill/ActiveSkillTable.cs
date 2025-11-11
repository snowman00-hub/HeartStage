using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class ActiveSkillCSVData
{
    public int skill_id { get; set; }
    public string skill_name { get; set; }
    public int use_type { get; set; }
    public int skill_dmg { get; set; }
    public float skill_cool { get; set; }
    public float skill_speed { get; set; }
    public float skill_crt { get; set; }
    public float skill_range { get; set; }
    public int status_id { get; set; }
    public int buff_id { get; set; }
}

public class ActiveSkillTable : DataTable
{
    public static readonly string Unknown = "스킬 ID 없음";

    private readonly Dictionary<int, ActiveSkillCSVData> table = new Dictionary<int, ActiveSkillCSVData>();

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

        var list = LoadCSV<ActiveSkillCSVData>(ta.text);

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

    public ActiveSkillCSVData Get(int skillId)
    {
        if (!table.ContainsKey(skillId))
        {
            Debug.LogWarning($"[ActiveSkillTable] skill_id {skillId} 없음");
            return null;
        }

        return table[skillId];
    }

    public Dictionary<int, ActiveSkillCSVData> GetAll()
    {
        return table;
    }
}
