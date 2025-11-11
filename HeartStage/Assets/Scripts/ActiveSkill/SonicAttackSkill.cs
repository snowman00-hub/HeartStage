using UnityEngine;

public class SonicAttackSkill : ISkillBehavior
{
    public void Execute(GameObject caster, ActiveSkillData data)
    {
        ActiveSkillCreator.Instance.CreateSonicAttack(caster, data);
        Debug.Log($"{data.skill_name} 사용");
    }
}