using UnityEngine;

public class HeartBombSkill : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "HeartBomb";
        poolId = "HeartBomb";
        skillId = 31208;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => GetCenterInMonsters();
    protected override Vector3 GetDirection() => Vector3.zero;

}