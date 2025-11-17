using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class DeceptionBossSkill : MonoBehaviour, ISkillBehavior
{  

    private string poolId;
    private bool isPoolInitialized = false;
    private bool isInitialized = false; // 중복 초기화 방지 플래그 추가
    private MonsterBehavior monsterBehavior;
    private SkillCSVData skillData;
    private MonsterData cachedMonsterData;  

    public async UniTask InitializeWithMonsterData(MonsterData monsterData)
    {
        if (isInitialized) return;

        if (monsterData == null)
        {
            Debug.LogError("DeceptionBossSkill: MonsterData가 null입니다");
            return;
        }

        monsterBehavior = GetComponent<MonsterBehavior>();
        if (monsterBehavior == null)
        {
            Debug.LogError("DeceptionBossSkill: MonsterBehavior를 찾을 수 없음");
            return;
        }

        await InitializeWithData(monsterData);
    }

    private async UniTask InitializeWithData(MonsterData monsterData)
    {
        // 스킬 ID 결정
        int skillId = GetSkillIdForBoss(monsterData.id);
        if (skillId == 0)
        {
            Debug.LogError($"보스 ID {monsterData.id}에 해당하는 스킬을 찾을 수 없음");
            return;
        }

        // 스킬 데이터 로드
        skillData = DataTableManager.SkillTable.Get(skillId);
        if (skillData == null)
        {
            Debug.LogError($"스킬 데이터를 찾을 수 없음 - ID: {skillId}");
            return;
        }

        poolId = skillData.summon_type.ToString();

        // MonsterData 캐시 로드
        cachedMonsterData = ResourceManager.Instance.Get<MonsterData>($"MonsterData_{skillData.summon_type}");
        if (cachedMonsterData == null)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<MonsterData>($"MonsterData_{skillData.summon_type}");
                cachedMonsterData = await handle.Task;

                if (cachedMonsterData != null)
                {
                    cachedMonsterData.InitFromCSV(skillData.summon_type);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MonsterData 로드 실패 - ID: {skillData.summon_type}, Error: {e.Message}");
                return;
            }
        }

        if (cachedMonsterData == null)
        {
            Debug.LogError($"MonsterData를 찾을 수 없음 - ID: {skillData.summon_type}");
            return;
        }

        await InitializePool();

        isInitialized = true; // 초기화 완료 플래그 설정
        Debug.Log($"DeceptionBossSkill 초기화 완료 - 보스: {monsterData.id}, 스킬: {skillId}, 소환몬스터: {poolId}");
    }


    private int GetSkillIdForBoss(int bossId)
    {
        return bossId switch
        {
            22201 => 31001,
            22214 => 31003,
            _ => 0
        };
    }

    private async UniTask InitializePool()
    {
        if (isPoolInitialized || skillData == null) return;

        if (PoolManager.Instance == null)
        {
            Debug.LogError("DeceptionBossSkill: PoolManager.Instance가 없습니다");
            return;
        }

        try
        {
            // 이미 Boot에서 로드된 MonsterPrefab을 사용
            var handle = Addressables.LoadAssetAsync<GameObject>("MonsterPrefab");
            var monsterPrefabGO = await handle.Task;

            if (monsterPrefabGO != null)
            {
                int poolSize = 10;
                PoolManager.Instance.CreatePool(poolId, monsterPrefabGO, poolSize, poolSize * 2);
                isPoolInitialized = true;
                Debug.Log($"DeceptionBossSkill 풀 생성 완료: {poolId}, 풀 크기: {poolSize}");
            }
            else
            {
                Debug.LogError("DeceptionBossSkill: MonsterPrefab 로드 실패");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DeceptionBossSkill 풀 초기화 실패: {e.Message}");
        }
    }

    public void Execute()
    {
        var bossAddScript = GetComponent<BossAddScript>();
        if (bossAddScript == null || !bossAddScript.IsBossSpawned())
        {
            return; // 로그 없이 조용히 리턴
        }

        if (!isPoolInitialized || skillData == null || cachedMonsterData == null)
        {
            Debug.LogWarning("DeceptionBossSkill: 초기화되지 않음");
            return;
        }

        Debug.Log($"스킬 실행: {skillData.skill_name} (쿨타임: {skillData.skill_cool}초)");
        ExecuteSkill().Forget();
    }

    private async UniTaskVoid ExecuteSkill()
    {
        // SkillTable 데이터에 따른 소환 개수 (15~18마리 → 2~3마리로 조정)
        int spawnCount = Random.Range(2, 4); // 2~3마리 고정
        Debug.Log($"대량 현혹 스킬 실행: poolId={poolId}, spawnCount={spawnCount}");

        // 보스 주위에 소환
        for (int i = 0; i < spawnCount; i++)
        {
            var monster = PoolManager.Instance.Get(poolId);
            if (monster != null)
            {
                SetupSummonedMonster(monster);
                Debug.Log($"DeceptionSkill 몬스터 소환 성공: {i + 1}/{spawnCount}");
            }
            else
            {
                Debug.LogWarning($"풀에서 몬스터를 가져올 수 없음: {i + 1}번째");
            }

            // 소환 간격 (0.3초)
            if (i < spawnCount - 1)
            {
                await UniTask.Delay(300);
            }
        }

        Debug.Log($"스킬 완료, 다음 스킬까지 {skillData.skill_cool}초 대기");
    }

    private void SetupSummonedMonster(GameObject monster)
    {
        // 보스 주위 위치 설정
        Vector3 spawnPos = GetRandomSpawnPosition();
        monster.transform.position = spawnPos;
        monster.transform.rotation = Quaternion.identity;

        // 몬스터 초기화
        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
        if (monsterBehavior != null)
        {
            monsterBehavior.Init(cachedMonsterData);
        }

        // 스프라이트 설정
        MonsterSpawner.SetMonsterSprite(monster, cachedMonsterData);

        // 이동 컴포넌트 초기화
        var monsterMovement = monster.GetComponent<MonsterMovement>();
        if (monsterMovement != null)
        {
            monsterMovement.Init(cachedMonsterData, Vector3.down);
        }

        monster.SetActive(true);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 bossPosition = transform.position;
        
        // 보스 주위 소환 범위 (스킬 범위보다 작게)
        float spawnRange = Mathf.Min(skillData.skill_range * 0.5f, 3f); // 최대 3 유닛 반경
        
        // 원형 패턴으로 보스 주위에 소환
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(spawnRange * 0.5f, spawnRange); // 너무 가깝지 않게
        
        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance,
            0f
        );
        
        Vector3 spawnPos = bossPosition + spawnOffset;
        
        // 화면 경계 체크
        Vector3 screenMin = Camera.main.ScreenToWorldPoint(new Vector3(50f, 50f, 0f));
        Vector3 screenMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 50f, Screen.height - 50f, 0f));
        
        spawnPos.x = Mathf.Clamp(spawnPos.x, screenMin.x, screenMax.x);
        spawnPos.y = Mathf.Clamp(spawnPos.y, screenMin.y, screenMax.y);
        spawnPos.z = 0f;
        
        return spawnPos;
    }
}