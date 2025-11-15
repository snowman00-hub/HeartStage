using UnityEngine;

// 실명 효과 주기
public class ReverseCharmSkillV2 : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject reverseCharmPrefab;
    private string reverseCharmAssetName = "ReverseCharm";
    private string skillDataAssetName = "넘치는 반전매력";

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        reverseCharmPrefab = ResourceManager.Instance.Get<GameObject>(reverseCharmAssetName);

        var prefabClone = Instantiate(reverseCharmPrefab);
        prefabClone.SetActive(false);
        // 스킬 범위 적용
        var collider = prefabClone.GetComponent<CircleCollider2D>();
        collider.radius = skillData.skill_range;
        var particle = prefabClone.GetComponentInChildren<ParticleSystem>();
        particle.transform.localScale = particle.transform.localScale * skillData.skill_range;
        // 오브젝트 풀 생성
        PoolManager.Instance.CreatePool(reverseCharmAssetName, prefabClone, 10, 30);
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

    // 스킬 사용
    public void Execute()
    {
        var projectileGo = PoolManager.Instance.Get(reverseCharmAssetName);

        Vector3 startPos = transform.position;
        Vector3 dir = Vector3.up;

        var proj = projectileGo.GetComponent<CharacterProjectile>();
        if (proj == null)
        {
            PoolManager.Instance.Release(reverseCharmAssetName, projectileGo);
            return;
        }

        float speed = skillData.skill_speed;
        int damage = skillData.skill_dmg;

        proj.SetMissile(reverseCharmAssetName, string.Empty, startPos, dir, speed, damage, PenetrationType.Penetrate, false);
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}