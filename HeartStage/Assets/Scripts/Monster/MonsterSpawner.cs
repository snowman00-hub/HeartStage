using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading;

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

[System.Serializable]
public struct SpawnRequest // 재시도 요청용 구조체
{
    public int monsterId;
    public float requestTime;
    public int retryCount;

    public SpawnRequest(int id)
    {
        monsterId = id;
        requestTime = Time.time;
        retryCount = 0;
    }
}


public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AssetReference monsterPrefab;
    [SerializeField] private AssetReference bossMonsterPrefab;
    [SerializeField] private GameObject monsterProjectilePrefab;

    [Header("Field")]
    private int poolSize = 250; // wave pool size    
    private int currentStageId;
    private int spawnedMonsterCount = 3;

    [Header("SpawnMonster")]
    [SerializeField] private int maxSpawnRetries = 10;
    private float spawnRadius = 1f; // 스폰 위치 충돌 검사 반경
    [SerializeField] private float spawnTime = 0.5f; // 대기열 처리 간격 

    private Queue<SpawnRequest> spawnQueue = new Queue<SpawnRequest>(); // 대기열
    private bool isProcessingQueue = false; // 대기열 처리 상태
    private UniTaskCompletionSource queueProcessCTS; // 대기열 완료 CTS

    // MonsterData SO 캐시 
    private Dictionary<int, MonsterData> monsterDataCache = new Dictionary<int, MonsterData>();

    // 몬스터 타입별 풀 관리 (주 사용)
    private Dictionary<int, List<GameObject>> monsterPools = new Dictionary<int, List<GameObject>>();

    private StageCSVData currentStageData;
    private List<int> stageWaveIds = new List<int>();
    private int currentWaveIndex = 0;
    private StageWaveCSVData currentWaveData;

    private List<WaveMonsterInfo> waveMonstersToSpawn = new List<WaveMonsterInfo>();
    private int totalMonstersSpawned = 0;
    private bool isWaveActive = false;
    public bool isInitialized = false;
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    public static string GetMonsterProjectilePoolId() => MonsterProjectilePoolId;


    //웨이브 정지용 CTS
    private CancellationTokenSource stageCTS;

    private async void Start()
    {
        // 테스트용: 강제로 튜토리얼로 리셋
        //PlayerPrefs.SetInt("SelectedStageID", 601);
        //PlayerPrefs.Save();

        currentStageId = PlayerPrefs.GetInt("SelectedStageID", 601); // 기본값은 601 (튜토리얼)
        await InitializeAsync();
    }

    // 전체 초기화 프로세스 관리
    private async UniTask InitializeAsync()
    {
        try
        {
            if (this == null || gameObject == null) return;

            await InitializePool();
            await LoadStageData();

            isInitialized = true;

            // 이전 루프 있으면 끊고 새로 시작
            stageCTS?.Cancel();
            stageCTS?.Dispose();
            stageCTS = new CancellationTokenSource();

            await StartStageProgression(stageCTS.Token);
        }
        catch (System.Exception e)
        {
            Debug.Log($"MonsterSpawner 초기화 실패: {e.Message}");
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
            return;
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetCurrentStageData(currentStageData);
            StageManager.Instance.SetBackgroundByStageData(currentStageData);
        }


        // 스테이지의 웨이브 ID 목록 가져오기
        stageWaveIds = DataTableManager.StageTable.GetWaveIds(currentStageId);
        currentWaveIndex = 0;

        // 웨이브가 없을 경우 처리
        if (stageWaveIds.Count == 0)
        {
            return;
        }

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
    private async UniTask StartStageProgression(CancellationToken token)
    {
        if (!isInitialized) return;
        if (stageWaveIds.Count == 0) return;

        for (currentWaveIndex = 0; currentWaveIndex < stageWaveIds.Count; currentWaveIndex++)
        {
            token.ThrowIfCancellationRequested();

            LoadCurrentWave();
            if (currentWaveData != null)
            {
                await StartWaveSpawning(token);
                await WaitForWaveCompletion(token);

                if (currentWaveIndex < stageWaveIds.Count - 1)
                    await UniTask.Delay(2000, cancellationToken: token);
            }
        }

        ProgressToNextStage();
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

        // 새 웨이브 시작 시 대기열 초기화 
        ClearSpawnQueue();

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
    private async UniTask StartWaveSpawning(CancellationToken token)
    {
        if (currentWaveData == null || waveMonstersToSpawn.Count == 0) return;

        isWaveActive = true;
        totalMonstersSpawned = 0;
        float spawnInterval = currentWaveData.enemy_spown_time;

        while (isWaveActive && !IsWaveSpawnCompleted())
        {
            token.ThrowIfCancellationRequested();

            for (int i = 0; i < spawnedMonsterCount; i++)
            {
                var nextMonster = GetNextMonsterToSpawn();
                if (!nextMonster.HasValue) break;

                if (SpawnMonster(nextMonster.Value.monsterId))
                {
                    UpdateSpawnCount(nextMonster.Value.monsterId);
                    totalMonstersSpawned++;
                }
            }

            await UniTask.Delay((int)(spawnInterval * 1000), cancellationToken: token);
        }
    }

    private async UniTask WaitForWaveCompletion(CancellationToken token)
    {
        while (GetRemainingMonsterCount() > 0)
        {
            token.ThrowIfCancellationRequested();
            await UniTask.Delay(100, cancellationToken: token);
        }
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
    private void ProgressToNextStage()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.CompleteStage();
        }
        else
        {
            Debug.Log("StageManager.Instance가 null입니다!");
        }
    }

    // 다음 스테이지 정보 가져오기
    public StageCSVData GetNextStage()
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
            return false;
        }

        foreach (var monster in pool)
        {
            if (monster != null && !monster.activeInHierarchy)
            {
                // 위치 설정
                bool isBoss = MonsterBehavior.IsBossMonster(monsterId);
                Vector3? safePos = FindSafeSpawnPosition(isBoss);

                if(safePos.HasValue)
                {
                    // 안전한 위치면 스폰
                    monster.transform.position = safePos.Value;

                    if (monsterDataCache.TryGetValue(monsterId, out var monsterData))
                    {
                        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                        if (monsterBehavior != null)
                        {
                            monsterBehavior.Init(monsterData);
                        }
                    }
                    else
                    {
                        return false;
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

                else
                {
                    AddToSpawnQueue(monsterId);
                    return false;
                }
            }
        }
        return false;
    }



    // 테스트용 몬스터 소환 메서드 (10마리 일괄 소환)
    public async UniTask SpawnTestMonsters(int monsterId, int count)
    {
        if (!isInitialized)
        {
            return;
        }

        if (!monsterPools.ContainsKey(monsterId))
        {
            return;
        }

        Debug.Log($"테스트 소환 시작: 몬스터 ID {monsterId}를 {count}마리 소환");

        // 모든 몬스터를 대기열에 추가
        for (int i = 0; i < count; i++)
        {
            AddToSpawnQueue(monsterId);

            // 짧은 딜레이로 순차적 추가 (대기열 오버플로우 방지)
            await UniTask.Delay(50);
        }

        //Debug.Log($"테스트 소환 요청 완료: {count}마리가 대기열에 추가되었습니다. 순차적으로 스폰됩니다.");
    }

    private void AddVisualChild(GameObject monster, MonsterData monsterData)
    {
        if (string.IsNullOrEmpty(monsterData.prefab1))
        {
            Debug.LogWarning($"Monster {monsterData.id}의 prefab1이 설정되지 않았습니다.");
            return;
        }

        try
        {
            GameObject visualPrefab = ResourceManager.Instance.Get<GameObject>(monsterData.prefab1);
            if (visualPrefab != null)
            {
                var visualChild = Instantiate(visualPrefab, monster.transform);
                visualChild.name = $"{monsterData.prefab1}";

                // 로컬 포지션을 (0,0,0)으로 설정하여 부모와 같은 위치에
                visualChild.transform.localPosition = Vector3.zero;
                visualChild.transform.localRotation = Quaternion.identity;
            }

        }
        catch (System.Exception e)
        {
            Debug.Log($"Monster {monsterData.id}의 시각적 자식 오브젝트 추가 실패: {e.Message}");
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
            try
            {
                var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{monsterId}");
                var monsterDataSO = await handle.Task;
                if (monsterDataSO != null)
                {
                    monsterDataSO.InitFromCSV(monsterId);
                    monsterDataCache[monsterId] = monsterDataSO;
                }
            }
            catch
            {
            }
        }

        // 몬스터 타입별 풀 생성 - 캐시에 있는 몬스터만 생성
        foreach (var kvp in monsterDataCache)
        {
            int monsterId = kvp.Key;
            var monsterDataSO = kvp.Value;

            bool isBoss = MonsterBehavior.IsBossMonster(monsterId);
            var prefab = isBoss ? bossMonsterPrefab : monsterPrefab;
            int poolCount = isBoss ? 1 : poolSize / monsterDataCache.Count;

            monsterPools[monsterId] = new List<GameObject>();

            for (int i = 0; i < poolCount; i++)
            {
                try
                {
                    Vector3 offScreenPosition = new Vector3(-10000, -10000, 0);
                    var handle = Addressables.InstantiateAsync(prefab, offScreenPosition, Quaternion.identity);
                    var monster = await handle.Task;

                    if (monster == null)
                    {
                        continue;
                    }

                    monster.SetActive(false);

                    AddVisualChild(monster, monsterDataSO);

                    // 몬스터 완전 초기화 (캐시된 데이터 사용)
                    var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                    if (monsterBehavior != null)
                    {
                        monsterBehavior.Init(monsterDataSO);
                        monsterBehavior.SetMonsterSpawner(this);
                    }

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


                    // 초기 상태 설정
                    monster.SetActive(false);
                    var renderers = monster.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        if (renderer != null)
                            renderer.enabled = false;
                    }

                    monsterPools[monsterId].Add(monster);
                }
                catch
                {
                }
            }
        }

        CreateAllPools();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-3.5f, 3.5f);
        float randomY = Random.Range(12f, 17f);
        var spawnPos = new Vector3(randomX, randomY, 0);

        return spawnPos;
    }

    // 보스 스폰 위치 계산
    private Vector3 GetBossSpawnPosition()
    {
        return new Vector3(0f, 15f, 0f); // 화면 중앙 위쪽에서 스폰
    }

    // 리소스 정리
    private void OnDestroy()
    {
        ClearSpawnQueue();

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
        if (!isInitialized) return;

        //  0) 이전 스테이지 진행 루프 완전 종료
        stageCTS?.Cancel();
        stageCTS?.Dispose();
        stageCTS = new CancellationTokenSource();

        //  1) 현재 진행/대기열 정리
        isWaveActive = false;
        ClearSpawnQueue();
        await UniTask.Yield(); // 큐 프로세서가 isWaveActive=false를 먹고 빠져나가게 한 프레임

        currentStageId = newStageId;

        // 2) 몬스터 비활성화
        DeactivateAllMonsters();

        // 3) 기존 풀/캐시 해제
        foreach (var pool in monsterPools.Values)
        {
            foreach (var monster in pool)
            {
                if (monster != null)
                    Addressables.ReleaseInstance(monster);
            }
        }
        monsterPools.Clear();
        monsterDataCache.Clear();

        // 4) 새 스테이지 초기화
        await InitializePool();
        await LoadStageData();

        // 5) StageManager 동기화
        if (StageManager.Instance != null && currentStageData != null)
        {
            var stageCsv = DataTableManager.StageTable.GetStage(newStageId);
            StageManager.Instance.SetCurrentStageData(stageCsv);
            StageManager.Instance.SetWaveInfo(stageCsv.stage_step1, 1);
        }

        currentWaveIndex = 0;

        // 6) 새 CTS로 새 루프 시작
        await StartStageProgression(stageCTS.Token);
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
    private void CreateAllPools()
    {
        PoolManager.Instance.CreatePool(MonsterProjectilePoolId, monsterProjectilePrefab, 100);
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
        if (currentStageData != null)
        {
            return (currentStageData.stage_step1, waveIndex);
        }

        return (1, waveIndex); // 기본 1스테이지
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

    // 스폰 위치 유효성 검사
    private bool IsSpawnPositionValid(Vector3 position)
    {
        Collider2D[] overlapping = Physics2D.OverlapCircleAll(position, spawnRadius, LayerMask.GetMask(Tag.Monster));

        foreach (var collider in overlapping)
        {
            if (collider.gameObject.activeInHierarchy)
            {
                return false;
            }
        }

        return true;
    }

    // 충돌 회피를 위한 안전한 스폰 위치 찾기
    private Vector3? FindSafeSpawnPosition(bool isBoss) 
    {
        for (int i = 0; i < maxSpawnRetries; i++)
        {
            Vector3 candidatePos = isBoss ? GetBossSpawnPosition() : GetRandomSpawnPosition();

            if (IsSpawnPositionValid(candidatePos))
            {
                return candidatePos;
            }
        }
        return null;
    }

    // 스폰 자리 차있으면 스폰 대기열에 추가
    private void AddToSpawnQueue(int monsterId)
    {
        var spawnRequest = new SpawnRequest(monsterId); // 스폰 요청 초기화 
        spawnQueue.Enqueue(spawnRequest);

        // 대기열 처리기가 실행 중이 아니면 시작
        if (!isProcessingQueue)
        {
            StartQueueProcessor().Forget();
        }
    }

    // 스폰 대기열 처리기
    private async UniTask StartQueueProcessor()
    {
        if (isProcessingQueue) return;

        isProcessingQueue = true;
        queueProcessCTS = new UniTaskCompletionSource();

        try
        {
            while (spawnQueue.Count > 0 && isWaveActive)
            {
                ProcessSpawnQueue();
                await UniTask.Delay((int)(spawnTime * 1000));
            }
        }
        
        finally
        {
            isProcessingQueue = false;
            queueProcessCTS?.TrySetResult();
        }
    }

    private void ProcessSpawnQueue()
    {
        if (spawnQueue.Count == 0) return;

        var request = spawnQueue.Dequeue();

        bool spawnSuccess = TrySpawnFromQueue(request.monsterId);

        if (!spawnSuccess)
        {            
            var retryRequest = new SpawnRequest(request.monsterId)
            {
                requestTime = request.requestTime, // 원래 요청 시간 유지
                retryCount = request.retryCount + 1 // 재시도 횟수 증가
            };

            if (retryRequest.retryCount < maxSpawnRetries)
            {
                spawnQueue.Enqueue(retryRequest);
                //Debug.Log($"몬스터 {request.monsterId} 스폰 재시도 요청. 재시도 횟수: {retryRequest.retryCount}");
            }
            else
            {
                var newRequest = new SpawnRequest(request.monsterId);
                spawnQueue.Enqueue(newRequest);
               //Debug.Log($"몬스터 {request.monsterId} 재시도 횟수 리셋하여 계속 시도");
            }
        }
        else
        {
            UpdateSpawnCount(request.monsterId);
            totalMonstersSpawned++;
            //Debug.Log($"대기열에서 몬스터 {request.monsterId} 스폰 성공");
        }
    }

    // 대기열에서의 스폰 시도 (대기열용)
    private bool TrySpawnFromQueue(int monsterId)
    {
        if (!monsterPools.TryGetValue(monsterId, out var pool))
        {
            return false;
        }

        foreach (var monster in pool)
        {
            if (monster != null && !monster.activeInHierarchy)
            {
                bool isBoss = MonsterBehavior.IsBossMonster(monsterId);
                Vector3? safePos = FindSafeSpawnPosition(isBoss);

                if (safePos.HasValue)
                {
                    monster.transform.position = safePos.Value;

                    if (monsterDataCache.TryGetValue(monsterId, out var monsterData))
                    {
                        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                        if (monsterBehavior != null)
                        {
                            monsterBehavior.Init(monsterData);
                        }
                    }
                    else
                    {
                        return false;
                    }

                    var renderers = monster.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.enabled = true;
                    }

                    monster.SetActive(true);
                    return true;
                }
            }
        }

        return false;
    }

    // 스폰 대기열 정리
    private void ClearSpawnQueue()
    {
        spawnQueue.Clear();

        if (isProcessingQueue)
        {
            isProcessingQueue = false;
            queueProcessCTS?.TrySetResult();
        }
    }

    // 리셋/테스트용 : 현재 씬에 살아있는 몬스터 전부 비활성화
    public void DespawnAllMonsters()
    {
        // 1) 웨이브/큐 진행 멈춤
        isWaveActive = false;
        ClearSpawnQueue(); // private이지만 같은 클래스라 호출 가능 :contentReference[oaicite:1]{index=1}

        // 2) 현재 활성 몬스터만 싹 끄기 (Destroy X, 풀 유지)
        foreach (var pool in monsterPools.Values)
        {
            foreach (var monster in pool)
            {
                if (monster == null || monster.Equals(null)) continue;

                if (monster.activeInHierarchy)
                {
                    // 렌더러도 꺼서 초기 풀 상태로
                    var renderers = monster.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                        if (r != null) r.enabled = false;

                    monster.SetActive(false);
                }
            }
        }

        // 3) 남은 몬스터 수/웨이브 카운터도 “현재 웨이브 기준” 초기화
        waveMonstersToSpawn.Clear();
        totalMonstersSpawned = 0;

        UpdateStageUI();
    }

}
