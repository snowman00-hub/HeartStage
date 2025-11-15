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

    // MonsterData SO 캐시 (방법 2의 핵심)
    private Dictionary<int, MonsterData> monsterDataCache = new Dictionary<int, MonsterData>();

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

    // 게임 시작 시 초기화 실행
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
                monsterDataSO.Init(monsterId); // 스테이지 시작 시에만 CSV로 초기화
                monsterDataCache[monsterId] = monsterDataSO; // 캐시에 저장
                Debug.Log($"MonsterData_{monsterId} 캐시에 로드됨");
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

    // 개별 몬스터 스폰 (캐시된 MonsterData 사용, Init 호출 안 함)
    private bool SpawnMonster(int monsterId)
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

                // 캐시에서 MonsterData 가져오기 (Init 호출 안 함 - 런타임 변경사항 유지)
                if (monsterDataCache.TryGetValue(monsterId, out var monsterDataSO))
                {
                    var monsterBehavior = monster.GetComponent<MonsterBehavior>();
                    monsterBehavior.Init(monsterDataSO);
                    monsterBehavior.SetMonsterSpawner(this);

                    var monsterMovement = monster.GetComponent<MonsterMovement>();
                    if (monsterMovement != null)
                    {
                        monsterMovement.Init(monsterDataSO, Vector3.down);
                    }

                    SetMonsterSprite(monster, monsterDataSO);
                }
                else
                {
                    Debug.LogError($"MonsterData_{monsterId}가 캐시에 없습니다.");
                    return false;
                }

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

    // 몬스터 오브젝트 풀 초기화
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

    // 랜덤 스폰 위치 계산
    private Vector3 GetRandomSpawnPosition()
    {
        int randomRange = Random.Range(0, Screen.width);
        int height = Random.Range(Screen.height, Screen.height + 200);

        Vector3 screenPosition = new Vector3(randomRange, height, 0);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPosition);
        spawnPos.z = 0f;

        return spawnPos;
    }

    // 보스 스폰 위치 계산
    private Vector3 GetBossSpawnPosition()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height, 0f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        worldPos.z = 0f;

        return worldPos;
    }

    // 리소스 정리
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

        // 캐시 정리
        monsterDataCache.Clear();
    }

    // 스테이지 변경
    public async UniTask ChangeStage(int newStageId)
    {
        isWaveActive = false;
        currentStageId = newStageId;

        // 모든 활성 몬스터 비활성화
        DeactivateAllMonsters();

        // 캐시 정리 (새 스테이지에서 다시 캐시 구축)
        monsterDataCache.Clear();

        await LoadStageData();
        await StartStageProgression();
    }

    // 모든 활성 몬스터 비활성화
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