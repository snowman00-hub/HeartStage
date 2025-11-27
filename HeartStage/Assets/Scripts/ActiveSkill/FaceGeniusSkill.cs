using UnityEngine;

public class FaceGeniusSkill : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "FaceGenius";
        poolId = "FaceGenius";
        skillId = 31202;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => transform.position;
}