using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// 몬스터 밀집 위치에 폭탄 생성
public class HeartBombSkill : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject heartBombPrefab;
    private string heartBombAssetName = "HeartBomb";
    private string skillDataAssetName = "섹시 다이너마이트";

    // 디버프 모음(몬스터에게 장착시킬) (ID, 수치, 지속시간)
    private List<(int id, float value, float duration)> debuffList = new List<(int, float, float)>();

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        heartBombPrefab = ResourceManager.Instance.Get<GameObject>(heartBombAssetName);

        var prefabClone = Instantiate(heartBombPrefab);
        prefabClone.SetActive(false);
        // 스킬 범위 적용
        var collider = prefabClone.GetComponent<CircleCollider2D>();
        collider.radius = skillData.skill_range;
        // 파티클 적용
        var particleGo = Instantiate(ResourceManager.Instance.Get<GameObject>(skillData.skillprojectile_prefab), prefabClone.transform);
        particleGo.transform.localScale = particleGo.transform.localScale * skillData.skill_range;
        // 오브젝트 풀 생성
        PoolManager.Instance.CreatePool(heartBombAssetName, prefabClone, 10, 30);
        Destroy(prefabClone);
        // 히트 이펙트 오브젝트 풀 생성        
        var hitEffectGo = ResourceManager.Instance.Get<GameObject>(skillData.skillhit_prefab);
        PoolManager.Instance.CreatePool(skillData.skillhit_prefab, hitEffectGo);
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
        var projectileGo = PoolManager.Instance.Get(heartBombAssetName);

        Vector3 startPos = GetCenterInMonsters();
        Vector3 dir = Vector3.zero;

        var proj = projectileGo.GetComponent<CharacterProjectile>();
        if (proj == null)
        {
            PoolManager.Instance.Release(heartBombAssetName, projectileGo);
            return;
        }

        proj.SetMissile(heartBombAssetName, skillData.skillhit_prefab, startPos, dir, 0, skillData.skill_dmg,
            PenetrationType.Penetrate, false, debuffList);
        ReleaseAsync(projectileGo, skillData.skill_duration).Forget();
    }

    public async UniTaskVoid ReleaseAsync(GameObject projectileGo, float time)
    {
        await UniTask.WaitForSeconds(time);
        PoolManager.Instance.Release(heartBombAssetName, projectileGo);
    }

    // 몬스터 밀집된 곳 얻기
    public Vector3 GetCenterInMonsters()
    {
        var objs = GameObject.FindGameObjectsWithTag(Tag.Monster); // 활성화된 몬스터만
        if (objs.Length == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var obj in objs)
        {
            if (obj.transform.position.y <= 10f) // Y 10 이하 필터
            {
                sum += obj.transform.position;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        return sum / count;
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}