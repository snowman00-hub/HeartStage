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
        // CSV 데이터에서 스킬 ID 가져오기
        var csvData = DataTableManager.MonsterTable.Get(bossId);
        if (csvData == null)
        {
            Debug.LogWarning($"몬스터 CSV 데이터를 찾을 수 없음: {bossId}");
            return;
        }

        Debug.Log($"📊 CSV 스킬 등록 - 보스 ID: {bossId}, skill_id1: {csvData.skill_id1}, skill_id2: {csvData.skill_id2}, skill_id3: {csvData.skill_id3}");

        // CSV 기반으로만 스킬 등록
        if (csvData.skill_id1 != 0) RegisterSkillById(csvData.skill_id1);
        if (csvData.skill_id2 != 0) RegisterSkillById(csvData.skill_id2);
        if (csvData.skill_id3 != 0) RegisterSkillById(csvData.skill_id3);
    }

    private void RegisterSkillById(int skillId)
    {
        switch (skillId)
        {
            case 31001: // 대량 현혹 튜토리얼 근접
            case 31002: // 대량 현혹 튜토리얼 원거리
            case 31003: // 대량 현혹 근접
            case 31004: // 대량 현혹 원거리
                RegisterDeceptionSkill(skillId);
                break;

            case 31101: // 야유 스킬
                RegisterBooingSkill(skillId);
                break;

            case 31201: // 단체 강화
                RegisterSpeedBuffSkill(skillId);
                break;

            default:
                Debug.LogWarning($"정의되지 않은 스킬 ID: {skillId}");
                break;
        }
    }

    private void RegisterDeceptionSkill(int skillId)
    {
        Debug.Log($"🔮 DeceptionSkill 등록 시작 - 스킬 ID: {skillId}");

        // DeceptionBossSkill 스크립트 할당
        ScriptAttacher.AttachById(this.gameObject, skillId);

        var monsterBehavior = GetComponent<MonsterBehavior>();
        var skillBehavior = GetComponent<DeceptionBossSkill>();

        if (skillBehavior != null)
        {
            // CSV에서 읽은 실제 스킬 ID 전달
            skillBehavior.SetSkillId(skillId);

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
            var skillData = DataTableManager.SkillTable.Get(skillId);
            speedBuffBehavior.Init(skillData);

            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, skillId, speedBuffBehavior);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, skillId);
            Debug.Log($"{gameObject.name}에 SpeedBuffBossSkill (ID: {skillId}) 등록 완료");
        }
        else
        {
            Debug.LogError($"SpeedBuffBossSkill 컴포넌트를 찾을 수 없음 (스킬 ID: {skillId})");
        }
    }

    private void RegisterBooingSkill(int skillId)
    {
        ScriptAttacher.AttachById(this.gameObject, skillId);

        var booingSkill = GetComponent<BooingBossSkill>();
        if (booingSkill != null)
        {
            var skillData = DataTableManager.SkillTable.Get(skillId);
            booingSkill.Init(skillData);

            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, skillId, booingSkill);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, skillId);
        }
    }

    private void UnregisterSkills()
    {
        if (ActiveSkillManager.Instance == null) return;

        var monsterBehavior = GetComponent<MonsterBehavior>();
        if (monsterBehavior == null) return;

        var monsterData = monsterBehavior.GetMonsterData();
        if (monsterData == null) return;

        // CSV 기반으로만 스킬 해제
        int bossId = monsterData.id;
        var csvData = DataTableManager.MonsterTable.Get(bossId);
        if (csvData == null) return;

        if (csvData.skill_id1 != 0) ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, csvData.skill_id1);
        if (csvData.skill_id2 != 0) ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, csvData.skill_id2);
        if (csvData.skill_id3 != 0) ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, csvData.skill_id3);
    }

    private void OnDestroy()
    {
        UnregisterSkills();
    }
}