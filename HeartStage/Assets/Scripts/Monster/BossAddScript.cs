using UnityEngine;

public class BossAddScript : MonoBehaviour
{
    private void Start()
    {
        ScriptAttacher.AttachById(this.gameObject, 9991);
        
        // ActiveSkillManager에 스킬 등록
        var skillBehavior = GetComponent<DeceptionBossSkill>();
        if (skillBehavior != null)
        {
            // 스킬 행동 등록
            ActiveSkillManager.Instance.RegisterSkillBehavior(this.gameObject, 9991, skillBehavior);
            
            // 스킬 타이머 등록 (15초마다 자동 실행)
            ActiveSkillManager.Instance.RegisterSkill(this.gameObject, 9991);
            
            Debug.Log("대량 현혹 스킬이 ActiveSkillManager에 등록되었습니다.");
        }
    }
    
    private void OnDestroy()
    {
        // 게임오브젝트 파괴 시 스킬 해제
        if (ActiveSkillManager.Instance != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(this.gameObject, 9991);
        }
    }
}