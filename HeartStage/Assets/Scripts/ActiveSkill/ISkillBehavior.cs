using UnityEngine;

public interface ISkillBehavior
{
    void Execute(GameObject caster, ActiveSkillData data);
}