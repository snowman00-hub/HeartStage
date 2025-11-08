using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AssetReference monsterPrefab;
    [SerializeField] private MonsterData monsterData;
    [SerializeField] private Transform target;
    [SerializeField] private List<Transform> targetPoints;

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
                monsterData.Init(); 

                var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                monsterBehavior.Init(monsterData);

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                monsterNav.targetPoints = targetPoints;
                monsterNav.SetUp();

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