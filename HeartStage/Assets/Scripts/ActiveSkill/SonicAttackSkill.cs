using UnityEngine;

// 컴포넌트 장착시 쿨타임마다 윗방향으로 음파 공격 스킬 발사
public class SonicAttackSkill : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject sonicAttackPrefab;
    private string sonicAttackId = "SonicAttack";
    private string skillDataAssetName = "만능 엔터테이너";

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        sonicAttackPrefab = ResourceManager.Instance.Get<GameObject>(sonicAttackId);

        var prefabClone = Instantiate(sonicAttackPrefab);
        prefabClone.SetActive(false);
        // 스킬 범위 적용
        var collider = prefabClone.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(skillData.skill_range,collider.size.y);
        var particle = prefabClone.GetComponentInChildren<ParticleSystem>();
        var particleScale = particle.transform.localScale;
        particleScale.x *= collider.size.x;
        particle.transform.localScale = particleScale;
        // 오브젝트 풀 생성
        PoolManager.Instance.CreatePool(sonicAttackId, prefabClone, 10, 30);
        Destroy(prefabClone);
        // 스킬매니저에 등록
        ActiveSkillManager.Instance.RegisterSkillBehavior(gameObject, skillData.skill_id, this);
        ActiveSkillManager.Instance.RegisterSkill(gameObject, skillData.skill_id);
        // 버프 적용
        var list = DataTableManager.SkillTable.GetEffectIds(skillData.skill_id);
        foreach (var id in list)
        {
            // 버프 적용 코드 추가
        }
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

        proj.SetMissile(sonicAttackId, string.Empty, startPos, dir, speed, damage, PenetrationType.Penetrate, false);
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}