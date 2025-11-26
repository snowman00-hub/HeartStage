using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

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
        bossSpawned = false;
        UnregisterSkills();
        skillsRegistered = false;
    }

    public bool IsBossSpawned()
    {
        return gameObject.activeInHierarchy && bossSpawned;
    }

    private async UniTask RegisterSkillsAsync()
    {
        var monsterBehavior = GetComponent<MonsterBehavior>();
        if (monsterBehavior == null)
        {
            return;
        }

        int maxWaitFrames = 60;
        int waitFrames = 0;

        while (monsterBehavior.GetMonsterData() == null && waitFrames < maxWaitFrames)
        {
            await UniTask.NextFrame();
            waitFrames++;
        }

        var monsterData = monsterBehavior.GetMonsterData();
        if (monsterData == null)
        {
            return;
        }

        int bossId = monsterData.id;
        RegisterBossSkills(bossId);
    }

    private void RegisterBossSkills(int bossId)
    {
        var csvData = DataTableManager.MonsterTable.Get(bossId);
        if (csvData == null)
        {
            Debug.LogWarning($"몬스터 CSV 데이터를 찾을 수 없음: {bossId}");
            return;
        }

        Debug.Log($"CSV 스킬 등록 - 보스 ID: {bossId}, skill_id1: {csvData.skill_id1}, skill_id2: {csvData.skill_id2}, skill_id3: {csvData.skill_id3}");

        var deceptionSkillIds = new List<int>();
        var otherSkillIds = new List<int>();

        if (csvData.skill_id1 != 0) ClassifySkill(csvData.skill_id1, deceptionSkillIds, otherSkillIds);
        if (csvData.skill_id2 != 0) ClassifySkill(csvData.skill_id2, deceptionSkillIds, otherSkillIds);
        if (csvData.skill_id3 != 0) ClassifySkill(csvData.skill_id3, deceptionSkillIds, otherSkillIds);

        // DeceptionSkill들은 한 번에 등록
        if (deceptionSkillIds.Count > 0)
        {
            RegisterDeceptionSkills(deceptionSkillIds);
        }

        // 다른 스킬들은 개별 등록
        foreach (int skillId in otherSkillIds)
        {
            RegisterOtherSkill(skillId);
        }
    }

    private void ClassifySkill(int skillId, List<int> deceptionSkillIds, List<int> otherSkillIds)
    {
        switch (skillId)
        {
            case 31001: // 대량 현혹 튜토리얼 근접
            case 31002: // 대량 현혹 튜토리얼 원거리
            case 31003: // 대량 현혹 근접
            case 31004: // 대량 현혹 원거리
                deceptionSkillIds.Add(skillId);
                break;

            case 31101: // 야유 스킬
            case 31201: // 단체 강화
                otherSkillIds.Add(skillId);
                break;

            default:
                Debug.LogWarning($"정의되지 않은 스킬 ID: {skillId}");
                break;
        }
    }

    private void RegisterDeceptionSkills(List<int> skillIds)
    {
        // 첫 번째 스킬로 컴포넌트 추가
        ScriptAttacher.AttachById(this.gameObject, skillIds[0]);

        var monsterBehavior = GetComponent<MonsterBehavior>();
        var skillBehavior = GetComponent<DeceptionBossSkill>();

        if (skillBehavior != null)
        {
            // 모든 스킬 ID를 전달
            foreach (int skillId in skillIds)
            {
                skillBehavior.SetSkillId(skillId);
            }

            // 초기화는 한 번만
            skillBehavior.InitializeWithMonsterData(monsterBehavior.GetMonsterData()).Forget();

            // ActiveSkillManager에는 첫 번째 스킬만 등록 (대표 스킬)
            int representativeSkillId = skillIds[0];
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, representativeSkillId, skillBehavior);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, representativeSkillId);

            Debug.Log($"{gameObject.name}에 DeceptionBossSkills ({skillIds.Count}개) 등록 완료");
        }
        else
        {
            Debug.LogError($"DeceptionBossSkill 컴포넌트를 찾을 수 없음");
        }
    }

    private void RegisterOtherSkill(int skillId)
    {
        switch (skillId)
        {
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

    private void RegisterSpeedBuffSkill(int skillId)
    {
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

        int bossId = monsterData.id;
        var csvData = DataTableManager.MonsterTable.Get(bossId);
        if (csvData == null) return;

        // DeceptionSkill 그룹의 대표 스킬만 등록 해제
        var deceptionSkillIds = new List<int>();
        var otherSkillIds = new List<int>();

        if (csvData.skill_id1 != 0) ClassifySkill(csvData.skill_id1, deceptionSkillIds, otherSkillIds);
        if (csvData.skill_id2 != 0) ClassifySkill(csvData.skill_id2, deceptionSkillIds, otherSkillIds);
        if (csvData.skill_id3 != 0) ClassifySkill(csvData.skill_id3, deceptionSkillIds, otherSkillIds);

        // DeceptionSkill 그룹의 첫 번째 스킬만 등록 해제
        if (deceptionSkillIds.Count > 0)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, deceptionSkillIds[0]);
        }

        // 다른 스킬들은 개별 등록 해제
        foreach (int skillId in otherSkillIds)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, skillId);
        }
    }

    private void OnDestroy()
    {
        UnregisterSkills();
    }
}