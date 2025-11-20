using UnityEngine;

public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData; // SO를 직접 참조 (런타임 변경사항 즉시 반영)
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    private float attackCooldown = 0;
    private bool isBoss = false;
    private MonsterSpawner monsterSpawner;
    private HealthBar healthBar;

    private readonly string attack = "Attack";
    //private readonly string die = "Die";

    private Animator animator;
    //혼란 전용 셀프 콜라이더
    private Collider2D selfCollider;


    private int currentHP;
    private int maxHP; // 최대 HP는 따로 저장 (SO 변경 시에도 유지)

    public int GetCurrentHP() => currentHP;
    public MonsterData GetMonsterData() => monsterData;
    public bool IsBossMonster() => isBoss;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // 몬스터 초기화 (SO 참조 설정, HP는 필요시에만 갱신)
    public void Init(MonsterData data)
    {
        monsterData = data;

        // 최초 스폰 시 또는 최대 HP가 변경된 경우에만 HP 설정
        if (currentHP <= 0 || maxHP != data.hp)
        {
            maxHP = data.hp;
            currentHP = data.hp;
        }       

        isBoss = IsBossMonster(data.id);
        InitHealthBar();

        animator = GetComponentInChildren<Animator>();
    }

    // 체력바 초기화
    private void InitHealthBar()
    {
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.Init(this, isBoss);
            healthBar.ShowHealthBar();
        }
    }

    public void SetMonsterSpawner(MonsterSpawner spawner)
    {
        monsterSpawner = spawner;
    }

    private void Update()
    {
        if (monsterData == null || EffectBase.Has<StunEffect>(gameObject))
            return;

        // SO의 최신 공격속도 값을 직접 사용 (런타임 변경사항 즉시 반영)
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            if (EffectBase.Has<ConfuseEffect>(gameObject))
            {
                ConfuseAttack();
            }
            else
            {
                Attack();
            }
            attackCooldown = monsterData.attackSpeed; // SO에서 직접 가져옴
        }
    }

  
    public void Attack()
    {
        switch (monsterData.attType)
        {
            case 1:
                MeleeAttack();
                break;
            case 2:
                RangedAttack();
                break;
        }
    }

    public void OnDamage(int damage, bool isCritical = false)
    {
        if (monsterData != null)
        {
            currentHP -= damage;
            SoundManager.Instance.PlayMonsterHitSound();            
            //Debug.Log($"{monsterData.monsterName}이(가) {damage}의 피해를 입었습니다. 남은 HP: {currentHP}");

            var ondamageEvents = GetComponents<IDamaged>();
            foreach(var ondamageEvent in ondamageEvents)
            {
                ondamageEvent.OnDamaged(damage, gameObject, isCritical);
            }
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (monsterSpawner != null && monsterData != null)
        {
            monsterSpawner.OnMonsterDied(monsterData.id);
        }        

        gameObject.SetActive(false);
        // 경험치 생성
        int rand = Random.Range(monsterData.minExp, monsterData.maxExp + 1);
        ItemManager.Instance.SpawnExp(transform.position, rand);
        // 드랍아이템 생성
        if (monsterData == null) // 왜 없는 경우가 있는지 ?
            return;

        var dropList = DataTableManager.MonsterTable.GetDropItemInfo(monsterData.id);
        foreach(var dropItem in dropList)
        {
            ItemManager.Instance.SpawnItem(dropItem.Key, dropItem.Value, transform.position);
        }
    }

    private void MeleeAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if(animator != null)
        {
            animator.SetTrigger(attack);
        }
        else
        {
            Debug.LogWarning("Animator가 null입니다!");
        }

        if (hit != null)
        {
            var target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.OnDamage(monsterData.att); 
            }
        }
    }

    private void RangedAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if (hit != null)
        {
            Vector3 direction = Vector3.down;

            var projectileObj = PoolManager.Instance.Get(MonsterProjectilePoolId);
            if (projectileObj != null)
            {
                projectileObj.transform.position = transform.position;
                projectileObj.transform.rotation = Quaternion.identity;

                var projectile = projectileObj.GetComponent<MonsterProjectile>();
                if (projectile != null)
                {
                    projectile.Init(direction, monsterData.bulletSpeed, monsterData.att);
                }

                projectileObj.SetActive(true);
            }
        }
    }

    private void ConfuseAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position,
        monsterData.attackRange,
        LayerMask.GetMask(Tag.Monster)
    );
        Collider2D targetCollider = null;

        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            // 자기 자신 콜라이더는 스킵
            if (hit == selfCollider)
                continue;

            targetCollider = hit;
            break; // 일단 하나만 때릴 거면 첫 번째만 선택
        }

        if(targetCollider != null)
        {
            var target = targetCollider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.OnDamage(monsterData.att);
            }
        }
    }


    public static bool IsBossMonster(int id)
    {
        if (DataTableManager.MonsterTable != null)
        {
            var monsterData = DataTableManager.MonsterTable.Get(id);
            if (monsterData != null)
            {
                return monsterData.mon_type == 2; 
            }
        }

        return id == 22201 || id == 22214; // 보스 id
    }
}