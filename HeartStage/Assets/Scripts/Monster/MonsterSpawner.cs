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
    [SerializeField] private int poolSize = 150; // wave pool size
    [SerializeField] private int currentStageId = 601; // 현재 스테이지 ID

    // MonsterData SO 캐시 
    private Dictionary<int, MonsterData> monsterDataCache = new Dictionary<int, MonsterData>();

    // 몬스터 타입별 풀 관리 (주 사용)
    private Dictionary<int, List<GameObject>> monsterPools = new Dictionary<int, List<GameObject>>();

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

    private async void Start()
    {
        await InitializeAsync();
    }

    // 전체 초기화 프로세스 관리
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

    // 스테이지 데이터 로드 및 MonsterData SO 캐시 구축
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

        // 스테이지에 등장하는 모든 몬스터 ID에 대해 SO 캐시 및 CSV로 초기화
        var monsterIds = new HashSet<int>();
        foreach (var waveId in stageWaveIds)
        {
            var waveData = DataTableManager.StageWaveTable.Get(waveId);
            if (waveData != null)
            {
                if (waveData.EnemyID1 > 0) monsterIds.Add(waveData.EnemyID1);
                if (waveData.EnemyID2 > 0) monsterIds.Add(waveData.EnemyID2);
                if (waveData.EnemyID3 > 0) monsterIds.Add(waveData.EnemyID3);
            }
        }

        // MonsterData SO 로드 및 캐시에 저장 (게임 시작/스테이지 시작 시에만 CSV로 초기화)
        foreach (var monsterId in monsterIds)
        {
            var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{monsterId}");
            var monsterDataSO = await handle.Task;
            if (monsterDataSO != null)
            {
                monsterDataSO.InitFromCSV(monsterId); // Init 대신 InitFromCSV 사용
                monsterDataCache[monsterId] = monsterDataSO;
            }
        }
    }

    // 스테이지 내 모든 웨이브 진행 관리
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

    // 현재 웨이브 데이터 로드 및 UI 업데이트
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

    // 웨이브에 등장할 몬스터 정보 설정
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

    // 웨이브 총 몬스터 수 계산
    private int GetTotalWaveMonsterCount()
    {
        int total = 0;
        foreach (var waveMonster in waveMonstersToSpawn)
        {
            total += waveMonster.count;
        }
        return total;
    }

    // 웨이브 남은 몬스터 수 계산
    private int GetRemainingMonsterCount()
    {
        int remaining = 0;
        foreach (var waveMonster in waveMonstersToSpawn)
        {
            remaining += waveMonster.remainMonster;
        }
        return remaining;
    }

    // 웨이브 몬스터 스폰 프로세스 관리
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
                    bool spawnSuccess = SpawnMonster(nextMonster.Value.monsterId);
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

    // 웨이브 완료까지 대기
    private async UniTask WaitForWaveCompletion()
    {
        // 웨이브의 모든 몬스터가 처치될 때까지 대기
        while (GetRemainingMonsterCount() > 0)
        {
            await UniTask.Delay(100);
        }
    }

    // 다음 스테이지로 진행
    private async UniTask ProgressToNextStage()
    {
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

    // 다음 스테이지 정보 가져오기
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

    // 웨이브 스폰 완료 여부 확인
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

    private bool SpawnMonster(int monsterId)
    {
        if (!monsterPools.TryGetValue(monsterId, out var pool))
        {
            Debug.LogError($"몬스터 ID {monsterId}에 대한 풀이 없습니다.");
            return false;
        }

        foreach (var monster in pool)
        {
            if (monster != null && !monster.activeInHierarchy)
            {
                // 위치 설정
                bool isBoss = MonsterBehavior.IsBossMonster(monsterId);
                Vector3 spawnPos = isBoss ? GetBossSpawnPosition() : GetRandomSpawnPosition();
                monster.transform.position = spawnPos;

                if (monsterDataCache.TryGetValue(monsterId, out var monsterData))
                {
                    var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                    if (monsterBehavior != null)
                    {
                        // MonsterBehavior 초기화만 수행 (HealthBar는 Init 내부에서 처리됨)
                        monsterBehavior.Init(monsterData);
                    }
                }

                // 렌더러 활성화 및 오브젝트 활성화
                var renderers = monster.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                }

                monster.SetActive(true);
                return true;
            }
        }

        return false;
    }

    // 몬스터 스프라이트 설정
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

    // 다음에 스폰할 몬스터 선택
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

    // 몬스터 스폰 카운트 업데이트
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

    // 몬스터 오브젝트 풀 초기화 - 모든 초기화를 여기서 처리
    private async UniTask InitializePool()
    {
        // 데이터 테이블 로딩 대기
        while (DataTableManager.StageTable == null || DataTableManager.StageWaveTable == null)
        {
            await UniTask.Delay(100);
        }

        // 현재 스테이지에서 사용할 몬스터 ID 수집
        var monsterIds = new HashSet<int>();
        var stageData = DataTableManager.StageTable.GetStage(currentStageId);
        if (stageData != null)
        {
            var waveIds = DataTableManager.StageTable.GetWaveIds(currentStageId);
            foreach (var waveId in waveIds)
            {
                var waveData = DataTableManager.StageWaveTable.Get(waveId);
                if (waveData != null)
                {
                    if (waveData.EnemyID1 > 0) monsterIds.Add(waveData.EnemyID1);
                    if (waveData.EnemyID2 > 0) monsterIds.Add(waveData.EnemyID2);
                    if (waveData.EnemyID3 > 0) monsterIds.Add(waveData.EnemyID3);
                }
            }
        }

        // MonsterData 캐시 로드
        foreach (var monsterId in monsterIds)
        {
            var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{monsterId}");
            var monsterDataSO = await handle.Task;
            if (monsterDataSO != null)
            {
                monsterDataSO.InitFromCSV(monsterId); // Init 대신 InitFromCSV 사용 ✅
                monsterDataCache[monsterId] = monsterDataSO;
            }
        }

        // 몬스터 타입별 풀 생성 
        foreach (var monsterId in monsterIds)
        {
            bool isBoss = MonsterBehavior.IsBossMonster(monsterId);
            var prefab = isBoss ? bossMonsterPrefab : monsterPrefab;
            int poolCount = isBoss ? 1 : poolSize / monsterIds.Count;

            monsterPools[monsterId] = new List<GameObject>();

            for (int i = 0; i < poolCount; i++)
            {
                Vector3 offScreenPosition = new Vector3(-10000, -10000, 0);
                var handle = Addressables.InstantiateAsync(prefab, offScreenPosition, Quaternion.identity);
                var monster = await handle.Task;

                monster.SetActive(false);

                // 몬스터 완전 초기화 (한 번만 수행)
                if (monsterDataCache.TryGetValue(monsterId, out var monsterDataSO))
                {
                    var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                    monsterBehavior.Init(monsterDataSO); // 이건 MonsterBehavior 초기화 (문제없음)
                    monsterBehavior.SetMonsterSpawner(this);

                    var monsterMovement = monster.GetComponent<MonsterMovement>();
                    if (monsterMovement != null)
                    {
                        monsterMovement.Init(monsterDataSO, Vector3.down);
                    }

                    var healthBar = monster.GetComponentInChildren<HealthBar>();
                    if (healthBar == null)
                    {
                        Debug.LogWarning($"Monster {monsterId}에 HealthBar가 없습니다.");
                    }

                    // 스프라이트 설정
                    SetMonsterSprite(monster, monsterDataSO);
                }

                // 초기 상태 설정
                monster.SetActive(false);
                var renderers = monster.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = false;
                }

                monsterPools[monsterId].Add(monster);
            }
        }

        await CreateAllPools();
    }

    // 랜덤 스폰 위치 계산
    private Vector3 GetRandomSpawnPosition()
    {
        int randomRange = Random.Range(0, Screen.width);
        int height = Random.Range(Screen.height, Screen.height + 500);

        Vector3 screenPosition = new Vector3(randomRange, height, 0);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);
        spawnPos.z = 0f;

        return spawnPos;
    }

    // 보스 스폰 위치 계산
    private Vector3 GetBossSpawnPosition()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height + 200, 0f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        worldPos.z = 0f;

        return worldPos;
    }

    // 리소스 정리 - monsterPools 사용
    private void OnDestroy()
    {
        foreach (var pool in monsterPools.Values)
        {
            foreach (var monster in pool)
            {
                if (monster != null && monster.gameObject != null)
                {
                    Addressables.ReleaseInstance(monster);
                }
            }
        }

        monsterDataCache.Clear();
        monsterPools.Clear();
    }

    // 스테이지 변경
    public async UniTask ChangeStage(int newStageId)
    {
        isWaveActive = false;
        currentStageId = newStageId;

        // 모든 활성 몬스터 비활성화
        DeactivateAllMonsters();

        // 기존 풀과 캐시 정리
        foreach (var pool in monsterPools.Values)
        {
            foreach (var monster in pool)
            {
                if (monster != null && monster.gameObject != null)
                {
                    Addressables.ReleaseInstance(monster);
                }
            }
        }
        monsterPools.Clear();
        monsterDataCache.Clear();

        // 새 스테이지 초기화
        await InitializePool();
        await LoadStageData();
        await StartStageProgression();
    }

    // 모든 활성 몬스터 비활성화 - monsterPools 사용
    private void DeactivateAllMonsters()
    {
        foreach (var pool in monsterPools.Values)
        {
            foreach (var monster in pool)
            {
                if (monster.activeInHierarchy)
                {
                    monster.SetActive(false);
                }
            }
        }
    }

    // 추가 오브젝트 풀 생성
    private async UniTask CreateAllPools()
    {
        PoolManager.Instance.CreatePool(MonsterProjectilePoolId, monsterProjectilePrefab, 100);

        var handle = Addressables.LoadAssetAsync<GameObject>(monsterPrefab);
        var monsterPrefabGO = await handle.Task;
        PoolManager.Instance.CreatePool("21101", monsterPrefabGO, 15); // test
    }

    // 스테이지 UI 업데이트
    private void UpdateStageUI()
    {
        if (StageManager.Instance != null)
        {
            var (stageNumber, waveOrder) = GetStageDisplayInfo(currentStageId, currentWaveIndex + 1);
            StageManager.Instance.SetWaveInfo(stageNumber, waveOrder);
            StageManager.Instance.RemainMonsterCount = GetRemainingMonsterCount();
        }
    }

    // 스테이지 표시 정보 계산
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

    // 몬스터 사망 처리
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