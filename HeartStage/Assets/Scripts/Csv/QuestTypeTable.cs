using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class QuestTypeTable : DataTable
{
    public static readonly string Unknown = "퀘스트 타입 ID 없음";

    private readonly Dictionary<int, QuestTypeData> table = new Dictionary<int, QuestTypeData>();

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

        var list = LoadCSV<QuestTypeData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Quest_type))
            {
                table.Add(item.Quest_type, item);
            }
        }

        Addressables.Release(handle);
    }

    public QuestTypeData Get(int questId)
    {
        if (!table.ContainsKey(questId))
        {
            return null;
        }

        return table[questId];
    }

    public bool TryGetNext(int currentId, out QuestTypeData next)
    {
        next = null;
        var cur = Get(currentId);
        if (cur == null)
            return false;
        return table.TryGetValue(cur.Quest_type, out next);
    }
}
