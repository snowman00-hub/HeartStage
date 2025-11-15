using UnityEngine;

// 컴포넌트 장착시 쿨타임마다 윗방향으로 음파 공격 스킬 발사
public class SonicAttackSkill : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject sonicAttackPrefab;
    private string sonicAttackId = "SonicAttack";
    private string skillDataAssetName = "얼굴천재";

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        sonicAttackPrefab = ResourceManager.Instance.Get<GameObject>(sonicAttackId);
        PoolManager.Instance.CreatePool(sonicAttackId, sonicAttackPrefab, 10, 30);
        // 스킬매니저에 등록
        ActiveSkillManager.Instance.RegisterSkillBehavior(gameObject, skillData.skill_id, this);
        ActiveSkillManager.Instance.RegisterSkill(gameObject, skillData.skill_id);
    }

    // 발사
    public void Execute()
    {
        var projectileGo = PoolManager.Instance.Get(sonicAttackId);

        Vector3 startPos = transform.position;
        Vector3 dir = Vector3.up;

        var proj = projectileGo.GetComponent<CharacterProjectile>();
        if (proj == null)
        {
            PoolManager.Instance.Release(sonicAttackId, projectileGo);
            return;
        }

        float speed = skillData.skill_speed;
        int damage = skillData.skill_dmg;

        proj.SetMissile(sonicAttackId, string.Empty, startPos, dir, speed, damage, PenetrationType.Penetrate);
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}