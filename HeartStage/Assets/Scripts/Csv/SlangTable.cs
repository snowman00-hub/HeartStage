using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SlangTable : DataTable
{
    public static readonly string Unknown = "슬랭 없음";

    private readonly HashSet<string> table = new HashSet<string>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (ta == null)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
            Addressables.Release(handle);
            return;
        }

        var list = LoadCSV<SlangData>(ta.text);

        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item?.slang))
                continue;

            if (!table.Add(item.slang))
            {
                Debug.LogError($"중복된 슬랭: {item.slang}");
            }
        }

        Addressables.Release(handle);
    }

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Unknown;

        return table.Contains(key) ? key : Unknown;
    }

    /// 입력 문자열 안에 금칙어가 포함돼 있는지 검사.
    /// (닉네임/상태메시지에서 사용)
    public bool ContainsSlangIn(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || table.Count == 0)
            return false;

        string lower = text.ToLowerInvariant();

        foreach (var slang in table)
        {
            if (string.IsNullOrWhiteSpace(slang))
                continue;

            // 부분 문자열로 들어가면 금칙어로 판정
            if (lower.Contains(slang.ToLowerInvariant()))
                return true;
        }

        return false;
    }
}
