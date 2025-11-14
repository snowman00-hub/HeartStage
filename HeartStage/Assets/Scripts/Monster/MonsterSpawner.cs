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

    [Header("Field")]
    [SerializeField] private int poolSize = 60; // wave pool size
    [SerializeField] private int currentStageId = 601; // 현재 스테이지 ID

    private StageCsvData currentStageData;
    private List<int> stageWaveIds = new List<int>();
    private int currentWaveIndex = 0;
    private StageWaveCSVData currentWaveData;

    private List<WaveMonsterInfo> waveMonstersToSpawn = new List<WaveMonsterInfo>();
    private int totalMonstersSpawned = 0;
    private bool isWaveActive = false;
    public bool isInitialized = false;
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    public static string GetMonsterProjectilePoolId() => MonsterProjectilePoolId;

    private List<GameObject> monsterList = new List<GameObject>();
    private List<GameObject> bossMonsterList = new List<GameObject>();

    public List<GameObject> MonsterList => monsterList;

    private async void Start()
    {
        await InitializeAsync();
    }

    private async UniTask InitializeAsync()
    {
        try
        {
            await InitializePool();
            await LoadStageData();
            isInitialized = true;

            await StartStageProgression();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MonsterSpawner 초기화 실패: {e.Message}");
        }
    }

    private async UniTask LoadStageData()
    {
        // 데이터 테이블 로딩 대기
        while (DataTableManager.StageTable == null || DataTableManager.StageWaveTable == null)
        {
            await UniTask.Delay(100);
        }

        currentStageData = DataTableManager.StageTable.GetStage(currentStageId);
        if (currentStageData == null)
        {
            Debug.LogError($"스테이지 데이터를 찾을 수 없음: {currentStageId}");
            return;
        }

        // 스테이지의 웨이브 ID 목록 가져오기
        stageWaveIds = DataTableManager.StageTable.GetWaveIds(currentStageId);
        currentWaveIndex = 0;

        Debug.Log($"스테이지 로드: {currentStageData.stage_name}, 웨이브 수: {stageWaveIds.Count}");
    }

    private async UniTask StartStageProgression()
    {
        if (!isInitialized)
            return;

        if (stageWaveIds.Count == 0)
        {
            Debug.LogError("스테이지에 웨이브가 없습니다.");
            return;
        }

        // 스테이지의 모든 웨이브 진행
        for (currentWaveIndex = 0; currentWaveIndex < stageWaveIds.Count; currentWaveIndex++)
        {
            LoadCurrentWave();
            if (currentWaveData != null)
            {
                await StartWaveSpawning();
                await WaitForWaveCompletion();

                Debug.Log($"웨이브 {currentWaveData.wave_name} 완료!");

                // 마지막 웨이브가 아니면 잠시 대기
                if (currentWaveIndex < stageWaveIds.Count - 1)
                {
                    await UniTask.Delay(2000);
                }
            }
        }

        Debug.Log($"스테이지 {currentStageData.stage_name} 완료!");
        await ProgressToNextStage();
    }

    private void LoadCurrentWave()
    {
        int currentWaveId = stageWaveIds[currentWaveIndex];
        currentWaveData = DataTableManager.StageWaveTable.Get(currentWaveId);

        if (currentWaveData == null)
        {
            Debug.LogError($"웨이브 데이터를 찾을 수 없음: {currentWaveId}");
            return;
        }

        SetUpWaveMonster();
        UpdateStageUI();
        Debug.Log($"웨이브 로드: {currentWaveData.wave_name}, 총 {GetTotalWaveMonsterCount()}마리, 간격: {currentWaveData.enemy_spown_time}초");
    }

    private void SetUpWaveMonster()
    {
        waveMonstersToSpawn.Clear();

        var enemies = new[]
        {
            (currentWaveData.EnemyID1, currentWaveData.EnemyCount1),
            (currentWaveData.EnemyID2, currentWaveData.EnemyCount2),
            (currentWaveData.EnemyID3, currentWaveData.EnemyCount3)
        };

        foreach (var (enemyId, enemyCount) in enemies)
        {
            if (enemyId > 0 && enemyCount > 0)
            {
                waveMonstersToSpawn.Add(new WaveMonsterInfo(enemyId, enemyCount));
            }
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
            Debug.LogError("웨이브 몬스터 정보가 없습니다.");
            return;
        }

        isWaveActive = true;
        totalMonstersSpawned = 0;
        float spawnInterval = currentWaveData.enemy_spown_time;

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
                }
                else
                {
                    break;
                }
            }
            await UniTask.Delay((int)(spawnInterval * 1000));
        }

        Debug.Log($"웨이브 {currentWaveData.wave_name} 스폰 완료!");
    }

    private async UniTask WaitForWaveCompletion()
    {
        // 웨이브의 모든 몬스터가 처치될 때까지 대기
        while (GetRemainingMonsterCount() > 0)
        {
            await UniTask.Delay(100);
        }
    }

    private async UniTask ProgressToNextStage()
    {
        // 다음 스테이지로 이동하는 로직
        // 현재는 로그만 출력하지만, 실제로는 다음 스테이지 ID를 계산하여 이동
        var nextStage = GetNextStage();
        if (nextStage != null)
        {
            Debug.Log($"다음 스테이지로 진행: {nextStage.stage_name}");
            await ChangeStage(nextStage.stage_ID);
        }
        else
        {
            Debug.Log("모든 스테이지 완료!");
        }
    }

    private StageCsvData GetNextStage()
    {
        var orderedStages = DataTableManager.StageTable.GetOrderedStages();
        int currentIndex = orderedStages.FindIndex(s => s.stage_ID == currentStageId);

        if (currentIndex >= 0 && currentIndex < orderedStages.Count - 1)
        {
            return orderedStages[currentIndex + 1];
        }

        return null;
    }

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

    private async UniTask<bool> SpawnMonster(int monsterId)
    {
        var targetList = MonsterBehavior.IsBossMonster(monsterId) ? bossMonsterList : monsterList;

        foreach (var monster in targetList)
        {
            if (monster != null && !monster.activeInHierarchy)
            {
                Vector3 spawnPos = MonsterBehavior.IsBossMonster(monsterId) ? GetBossSpawnPosition() : GetRandomSpawnPosition();

                monster.transform.position = spawnPos;

                var renderers = monster.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                }

                monster.SetActive(true);

                try
                {
                    var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{monsterId}");
                    var monsterDataSO = await handle.Task;

                    if (monsterDataSO != null)
                    {
                        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                        monsterBehavior.Init(monsterDataSO);
                        monsterBehavior.SetMonsterSpawner(this);

                        var monsterMovement = monster.GetComponent<MonsterMovement>();
                        if(monsterMovement != null)
                        {
                            monsterMovement.Init(monsterDataSO, Vector3.down); // 아래 방향으로 이동
                        }

                        SetMonsterSprite(monster, monsterDataSO);
                        //Debug.Log($"몬스터 소환: ID={monsterId}, 이름={monsterDataSO.monsterName}");
                    }
                    else
                    {
                        Debug.LogError($"MonsterData_{monsterId}를 로드할 수 없습니다.");
                        return false;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"MonsterData_{monsterId} 로드 실패: {e.Message}");
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
        // 일반 몬스터 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            var handle = Addressables.InstantiateAsync(monsterPrefab, GetRandomSpawnPosition(), Quaternion.identity);
            var monster = await handle.Task;

            monster.SetActive(false);

            var renderers = monster.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            monsterList.Add(monster);
        }

        // 보스 몬스터 풀 생성
        var bossHandle = Addressables.InstantiateAsync(bossMonsterPrefab, GetBossSpawnPosition(), Quaternion.identity);
        var bossMonster = await bossHandle.Task;

        bossMonster.SetActive(false);

        bossMonsterList.Add(bossMonster);

        var bossRenderers = bossMonster.GetComponentsInChildren<Renderer>();
        foreach (var renderer in bossRenderers)
        {
            renderer.enabled = false;
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



        Debug.LogWarning("보스 스폰 위치를 찾을 수 없습니다!");
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

        foreach (var bossMonster in bossMonsterList)
        {
            if (bossMonster != null && bossMonster.gameObject != null)
            {
                Addressables.ReleaseInstance(bossMonster);
            }
        }
    }

    public async UniTask ChangeStage(int newStageId)
    {
        isWaveActive = false;
        currentStageId = newStageId;

        // 모든 활성 몬스터 비활성화
        DeactivateAllMonsters();

        await LoadStageData();
        await StartStageProgression();
    }

    private void DeactivateAllMonsters()
    {
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
    }

    private async UniTask CreateAllPools()
    {
        PoolManager.Instance.CreatePool(MonsterProjectilePoolId, monsterProjectilePrefab, 100);

        var handle = Addressables.LoadAssetAsync<GameObject>(monsterPrefab);
        var monsterPrefabGO = await handle.Task;
        PoolManager.Instance.CreatePool("21101", monsterPrefabGO, 15); // test
    }

    private void UpdateStageUI()
    {
        if (StageManager.Instance != null)
        {
            var (stageNumber, waveOrder) = GetStageDisplayInfo(currentStageId, currentWaveIndex + 1);
            StageManager.Instance.SetWaveInfo(stageNumber, waveOrder);
            StageManager.Instance.RemainMonsterCount = GetRemainingMonsterCount();
        }
    }

    private (int stageNumber, int waveOrder) GetStageDisplayInfo(int stageId, int waveIndex)
    {
        if (stageId == 601)
        {
            return (0, waveIndex); 
        }
        else if (stageId >= 611 && stageId <= 619)
        {
            int stageNumber = stageId - 610; 
            return (stageNumber, waveIndex);
        }

        // 기본값
        return (stageId % 100, waveIndex);
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

        UpdateStageUI();
    }
}