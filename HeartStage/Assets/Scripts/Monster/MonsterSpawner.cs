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
    [SerializeField] private Transform target;

    [Header("Field")]
    private int spawneTimeTest = 1;

    private List<GameObject> monsterList = new List<GameObject>();
    public List<GameObject> MonsterList => monsterList;

    [SerializeField] private int poolSize = 1;
    [SerializeField] private int maxMonsterCount = 1;

    private async void Start()
    {
        await InitializePool();
        await SpawnMonstersLoop(spawneTimeTest);
    }
    private async UniTask SpawnMonstersLoop(int spawneTimeTest)
    {
        while (true)
        {
            int activeMonsterCount = 0; 
            foreach (var monster in monsterList)
            {
                if (monster.activeInHierarchy)
                {
                    activeMonsterCount++;
                }
            }

            if (activeMonsterCount < maxMonsterCount)
            {
                await SpawnMonster(spawneTimeTest);
            }

            else
            {
                await UniTask.Delay(spawneTimeTest * 1000); 
            }
        }
    }

    private async UniTask InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            int randomRange = Random.Range(0, Screen.width);
            int height = Screen.height; 
            
            Vector3 screenPosition = new Vector3(randomRange, height - 100, 0);
            Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);
            spawnPos.z = 0f;

            var handle = Addressables.InstantiateAsync(monsterPrefab, spawnPos, Quaternion.identity);
            await handle.Task;
            var monster = handle.Result;

            monsterList.Add(monster);         

            monster.SetActive(false);
        }
    }

    private async UniTask SpawnMonster(int spawneTimeTest)
    {
        await UniTask.Delay(spawneTimeTest * 2000); // Test

        foreach (var monster in monsterList)
        {
            if (!monster.activeInHierarchy && monster != null)
            {
                var monsterDataController = monster.GetComponent<MonsterDataController>();
                monsterDataController.Init(monsterData); // Monster Init

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                monsterNav.SetUp(target);

                monster.SetActive(true);
                Debug.Log($"몬스터 활성화 완료 - HP: {monsterDataController.hp}");

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

    private void Update()
    {
        //foreach (var monster in monsterList)
        //{
        //    if (monster != null)
        //    {
        //        Debug.Log($"몬스터 활성화: {monster.activeInHierarchy}, 위치: {monster.transform.position}");
        //    }
        //}
    }
}
