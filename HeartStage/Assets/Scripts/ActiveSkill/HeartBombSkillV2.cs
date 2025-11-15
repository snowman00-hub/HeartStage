using Cysharp.Threading.Tasks;
using UnityEngine;

// 몬스터 밀집 위치에 폭탄 생성
public class HeartBombSkillV2 : MonoBehaviour, ISkillBehavior
{
    private SkillData skillData;
    private GameObject heartBombPrefab;
    private string heartBombAssetName = "HeartBomb";
    private string skillDataAssetName = "폭룡적인 섹시 다이너마이트";
    private string hitEffectAssetName = "StoneHit";

    private void Start()
    {
        skillData = ResourceManager.Instance.Get<SkillData>(skillDataAssetName);
        heartBombPrefab = ResourceManager.Instance.Get<GameObject>(heartBombAssetName);

        var prefabClone = Instantiate(heartBombPrefab);
        prefabClone.SetActive(false);
        // 스킬 범위 적용
        var collider = prefabClone.GetComponent<CircleCollider2D>();
        collider.radius = skillData.skill_range;
        var particle = prefabClone.GetComponentInChildren<ParticleSystem>();
        particle.transform.localScale = particle.transform.localScale * skillData.skill_range;
        // 오브젝트 풀 생성
        PoolManager.Instance.CreatePool(heartBombAssetName, prefabClone, 10, 30);
        Destroy(prefabClone);
        // 히트 이펙트 오브젝트 풀 생성
        var hitEffectGo = ResourceManager.Instance.Get<GameObject>(hitEffectAssetName);
        PoolManager.Instance.CreatePool(hitEffectAssetName, hitEffectGo);
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
        var projectileGo = PoolManager.Instance.Get(heartBombAssetName);

        Vector3 startPos = GetCenterInMonsters();
        Vector3 dir = Vector3.zero;

        var proj = projectileGo.GetComponent<CharacterProjectile>();
        if (proj == null)
        {
            PoolManager.Instance.Release(heartBombAssetName, projectileGo);
            return;
        }

        proj.SetMissile(heartBombAssetName, hitEffectAssetName, startPos, dir, 0, skillData.skill_dmg, PenetrationType.Penetrate, false);
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
        var objs = GameObject.FindGameObjectsWithTag(Tag.Monster); // SetActive True인 애들만 
        if (objs.Length == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var obj in objs)
        {
            sum += obj.transform.position; 
        }

        return sum / objs.Length;
    }

    private void OnDisable()
    {
        if (ActiveSkillManager.Instance != null && skillData != null)
        {
            ActiveSkillManager.Instance.UnRegisterSkill(gameObject, skillData.skill_id);
        }
    }
}