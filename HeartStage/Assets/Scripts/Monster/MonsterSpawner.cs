using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

[System.Serializable]
public struct WaveMonsterInfo
{
    public int monsterId;
    public int count;
    public int spawned;
    public int remainMonster;

    public WaveMonsterInfo(int id, int cnt)
    {
        monsterId = id;
        count = cnt;
        spawned = 0;
        remainMonster = cnt;
    }
}
public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AssetReference monsterPrefab;
    [SerializeField] private AssetReference bossMonsterPrefab;
    [SerializeField] private GameObject monsterProjectilePrefab;
    [SerializeField] private GameObject testMonsterPrefab; // test

    [Header("Field")]
    [SerializeField] private int poolSize = 60; // wave pool size
    [SerializeField] private int currentWaveId = 61011; // test  61034
    [SerializeField] private int testMonsterId = 111011; // test
    [SerializeField] private int testSpawnManyCount = 200; // test

    private StageWaveCSVData currentWaveData;
    private List<WaveMonsterInfo> waveMonstersToSpawn = new List<WaveMonsterInfo>();
    private int totalMonstersSpawned = 0;
    private bool isWaveActive = false;

    private const string MonsterProjectilePoolId = "MonsterProjectile"; // 임시 아이디 그냥 쭉 써도 될듯
    public static string GetMonsterProjectilePoolId() => MonsterProjectilePoolId;

    private List<GameObject> monsterList = new List<GameObject>();
    private List<GameObject> bossMonsterList = new List<GameObject>(); // 보스 풀

    public List<GameObject> MonsterList => monsterList;

    private async void Start()
    {
        await InitializePool();
        await LoadWaveData();
        await StartWaveSpawning();
        //SpawnManyMonster().Forget();
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
        UpdateStageUI();
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

    // 남은 몬스터 수 계산 (새로 추가)
    private int GetRemainingMonsterCount()
    {
        int remaining = 0;
        foreach (var waveMonster in waveMonstersToSpawn)
        {
            remaining += waveMonster.remainMonster;
        }
        return remaining;
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

        while (isWaveActive && !IsWaveSpawnCompleted())
        {
            for (int i = 0; i < 2; i++)
            {
                var nextMonster = GetNextMonsterToSpawn();
                if (nextMonster.HasValue)
                {
                    bool spawnSuccess = await SpawnMonster(nextMonster.Value.monsterId);
                    if (spawnSuccess)
                    {
                        UpdateSpawnCount(nextMonster.Value.monsterId);
                        totalMonstersSpawned++;
                    }
                    await UniTask.Delay((int)(spawnInterval * 1000));
                }
                else
                {
                    break;
                }
            }
        }

        Debug.Log($"웨이브 {currentWaveData.wave_name} 스폰 완료!");
    }

    // 웨이브 완료 체크
    private void CheckWaveCompletion()
    {
        int totalRemaining = GetRemainingMonsterCount();

        if (totalRemaining <= 0 && IsWaveSpawnCompleted())
        {
            Debug.Log($"웨이브 {currentWaveData.wave_name} 완료!");
            ProgressToNextWave().Forget();
        }
    }

    // 다음 웨이브로 진행
    private async UniTaskVoid ProgressToNextWave()
    {
        isWaveActive = false;

        // 다음 웨이브 ID 계산
        int nextWaveId = GetNextWaveId();

        if (nextWaveId > 0)
        {
            Debug.Log($"다음 웨이브로 진행: {nextWaveId}");
            await UniTask.Delay(2000); // 2초 대기
            await ChangeWave(nextWaveId);
        }
        else
        {
            Debug.Log("모든 웨이브 완료!");
        }
    }

    // 다음 웨이브 ID 계산
    private int GetNextWaveId()
    {
        var nextWaveData = DataTableManager.StageWaveTable.GetNextWave(currentWaveId);

        if (nextWaveData != null)
        {
            return nextWaveData.wave_id;
        }

        return -1;
    }

    // 스폰 완료 체크를 별도 메서드로 분리
    private bool IsWaveSpawnCompleted()
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

    private async UniTaskVoid SpawnManyMonster()
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{testMonsterId}"); // test
            var monsterData = await handle.Task;

            for (int i = 0; i < testSpawnManyCount; i++)
            {
                var spawnPos = GetRandomSpawnPosition();
                var monster = Instantiate(testMonsterPrefab, spawnPos, Quaternion.identity);

                var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                if (monsterBehavior != null)
                {
                    monsterBehavior.Init(monsterData);
                }

                SetMonsterSprite(monster, monsterData);

                var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                if (monsterNav != null)
                {
                    monsterNav.ApplyMoveSpeed(monsterData.moveSpeed);
                    monsterNav.SetUp();
                }
            }
        }

        catch (System.Exception e)
        {
            Debug.LogError($"SpawnManyMonster Addressables 로드 실패: {e.Message}");
        }

    }

    private async UniTask<bool> SpawnMonster(int monsterId)
    {
        var targetList = MonsterBehavior.IsBossMonster(monsterId) ? bossMonsterList : monsterList;

        foreach (var monster in targetList)
        {
            if (monster != null && !monster.activeInHierarchy)
            {
                Vector3 spawnPos = MonsterBehavior.IsBossMonster(monsterId) ? GetBossSpawnPosition() : GetRandomSpawnPosition();

                monster.transform.position = spawnPos;
                monster.SetActive(true);

                try
                {
                    // Addressables로 MonsterData SO 로드
                    var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{monsterId}");
                    var monsterDataSO = await handle.Task;

                    if (monsterDataSO != null)
                    {
                        // var csvData = DataTableManager.MonsterTable.Get(monsterId);
                        // monsterDataSO.UpdateData(csvData); // 

                        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                        monsterBehavior.Init(monsterDataSO);

                        // 몬스터에게 MonsterSpawner 참조 설정 (새로 추가)
                        monsterBehavior.SetMonsterSpawner(this);

                        SetMonsterSprite(monster, monsterDataSO);

                        var monsterNav = monster.GetComponent<MonsterNavMeshAgent>();
                        if (monsterNav != null)
                        {
                            monsterNav.ApplyMoveSpeed(monsterDataSO.moveSpeed);
                            monsterNav.SetUp();
                        }

                        Debug.Log($"몬스터 소환 (SO 원본값): ID={monsterId}, 이름={monsterDataSO.monsterName}, HP={monsterDataSO.hp}");
                    }
                    else
                    {
                        Debug.LogError($"Addressables에서 MonsterData_{monsterId}를 로드할 수 없습니다.");
                        return false;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"MonsterData_{monsterId} Addressables 로드 실패: {e.Message}");
                    return false;
                }

                return true;
            }
        }
        return false;
    }

    public static void SetMonsterSprite(GameObject monster, MonsterData monsterData)
    {
        if (!string.IsNullOrEmpty(monsterData.image_AssetName))
        {
            var spriteRenderer = monster.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var texture = ResourceManager.Instance.Get<Texture2D>(monsterData.image_AssetName);
                if (texture != null)
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    spriteRenderer.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"몬스터 이미지 로드 실패: {monsterData.image_AssetName}");
                }
            }
        }
    }

    private WaveMonsterInfo? GetNextMonsterToSpawn()
    {
        for (int i = 0; i < waveMonstersToSpawn.Count; i++)
        {
            var monsterInfo = waveMonstersToSpawn[i];
            if (monsterInfo.spawned < monsterInfo.count)
            {
                return monsterInfo;
            }
        }

        Debug.Log("스폰할 몬스터가 없습니다.");
        return null;
    }

    private void UpdateSpawnCount(int monsterId)
    {
        for (int i = 0; i < waveMonstersToSpawn.Count; i++)
        {
            var monsterInfo = waveMonstersToSpawn[i];
            if (monsterInfo.monsterId == monsterId)
            {
                monsterInfo.spawned++;
                waveMonstersToSpawn[i] = monsterInfo;
                break;
            }
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

        for (int i = 0; i < 1; i++)
        {
            Vector3 spawnPos = GetBossSpawnPosition();

            var handle = Addressables.InstantiateAsync(bossMonsterPrefab, spawnPos, Quaternion.identity);
            await handle.Task;
            var bossMonster = handle.Result;

            bossMonsterList.Add(bossMonster);
            bossMonster.SetActive(false);
        }
        await CreateAllPools();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int randomRange = Random.Range(0, Screen.width);
        int height = Random.Range(Screen.height, Screen.height + 200);

        Vector3 screenPosition = new Vector3(randomRange, height, 0);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);
        spawnPos.z = 0f;

        return spawnPos;
    }

    private Vector3 GetBossSpawnPosition()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height, 0f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        worldPos.z = 0f;

        // NavMesh 위의 유효한 위치 찾기
        NavMeshHit hit;
        if (NavMesh.SamplePosition(worldPos, out hit, 10f, NavMesh.AllAreas))
        {
            Debug.Log($"보스 몬스터 화면 중앙 상단 스폰: {hit.position}");
            return hit.position;
        }

        Debug.LogWarning("보스 스폰을 위한 NavMesh 위치를 찾을 수 없습니다!");
        return worldPos;
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

        foreach (var monster in monsterList)
        {
            if (monster.activeInHierarchy)
            {
                monster.SetActive(false);
            }
        }

        foreach (var bossMonster in bossMonsterList)
        {
            if (bossMonster.activeInHierarchy)
            {
                bossMonster.SetActive(false);
            }
        }

        await LoadWaveData();
        await StartWaveSpawning();
    }

    private async UniTask CreateAllPools()
    {
        // 몬스터 발사체 풀
        PoolManager.Instance.CreatePool(MonsterProjectilePoolId, monsterProjectilePrefab, 100);

        // DeceptionBossSkill용 몬스터 풀 (일반 몬스터 프리팹 사용)
        var handle = Addressables.LoadAssetAsync<GameObject>(monsterPrefab);
        await handle.Task;
        var monsterPrefabGO = handle.Result;

        PoolManager.Instance.CreatePool("121042", monsterPrefabGO, 15);
    }

    // UI 업데이트
    private void UpdateStageUI()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.WaveCount = currentWaveData.wave_count;
            StageManager.Instance.RemainMonsterCount = GetRemainingMonsterCount();
        }
    }

    public void OnMonsterDied(int monsterId)
    {
        for (int i = 0; i < waveMonstersToSpawn.Count; i++)
        {
            var monsterInfo = waveMonstersToSpawn[i];
            if (monsterInfo.monsterId == monsterId && monsterInfo.remainMonster > 0)
            {
                monsterInfo.remainMonster--;
                waveMonstersToSpawn[i] = monsterInfo;
                break;
            }
        }

        // UI 업데이트 및 웨이브 완료 체크
        UpdateStageUI();
        CheckWaveCompletion();
    }
}