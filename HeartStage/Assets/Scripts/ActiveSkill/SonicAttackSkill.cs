using System.Collections.Generic;
using UnityEngine;

// 컴포넌트 장착시 쿨타임마다 윗방향으로 음파 공격 스킬 발사
public class SonicAttackSkill : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject sonicAttackPrefab;
    private string sonicAttackId = "SonicAttack";
    private string skillDataAssetName = "만능 엔터테이너";

    // 디버프 모음(몬스터에게 장착시킬) (ID, 수치, 지속시간)
    private List<(int id, float value, float duration)> debuffList = new List<(int, float, float)>();

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        sonicAttackPrefab = ResourceManager.Instance.Get<GameObject>(sonicAttackId);

        var prefabClone = Instantiate(sonicAttackPrefab);
        prefabClone.SetActive(false);
        // 스킬 범위 적용
        var collider = prefabClone.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(skillData.skill_range,collider.size.y);
        // 파티클 적용
        var particleGo = Instantiate(ResourceManager.Instance.Get<GameObject>(skillData.skillprojectile_prefab), prefabClone.transform);
        var particleScale = particleGo.transform.localScale;
        particleScale.x *= collider.size.x;
        particleGo.transform.localScale = particleScale;
        // 오브젝트 풀 생성
        PoolManager.Instance.CreatePool(sonicAttackId, prefabClone, 10, 30);
        Destroy(prefabClone);
        // 스킬매니저에 등록
        ActiveSkillManager.Instance.RegisterSkillBehavior(gameObject, skillData.skill_id, this);
        ActiveSkillManager.Instance.RegisterSkill(gameObject, skillData.skill_id);
        // 디버프 등록
        if (skillData.skill_eff1 != 0)
        {
            debuffList.Add((skillData.skill_eff1, skillData.skill_eff1_val, skillData.skill_eff1_duration));
        }
        if (skillData.skill_eff2 != 0)
        {
            debuffList.Add((skillData.skill_eff2, skillData.skill_eff2_val, skillData.skill_eff2_duration));
        }
        if (skillData.skill_eff3 != 0)
        {
            debuffList.Add((skillData.skill_eff3, skillData.skill_eff3_val, skillData.skill_eff3_duration));
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

        proj.SetMissile(sonicAttackId, string.Empty, startPos, dir, speed, damage,
            PenetrationType.Penetrate, false, debuffList);
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}