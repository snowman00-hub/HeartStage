using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Data
{
    public int id { get; set; }
    public string mon_name { get; set; }
    public int mon_type { get; set; }
    public int stage_num { get; set; }
    public int atk_type { get; set; }
    public int atk_dmg { get; set; }
    public int atk_speed { get; set; }
    public int atk_range { get; set; }
    public int bullet_speed { get; set; }
    public int hp { get; set; }
    public int speed { get; set; }
    public int skill_id { get; set; }
    public int min_level { get; set; }
    public int max_level { get; set; }
}

public class MonsterTable : DataTable
{
    private readonly Dictionary<int, Data> table = new Dictionary<int, Data>();

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

        var list = LoadCSV<Data>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.id))
            {
                table.Add(item.id, item);
            }
            else
            {
                Debug.LogError($"몬스터 아이디 중복: {item.id}");
            }
        }

        Addressables.Release(handle);
    }

    public Data Get(int monsterId)
    {
        if (!table.ContainsKey(monsterId))
        {
            Debug.LogWarning($"몬스터 아이디를 찾을 수 없음: {monsterId}");
            return null;
        }
        return table[monsterId];
    }
}
