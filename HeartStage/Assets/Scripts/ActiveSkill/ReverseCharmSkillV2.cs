using UnityEngine;

public class ReverseCharmSkillV2 : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "ReverseCharm";
        poolId = "ReverseCharmV2";
        skillId = 31207;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => transform.position;
}