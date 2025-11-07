using System.Collections.Generic;
using UnityEngine;
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

    public void CreatePool(string key, GameObject prefab, int defaultCapacity = 30, int maxSize = 300)
    {
        if (poolDict.ContainsKey(key))
            return;

        var parentGo = new GameObject();
        parentGo.name = key;
        parentGo.transform.SetParent(transform);

        var pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab, parentGo.transform),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        poolDict.Add(key, pool);
        WarmUp(key, prefab, defaultCapacity);
    }

    public GameObject Get(string key, GameObject prefab)
    {
        if (!poolDict.ContainsKey(key))
        {
            CreatePool(key, prefab);
        }

        return poolDict[key].Get();
    }

    private void WarmUp(string key, GameObject prefab, int count)
    {
        if (!poolDict.ContainsKey(key))
            CreatePool(key, prefab);

        var pool = poolDict[key];

        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var obj = pool.Get();
            temp.Add(obj);
        }

        foreach (var obj in temp)
            pool.Release(obj);
    }

    public void Release(string key, GameObject obj)
    {
        if (poolDict.ContainsKey(key))
        {
            poolDict[key].Release(obj);
        }
        else
        {
            Debug.Log("파괴됨");
            Destroy(obj); // 혹시 모를 예외 대비
        }
    }
}