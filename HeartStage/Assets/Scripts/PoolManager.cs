using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, IObjectPool<GameObject>> poolDict = new Dictionary<string, IObjectPool<GameObject>>();
    private bool isDestroying = false; // 파괴중인지 여부 확인용 플래그

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
           actionOnRelease: obj =>
           {
               if (obj == null || obj.Equals(null)) 
                   return;
               obj.SetActive(false);
           },
           actionOnDestroy: obj => Destroy(obj),
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
        if(isDestroying)
        {
            return null;
        }

        if (string.IsNullOrEmpty(id)) 
        {
            Debug.LogError("[ObjectPool] ID가 null 또는 empty입니다");
            return null;
        }

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
        if (obj == null || obj.Equals(null))
            return;

        if (!poolDict.ContainsKey(id))
        {
            Destroy(obj);
            return;
        }

        poolDict[id].Release(obj);
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

    public void CleanupForSceneTransition() // 이 메서드 추가
    {
        isDestroying = true;
    }

    private void OnDestroy()
    {
        isDestroying = true;
    }
}