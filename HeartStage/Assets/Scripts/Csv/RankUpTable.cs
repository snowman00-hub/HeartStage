using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RankUpTable : DataTable
{
    public static readonly string Unknown = "랭크업 ID 없음";

    private readonly Dictionary<int, RankUpData> table = new Dictionary<int, RankUpData>();

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

        var list = LoadCSV<RankUpData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.Upgrade_ingrd_C1))
            {
                table.Add(item.Upgrade_ingrd_C1, item);
            }
            else
            {
                Debug.LogError($"랭크업 ID 중복! Upgrade_ID: {item.Upgrade_ingrd_C1}");
            }
        }

        Addressables.Release(handle);
    }

    public RankUpData Get(int rankupId)
    {
        if (!table.ContainsKey(rankupId))
        {
            Debug.LogWarning($"[RankUpTable] Upgrade_ID {rankupId} 없음");
            return null;
        }

        return table[rankupId];
    }

    public bool TryGetNext(int currentId, out RankUpData next)
    {
        next = null;
        var cur = Get(currentId);
        if (cur == null)
            return false;
        return table.TryGetValue(cur.Upgrade_ingrd_C1, out next);
    }
}
