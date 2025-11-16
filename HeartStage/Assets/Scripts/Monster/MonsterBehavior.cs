using UnityEngine;

public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData;
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    private float attackCooldown = 0;    
    private bool isBoss = false;
    private MonsterSpawner monsterSpawner;
    private HealthBar healthBar;

    private int currentHP;

    public int GetCurrentHP() => currentHP;
    public MonsterData GetMonsterData() => monsterData;
    public bool IsBossMonster() => isBoss;

    public void Init(MonsterData data)
    {
        monsterData = data;
        currentHP = data.hp;
        isBoss = IsBossMonster(data.id);
        InitHealthBar();
    }

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
        if (monsterData == null)
            return;

        // 공격 쿨다운
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = monsterData.attackSpeed;
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
        if(monsterSpawner != null && monsterData != null)
        {
            monsterSpawner.OnMonsterDied(monsterData.id);
        }

        gameObject.SetActive(false);
        // 경험치 생성
        ItemManager.Instance.SpawnExp(transform.position);
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
            Vector3 targetPosition = hit.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;

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

    public static bool IsBossMonster(int id)
    {
        if(DataTableManager.MonsterTable != null)
        {
            var monsterData = DataTableManager.MonsterTable.Get(id);
            if(monsterData != null)
            {
                return monsterData.mon_type == 2; // 2가 보스 몬스터 타입이라고 가정
            }
        }

        return id == 22201 || id == 22214;
    }
}