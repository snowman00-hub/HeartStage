using UnityEngine;

public class BlindSkill : ISkillBehavior
{
    public void Execute(GameObject caster, ActiveSkillData data)
    {
        Debug.Log($"{caster.name} 스킬 발동! {data.skill_name}");
    }
}