using UnityEngine;

public class BossAddScript : MonoBehaviour
{
    private void Start()
    {
        ScriptAttacher.AttachById(this.gameObject, 9991);
        ScriptAttacher.AttachById(this.gameObject, 9992);

        // ActiveSkillManager에 스킬 등록
        var skillBehavior = GetComponent<DeceptionBossSkill>();
        var speedBuffBehavior = GetComponent<SpeedBuffBossSkill>();
        if (skillBehavior != null)
        {
            // 스킬 행동 등록
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9991, skillBehavior);
            
            // 스킬 타이머 등록 (15초마다 자동 실행)
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9991);            
        }
        if (speedBuffBehavior != null)
        {
            // 스킬 행동 등록
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9992, speedBuffBehavior);

            // 스킬 타이머 등록 (5초마다 자동 실행)
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9992);
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