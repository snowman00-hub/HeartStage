using System.Collections.Generic;
using UnityEngine;

// 실명 효과 주기
public class FaceGeniusSkill : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject faceGeniusPrefab;
    private string faceGeniusAssetName = "FaceGenius";
    private string skillDataAssetName = "얼굴 천재";

    // 디버프 모음(몬스터에게 장착시킬) (ID, 수치, 지속시간)
    private List<(int id, float value, float duration)> debuffList = new List<(int, float, float)>();

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        faceGeniusPrefab = ResourceManager.Instance.Get<GameObject>(faceGeniusAssetName);

        var prefabClone = Instantiate(faceGeniusPrefab);
        prefabClone.SetActive(false);
        // 스킬 범위 적용
        var collider = prefabClone.GetComponent<CircleCollider2D>();
        collider.radius = skillData.skill_range;
        // 파티클 적용
        var particleGo = Instantiate(ResourceManager.Instance.Get<GameObject>(skillData.skillprojectile_prefab), prefabClone.transform);
        var particleScale = particleGo.transform.localScale;
        particleScale.x *= collider.radius;
        particleGo.transform.localScale = particleScale;
        // 오브젝트 풀 생성
        PoolManager.Instance.CreatePool(faceGeniusAssetName, prefabClone, 10, 30);
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

    // 스킬 사용
    public void Execute()
    {
        var projectileGo = PoolManager.Instance.Get(faceGeniusAssetName);

        Vector3 startPos = transform.position;
        Vector3 dir = Vector3.up;

        var proj = projectileGo.GetComponent<CharacterProjectile>();
        if (proj == null)
        {
            PoolManager.Instance.Release(faceGeniusAssetName, projectileGo);
            return;
        }

        float speed = skillData.skill_speed;
        int damage = skillData.skill_dmg;

        proj.SetMissile(faceGeniusAssetName, string.Empty, startPos, dir, speed, damage,
            PenetrationType.NonPenetrate, false, debuffList);
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}