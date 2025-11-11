using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using TMPro.EditorUtilities;


[System.Serializable]
public struct WaveMonsterInfo
{
    public int monsterId;
    public int count;
    public int spawned;

    public WaveMonsterInfo(int id, int cnt)
    {
        monsterId = id;
        count = cnt;
        spawned = 0;
    }
}

public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AssetReference monsterPrefab;
    [SerializeField] private MonsterData monsterData;
    [SerializeField] private List<Transform> targetPoints;
    [SerializeField] private GameObject monsterProjectilePrefab;


    [Header("Wave")]
    [SerializeField] private int currentWaveId = 61034; // test
    [SerializeField] private int poolSize = 60; // wave pool size

    private StageWaveCSVData currentWaveData;
    private List<WaveMonsterInfo> waveMonstersToSpawn = new List<WaveMonsterInfo>();
    private int totalMonstersSpawned = 0;
    private bool isWaveActive = false;


    private const string MonsterProjectilePoolId = "MonsterProjectile"; // 임시 아이디 그냥 쭉 써도 될듯
    public static string GetMonsterProjectilePoolId() => MonsterProjectilePoolId;

    private List<GameObject> monsterList = new List<GameObject>();
    public List<GameObject> MonsterList => monsterList;


    private async void Start()
    {
        await InitializePool();
        await LoadWaveData();
        await StartWaveSpawning();
    }

    private async UniTask LoadWaveData()
    {
        while (DataTableManager.StageWaveTable == null)
        {
            await UniTask.Delay(100);
        }

        currentWaveData = DataTableManager.StageWaveTable.Get(currentWaveId);

        if (currentWaveData == null)
        {
            Debug.Log("웨이브 데이터가 null 입니다.");
            return;
        }

        SetUpWaveMonster();
        Debug.Log($"웨이브 로드: {currentWaveData.wave_name}, 총 {GetTotalWaveMonsterCount()}마리, 간격: {currentWaveData.spown_time}초");
    }

    private void SetUpWaveMonster()
    {
        waveMonstersToSpawn.Clear();

        if (currentWaveData.EnemyID1 > 0 && currentWaveData.EnemyCount1 > 0)
        {
            waveMonstersToSpawn.Add(new WaveMonsterInfo(currentWaveData.EnemyID1, currentWaveData.EnemyCount1));
        }

        if (currentWaveData.EnemyID2 > 0 && currentWaveData.EnemyCount2 > 0)
        {
            waveMonstersToSpawn.Add(new WaveMonsterInfo(currentWaveData.EnemyID2, currentWaveData.EnemyCount2));
        }

        if (currentWaveData.EnemyID3 > 0 && currentWaveData.EnemyCount3 > 0)
        {
            waveMonstersToSpawn.Add(new WaveMonsterInfo(currentWaveData.EnemyID3, currentWaveData.EnemyCount3));
        }
    }

    private int GetTotalWaveMonsterCount()
    {
        int total = 0;
        foreach (var waveMonster in waveMonstersToSpawn)
        {
            total += waveMonster.count;
        }
        return total;
    }

    private async UniTask StartWaveSpawning()
    {
        if (currentWaveData == null || waveMonstersToSpawn.Count == 0)
        {
            Debug.Log("웨이브 몬스터 정보가 없습니다.");
            return;
        }

        isWaveActive = true;
        totalMonstersSpawned = 0;

        float spawnInterval = currentWaveData.spown_time;

        while (isWaveActive && !IsWaveCompleted())
        {
            var nextMonster = GetNextMonsterToSpawn();
            if (nextMonster.HasValue)
            {
                SpawnMonster(nextMonster.Value.monsterId);
                UpdateSpawnCount(nextMonster.Value.monsterId);
                totalMonstersSpawned++;

                await UniTask.Delay((int)(spawnInterval * 1000));
            }

            else
            {
                break;
            }
        }

        if (IsWaveCompleted())
        {
            Debug.Log($"웨이브 {currentWaveData.wave_name} 완료!");
            isWaveActive = false;
        }
    }

    private bool SpawnMonster(int monsterId)
    {
        foreach (var monster in monsterList)
        {
            if (!monster.activeInHierarchy && monster != null)
            {
                var waveMonsterData = ScriptableObject.CreateInstance<MonsterData>();
                waveMonsterData.Init(monsterId); // MonsterTable에서 monsterId로 데이터 로드

                var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                monsterBehavior.Init(monsterData); // scriptablObject 데이터로 초기화
                //monsterBehavior.Init(waveMonsterData); // wave 데이터로 초기화

                if (!string.IsNullOrEmpty(waveMonsterData.image_AssetName))
                {
                    var spriteRenderer = monster.GetComponentInChildren<SpriteRenderer>();
                    if(spriteRenderer != null)
                    {
                        var texture = ResourceManager.Instance.Get<Texture2D>(waveMonsterData.image_AssetName);
                        if(texture != null)
                        {
                            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            spriteRenderer.sprite = sprite;
                        }
                        else
                        {
                            Debug.LogWarning($"몬스터 이미지 로드 실패: {waveMonsterData.image_AssetName}");
                        }
                    }
                }


                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                monsterNav.targetPoints = targetPoints;
                monsterNav.SetUp();

                monster.transform.position = GetRandomSpawnPosition();
                monster.SetActive(true);

                Debug.Log($"몬스터 소환: ID={monsterId}, 이름={waveMonsterData.monsterName}");
                return true;
            }
        }
        return false;
    }


    private WaveMonsterInfo? GetNextMonsterToSpawn()
    {
        for (int i = 0; i < waveMonstersToSpawn.Count; i++)
        {
            var monsterInfo = waveMonstersToSpawn[i];
            Debug.Log($"몬스터 확인: ID={monsterInfo.monsterId}, Spawned={monsterInfo.spawned}, Count={monsterInfo.count}");

            if (monsterInfo.spawned < monsterInfo.count)
            {
                Debug.Log($"다음 스폰 몬스터: ID={monsterInfo.monsterId}");

                return monsterInfo;
            }
        }
        Debug.Log("스폰할 몬스터가 없습니다.");

        return null;
    }

    private void UpdateSpawnCount(int monsterId)
    {
        for(int i = 0; i < waveMonstersToSpawn.Count; i++)
        {
            var monsterInfo = waveMonstersToSpawn[i];
            if(monsterInfo.monsterId == monsterId)
            {
                monsterInfo.spawned++;
                waveMonstersToSpawn[i] = monsterInfo;
                break;
            }
        }
    }

    private bool IsWaveCompleted()
    {
        foreach (var monsterInfo in waveMonstersToSpawn)
        {
           if (monsterInfo.spawned < monsterInfo.count)
            {
                return false;
            }
        }
        return true;
    }

    private void SpawnProjectile()
    {
        PoolManager.Instance.CreatePool(MonsterProjectilePoolId, monsterProjectilePrefab, 100);

        for (int i = 0; i < 100; i++)
        {
            var projectile = PoolManager.Instance.Get(MonsterProjectilePoolId);
            projectile.SetActive(false);
        }
    }

    private async UniTask InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();

            var handle = Addressables.InstantiateAsync(monsterPrefab, spawnPos, Quaternion.identity);
            await handle.Task;
            var monster = handle.Result;

            monsterList.Add(monster);
            monster.SetActive(false);
        }

        SpawnProjectile();
        await CreateMonsterPool();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int randomRange = Random.Range(0, Screen.width);
        int height = Screen.height;

        Vector3 screenPosition = new Vector3(randomRange, height, 0);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);
        spawnPos.z = 0f;

        return spawnPos;
    }

    private async UniTask CreateMonsterPool()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(monsterPrefab);
        await handle.Task;
        var monsterPrefabGO = handle.Result;

        // 몬스터 풀 생성 (DeceptionBossSKill에서 사용할 ID와 동일하게)
        PoolManager.Instance.CreatePool("121042", monsterPrefabGO, 10);
    }
    private void OnDestroy()
    {
        foreach (var monster in monsterList)
        {
            if (monster != null && monster.gameObject != null)
            {
                Addressables.ReleaseInstance(monster);
            }
        }
    }

    public async UniTask ChangeWave(int newWaveId)
    {
        isWaveActive = false;
        currentWaveId = newWaveId;

        foreach(var monster in monsterList)
        {
            if (monster.activeInHierarchy)
            {
                monster.SetActive(false);
            }
        }
        await LoadWaveData();
        await StartWaveSpawning();
    }
}