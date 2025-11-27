using UnityEngine;

public class SonicAttackSkillV2 : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "SonicAttack";
        poolId = "SonicAttackV2";
        skillId = 31205;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<BoxCollider2D>();
        col.size = new Vector2(skillData.skill_range, col.size.y);
    }

    protected override void SetupParticle(GameObject particle, GameObject clone)
    {
        var particleScale = particle.transform.localScale;
        particleScale.x *= skillData.skill_range;
        particle.transform.localScale = particleScale;
    }

    protected override Vector3 GetStartPosition() => transform.position;
}