using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StringTable : DataTable
{
    public static readonly string Unknown = "키 없음";

    public class Data
    {
        public string Id { get; set; }
        public string String { get; set; }
    }

    private readonly Dictionary<string, string> table = new Dictionary<string, string>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
        }

        var list = LoadCSV<Data>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Id))
            {
                table.Add(item.Id, item.String);
            }
            else
            {
                Debug.LogError("몬스터 아이디 중복!");
            }
        }


        Addressables.Release(handle);
    }

    public string Get(string key)
    {
        if (!table.ContainsKey(key))
        {
            return Unknown;
        }
        return table[key];
    }
}
