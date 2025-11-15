using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class CharacterCSVData
{
    public int char_id { get; set; }
    public string char_name { get; set; }
    public int char_lv { get; set; }
    public int char_exp { get; set; }
    public int char_rank { get; set; }
    public int char_type { get; set; }

    public int atk_dmg { get; set; }
    public float atk_speed { get; set; }
    public float atk_range { get; set; }
    public float atk_addcount { get; set; }

    public int bullet_count { get; set; }
    public float bullet_speed { get; set; }
    public int char_hp { get; set; }

    public float crt_chance { get; set; }
    public float crt_dmg { get; set; }

    public int skill_id1 { get; set; }
    public int skill_id2 { get; set; }
    public int skill_id3 { get; set; }
    public int skill_id4 { get; set; }
    public int skill_id5 { get; set; }
    public int skill_id6 { get; set; }

    public string Info { get; set; }

    public string image_AssetName { get; set; }
    public string data_AssetName { get; set; }
    public string bullet_PrefabName { get; set; }
    public string projectile_AssetName { get; set; }
    public string hitEffect_AssetName { get; set; }
}

public class CharacterTable : DataTable
{
    public static readonly string Unknown = "키 없음";

    private readonly Dictionary<int, CharacterCSVData> table = new Dictionary<int, CharacterCSVData>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
        }

        var list = LoadCSV<CharacterCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.char_id))
            {
                table.Add(item.char_id, item);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }

        Addressables.Release(handle);
    }

    public CharacterCSVData Get(int key)
    {
        if (!table.ContainsKey(key))
        {
            Debug.Log($"[CharacterCSVData] Get {key} 실패");
            return null;
        }
        return table[key];
    }

    public List<int> GetSkillIds(int id)
    {
        var data = table[id];
        var skills = new[] { data.skill_id1, data.skill_id2, data.skill_id3, data.skill_id4, data.skill_id5, data.skill_id6 };

        return skills.Where(s => s != 0).ToList();
    }
}