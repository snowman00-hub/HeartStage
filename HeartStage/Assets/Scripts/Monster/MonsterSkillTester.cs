using UnityEngine;

public class MonsterSkillTester : MonoBehaviour
{
    private bool skillsRegistered = false;

    private void Start()
    {
        skillsRegistered = false;
    }


    private void Update()
    {
        // 스킬 자동 등록 (한 번만)
        if (!skillsRegistered)
        {
            RegisterSkillsIfExists();
            skillsRegistered = true;
        }
    }

    private void RegisterSkillsIfExists()
    {
        // DeceptionBossSkill이 붙어있으면 등록
        var deceptionSkill = GetComponent<DeceptionBossSkill>();
        if (deceptionSkill != null)
        {
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9991, deceptionSkill);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9991);
            Debug.Log($"{gameObject.name}에 DeceptionBossSkill 등록 완료");
        }

        // SpeedBuffBossSkill이 붙어있으면 등록
        var speedBuffSkill = GetComponent<SpeedBuffBossSkill>();
        if (speedBuffSkill != null)
        {
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9992, speedBuffSkill);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9992);
            Debug.Log($"{gameObject.name}에 SpeedBuffBossSkill 등록 완료");
        }
    }
    private void OnDestroy()
    {
        // 게임오브젝트 파괴 시 스킬 해제
        if (ActiveSkillManager.Instance != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 9991);
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 9992);
        }
    }
}