using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();
    private readonly List<AsyncOperationHandle> _handles = new List<AsyncOperationHandle>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public async UniTask PreloadLabelAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<Object>(label, asset =>
        {
            if (!_assetCache.ContainsKey(asset.name))
            {
                _assetCache[asset.name] = asset;
            }
        });

        _handles.Add(handle);
        await handle.Task;
        Debug.Log($"[ResourceManager] Label {label} 로드 완료 ({_assetCache.Count}개 캐싱됨)");
    }

    public T Get<T>(string assetName) where T : Object
    {
        if (_assetCache.TryGetValue(assetName, out var asset))
            return asset as T;

        Debug.LogWarning($"[ResourceManager] {assetName} 로드실패");
        return null;
    }

    public void ReleaseAll()
    {
        foreach (var handle in _handles)
            Addressables.Release(handle);

        _handles.Clear();
        _assetCache.Clear();
        Debug.Log("[ResourceManager] 모든 리소스 해제 완료");
    }
}
