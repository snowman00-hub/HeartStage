using System.Collections.Generic;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [HideInInspector]
    public int id = 11010101; // 테스트 id

    private CharacterData data;
    private List<GameObject> monsters = new List<GameObject>();
    private float nextAttackTime;
    private float cleanupTimer = 0f;

    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        // CSV → ScriptableObject 반영
        var csvData = DataTableManager.CharacterTable.Get(id);
        data = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
        data.UpdateData(csvData);
        // bullet 프리팹과 projectile 프리팹 로드
        var bulletPrefab = ResourceManager.Instance.Get<GameObject>(data.bullet_PrefabName);
        var projectilePrefab = ResourceManager.Instance.Get<GameObject>(data.projectile_AssetName);
        // 런타임 조립: bullet 안에 projectile 추가
        var combined = Instantiate(bulletPrefab);
        var projectileInstance = Instantiate(projectilePrefab, combined.transform);
        projectileInstance.transform.localPosition = Vector3.zero;
        // 풀 생성: "완성된 조합 프리팹"으로 등록
        PoolManager.Instance.CreatePool(data.projectile_AssetName, combined);
        // 히트 이펙트 풀 생성
        var hitEffectGo = ResourceManager.Instance.Get<GameObject>(data.hitEffect_AssetName);
        PoolManager.Instance.CreatePool(data.hitEffect_AssetName, hitEffectGo);
        // 범위 설정
        circleCollider.radius = data.atk_range;
        // 캐릭터 스프라이트 변경
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        var texture = ResourceManager.Instance.Get<Texture2D>(data.image_AssetName);
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        // 액티브 스킬 등록
        ActiveSkillManager.Instance.RegisterSkill(gameObject, data.skill_id);


        EffectRegistry.Apply(gameObject, 3001, 0.15f, 10f);
    }

    private void Update()
    {
        cleanupTimer += Time.deltaTime;
        if (cleanupTimer >= 1f)
        {
            cleanupTimer = 0f;
            monsters.RemoveAll(m => m == null); // 죽은 몬스터 정리
        }

        if (monsters.Count == 0)
            return;

        if (Time.time < nextAttackTime)
            return;

        GameObject target = GetClosestEnemy();
        if (target != null)
        {
            Fire(target.transform.position);
            nextAttackTime = Time.time + data.atk_speed;
        }
    }

    private void Fire(Vector3 targetPos)
    {
        GameObject projectile = PoolManager.Instance.Get(data.projectile_AssetName);
        if (projectile == null)
            return;

        // 1) 기본 공격력 (나중에 런타임 스탯으로 바꿔도 됨)
        int baseAtk = data.atk_dmg;
        Debug.Log($"CharacterAttack.Fire: baseAtk={baseAtk}");
        // 2) 이 캐릭터에 붙어 있는 모든 IStatMulSource들 중
        //    Attack에 해당하는 배율을 전부 곱한 값
        float atkMul = StatMultiplier.GetTotalMultiplier(gameObject, StatType.Attack);
        // 또는 this.gameObject.GetStatMul(StatType.Attack);

        // 3) 최종 대미지 계산
        int finalDmg = Mathf.RoundToInt(baseAtk * atkMul);
        Debug.Log($"CharacterAttack.Fire: baseAtk={baseAtk}, atkMul={atkMul}, finalDmg={finalDmg}");

        var dir = (targetPos - transform.position).normalized;

        projectile.GetComponent<CharacterProjectile>()
            .SetMissile(data.projectile_AssetName, data.hitEffect_AssetName, transform.position, dir, data.bullet_speed, finalDmg);
    }

    private GameObject GetClosestEnemy()
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var monster in monsters)
        {
            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = monster;
            }
        }

        return closest;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            if (!monsters.Contains(collision.gameObject))
                monsters.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            monsters.Remove(collision.gameObject);
        }
    }
}