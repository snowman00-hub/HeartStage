using UnityEngine;

public class SpeedBuffBossSkill : MonoBehaviour, ISkillBehavior
{
    private float speedMultiplier;
    private float buffDuration;
    private float coolTime;

    private float nextSkillTime = 0f;
    private bool isInitialized = false;

    // 3010 효과 등록을 위한 static 생성자
    static SpeedBuffBossSkill()
    {
        // 3010: 몬스터 이동속도 버프 효과 등록
        EffectRegistry.Register(3010, ApplyMoveSpeedBuff);
    }

    public void Init(SkillCSVData data)
    {
        speedMultiplier = data.skill_eff1_val; 
        buffDuration = data.skill_duration;   
        coolTime = data.skill_cool;

        isInitialized = true;
        nextSkillTime = Time.time + coolTime;
    }

    private void Update()
    {
        if (!isInitialized) return;

        var bossAddScript = GetComponent<BossAddScript>();
        if (bossAddScript == null || !bossAddScript.IsBossSpawned())
        {
            return;
        }

        // 스킬 실행 체크
        if (Time.time >= nextSkillTime)
        {
            Execute();
            nextSkillTime = Time.time + coolTime;
        }
    }

    public void Execute()
    {
        var bossAddScript = GetComponent<BossAddScript>();
        if (bossAddScript == null || !bossAddScript.IsBossSpawned())
        {
            return;
        }

        ApplySpeedBuffToAllMonsters();
    }

    private void ApplySpeedBuffToAllMonsters()
    {
        // Monster 레이어로 2D 검색
        int monsterLayerMask = LayerMask.GetMask("Monster");
        var monsters2D = Physics2D.OverlapCircleAll(transform.position, 1000f, monsterLayerMask);

        int buffedCount = 0;

        foreach (var collider in monsters2D)
        {
            var monsterBehavior = collider.GetComponent<MonsterBehavior>();
            if (monsterBehavior != null && collider.gameObject != this.gameObject)
            {
                EffectRegistry.Apply(collider.gameObject, 3010, speedMultiplier, buffDuration);
                buffedCount++;
            }
        }

        Debug.Log($"단체 강화 효과 적용: {buffedCount}마리의 몬스터에게 이동속도 {speedMultiplier}배 버프 ({buffDuration}초간)");
    }

    // 3010 효과 구현: 몬스터 이동속도 버프
    private static void ApplyMoveSpeedBuff(GameObject target, float value, float duration, float tickInterval)
    {
        if (target == null) return;

        var monsterBehavior = target.GetComponent<MonsterBehavior>();
        if (monsterBehavior == null) return;

        var monsterData = monsterBehavior.GetMonsterData();
        if (monsterData == null) return;

        // MoveSpeedBuffEffect 컴포넌트 추가 또는 가져오기
        var buffEffect = target.GetComponent<MoveSpeedBuffEffect>();
        if (buffEffect == null)
        {
            buffEffect = target.AddComponent<MoveSpeedBuffEffect>();
        }

        // 버프 적용
        buffEffect.ApplyBuff(monsterData, value, duration);
    }
}

// 이동속도 버프 효과를 관리하는 컴포넌트
public class MoveSpeedBuffEffect : MonoBehaviour
{
    private MonsterData monsterData;
    private float buffEndTime;
    private bool isBuffActive = false;
    private float monsterOriginalSpeed; // 개별 몬스터의 원본 속도

    public void ApplyBuff(MonsterData data, float multiplier, float duration)
    {
        monsterData = data;

        // 버프가 활성화되어 있지 않을 때만 개별 몬스터 속도 저장
        if (!isBuffActive)
        {
            // CSV에서 이 몬스터의 원본 속도를 가져오기
            var csvData = DataTableManager.MonsterTable.Get(monsterData.id);
            if (csvData != null)
            {
                monsterOriginalSpeed = csvData.speed; // CSV 원본 속도 사용
            }
            else
            {
                monsterOriginalSpeed = monsterData.moveSpeed; // 대체값
            }
        }

        // 개별 몬스터 원본 속도 기준으로 계산
        monsterData.moveSpeed = monsterOriginalSpeed * multiplier;
        buffEndTime = Time.time + duration;
        isBuffActive = true;
    }

    private void Update()
    {
        if (isBuffActive && Time.time >= buffEndTime)
        {
            RemoveBuff();
        }
    }

    private void RemoveBuff()
    {
        if (isBuffActive && monsterData != null)
        {
            monsterData.moveSpeed = monsterOriginalSpeed; // 개별 원본으로 복구
            isBuffActive = false;
        }
    }

    private void OnDestroy()
    {
        RemoveBuff();
    }
}