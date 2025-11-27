using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class QuestProgressTable : DataTable
{
    public static readonly string Unknown = "퀘스트 진척도 ID 없음";

    private readonly Dictionary<int, QuestProgressData> table = new Dictionary<int, QuestProgressData>();

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

        var list = LoadCSV<QuestProgressData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Progress_Reward_ID))
            {
                table.Add(item.Progress_Reward_ID, item);
            }
        }

        Addressables.Release(handle);
    }

    public QuestProgressData Get(int questId)
    {
        if (!table.ContainsKey(questId))
        {
            return null;
        }

        return table[questId];
    }

    public bool TryGetNext(int currentId, out QuestProgressData next)
    {
        next = null;
        var cur = Get(currentId);
        if (cur == null)
            return false;
        return table.TryGetValue(cur.Progress_Reward_ID, out next);
    }
}
