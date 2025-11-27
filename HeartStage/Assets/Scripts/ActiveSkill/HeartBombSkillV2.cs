using UnityEngine;

public class HeartBombSkillV2 : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "HeartBomb";
        poolId = "HeartBombV2";
        skillId = 31209;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => GetCenterInMonsters();
    protected override Vector3 GetDirection() => Vector3.zero;
}