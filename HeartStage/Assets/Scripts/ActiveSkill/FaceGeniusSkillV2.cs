using UnityEngine;

public class FaceGeniusSkillV2 : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "FaceGenius";
        poolId = "FaceGeniusV2";
        skillId = 31203;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => transform.position;
}