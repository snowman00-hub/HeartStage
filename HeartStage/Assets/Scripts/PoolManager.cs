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

    // 오브젝트 풀 생성
    public void CreatePool(string id, GameObject prefab, int defaultCapacity = 30, int maxSize = 300)
    {
        if (poolDict.ContainsKey(id))
            return;

        var parentGo = new GameObject();
        parentGo.name = id;
        parentGo.transform.SetParent(transform);

        var pool = new ObjectPool<GameObject>
        (
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

        poolDict.Add(id, pool);
        WarmUp(id, defaultCapacity);
    }

    // 가져가기
    public GameObject Get(string id)
    {
        if (!poolDict.ContainsKey(id))
        {
            Debug.LogError($"[ObjectPool] 키 없음 {id}");
            return null;
        }

        return poolDict[id].Get();
    }

    // 해당 오브젝트를 풀로 복귀시키기
    public void Release(string id, GameObject obj)
    {
        if (poolDict.ContainsKey(id))
        {
            poolDict[id].Release(obj);
        }
        else
        {
            Debug.Log("파괴됨");
            Destroy(obj); // 혹시 모를 예외 대비
        }
    }

    // 오브젝트 풀 사이즈 증가
    private void WarmUp(string id, int count)
    {
        var pool = poolDict[id];

        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var obj = pool.Get();
            temp.Add(obj);
        }

        foreach (var obj in temp)
            pool.Release(obj);
    }
}