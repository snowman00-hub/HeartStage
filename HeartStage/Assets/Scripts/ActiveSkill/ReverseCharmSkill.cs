using UnityEngine;

public class ReverseCharmSkill : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "ReverseCharm";
        poolId = "ReverseCharm";
        skillDataName = "반전매력";
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => transform.position;
    protected override Vector3 GetDirection() => Vector3.up;
}