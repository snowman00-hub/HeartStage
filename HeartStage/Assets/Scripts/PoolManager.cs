using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, IObjectPool<GameObject>> poolDict = new Dictionary<string, IObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void CreatePool(string assetName, int defaultCapacity = 30, int maxSize = 300)
    {
        if (poolDict.ContainsKey(assetName))
            return;

        var parentGo = new GameObject();
        parentGo.name = assetName;
        parentGo.transform.SetParent(transform);

        GameObject prefab = ResourceManager.Instance.Get<GameObject>(assetName);

        var pool = new ObjectPool<GameObject>(
           createFunc: () =>
           {
               var obj = Instantiate(prefab, parentGo.transform);
               return obj;
           },
           actionOnGet: obj => obj.SetActive(true),
           actionOnRelease: obj => obj.SetActive(false),
           actionOnDestroy: obj => Addressables.ReleaseInstance(obj),
           collectionCheck: false,
           defaultCapacity: defaultCapacity,
           maxSize: maxSize
        );

        poolDict.Add(assetName, pool);
        WarmUp(assetName, defaultCapacity);
    }

    public GameObject Get(string key)
    {
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogError($"[ObjectPool] 키 없음 {key}");
            return null;
        }

        return poolDict[key].Get();
    }

    private void WarmUp(string assetName, int count)
    {
        var pool = poolDict[assetName];

        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var obj = pool.Get();
            temp.Add(obj);
        }

        foreach (var obj in temp)
            pool.Release(obj);
    }

    public void Release(string assetName, GameObject obj)
    {
        if (poolDict.ContainsKey(assetName))
        {
            poolDict[assetName].Release(obj);
        }
        else
        {
            Debug.Log("파괴됨");
            Destroy(obj); // 혹시 모를 예외 대비
        }
    }
}