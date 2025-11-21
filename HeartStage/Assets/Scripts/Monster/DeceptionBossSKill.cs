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
    private MonsterSpawner monsterSpawner; 


    public async UniTask InitializeWithMonsterData(MonsterData monsterData)
    {
        if (isInitialized) return;

        if (monsterData == null)
        {
            return;
        }

        monsterBehavior = GetComponent<MonsterBehavior>();

        if (monsterBehavior == null)
        {
            return;
        }

        if(monsterSpawner == null)
        {
            monsterSpawner = FindObjectOfType<MonsterSpawner>(); // FindObject 쓰면 안좋음 변경 하긴 해야함
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
            22201 => 31001, // 보스 아이디 22201에 대한 스킬 아이디
            22214 => 31003, // 보스 아이디 22214에 대한 스킬 아이디
            _ => 0
        };
    }

    private async UniTask InitializePool()
    {
        if (isPoolInitialized || skillData == null) return;

        if (PoolManager.Instance == null)
        {
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
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"풀 초기화 실패 - ID: {poolId}, Error: {e.Message}");
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
            return;
        }

        ExecuteSkill().Forget();
    }

    private async UniTaskVoid ExecuteSkill()
    {
        int spawnCount = Random.Range(2, 4); // 2~3마리

        // 보스 주위에 소환
        for (int i = 0; i < spawnCount; i++)
        {
            var monster = PoolManager.Instance.Get(poolId);
            if (monster != null)
            {
                SetupSummonedMonster(monster);
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

    }

    private void SetupSummonedMonster(GameObject monster)
    {
        // 보스 주위 위치 설정
        Vector3 spawnPos = GetRandomSpawnPosition();
        monster.transform.position = spawnPos;
        monster.transform.rotation = Quaternion.identity;

        monster.tag = Tag.Monster;

        // ⭐ 순서 변경: 시각적 자식을 먼저 추가
        AddVisualChild(monster, cachedMonsterData);

        // 그 다음 몬스터 초기화
        var monsterBehavior = monster.GetComponent<MonsterBehavior>();
        if (monsterBehavior != null)
        {
            monsterBehavior.Init(cachedMonsterData);

            if (monsterSpawner != null)
            {
                monsterBehavior.SetMonsterSpawner(monsterSpawner);
            }
        }

        // 이동 컴포넌트 초기화
        var monsterMovement = monster.GetComponent<MonsterMovement>();
        if (monsterMovement != null)
        {
            monsterMovement.Init(cachedMonsterData, Vector3.down);
        }

        monster.SetActive(true);
    }



    private void AddVisualChild(GameObject monster, MonsterData monsterData)
    {
        try
        {
            if (!string.IsNullOrEmpty(monsterData.prefab1))
            {
                var prefabGO = ResourceManager.Instance.Get<GameObject>(monsterData.prefab1);
                if (prefabGO != null)
                {
                    var visualChild = Instantiate(prefabGO, monster.transform);

                    // 로컬 포지션을 (0,0,0)으로 설정하여 부모와 같은 위치에
                    visualChild.transform.localPosition = Vector3.zero;
                    visualChild.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    Debug.Log($"prefab1을 ResourceManager에서 찾을 수 없음: {monsterData.prefab1}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"소환된 Monster {monsterData.id}의 시각적 자식 오브젝트 추가 실패: {e.Message}");
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 bossPosition = transform.position;

        // 보스 주위 소환 범위 
        float sideDistance = Random.Range(2f, 5f);
        float side = Random.Range(0, 2) == 0 ? -1f : 1f; // 왼쪽 또는 오른쪽

        float yOffset = Random.Range(5f, 10f); // 약간 뒤쪽

        Vector3 spawnOffset = new Vector3
            (
                side * sideDistance,
                yOffset,
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