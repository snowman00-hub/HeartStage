using UnityEngine;
using Cysharp.Threading.Tasks;

public class BossAddScript : MonoBehaviour
{
    private bool skillsRegistered = false;
    private bool bossSpawned = false;   

    private async void OnEnable()
    {
        bossSpawned = true;

        if (!skillsRegistered)
        {
            await RegisterSkillsAsync();
            skillsRegistered = true;
        }
    }

    private void OnDisable()
    {
        // 비활성화될 때 스킬 해제
        bossSpawned = false;

        UnregisterSkills();
        skillsRegistered = false;
    }

    public bool IsBossSpawned()
    {
        if (gameObject.activeInHierarchy)
        {
            return bossSpawned;
        }
        else
        {
            return false;
        }
    }

    private async UniTask RegisterSkillsAsync() 
    {
        // MonsterBehavior 초기화 대기
        var monsterBehavior = GetComponent<MonsterBehavior>();
        if (monsterBehavior == null)
        {
            Debug.LogError("BossAddScript: MonsterBehavior를 찾을 수 없음");
            return;
        }

        // MonsterData가 초기화될 때까지 대기
        int maxWaitFrames = 60; // 최대 1초 대기 (60fps 기준)
        int waitFrames = 0;

        while (monsterBehavior.GetMonsterData() == null && waitFrames < maxWaitFrames)
        {
            await UniTask.NextFrame();
            waitFrames++;
        }

        var monsterData = monsterBehavior.GetMonsterData();
        if (monsterData == null)
        {
            Debug.LogError("BossAddScript: MonsterData 초기화 대기 시간 초과");
            return;
        }

        // 보스 ID에 따른 스킬 할당
        int bossId = monsterData.id;
        RegisterBossSkills(bossId);
    }

    private void RegisterBossSkills(int bossId)
    {
        switch (bossId)
        {
            case 22201: // 치프 스테프
                RegisterDeceptionSkill(31001); // 대량 현혹 튜토리얼 근접
                break;

            case 22214: // 사람을 홀리는 악마
                RegisterDeceptionSkill(31003); // 대량 현혹 근접
                RegisterSpeedBuffSkill(31201); // 단체 강화
                break;

            default:
                Debug.LogWarning($"정의되지 않은 보스 ID: {bossId}");
                break;
        }
    }

    private void RegisterDeceptionSkill(int skillId)
    {
        // DeceptionBossSkill 스크립트 할당
        ScriptAttacher.AttachById(this.gameObject, skillId);

        var monsterBehavior = GetComponent<MonsterBehavior>();
        var skillBehavior = GetComponent<DeceptionBossSkill>();

        if (skillBehavior != null)
        {
            // MonsterData가 준비된 후에 스킬 초기화를 수동으로 트리거
            skillBehavior.InitializeWithMonsterData(monsterBehavior.GetMonsterData()).Forget();

            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, skillId, skillBehavior);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, skillId);
            Debug.Log($"{gameObject.name}에 DeceptionBossSkill (ID: {skillId}) 등록 완료");
        }
        else
        {
            Debug.LogError($"DeceptionBossSkill 컴포넌트를 찾을 수 없음 (스킬 ID: {skillId})");
        }
    }

    private void RegisterSpeedBuffSkill(int skillId)
    {
        // SpeedBuffBossSkill 스크립트 할당
        ScriptAttacher.AttachById(this.gameObject, skillId);

        var speedBuffBehavior = GetComponent<SpeedBuffBossSkill>();
        if (speedBuffBehavior != null)
        {
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, skillId, speedBuffBehavior);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, skillId);
            Debug.Log($"{gameObject.name}에 SpeedBuffBossSkill (ID: {skillId}) 등록 완료");
        }
        else
        {
            Debug.LogError($"SpeedBuffBossSkill 컴포넌트를 찾을 수 없음 (스킬 ID: {skillId})");
        }
    }

    private void UnregisterSkills() // 스킬 해제 
    {
        if (ActiveSkillManager.Instance == null) return;

        var monsterBehavior = GetComponent<MonsterBehavior>();
        if (monsterBehavior == null) return;

        var monsterData = monsterBehavior.GetMonsterData();
        if (monsterData == null) return;

        // 보스 ID에 따른 스킬 해제
        int bossId = monsterData.id;
        UnregisterBossSkills(bossId);
    }

    private void UnregisterBossSkills(int bossId)
    {
        switch (bossId)
        {
            case 22201: // 치프 스테프
                ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 31001);
                Debug.Log($"{gameObject.name} DeceptionBossSkill (ID: 31001) 등록 해제");
                break;

            case 22214: // 사람을 홀리는 악마
                ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 31003);
                ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 31201);
                Debug.Log($"{gameObject.name} 보스 스킬들 (ID: 31003, 31201) 등록 해제");
                break;

            default:
                break;
        }
    }

    private void OnDestroy()
    {
        UnregisterSkills();
    }
}