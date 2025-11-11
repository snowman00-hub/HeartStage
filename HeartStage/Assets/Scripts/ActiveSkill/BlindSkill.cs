using UnityEngine;

public class BlindSkill : ISkillBehavior
{
    public void Execute(GameObject caster, ActiveSkillData data)
    {
        var characterAttack = caster.GetComponent<CharacterAttack>();
        characterAttack.Test();
        Debug.Log($"{caster.name} 스킬 발동! {data.skill_name}");
    }
}