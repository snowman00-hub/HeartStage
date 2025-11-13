using UnityEngine;
using System.Collections.Generic;

public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData;
    private const string MonsterProjectilePoolId = "MonsterProjectile";
    private float attackCooldown = 0;    
    private bool isBoss = false;
    private MonsterSpawner monsterSpawner;

    private int currentHP;
    public void Init(MonsterData data)
    {
        monsterData = data;
        currentHP = data.hp;
        isBoss = IsBossMonster(data.id);
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

    public void OnDamage(int damage)
    {
        if (monsterData != null)
        {
            currentHP -= damage;
            //Debug.Log($"{monsterData.monsterName}이(가) {damage}의 피해를 입었습니다. 남은 HP: {currentHP}");
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