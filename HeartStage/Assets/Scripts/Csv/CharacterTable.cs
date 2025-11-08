using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class CharacterCSVData
{
    public int ID { get; set; }
    public float atk_dmg { get; set; }
    public float atk_interval { get; set; }
    public float atk_range { get; set; }
    public float bullet_speed { get; set; }
    public float bullet_count { get; set; }
    public float hp { get; set; }
    public float crt_chance { get; set; }
    public float crt_hit_rate { get; set; }
    public string bullet_PrefabName { get; set; }
    public string data_AssetName { get; set; }
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

    public CharacterCSVData Get(int key)
    {
        if (!table.ContainsKey(key))
        {
            Debug.Log($"[CharacterCSVData] Get {key} 실패");
            return null;
        }
        return table[key];
    }
}