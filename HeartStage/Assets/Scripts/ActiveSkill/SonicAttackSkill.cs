using UnityEngine;

public class SonicAttackSkill : MonoBehaviour, ISkillBehavior
{
    private ActiveSkillData skillData;
    private GameObject sonicAttackPrefab;
    private string sonicAttackId = "SonicAttack";
    private string skillDataAssetName = "얼굴천재";

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<ActiveSkillData>(skillDataAssetName);
        sonicAttackPrefab = ResourceManager.Instance.Get<GameObject>(sonicAttackId);
        PoolManager.Instance.CreatePool(sonicAttackId, sonicAttackPrefab, 10, 30);
        ActiveSkillManager.Instance.RegisterSkill(gameObject, skillData.skill_id);
    }

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
        ActiveSkillManager.Instance.UnRegisterSkill(skillData.skill_id);
    }
}