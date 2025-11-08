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
                // monsterData.Init(111011); // test 

                var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                monsterBehavior.Init(monsterData);

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                monsterNav.targetPoints = targetPoints;
                monsterNav.SetUp();

                monster.SetActive(true);
                Debug.Log(
                    $"소환된 몬스터 정보 - " +
                    $"ID: {monsterData.id}, " +
                    $"이름: {monsterData.monsterName}, " +
                    $"타입: {monsterData.monsterType}, " +
                    $"HP: {monsterData.hp}, " +
                    $"공격력: {monsterData.att}, " +
                    $"공격타입: {monsterData.attType}, " +
                    $"공격속도: {monsterData.attackSpeed}, " +
                    $"공격범위: {monsterData.attackRange}, " +
                    $"탄속: {monsterData.bulletSpeed}, " +
                    $"이동속도: {monsterData.moveSpeed}, " +
                    $"최소경험치: {monsterData.minExp}, " +
                    $"최대경험치: {monsterData.maxExp}"
                );
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