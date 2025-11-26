using UnityEngine;

public class FaceGeniusSkillV2 : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "FaceGenius";
        poolId = "FaceGeniusSkillV2";
        skillDataName = "얼굴 천재";
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => transform.position;
    protected override Vector3 GetDirection() => Vector3.up;
}