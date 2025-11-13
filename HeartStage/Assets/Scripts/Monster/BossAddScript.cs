using UnityEngine;

public class BossAddScript : MonoBehaviour
{
    private bool skillsRegistered = false;

    private void OnEnable() // Start 대신 OnEnable 사용
    {
        if (!skillsRegistered)
        {
            RegisterSkills();
            skillsRegistered = true;
        }
    }

    private void OnDisable()
    {
        // 비활성화될 때 스킬 해제
        UnregisterSkills();
        skillsRegistered = false;
    }

    private void RegisterSkills()
    {
        ScriptAttacher.AttachById(this.gameObject, 9991);
        ScriptAttacher.AttachById(this.gameObject, 9992);

        var skillBehavior = GetComponent<DeceptionBossSkill>();
        var speedBuffBehavior = GetComponent<SpeedBuffBossSkill>();

        if (skillBehavior != null)
        {
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9991, skillBehavior);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9991);
            Debug.Log($"{gameObject.name}에 DeceptionBossSkill 등록 완료");
        }

        if (speedBuffBehavior != null)
        {
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9992, speedBuffBehavior);
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9992);
            Debug.Log($"{gameObject.name}에 SpeedBuffBossSkill 등록 완료");
        }
    }

    private void UnregisterSkills()
    {
        if (ActiveSkillManager.Instance != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 9991);
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 9992);
            Debug.Log($"{gameObject.name} 보스 스킬 등록 해제");
        }
    }

    private void OnDestroy()
    {
        UnregisterSkills();
    }
}