using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.CompilerServices;
using NUnit.Framework;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AssetReference monsterPrefab;
    [SerializeField] private MonsterData monsterData;

    [Header("Field")]
    private int spawneTimeTest = 1;
    private List<GameObject> monsterList = new List<GameObject>();
    public List<GameObject> MonsterList => monsterList;
    [SerializeField] private int poolSize = 20;

    private async void Start()
    {
        await InitializePool();
        await SpawnMonstersLoop(spawneTimeTest);
    }
    private async UniTask SpawnMonstersLoop(int spawneTimeTest)
    {
        while (true)
        {
            await SpawnMonster(spawneTimeTest);
        }
    }

    private async UniTask InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            int randomRange = Random.Range(0, Screen.width);
            int randomHeight = Random.Range(0, Screen.height); // Test
            int height = Screen.height; 
            
            Vector3 screenPosition = new Vector3(randomRange, randomHeight, Camera.main.nearClipPlane);
            Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);

            var handle = Addressables.InstantiateAsync(monsterPrefab, spawnPos, Quaternion.identity);
            await handle.Task;
            var monster = handle.Result;

            monsterList.Add(monster);            

            var monsterDataController = monster.GetComponent<MonsterDataController>();
            if(monsterDataController != null)
            {
                monsterDataController.Init(monsterData);
            }

            monster.SetActive(false);
        }
    }

    private async UniTask SpawnMonster(int spawneTimeTest)
    {
        await UniTask.Delay(spawneTimeTest * 2000); // Test

        foreach(var monster in monsterList)
        {
            if (!monster.activeInHierarchy && monster != null)
            {
                var monsterDataController = monster.GetComponent<MonsterDataController>();
                monsterDataController.Init(monsterData); // Monster Init

                monster.SetActive(true);
                return;
            }
        }
    }
    private void OnDestroy()
    {
        foreach (var monster in monsterList)
        {
            Addressables.ReleaseInstance(monster);
        }
    }
}
