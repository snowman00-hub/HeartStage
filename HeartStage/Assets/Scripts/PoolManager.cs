//using Cysharp.Threading.Tasks;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.Pool;

//public class PoolManager : MonoBehaviour
//{
//    public static PoolManager Instance { get; private set; }

//    private Dictionary<string, IObjectPool<GameObject>> poolDict = new Dictionary<string, IObjectPool<GameObject>>();

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }
//    }

//    public async UniTask CreatePool(string key, AssetReferenceGameObject prefabRef, int defaultCapacity = 30, int maxSize = 300)
//    {
//        if (poolDict.ContainsKey(key))
//            return;

//        var parentGo = new GameObject();
//        parentGo.name = key;
//        parentGo.transform.SetParent(transform);

//        GameObject prefab = await prefabRef.LoadAssetAsync().Task;

//        var pool = new ObjectPool<GameObject>(
//           createFunc: () =>
//           {
//               var obj = Instantiate(prefab, parentGo.transform);
//               return obj;
//           },
//           actionOnGet: obj => obj.SetActive(true),
//           actionOnRelease: obj => obj.SetActive(false),
//           actionOnDestroy: obj => Addressables.ReleaseInstance(obj),
//           collectionCheck: false,
//           defaultCapacity: defaultCapacity,
//           maxSize: maxSize
//        );

//        poolDict.Add(key, pool);
//        WarmUp(key, prefabRef, defaultCapacity);
//    }

//    public GameObject Get(string key, AssetReferenceGameObject prefabRef)
//    {
//        if (!poolDict.ContainsKey(key))
//        {
//            CreatePool(key, prefabRef);
//        }

//        return poolDict[key].Get();
//    }

//    private void WarmUp(string key, AssetReferenceGameObject prefabRef, int count)
//    {
//        if (!poolDict.ContainsKey(key))
//            CreatePool(key, prefabRef);

//        var pool = poolDict[key];

//        List<GameObject> temp = new List<GameObject>();
//        for (int i = 0; i < count; i++)
//        {
//            var obj = pool.Get();
//            temp.Add(obj);
//        }

//        foreach (var obj in temp)
//            pool.Release(obj);
//    }

//    public void Release(string key, GameObject obj)
//    {
//        if (poolDict.ContainsKey(key))
//        {
//            poolDict[key].Release(obj);
//        }
//        else
//        {
//            Debug.Log("파괴됨");
//            Destroy(obj); // 혹시 모를 예외 대비
//        }
//    }
//}