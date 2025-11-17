using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class SelectData
{
    public int select_id {  get; set; }
    public string select_name { get; set; }
    public int skill_target { get; set; }
    public int effect_type1 { get; set; }
    public float value1 { get; set; }
    public int effect_type2 { get; set; }
    public float value2 { get; set; }
    public string info { get; set; }
    public string prefab { get; set; }
}

public class SelectTable : DataTable
{
    public static readonly string Unknown = "키 없음";
    
    private readonly Dictionary<int, SelectData> table = new Dictionary<int, SelectData>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
        }

        var list = LoadCSV<SelectData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.select_id))
            {
                table.Add(item.select_id, item);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }

        Addressables.Release(handle);
    }

    public SelectData Get(int id)
    {
        if (!table.ContainsKey(id))
        {
            return null;
        }
        return table[id];
    }

    // 랜덤으로 ID 3개 반환
    public List<SelectData> GetRandomThree()
    {
        List<int> keys = new List<int>(table.Keys);
        List<SelectData> result = new List<SelectData>(3);

        // 랜덤 사용
        System.Random random = new System.Random();
        for (int i = 0; i < 3; i++)
        {
            int index = random.Next(keys.Count);     // 남아 있는 키 중 랜덤 선택
            int id = keys[index];

            result.Add(table[id]);

            keys.RemoveAt(index);  // 뽑힌 키는 제거 → 중복 방지
        }

        return result;
    }
}